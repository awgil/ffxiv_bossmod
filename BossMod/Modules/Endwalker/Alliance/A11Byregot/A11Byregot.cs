namespace BossMod.Endwalker.Alliance.A11Byregot
{
    class ByregotWard : Components.BaitAwayCast
    {
        public ByregotWard() : base(ActionID.MakeSpell(AID.ByregotWard), new AOEShapeCone(10, 45.Degrees())) { }
    }

    [ModuleInfo(CFCID = 866, NameID = 11281)]
    public class A11Byregot : BossModule
    {
        public A11Byregot(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, 700), 25)) { }
    }
}
