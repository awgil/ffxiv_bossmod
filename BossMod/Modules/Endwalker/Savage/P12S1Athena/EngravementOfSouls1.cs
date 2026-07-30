namespace BossMod.Endwalker.Savage.P12S1Athena;

class EngravementOfSouls1Spread(BossModule module) : Components.UniformStackSpread(module, default, 3f, raidwideOnResolve: false, includeDeadTargets: true)
{
    public enum DebuffType { None, Light, Dark }

    struct PlayerState
    {
        public DebuffType Debuff;
        public WPos CachedSafespot;
    }

    private readonly P12S1AthenaConfig _config = Service.Config.Get<P12S1AthenaConfig>();
    private readonly EngravementOfSoulsTethers? _tethers = module.FindComponent<EngravementOfSoulsTethers>();
    private readonly PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var safespot = CalculateSafeSpot(pcSlot);
        if (safespot != default)
        {
            Arena.ZoneCircleOutline(safespot, 1f, Colors.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var type = status.ID switch
        {
            (uint)SID.UmbralbrightSoul => DebuffType.Light,
            (uint)SID.AstralbrightSoul => DebuffType.Dark,
            _ => DebuffType.None
        };
        if (type != DebuffType.None && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _states[slot].Debuff = type;
            AddSpread(actor, WorldState.FutureTime(10.1d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.UmbralGlow or (uint)AID.AstralGlow)
        {
            Spreads.Clear();
        }
    }

    private WPos CalculateSafeSpot(int slot)
    {
        if (_states[slot].CachedSafespot == default && _states[slot].Debuff != DebuffType.None && _tethers != null && _tethers.CurrentBaits.Count == 4)
        {
            switch (_config.Engravement1Hints)
            {
                case P12S1AthenaConfig.EngravementOfSouls1Strategy.Default:
                    WDir[] offsets = [new(+1f, -1f), new(+1f, +1f), new(-1f, +1f), new(-1f, -1f)]; // CW from N
                    var relevantTether = _states[slot].Debuff == DebuffType.Light ? EngravementOfSoulsTethers.TetherType.Dark : EngravementOfSoulsTethers.TetherType.Light;
                    var expectedPositions = _tethers.States.Where(s => s.Source != null).Select(s => (s.Source!.Position + 40f * s.Source.Rotation.ToDirection(), s.Tether == relevantTether)).ToList();
                    var offsetsOrdered = (Raid[slot]?.Class.IsSupport() ?? false) ? offsets.AsEnumerable() : offsets.AsEnumerable().Reverse();
                    var positionsOrdered = offsetsOrdered.Select(d => Arena.Center + 7f * d);
                    var firstMatch = positionsOrdered.First(p => expectedPositions.MinBy(ep => (ep.Item1 - p).LengthSq()).Item2);
                    _states[slot].CachedSafespot = firstMatch;
                    break;
            }
        }
        return _states[slot].CachedSafespot;
    }
}
