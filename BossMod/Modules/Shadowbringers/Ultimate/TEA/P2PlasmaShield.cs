namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P2PlasmaShield : Components.DirectionalParry
    {
        public P2PlasmaShield() : base((uint)OID.PlasmaShield) { }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.PlasmaShield)
                PredictParrySide(actor.InstanceID, Side.Left | Side.Right | Side.Back);
        }
    }
}
