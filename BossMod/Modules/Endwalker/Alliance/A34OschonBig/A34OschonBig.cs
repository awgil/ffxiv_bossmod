namespace BossMod.Endwalker.Alliance.A34OschonBig;

class TheArrowTankbuster : Components.BaitAwayCast
{
    public TheArrowTankbuster() : base(ActionID.MakeSpell(AID.TheArrowTankbuster), new AOEShapeCircle(10), true) { }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11300)]
public class A34Oschon : BossModule
{
    public A34Oschon(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 750), 20, 20)) { }
}