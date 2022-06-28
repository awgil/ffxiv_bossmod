namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn
{
    class Echoes : CommonComponents.FullPartyStack
    {
        public Echoes() : base(ActionID.MakeSpell(AID.EchoesAOE), 6) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Echoes)
            {
                Target = actor;
            }
        }
    }
}
