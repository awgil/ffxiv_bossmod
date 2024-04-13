namespace BossMod.Endwalker.Alliance.A24Menphina;

class LunarKiss(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(60, 3);

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.LunarKiss)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LunarKissAOE)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}
