namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class SerpentsTide(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeRect _shape = new(80, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SerpentsTide)
            AOEs.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SerpentsTideAOEPerykosNS or AID.SerpentsTideAOEPerykosEW or AID.SerpentsTideAOEThalaosNS or AID.SerpentsTideAOEThalaosEW)
        {
            AOEs.Clear();
            ++NumCasts;
        }
    }
}
