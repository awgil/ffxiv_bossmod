namespace BossMod.Endwalker.Extreme.Ex6Golbez
{
    class VoidMeteor : Components.GenericBaitAway
    {
        public VoidMeteor() : base(ActionID.MakeSpell(AID.VoidMeteorAOE), centerAtTarget: true) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.VoidMeteor)
                CurrentBaits.Add(new(module.PrimaryActor, actor, new AOEShapeCircle(6)));
        }
    }
}
