namespace BossMod.Endwalker.Ultimate.DSW2;

// leap (icons spread) + rage (rest stack)
// note: we currently don't show stack hints, that happens automatically if mechanic is resolved properly
// TODO: figure out rage target - it is probably a random non-tank non-spread
class P2StrengthOfTheWard2SpreadStack : Components.UniformStackSpread
{
    public bool LeapsDone { get; private set; }
    public bool RageDone { get; private set; }
    private Actor? _leftCharge;
    private Actor? _rightCharge;
    private Angle _dirToStackPos;

    public P2StrengthOfTheWard2SpreadStack() : base(8, 24, 5) { }

    public override void Init(BossModule module)
    {
        var c1 = module.Enemies(OID.SerAdelphel).FirstOrDefault();
        var c2 = module.Enemies(OID.SerJanlenoux).FirstOrDefault();
        if (c1 == null || c2 == null)
        {
            module.ReportError(this, $"Failed to find charge sources");
            return;
        }

        var offset1 = c1.Position - module.Bounds.Center;
        var offset2 = c2.Position - module.Bounds.Center;
        var toStack = -(offset1 + offset2);
        (_leftCharge, _rightCharge) = toStack.OrthoL().Dot(offset1) > 0 ? (c1, c2) : (c2, c1);
        _dirToStackPos = Angle.FromDirection(toStack);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (movementHints != null)
            foreach (var safespot in EnumSafeSpots(module, actor))
                movementHints.Add(actor.Position, safespot, ArenaColor.Safe);
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);
        foreach (var safespot in EnumSafeSpots(module, pc))
            arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.SkywardLeapP2)
            AddSpread(actor);
    }

    private IEnumerable<WPos> EnumSafeSpots(BossModule module, Actor player)
    {
        if (IsSpreadTarget(player))
        {
            if (!LeapsDone)
            {
                // TODO: select single safe spot for a player based on some criterion...
                yield return SafeSpotAt(module, _dirToStackPos + 100.Degrees());
                yield return SafeSpotAt(module, _dirToStackPos + 180.Degrees());
                yield return SafeSpotAt(module, _dirToStackPos - 100.Degrees());
            }
        }
        else if (_leftCharge?.Tether.Target == player.InstanceID)
        {
            yield return SafeSpotAt(module, _dirToStackPos - 18.Degrees());
        }
        else if (_rightCharge?.Tether.Target == player.InstanceID)
        {
            yield return SafeSpotAt(module, _dirToStackPos + 18.Degrees());
        }
        else if (!RageDone)
        {
            yield return SafeSpotAt(module, _dirToStackPos);
        }
    }

    private WPos SafeSpotAt(BossModule module, Angle dir) => module.Bounds.Center + 20 * dir.ToDirection();
}

// growing voidzones
class P2StrengthOfTheWard2Voidzones : Components.LocationTargetedAOEs
{
    public P2StrengthOfTheWard2Voidzones() : base(ActionID.MakeSpell(AID.DimensionalCollapseAOE), 9, "GTFO from voidzone aoe!") { }
}

// charges on tethered targets
class P2StrengthOfTheWard2Charges : Components.CastCounter
{
    private List<Actor> _chargeSources = new();

    private static readonly float _chargeHalfWidth = 4;

    public P2StrengthOfTheWard2Charges() : base(ActionID.MakeSpell(AID.HolyShieldBash)) { }

    public override void Init(BossModule module)
    {
        _chargeSources.AddRange(module.Enemies(OID.SerAdelphel));
        _chargeSources.AddRange(module.Enemies(OID.SerJanlenoux));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (NumCasts > 0)
            return;

        var tetherSource = _chargeSources.Find(s => s.Tether.Target == actor.InstanceID);
        if (actor.Role == Role.Tank)
        {
            if (tetherSource == null)
                hints.Add("Grab tether!");
            else if (ChargeHitsNonTanks(module, tetherSource, actor))
                hints.Add("Move away from raid!");
        }
        else
        {
            if (tetherSource != null)
                hints.Add("Pass tether!");
            else if (IsInChargeAOE(module, actor))
                hints.Add("GTFO from tanks!");
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var source in _chargeSources)
        {
            var target = module.WorldState.Actors.Find(source.Tether.Target);
            if (target != null)
            {
                arena.ZoneRect(source.Position, target.Position, _chargeHalfWidth, ArenaColor.AOE);
            }
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // draw tethers
        foreach (var source in _chargeSources)
        {
            module.Arena.Actor(source, ArenaColor.Enemy, true);
            var target = module.WorldState.Actors.Find(source.Tether.Target);
            if (target != null)
                module.Arena.AddLine(source.Position, target.Position, ArenaColor.Danger);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _chargeSources.Remove(caster);
            ++NumCasts;
        }
    }

    private bool ChargeHitsNonTanks(BossModule module, Actor source, Actor target)
    {
        var dir = target.Position - source.Position;
        var len = dir.Length();
        dir /= len;
        return module.Raid.WithoutSlot().Any(p => p.Role != Role.Tank && p.Position.InRect(source.Position, dir, len, 0, _chargeHalfWidth));
    }

    private bool IsInChargeAOE(BossModule module, Actor player)
    {
        foreach (var source in _chargeSources)
        {
            var target = module.WorldState.Actors.Find(source.Tether.Target);
            if (target != null && player.Position.InRect(source.Position, target.Position - source.Position, _chargeHalfWidth))
                return true;
        }
        return false;
    }
}

// towers
// TODO: assign tower to proper player
class P2StrengthOfTheWard2Towers : Components.CastTowers
{
    public P2StrengthOfTheWard2Towers() : base(ActionID.MakeSpell(AID.Conviction1AOE), 3) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(spell.LocXZ, Radius, forbiddenSoakers: module.Raid.WithSlot(true).WhereActor(p => p.Role == Role.Tank).Mask()));
    }
}
