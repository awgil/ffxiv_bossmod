﻿namespace BossMod.RealmReborn.Dungeon.D08Qarn.D083Adjudicator;

public enum OID : uint
{
    Boss = 0x6DC, // x1
    SunJuror = 0x6E1, // spawn during fight
    MythrilVerge = 0x6DD, // spawn during fight
    Platform1 = 0x1E870F, // x1, EventObj type
    Platform2 = 0x1E8710, // x1, EventObj type
    Platform3 = 0x1E8711, // x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/SunJuror->player, no cast
    Darkness = 928, // Boss/SunJuror->self, 2.5s cast, range 7.5 120-degree cone aoe
    Paralyze = 308, // Boss->player, 4.0s cast, single-target
    CreepingDarkness = 927, // Boss->self, 2.5s cast, raidwide
    VergeLine = 929, // MythrilVerge->self, 4.0s cast, range 60 width 4 rect aoe
    VergePulse = 930, // MythrilVerge->self, 10.0s cast, range 60 ???
}

class Darkness(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.Darkness), new AOEShapeCone(7.5f, 60.Degrees()));
class VergeLine(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.VergeLine), new AOEShapeRect(60, 2));

class D083AdjudicatorStates : StateMachineBuilder
{
    public D083AdjudicatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Darkness>()
            .ActivateOnEnter<VergeLine>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1570)]
public class D083Adjudicator(WorldState ws, Actor primary) : BossModule(ws, primary, new(238, 0), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.MythrilVerge => 3,
                OID.SunJuror => WorldState.Actors.Where(other => (OID)other.OID is OID.Platform1 or OID.Platform2 or OID.Platform3).InRadius(e.Actor.Position, 1).Any() ? 2 : AIHints.Enemy.PriorityForbidAI,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
