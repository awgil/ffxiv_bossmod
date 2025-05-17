namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D150ScholaColossus;

public enum OID : uint
{
    Boss = 0x2526, //R=3.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    HomingLaserBossCast = 14467, // Boss->self, 4.0s cast, single-target
    HomingLaser = 14468, // Helper->player, 5.0s cast, range 6 circle, spread
    GrandStrike = 14965, // Boss->self, 2.5s cast, range 45+R width 4 rect
}

class HomingLaser(BossModule module) : Components.SpreadFromCastTargets(module, AID.HomingLaser, 6);
class GrandStrike(BossModule module) : Components.StandardAOEs(module, AID.GrandStrike, new AOEShapeRect(48.2f, 2));

class D150ScholaColossusStates : StateMachineBuilder
{
    public D150ScholaColossusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HomingLaser>()
            .ActivateOnEnter<GrandStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7886, SortOrder = -1)]
public class D150ScholaColossus(WorldState ws, Actor primary) : BossModule(ws, primary, new(295, -109.79f), new ArenaBoundsCircle(17));
