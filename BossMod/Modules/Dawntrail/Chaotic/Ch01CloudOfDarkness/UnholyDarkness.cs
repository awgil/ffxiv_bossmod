namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class UnholyDarkness(BossModule module) : Components.StackWithIcon(module, (uint)IconID.UnholyDarkness, AID.UnholyDarknessAOE, 6, 8.1f, 4)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            Stacks.Clear(); // if one of the target dies, it won't get hit
            ++NumFinishedStacks;
        }
    }
}
