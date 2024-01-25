﻿namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE51ThereWouldBeBlood
{
    public enum OID : uint
    {
        Boss = 0x319A, // R6.000, x1
        Helper = 0x233C, // R0.500, x17
        EmbitteredSoul = 0x319B, // R3.600, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6497, // Boss->player, no cast, single-target
        Electrocution = 24706, // Helper->self, no cast, range 25-30 donut, deathwall around arena
        CloudOfLocusts = 23568, // Boss->self, 6.5s cast, range 15 circle
        PlagueOfLocusts = 23569, // Boss->self, 6.5s cast, range 6-40 donut
        DivestingGale = 23570, // Helper->location, 3.0s cast, range 5 circle
        Camisado = 23469, // Boss->player, 4.0s cast, single-target, tankbuster
        DreadWind = 23470, // Boss->self, 4.0s cast, raidwide
        Teleport = 23580, // Boss->location, no cast, teleport
        GaleCannon = 21475, // Boss->self, 5.0s cast, range 30 width 12 rect aoe

        FlightOfTheMalefic1 = 24810, // Boss->self, 7.0s cast, single-target, visual (45, -45, -135)
        FlightOfTheMalefic2 = 24811, // Boss->self, 7.0s cast, single-target, visual (-45, -135, 135)
        FlightOfTheMalefic3 = 24812, // Boss->self, 7.0s cast, single-target, visual (45, -45, 135)
        FlightOfTheMalefic4 = 24813, // Boss->self, 7.0s cast, single-target, visual (45, -135, 135)
        FlightOfTheMaleficAOECone = 23579, // Helper->self, 7.0s cast, range 30 90-degree cone aoe
        FlightOfTheMaleficAOECenter = 24322, // Helper->location, 7.0s cast, range 6 circle aoe

        SummonDarkness = 23571, // Boss->self, 3.0s cast, single-target, visual
        TempestOfAnguish = 23572, // EmbitteredSoul->self, 6.5s cast, range 55 width 10 rect aoe
        TragicalGaze = 23573, // EmbitteredSoul->self, 7.5s cast, range 55 circle
    };

    class CloudOfLocusts : Components.SelfTargetedAOEs
    {
        public CloudOfLocusts() : base(ActionID.MakeSpell(AID.CloudOfLocusts), new AOEShapeCircle(15)) { }
    }

    class PlagueOfLocusts : Components.SelfTargetedAOEs
    {
        public PlagueOfLocusts() : base(ActionID.MakeSpell(AID.PlagueOfLocusts), new AOEShapeDonut(6, 40)) { }
    }

    class DivestingGale : Components.LocationTargetedAOEs
    {
        public DivestingGale() : base(ActionID.MakeSpell(AID.DivestingGale), 5) { }
    }

    class Camisado : Components.SingleTargetCast
    {
        public Camisado() : base(ActionID.MakeSpell(AID.Camisado)) { }
    }

    class DreadWind : Components.RaidwideCast
    {
        public DreadWind() : base(ActionID.MakeSpell(AID.DreadWind)) { }
    }

    class GaleCannon : Components.SelfTargetedAOEs
    {
        public GaleCannon() : base(ActionID.MakeSpell(AID.GaleCannon), new AOEShapeRect(30, 6)) { }
    }

    class FlightOfTheMaleficCone : Components.SelfTargetedAOEs
    {
        public FlightOfTheMaleficCone() : base(ActionID.MakeSpell(AID.FlightOfTheMaleficAOECone), new AOEShapeCone(30, 45.Degrees())) { }
    }

    class FlightOfTheMaleficCenter : Components.LocationTargetedAOEs
    {
        public FlightOfTheMaleficCenter() : base(ActionID.MakeSpell(AID.FlightOfTheMaleficAOECenter), 6) { }
    }

    class TempestOfAnguish : Components.SelfTargetedAOEs
    {
        public TempestOfAnguish() : base(ActionID.MakeSpell(AID.TempestOfAnguish), new AOEShapeRect(55, 5)) { }
    }

    class TragicalGaze : Components.CastGaze
    {
        public TragicalGaze() : base(ActionID.MakeSpell(AID.TragicalGaze)) { }
    }

    class CE51ThereWouldBeBloodStates : StateMachineBuilder
    {
        public CE51ThereWouldBeBloodStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CloudOfLocusts>()
                .ActivateOnEnter<PlagueOfLocusts>()
                .ActivateOnEnter<DivestingGale>()
                .ActivateOnEnter<Camisado>()
                .ActivateOnEnter<DreadWind>()
                .ActivateOnEnter<GaleCannon>()
                .ActivateOnEnter<FlightOfTheMaleficCone>()
                .ActivateOnEnter<FlightOfTheMaleficCenter>()
                .ActivateOnEnter<TempestOfAnguish>()
                .ActivateOnEnter<TragicalGaze>();
        }
    }

    [ModuleInfo(CFCID = 778, DynamicEventID = 24)]
    public class CE51ThereWouldBeBlood : BossModule
    {
        public CE51ThereWouldBeBlood(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-390, 230), 25)) { }
    }
}
