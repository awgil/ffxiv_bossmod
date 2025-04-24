﻿namespace BossMod.Endwalker.Hunt.RankA.Petalodus;

public enum OID : uint
{
    Boss = 0x35FB, // R5.400, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    MarineMayhem = 27063, // Boss->self, 5.0s cast, range 40 circle, triple raidwide + damage up
    MarineMayhemAOE1 = 27064, // Boss->self, no cast, range 40 circle, second raidwide hit
    MarineMayhemAOE2 = 27065, // Boss->self, no cast, range 40 circle, third raidwide hit
    Waterga = 27067, // Boss->players, 5.0s cast, range 6 circle, applies magic vulnerability up (2nd hit deadly even for tanks)
    TidalGuillotine = 27068, // Boss->self, 4.0s cast, range 13 circle
    AncientBlizzard = 27069, // Boss->self, 4.0s cast, range 40 45-degree cone
}

class MarineMayhem(BossModule module) : Components.CastInterruptHint(module, AID.MarineMayhem, hintExtra: "Raidwide x3");
class Waterga(BossModule module) : Components.SpreadFromCastTargets(module, AID.Waterga, 6);
class TidalGuillotine(BossModule module) : Components.SelfTargetedAOEs(module, AID.TidalGuillotine, new AOEShapeCircle(13));
class AncientBlizzard(BossModule module) : Components.SelfTargetedAOEs(module, AID.AncientBlizzard, new AOEShapeCone(40, 22.5f.Degrees()));

class PetalodusStates : StateMachineBuilder
{
    public PetalodusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MarineMayhem>()
            .ActivateOnEnter<Waterga>()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<AncientBlizzard>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10632)]
public class Petalodus(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
