namespace BossMod.Shadowbringers.Ultimate.TEA;

class P2PlasmaShield(BossModule module) : Components.DirectionalParry(module, (uint)OID.PlasmaShield)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.PlasmaShield)
            PredictParrySide(actor.InstanceID, Side.Left | Side.Right | Side.Back);
    }
}
