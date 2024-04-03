namespace BossMod.Endwalker.Alliance.A32Llymlaen
{
    class RightStraitCone : Components.SelfTargetedAOEs
    {
        public RightStraitCone() : base(ActionID.MakeSpell(AID.RightStraitCone), new AOEShapeCone(60, 90.Degrees())) { }
    }

    class LeftStraitCone : Components.SelfTargetedAOEs
    {
        public LeftStraitCone() : base(ActionID.MakeSpell(AID.LeftStraitCone), new AOEShapeCone(60, 90.Degrees())) { }
    }
}
