namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

// TODO: consider showing something before clones jump?
class BalefulFirestorm : Components.GenericAOEs
{
    private List<(Actor caster, AOEInstance aoe)> _casters = new();
    private static readonly AOEShapeRect _shape = new(50, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        return _casters.Take(3).Select(c => c.aoe);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BalefulComet:
                var delay = 7.6f + _casters.Count * 1.0f;
                _casters.Add((caster, new(_shape, caster.Position, caster.Rotation, module.WorldState.CurrentTime.AddSeconds(delay))));
                break;
            case AID.BalefulFirestorm:
                _casters.RemoveAll(c => c.caster == caster);
                ++NumCasts;
                break;
        }
    }
}
