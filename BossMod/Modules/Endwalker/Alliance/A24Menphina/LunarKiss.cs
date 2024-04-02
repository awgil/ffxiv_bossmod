namespace BossMod.Endwalker.Alliance.A24Menphina;

class LunarKiss : Components.GenericBaitAway
{
    private static readonly AOEShapeRect _shape = new(60, 3);

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.LunarKiss)
            CurrentBaits.Add(new(module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LunarKissAOE)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}
