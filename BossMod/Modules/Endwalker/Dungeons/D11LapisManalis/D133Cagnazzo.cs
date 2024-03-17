// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Dungeon.D13LapisManalis.D133Cagnazzo
{
    public enum OID : uint
    {
        Boss = 0x3AE2, //R=8.0
        FearsomeFlotsam = 0x3AE3, //R=2.4
        Helper = 0x233C,
    }

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        StygianDeluge = 31139, // Boss->self, 5,0s cast, range 80 circle
        Antediluvian = 31119, // Boss->self, 5,0s cast, single-target
        Antediluvian2 = 31120, // Helper->self, 6,5s cast, range 15 circle
        BodySlam = 31121, // Boss->location, 6,5s cast, single-target
        BodySlam2 = 31122, // Helper->self, 7,5s cast, range 60 circle, knockback 10, away from source
        BodySlam3 = 31123, // Helper->self, 7,5s cast, range 8 circle
        Teleport = 31131, // Boss->location, no cast, single-target, boss teleports 
        HydrobombTelegraph = 32695, // Helper->location, 2,0s cast, range 4 circle
        HydraulicRamTelegraph = 32693, // Helper->location, 2,0s cast, width 8 rect charge
        HydraulicRam = 32692, // Boss->self, 6,0s cast, single-target
        HydraulicRam2 = 32694, // Boss->location, no cast, width 8 rect charge
        Hydrobomb = 32696, // Helper->location, no cast, range 4 circle
        StartHydrofall = 31126, // Boss->self, no cast, single-target
        Hydrofall = 31375, // Boss->self, 5,0s cast, single-target
        Hydrofall2 = 31376, // Helper->players, 5,5s cast, range 6 circle
        CursedTide = 31130, // Boss->self, 5,0s cast, single-target
        StartLimitbreakPhase = 31132, // Boss->self, no cast, single-target
        NeapTide = 31134, // Helper->player, no cast, range 6 circle
        Hydrovent = 31136, // Helper->location, 5,0s cast, range 6 circle
        SpringTide = 31135, // Helper->players, no cast, range 6 circle
        Tsunami = 31137, // Helper->self, no cast, range 80 width 60 rect
        Voidcleaver = 31110, // Boss->self, 4,0s cast, single-target
        Voidcleaver2 = 31111, // Helper->self, no cast, range 100 circle
        VoidMiasma = 32691, // Helper->self, 3,0s cast, range 50 30-degree cone
        Lifescleaver = 31112, // Boss->self, 4,0s cast, single-target
        ifescleaver2 = 31113, // Helper->self, 5,0s cast, range 50 30-degree cone
        VoidTorrent = 31118, // Boss->self/player, 5,0s cast, range 60 width 8 rect
    };


    class D133CagnazzoStates : StateMachineBuilder
    {
        public D133CagnazzoStates(BossModule module) : base(module)
        {
            TrivialPhase();
                // .ActivateOnEnter<TenebrismTowers>()
                // .ActivateOnEnter<InOutAOE>()
                // .ActivateOnEnter<OutInAOE>()
                // .ActivateOnEnter<GlassyEyed>()
                // .ActivateOnEnter<SoulScythe>()
                // .ActivateOnEnter<ScarecrowChase>()
        }
    }

    [ModuleInfo(CFCID = 896, NameID = 11995)]
    public class D133Cagnazzo : BossModule
    {
        public D133Cagnazzo(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-250, 130), 20)) { }
    }
}
