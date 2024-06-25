namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

// TODO: generalize to 'baited puddles' component
class RapidBoltsBait(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.RapidBolts)
            AddSpread(actor);
    }
}

class RapidBoltsAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos pos, int numCasts)> _puddles = [];
    private static readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _puddles.Select(p => new AOEInstance(_shape, p.pos));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RapidBoltsAOE)
        {
            ++NumCasts;
            int index = _puddles.FindIndex(p => p.pos.InCircle(spell.TargetXZ, 1));
            if (index < 0)
                _puddles.Add((spell.TargetXZ, 1));
            else if (_puddles[index].numCasts < 11)
                _puddles[index] = (spell.TargetXZ, _puddles[index].numCasts + 1);
            else
                _puddles.RemoveAt(index);
        }
    }
}
