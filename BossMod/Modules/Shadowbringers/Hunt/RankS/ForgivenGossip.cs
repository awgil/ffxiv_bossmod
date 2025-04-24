﻿namespace BossMod.Shadowbringers.Hunt.RankS.ForgivenGossip;

public enum OID : uint
{
    Boss = 0x2A03, // R=0.75
}

public enum AID : uint
{
    AutoAttack = 18129, // Boss->player, no cast, single-target
    Icefall = 17043, // Boss->location, 3.0s cast, range 5 circle, deadly if petrified by gaze
    PetrifyingEye = 18041, // Boss->self, 3.0s cast, range 40 circle
}

class Icefall(BossModule module) : Components.LocationTargetedAOEs(module, AID.Icefall, 5);
class PetrifyingEye(BossModule module) : Components.CastGaze(module, AID.PetrifyingEye);

class ForgivenGossipStates : StateMachineBuilder
{
    public ForgivenGossipStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PetrifyingEye>()
            .ActivateOnEnter<Icefall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.SS, NameID = 8916)]
public class ForgivenGossip(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
