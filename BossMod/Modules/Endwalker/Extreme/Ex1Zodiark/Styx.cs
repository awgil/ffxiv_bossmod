namespace BossMod.Endwalker.Extreme.Ex1Zodiark
{
    class Styx : CommonComponents.FullPartyStack
    {
        public Styx() : base(ActionID.MakeSpell(AID.StyxAOE), 5) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Styx)
            {
                Target = actor;
            }
        }
    }
}
