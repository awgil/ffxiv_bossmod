namespace BossMod.Dawntrail.Ultimate.FRU;

class P1CyclonicBreakSpreadStack(BossModule module) : Components.UniformStackSpread(module, 6, 6, 2, 2, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CyclonicBreakBossStack:
            case AID.CyclonicBreakImageStack:
                // TODO: this can target either supports or dd
                AddStacks(Module.Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell, 2.7f));
                break;
            case AID.CyclonicBreakBossSpread:
            case AID.CyclonicBreakImageSpread:
                AddSpreads(Module.Raid.WithoutSlot(true), Module.CastFinishAt(spell, 2.7f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CyclonicBreakSinsmoke:
                Stacks.Clear();
                break;
            case AID.CyclonicBreakSinsmite:
                Spreads.Clear();
                break;
        }
    }
}

class P1CyclonicBreakProtean(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeCone(60, 11.25f.Degrees()), ActionID.MakeSpell(AID.CyclonicBreakAOEFirst)); // TODO: verify angle

class P1CyclonicBreakCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _currentBundle;

    private static readonly AOEShapeCone _shape = new(60, 11.25f.Degrees()); // TODO: verify angle

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CyclonicBreakAOEFirst:
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, WorldState.FutureTime(2)));
                break;
            case AID.CyclonicBreakAOERest:
                if (WorldState.CurrentTime > _currentBundle)
                {
                    _currentBundle = WorldState.CurrentTime.AddSeconds(1);
                    ++NumCasts;
                    foreach (ref var aoe in _aoes.AsSpan())
                        aoe.Rotation -= 22.5f.Degrees();
                }
                if (!_aoes.Any(aoe => aoe.Rotation.AlmostEqual(spell.Rotation - 22.5f.Degrees(), 0.1f)))
                    ReportError($"Failed to find protean @ {spell.Rotation}");
                break;
        }
    }
}
