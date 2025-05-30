﻿namespace BossMod.RealmReborn.Dungeon.D08Qarn.D082TempleGuardian;

public enum OID : uint
{
    Boss = 0x6DB, // x1
    GolemSoulstone = 0x7FA, // x1, Part type, and more spawn during fight
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast
    BoulderClap = 1417, // Boss->self, 2.5s cast, range 14.2 120-degree cone aoe
    TrueGrit = 1418, // Boss->self, 3.0s cast, range 14.2 120-degree cone aoe
    Rockslide = 1419, // Boss->self, 2.5s cast, range 16.2 width 8 rect aoe
    StoneSkull = 1416, // Boss->player, no cast, random single-target
    Obliterate = 680, // Boss->self, 2.0s cast, range 6? ??? aoe
}

class BoulderClap(BossModule module) : Components.StandardAOEs(module, AID.BoulderClap, new AOEShapeCone(14.2f, 60.Degrees()));
class TrueGrit(BossModule module) : Components.StandardAOEs(module, AID.TrueGrit, new AOEShapeCone(14.2f, 60.Degrees()));
class Rockslide(BossModule module) : Components.StandardAOEs(module, AID.Rockslide, new AOEShapeRect(16.2f, 4));

class D082TempleGuardianStates : StateMachineBuilder
{
    public D082TempleGuardianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BoulderClap>()
            .ActivateOnEnter<TrueGrit>()
            .ActivateOnEnter<Rockslide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1569)]
public class D082TempleGuardian(WorldState ws, Actor primary) : BossModule(ws, primary, new(50, -10), new ArenaBoundsCircle(15))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GolemSoulstone => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
