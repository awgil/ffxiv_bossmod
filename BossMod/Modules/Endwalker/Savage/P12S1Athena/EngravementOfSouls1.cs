namespace BossMod.Endwalker.Savage.P12S1Athena;

class EngravementOfSouls1Spread(BossModule module) : Components.UniformStackSpread(module, 0, 3, alwaysShowSpreads: true, raidwideOnResolve: false, includeDeadTargets: true)
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
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var type = (SID)status.ID switch
        {
            SID.UmbralbrightSoul => DebuffType.Light,
            SID.AstralbrightSoul => DebuffType.Dark,
            _ => DebuffType.None
        };
        if (type != DebuffType.None && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _states[slot].Debuff = type;
            AddSpread(actor, WorldState.FutureTime(10.1f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.UmbralGlow or AID.AstralGlow)
            Spreads.Clear();
    }

    private WPos CalculateSafeSpot(int slot)
    {
        if (_states[slot].CachedSafespot == default && _states[slot].Debuff != DebuffType.None && _tethers != null && _tethers.CurrentBaits.Count == 4)
        {
            switch (_config.Engravement1Hints)
            {
                case P12S1AthenaConfig.EngravementOfSouls1Strategy.Default:
                    WDir[] offsets = [new(+1, -1), new(+1, +1), new(-1, +1), new(-1, -1)]; // CW from N
                    var relevantTether = _states[slot].Debuff == DebuffType.Light ? EngravementOfSoulsTethers.TetherType.Dark : EngravementOfSoulsTethers.TetherType.Light;
                    var expectedPositions = _tethers.States.Where(s => s.Source != null).Select(s => (s.Source!.Position + 40 * s.Source.Rotation.ToDirection(), s.Tether == relevantTether)).ToList();
                    var offsetsOrdered = (Raid[slot]?.Class.IsSupport() ?? false) ? offsets.AsEnumerable() : offsets.Reverse();
                    var positionsOrdered = offsetsOrdered.Select(d => Module.Center + 7 * d);
                    var firstMatch = positionsOrdered.First(p => expectedPositions.MinBy(ep => (ep.Item1 - p).LengthSq()).Item2);
                    _states[slot].CachedSafespot = firstMatch;
                    break;
            }
        }
        return _states[slot].CachedSafespot;
    }
}
