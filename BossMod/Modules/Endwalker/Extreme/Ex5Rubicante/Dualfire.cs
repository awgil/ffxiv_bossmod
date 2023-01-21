namespace BossMod.Endwalker.Extreme.Ex5Rubicante
{
    class Dualfire : Components.GenericBaitAway
    {
        private static AOEShapeCone _shape = new(60, 60.Degrees()); // TODO: verify angle

        public Dualfire() : base(ActionID.MakeSpell(AID.DualfireAOE)) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Dualfire)
                CurrentBaits.Add((module.PrimaryActor, actor, _shape));
        }
    }
}
