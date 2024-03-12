using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.MaskedCarnivale.Stage01;
using BossMod.PlanTarget;
using BossMod.RealmReborn.Dungeon.D16Amdapor.D162DemonWall;
using Lumina.Data.Parsing.Layer;
using static BossMod.ActorCastEvent;

/*
everything we need to understand the mechanics is in here
https://www.thegamer.com/final-fantasy-14-endwalker-tower-of-zot-dungeon-guide-walkthrough/
*/

namespace BossMod.Endwalker.Dungeon.D01TheTowerofZot.D01Sanduruva
{
    /*
    notes to self bnpcname has nameID, contentfindercondition has the CFC
    iconid is a row in lockon sheet
    tetherid is a row in channeling or w/e sheet
    */

    //this one has no 0x233C helpers weird
    public enum OID : uint
    {
        Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
        Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
        Boss = 0x33EF, // R2.500, x1
        BerserkerSphere = 0x33F0, // R1.500-2.500, spawn during fight
    };

    public enum AID : uint
    {
        Ability_UnknownB2 = 25254,  // Boss->location, no cast, single-target
        Attack = 871,               // Boss->none, no cast, single-target
        ExplosiveForce = 25250,     //done. Boss->self, 3.0s cast, single-target
        IsitvaSiddhi = 25257,       // Boss->none, 4.0s cast, single-target
        ManusyaBerserk = 25249,     // Boss->self, 3.0s cast, single-target
        ManusyaConfuse = 25253,     //done. Boss->self, 3.0s cast, range 40 circle
        ManusyaStop = 25255,        //done. Boss->self, 3.0s cast, range 40 circle
        PrakamyaSiddhi = 25251,     //done. Boss->self, 4.0s cast, range 5 circle
        PraptiSiddhi = 25256,       //done. Boss->self, 2.0s cast, range 40 width 4 rect
        SphereShatter = 25252,      //done. BerserkerSphere->self, 2.0s cast, range 15 circle
    };
    public enum SID : uint
    {
        ManusyaStop = 2653, // none->player/33CE/33CD/316C, extra=0x0
        TemporalDisplacement = 900, // none->33CD/player/33CE/316C, extra=0x0
        ManusyaConfuse = 2652, // Boss->33CD/player/33CE/316C, extra=0x1C6
        WhoIsShe = 2654, // none->BerserkerSphere, extra=0x1A8
        WhoIsShe2 = 2655, // none->Boss, extra=0x0
        ManusyaBerserk = 2651, // Boss->player, extra=0x0
        VulnerabilityUp = 1789, // Boss->player, extra=0x1
    };
class WhoIsSheAnyways : BossComponent
{
    public int ActiveDebuffs { get; private set; }
    public bool FoundHer = false;
    public WPos IsShe = new WPos();

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        IsShe = module.PrimaryActor.Position;
        FoundHer = module.Enemies(OID.BerserkerSphere).Any(x => !x.IsDead);

        if (FoundHer)
        {
            IsShe = NearestPointOnCircle(IsShe.X, IsShe.Z, -258, -26, 20);
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(IsShe, 1f));
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (FoundHer)
            arena.AddCircleFilled(new WPos(IsShe.X, IsShe.Z), 2.5f, ArenaColor.SafeFromAOE);
        else
            arena.AddCircleFilled(new WPos(IsShe.X, IsShe.Z), 2.5f, ArenaColor.Background);
    }

        public static WPos NearestPointOnCircle(double x, double y, double a, double b, double radius)
        {
            // Calculate the vector from the circle's center to the point (x, y)
            double dx = x - a;
            double dy = y - b;

            // Calculate the angle between the x-axis and the vector
            double angle = Math.Atan2(dy, dx);

            // Calculate the adjusted radius so we can actually path to it
            double adjustedRadius = radius - 0.5;

            // Calculate the point on the adjusted circle's edge using trigonometry
            double newX = a + adjustedRadius * Math.Cos(angle);
            double newY = b + adjustedRadius * Math.Sin(angle);

            WPos nearestPoint = new WPos((float)newX, (float)newY);
            return nearestPoint;
        }


    }


    class ExplosiveForce : Components.SingleTargetCast
    {
        public ExplosiveForce() : base(ActionID.MakeSpell(AID.ExplosiveForce)) { }
    }
    class PraptiSiddhi : Components.SelfTargetedAOEs
    {
        public PraptiSiddhi() : base(ActionID.MakeSpell(AID.PraptiSiddhi), new AOEShapeRect(60, 4)) { }
    }
    class SphereShatter : Components.SelfTargetedAOEs
    {
        public SphereShatter() : base(ActionID.MakeSpell(AID.SphereShatter), new AOEShapeCircle(0.1f)) { }
    }
    class PrakamyaSiddhi : Components.SelfTargetedAOEs
    {
        public PrakamyaSiddhi() : base(ActionID.MakeSpell(AID.PrakamyaSiddhi), new AOEShapeCircle(0.1f)) { }
    }
    class ManusyaConfuse : Components.SelfTargetedAOEs
    {
        public ManusyaConfuse() : base(ActionID.MakeSpell(AID.ManusyaConfuse), new AOEShapeCircle(0.1f)) { }
    }
    class ManusyaStop : Components.SelfTargetedAOEs
    {
        public ManusyaStop() : base(ActionID.MakeSpell(AID.ManusyaStop), new AOEShapeCircle(0.1f)) { }
    }

    public enum IconID : uint
    {
        Icon_198 = 198, // 33CE
    };

    class D01SanduruvaStates : StateMachineBuilder
    {
        public D01SanduruvaStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<ManusyaConfuse>()  //these are not avoidable anyways
            .ActivateOnEnter<ManusyaStop>()  //these are not avoidable anyways
            //.ActivateOnEnter<ExplosiveForce>()
            .ActivateOnEnter<PrakamyaSiddhi>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<WhoIsSheAnyways>()
            .ActivateOnEnter<PraptiSiddhi>();
        }
    }
    //it runs from bezerker orbs but doesnt go to the right place. maybe arena size is wrong.
    /*    notes to self bnpcname has nameID, contentfindercondition has the CFC
    */
    [ModuleInfo(CFCID = 783, NameID = 10257)]
    public class D01Sanduruva : BossModule
    {
        public D01Sanduruva(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-258, -26), 38)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
        }
    }
}
