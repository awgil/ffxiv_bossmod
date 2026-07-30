namespace BossMod.Endwalker.Ultimate.DSW2;

// leap (icons spread) + rage (rest stack)
// note: we currently don't show stack hints, that happens automatically if mechanic is resolved properly
// TODO: figure out rage target - it is probably a random non-tank non-spread
sealed class P2StrengthOfTheWard2SpreadStack : Components.UniformStackSpread
{
    public bool LeapsDone;
    public bool RageDone;
    private readonly Actor? _leftCharge;
    private readonly Actor? _rightCharge;
    private readonly Angle _dirToStackPos;

    public P2StrengthOfTheWard2SpreadStack(BossModule module) : base(module, 8, 24, 5)
    {
        var c1 = module.Enemies((uint)OID.SerAdelphel).FirstOrDefault();
        var c2 = module.Enemies((uint)OID.SerJanlenoux).FirstOrDefault();
        if (c1 == null || c2 == null)
        {
            ReportError($"Failed to find charge sources");
            return;
        }

        var offset1 = c1.Position - Arena.Center;
        var offset2 = c2.Position - Arena.Center;
        var toStack = -(offset1 + offset2);
        (_leftCharge, _rightCharge) = toStack.OrthoL().Dot(offset1) > 0 ? (c1, c2) : (c2, c1);
        _dirToStackPos = Angle.FromDirection(toStack);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var safespot in EnumSafeSpots(actor))
            movementHints.Add(actor.Position, safespot, Colors.Safe);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var safespot in EnumSafeSpots(pc))
            Arena.ZoneCircleOutline(safespot, 1, Colors.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SkywardLeapP2:
                LeapsDone = true;
                Spreads.Clear();
                break;
            case (uint)AID.DragonsRageAOE:
                RageDone = true;
                Stacks.Clear();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SkywardLeapP2)
            AddSpread(actor);
    }

    private WPos[] EnumSafeSpots(Actor player)
    {
        if (IsSpreadTarget(player))
        {
            if (!LeapsDone)
            {
                // TODO: select single safe spot for a player based on some criterion...
                return
                [
                    SafeSpotAt(_dirToStackPos + 100f.Degrees()),
                    SafeSpotAt(_dirToStackPos + 180f.Degrees()),
                    SafeSpotAt(_dirToStackPos - 100f.Degrees())
                ];
            }
        }
        else if (_leftCharge?.Tether.Target == player.InstanceID)
        {
            return [SafeSpotAt(_dirToStackPos - 18f.Degrees())];
        }
        else if (_rightCharge?.Tether.Target == player.InstanceID)
        {
            return [SafeSpotAt(_dirToStackPos + 18f.Degrees())];
        }
        else if (!RageDone)
        {
            return [SafeSpotAt(_dirToStackPos)];
        }
        return [];
    }

    private WPos SafeSpotAt(Angle dir) => Arena.Center + 20f * dir.ToDirection();
}

// growing voidzones
sealed class P2StrengthOfTheWard2Voidzones(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalCollapseAOE, 9f);

// charges on tethered targets
sealed class P2StrengthOfTheWard2Charges(BossModule module) : Components.CastCounter(module, (uint)AID.HolyShieldBash)
{
    private readonly List<Actor> _chargeSources = [.. module.Enemies((uint)OID.SerAdelphel), .. module.Enemies((uint)OID.SerJanlenoux)];

    private const float _chargeHalfWidth = 4f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts > 0)
            return;

        var tetherSource = _chargeSources.Find(s => s.Tether.Target == actor.InstanceID);
        if (actor.Role == Role.Tank)
        {
            if (tetherSource == null)
                hints.Add("Grab tether!");
            else if (ChargeHitsNonTanks(tetherSource, actor))
                hints.Add("Move away from raid!");
        }
        else
        {
            if (tetherSource != null)
                hints.Add("Pass tether!");
            else if (IsInChargeAOE(actor))
                hints.Add("GTFO from tanks!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var source in _chargeSources)
        {
            var target = WorldState.Actors.Find(source.Tether.Target);
            if (target != null)
            {
                Arena.ZoneRect(source.Position, target.Position, _chargeHalfWidth, Colors.AOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw tethers
        foreach (var source in _chargeSources)
        {
            Arena.Actor(source, Colors.Enemy, true);
            var target = WorldState.Actors.Find(source.Tether.Target);
            if (target != null)
                Arena.AddLine(source.Position, target.Position);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _chargeSources.Remove(caster);
            ++NumCasts;
        }
    }

    private bool ChargeHitsNonTanks(Actor source, Actor target)
    {
        var dir = target.Position - source.Position;
        var len = dir.Length();
        dir /= len;
        return Raid.WithoutSlot(false, true, true).Any(p => p.Role != Role.Tank && p.Position.InRect(source.Position, dir, len, default, _chargeHalfWidth));
    }

    private bool IsInChargeAOE(Actor player)
    {
        foreach (var source in _chargeSources)
        {
            var target = WorldState.Actors.Find(source.Tether.Target);
            if (target != null && player.Position.InRect(source.Position, target.Position - source.Position, _chargeHalfWidth))
                return true;
        }
        return false;
    }
}

// towers
// TODO: assign tower to proper player
sealed class P2StrengthOfTheWard2Towers(BossModule module) : Components.CastTowers(module, (uint)AID.Conviction1AOE, 3f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Towers.Add(new(spell.LocXZ, Radius, forbiddenSoakers: Raid.WithSlot(true, true, true).WhereActor(p => p.Role == Role.Tank).Mask()));
    }
}
