namespace BossMod.Dawntrail.Ultimate.FRU;

class P4AkhRhai(BossModule module) : Components.GenericAOEs(module, AID.AkhRhaiAOE)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (AOEs.Count == 0)
        {
            // preposition for baits - note that this is very arbitrary...
            var off = 10 * 45.Degrees().ToDirection();
            var p1 = ShapeContains.Circle(Module.Center + off, 1);
            var p2 = ShapeContains.Circle(Module.Center - off, 1);
            hints.AddForbiddenZone(p => !(p1(p) || p2(p)), DateTime.MaxValue);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhRhai)
            AOEs.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }
}
