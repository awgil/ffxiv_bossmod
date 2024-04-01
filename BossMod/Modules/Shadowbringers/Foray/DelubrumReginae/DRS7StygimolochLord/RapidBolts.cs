namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

// TODO: generalize to 'baited puddles' component
class RapidBoltsBait : Components.UniformStackSpread
{
    public RapidBoltsBait() : base(0, 5, alwaysShowSpreads: true) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.RapidBolts)
            AddSpread(actor);
    }
}

class RapidBoltsAOE : Components.GenericAOEs
{
    private List<(WPos pos, int numCasts)> _puddles = new();
    private static readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        return _puddles.Select(p => new AOEInstance(_shape, p.pos));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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
