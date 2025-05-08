namespace BossMod.Dawntrail.Hunt.RankA.Starcrier;

public enum OID : uint
{
    Boss = 0x41FC, // R5.000, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WingsbreadthWinds = 37038, // Boss->self, 5.0s cast, range 8 circle
    StormwallWinds = 37039, // Boss->self, 5.0s cast, range 8-25 donut
    DirgeOfTheLost = 37040, // Boss->self, 3.0s cast, range 40 circle, apply temporary misdirection
    AeroIV = 37163, // Boss->self, 4.0s cast, range 20 circle, raidwide
    SwiftwindSerenade = 37305, // Boss->self, 4.0s cast, range 40 width 8 rect
}

public enum SID : uint
{
    TemporaryMisdirection = 3909, // Boss->player, extra=0x168
}

class WingsbreadthWinds(BossModule module) : Components.StandardAOEs(module, AID.WingsbreadthWinds, new AOEShapeCircle(8));
class StormwallWinds(BossModule module) : Components.StandardAOEs(module, AID.StormwallWinds, new AOEShapeDonut(8, 25));
class DirgeOfTheLost(BossModule module) : Components.CastHint(module, AID.DirgeOfTheLost, "Apply temporary misdirection");
class AeroIV(BossModule module) : Components.RaidwideCast(module, AID.AeroIV);
class SwiftwindSerenade(BossModule module) : Components.StandardAOEs(module, AID.SwiftwindSerenade, new AOEShapeRect(40, 4));

class StarcrierStates : StateMachineBuilder
{
    public StarcrierStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WingsbreadthWinds>()
            .ActivateOnEnter<StormwallWinds>()
            .ActivateOnEnter<DirgeOfTheLost>()
            .ActivateOnEnter<AeroIV>()
            .ActivateOnEnter<SwiftwindSerenade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 12692)]
public class Starcrier(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
