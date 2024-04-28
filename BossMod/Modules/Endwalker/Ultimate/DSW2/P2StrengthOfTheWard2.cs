namespace BossMod.Endwalker.Ultimate.DSW2;

// leap (icons spread) + rage (rest stack)
// note: we currently don't show stack hints, that happens automatically if mechanic is resolved properly
// TODO: figure out rage target - it is probably a random non-tank non-spread
class P2StrengthOfTheWard2SpreadStack : Components.UniformStackSpread
{
    public bool LeapsDone { get; private set; }
    public bool RageDone { get; private set; }
    private readonly Actor? _leftCharge;
    private readonly Actor? _rightCharge;
    private Angle _dirToStackPos;

    public P2StrengthOfTheWard2SpreadStack(BossModule module) : base(module, 8, 24, 5)
    {
        var c1 = module.Enemies(OID.SerAdelphel).FirstOrDefault();
        var c2 = module.Enemies(OID.SerJanlenoux).FirstOrDefault();
        if (c1 == null || c2 == null)
        {
            ReportError($"Failed to find charge sources");
            return;
        }

        var offset1 = c1.Position - Module.Center;
        var offset2 = c2.Position - Module.Center;
        var toStack = -(offset1 + offset2);
        (_leftCharge, _rightCharge) = toStack.OrthoL().Dot(offset1) > 0 ? (c1, c2) : (c2, c1);
        _dirToStackPos = Angle.FromDirection(toStack);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var safespot in EnumSafeSpots(actor))
            movementHints.Add(actor.Position, safespot, ArenaColor.Safe);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var safespot in EnumSafeSpots(pc))
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SkywardLeapP2:
                LeapsDone = true;
                Spreads.Clear();
                break;
            case AID.DragonsRageAOE:
                RageDone = true;
                Stacks.Clear();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.SkywardLeapP2)
            AddSpread(actor);
    }

    private IEnumerable<WPos> EnumSafeSpots(Actor player)
    {
        if (IsSpreadTarget(player))
        {
            if (!LeapsDone)
            {
                // TODO: select single safe spot for a player based on some criterion...
                yield return SafeSpotAt(_dirToStackPos + 100.Degrees());
                yield return SafeSpotAt(_dirToStackPos + 180.Degrees());
                yield return SafeSpotAt(_dirToStackPos - 100.Degrees());
            }
        }
        else if (_leftCharge?.Tether.Target == player.InstanceID)
        {
            yield return SafeSpotAt(_dirToStackPos - 18.Degrees());
        }
        else if (_rightCharge?.Tether.Target == player.InstanceID)
        {
            yield return SafeSpotAt(_dirToStackPos + 18.Degrees());
        }
        else if (!RageDone)
        {
            yield return SafeSpotAt(_dirToStackPos);
        }
    }

    private WPos SafeSpotAt(Angle dir) => Module.Center + 20 * dir.ToDirection();
}

// growing voidzones
class P2StrengthOfTheWard2Voidzones(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DimensionalCollapseAOE), 9, "GTFO from voidzone aoe!");

// charges on tethered targets
class P2StrengthOfTheWard2Charges(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.HolyShieldBash))
{
    private readonly List<Actor> _chargeSources = [.. module.Enemies(OID.SerAdelphel), .. module.Enemies(OID.SerJanlenoux)];

    private const float _chargeHalfWidth = 4;

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
                Arena.ZoneRect(source.Position, target.Position, _chargeHalfWidth, ArenaColor.AOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw tethers
        foreach (var source in _chargeSources)
        {
            Arena.Actor(source, ArenaColor.Enemy, true);
            var target = WorldState.Actors.Find(source.Tether.Target);
            if (target != null)
                Arena.AddLine(source.Position, target.Position, ArenaColor.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
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
        return Raid.WithoutSlot().Any(p => p.Role != Role.Tank && p.Position.InRect(source.Position, dir, len, 0, _chargeHalfWidth));
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
class P2StrengthOfTheWard2Towers(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.Conviction1AOE), 3)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(spell.LocXZ, Radius, forbiddenSoakers: Raid.WithSlot(true).WhereActor(p => p.Role == Role.Tank).Mask()));
    }
}
