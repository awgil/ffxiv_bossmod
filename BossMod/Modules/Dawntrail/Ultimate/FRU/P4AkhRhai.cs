namespace BossMod.Dawntrail.Ultimate.FRU;

class P4AkhRhai(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.AkhRhaiAOE))
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhRhai)
            AOEs.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }
}
