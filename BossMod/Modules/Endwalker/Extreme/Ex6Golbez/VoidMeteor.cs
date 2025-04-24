namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class VoidMeteor(BossModule module) : Components.GenericBaitAway(module, AID.VoidMeteorAOE, centerAtTarget: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.VoidMeteor)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCircle(6)));
    }
}
