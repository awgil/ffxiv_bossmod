namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

class RightArmBlasterFragment : Components.SelfTargetedAOEs
{
    public RightArmBlasterFragment() : base(ActionID.MakeSpell(AID.RightArmBlasterFragment), new AOEShapeRect(100, 3)) { }
}

class RightArmBlasterBoss : Components.SelfTargetedAOEs
{
    public RightArmBlasterBoss() : base(ActionID.MakeSpell(AID.RightArmBlasterBoss), new AOEShapeRect(100, 3)) { }
}

class LeftArmSlash : Components.SelfTargetedAOEs
{
    public LeftArmSlash() : base(ActionID.MakeSpell(AID.LeftArmSlash), new AOEShapeCone(10, 90.Degrees())) { } // TODO: verify angle
}

class LeftArmWave : Components.LocationTargetedAOEs
{
    public LeftArmWave() : base(ActionID.MakeSpell(AID.LeftArmWaveAOE), 24) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 19)] // bnpcname=9958
public class Duel4Dabog : BossModule
{
    public Duel4Dabog(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(250, 710), 20)) { }
}
