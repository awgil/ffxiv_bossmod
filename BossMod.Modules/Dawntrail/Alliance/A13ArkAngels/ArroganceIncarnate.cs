namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class ArroganceIncarnate(BossModule module) : Components.StackWithIcon(module, (uint)IconID.ArroganceIncarnate, AID.ArroganceIncarnateAOE, 6, 5.8f, 8)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArroganceIncarnate)
            NumFinishedStacks = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            if (++NumFinishedStacks >= 5)
            {
                Stacks.Clear();
            }
        }
    }
}
