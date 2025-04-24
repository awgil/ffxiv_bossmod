﻿namespace BossMod.Stormblood.Hunt.RankA.Gajasura;

public enum OID : uint
{
    Boss = 0x1ABF, // R=3.23
}

public enum AID : uint
{
    AutoAttack = 872, // 1ABF->player, no cast, single-target
    Spin = 8188, // 1ABF->self, 3.0s cast, range 5+R 120-degree cone
    Hurl = 8187, // 1ABF->location, 3.0s cast, range 6 circle
    Buffet = 8189, // 1ABF->none, 3.0s cast, single-target, randomly hits a target that isn't tanking, only happens when at least 2 actors are in combat with Gajasura (chocobos count)
}

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, AID.Spin, new AOEShapeCone(8.23f, 60.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, AID.Hurl, 6);
class Buffet(BossModule module) : Components.SingleTargetCast(module, AID.Buffet, "Heavy damage on random target (except tank)");

class GajasuraStates : StateMachineBuilder
{
    public GajasuraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<Buffet>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 5998)]
public class Gajasura(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
