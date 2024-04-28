namespace BossMod.Endwalker.Trial.T08Asura;

class MyriadAspects(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40, 15.Degrees());
    private DateTime _activation1;
    private DateTime _activation2;
    private readonly List<ActorCastInfo> _spell1 = [];
    private readonly List<ActorCastInfo> _spell2 = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts < 6 && _spell1.Count > 0)
            foreach (var c in _spell1)
                yield return new(cone, Module.Center, c.Rotation, _activation1);
        if (NumCasts >= 6 && _spell2.Count > 0)
            foreach (var c in _spell2)
                yield return new(cone, Module.Center, c.Rotation, _activation2);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MyriadAspects1)
        {
            _activation1 = spell.NPCFinishAt;
            _spell1.Add(spell);
        }
        if ((AID)spell.Action.ID == AID.MyriadAspects2)
        {
            _activation2 = spell.NPCFinishAt;
            _spell2.Add(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MyriadAspects1 or AID.MyriadAspects2)
            ++NumCasts;
        if (NumCasts == 12)
        {
            NumCasts = 0;
            _spell1.Clear();
            _spell2.Clear();
        }
    }
}
