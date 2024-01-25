namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4Phantom
{
    class MaledictionOfAgony : Components.CastCounter
    {
        public MaledictionOfAgony() : base(ActionID.MakeSpell(AID.MaledictionOfAgonyAOE)) { }
    }

    [ModuleInfo(CFCID = 761, NameID = 9755)]
    public class DRS4 : BossModule
    {
        public DRS4(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(202, -370), 24)) { }
    }
}
