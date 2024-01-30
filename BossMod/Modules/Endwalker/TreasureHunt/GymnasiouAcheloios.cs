// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.TreasureHunt.GymnasiouAcheloios
{
    public enum OID : uint
    {
        Boss = 0x3D3E, //R=4.0
        BossAdd = 0x3D3F, //R=2.7
        BossHelper = 0x233C,
        GymnasticGarlic = 0x3D51, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards, despawn if not killed fast enough
        GymnasticQueen = 0x3D53, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards, despawn if not killed fast enough
        GymnasticEggplant = 0x3D50, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards, despawn if not killed fast enough
        GymnasticOnion = 0x3D4F, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards, despawn if not killed fast enough
        GymnasticTomato = 0x3D52, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards, despawn if not killed fast enough
    };

    public enum AID : uint
    {
        AutoAttack2 = 870, // Boss/BossAdd->player, no cast, single-target
        DoubleHammerA = 32284, // Boss->self, 4,2s cast, single-target
        DoubleHammerB = 32281, // Boss->self, 4,2s cast, single-target
        DoubleHammer = 32859, // BossHelper->self, 5,0s cast, range 30 180-degree cone
        RightHammer1 = 32282, // Boss->self, 0,5s cast, single-target
        RightHammer2 = 32860, // BossHelper->self, 1,0s cast, range 30 180-degree cone
        TailSwing = 32279, // Boss->self, 3,5s cast, range 13 circle
        QuadrupleHammerA = 32280, // Boss->self, 4,2s cast, single-target
        QuadrupleHammerB = 32283, // Boss->self, 4,2s cast, single-target
        QuadrupleHammer2 = 32858, // BossHelper->self, 5,0s cast, range 30 180-degree cone
        LeftHammer1 = 32285, // Boss->self, 0,5s cast, single-target
        LeftHammer2 = 32861, // BossHelper->self, 1,0s cast, range 30 180-degree cone
        CriticalBite = 32286, // BossAdd->self, 3,0s cast, range 10 120-degree cone
        DeadlyHold = 32275, // Boss->player, 5,0s cast, single-target
        Earthbreak = 32277, // Boss->self, 2,1s cast, single-target
        Earthbreak2 = 32278, // BossHelper->location, 3,0s cast, range 5 circle
        VolcanicHowl = 32276, // Boss->self, 5,0s cast, range 40 circle
        PluckAndPrune = 32302, // GymnasticEggplant->self, 3,5s cast, range 7 circle
        Pollen = 32305, // GymnasticQueen->self, 3,5s cast, range 7 circle
        HeirloomScream = 32304, // GymnasticTomato->self, 3,5s cast, range 7 circle
        PungentPirouette = 32303, // GymnasticGarlic->self, 3,5s cast, range 7 circle
        TearyTwirl = 32301, // GymnasticOnion->self, 3,5s cast, range 7 circle
        Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
    };
    class QuadrupleHammerCW : Components.SimpleRotationAOE
    {
        public QuadrupleHammerCW() : base(ActionID.MakeSpell(AID.QuadrupleHammerA), ActionID.MakeSpell(AID.LeftHammer2), ActionID.MakeSpell(AID.RightHammer2), default, new AOEShapeCone(30,90.Degrees()), 4, -90.Degrees()) { }
    }
    class QuadrupleHammerCCW : Components.SimpleRotationAOE
    {
        public QuadrupleHammerCCW() : base(ActionID.MakeSpell(AID.QuadrupleHammerB), ActionID.MakeSpell(AID.LeftHammer2), ActionID.MakeSpell(AID.RightHammer2), default, new AOEShapeCone(30,90.Degrees()), 4, 90.Degrees()) { }
    }
    class DoubleSlammer : Components.GenericAOEs
    {
        private int _remainingHits;
        private Angle _RotationDir;
        // private bool rotationA;
        // private bool rotationB;
        private bool doubleA;
        private bool doubleB;       

        private static AOEShapeCone cone = new(30, 90.Degrees());
 
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
// //clockwise rotation
//             if (_remainingHits == 4 && rotationA)
//                 yield return new(cone, module.PrimaryActor.Position, _RotationDir - 90.Degrees());
//             if (_remainingHits == 3 && rotationA)
//                 yield return new(cone, module.PrimaryActor.Position, _RotationDir - 180.Degrees());
//             if (_remainingHits == 2 && rotationA)
//                 yield return new(cone, module.PrimaryActor.Position, _RotationDir - 270.Degrees());
//             if (_remainingHits == 1 && rotationA)
//                 yield return new(cone, module.PrimaryActor.Position, _RotationDir);
// //counter clockwise rotation
//             if (_remainingHits == 4 && rotationB)
//                 yield return new(cone, module.PrimaryActor.Position, _RotationDir + 90.Degrees());
//             if (_remainingHits == 3 && rotationB)
//                 yield return new(cone, module.PrimaryActor.Position, _RotationDir + 180.Degrees());
//             if (_remainingHits == 2 && rotationB)
//                 yield return new(cone, module.PrimaryActor.Position, _RotationDir + 270.Degrees());
//             if (_remainingHits == 1 && rotationB)
//                 yield return new(cone, module.PrimaryActor.Position, _RotationDir);
//double slams
            if (_remainingHits == 2 && doubleA)
                yield return new(cone, module.PrimaryActor.Position, _RotationDir + 90.Degrees());
            if (_remainingHits == 1 && doubleB)
                yield return new(cone, module.PrimaryActor.Position, _RotationDir + 90.Degrees());
            if (_remainingHits == 1 && doubleA)
                yield return new(cone, module.PrimaryActor.Position, _RotationDir - 90.Degrees());
            if (_remainingHits == 2 && doubleB)
                yield return new(cone, module.PrimaryActor.Position, _RotationDir - 90.Degrees());
            if (_remainingHits == 0)
                {
                    // rotationA = false;
                    // rotationB = false;
                    doubleA = false;
                    doubleB = false;
                }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                // case AID.QuadrupleHammerA:
                //     rotationA = true;
                //     _remainingHits = 4;
                //     _RotationDir = spell.Rotation;
                // break;
                // case AID.QuadrupleHammerB:
                //     rotationB = true;
                //     _remainingHits = 4;
                //     _RotationDir = spell.Rotation;
                // break;
                case AID.DoubleHammerA:
                    doubleA = true;
                    _remainingHits = 2;
                    _RotationDir = spell.Rotation;
                break;
                case AID.DoubleHammerB:
                    doubleB = true;
                    _remainingHits = 2;
                    _RotationDir = spell.Rotation;
                break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.LeftHammer2:
                case AID.RightHammer2:
                // case AID.QuadrupleHammer2:
                case AID.DoubleHammer:
                    --_remainingHits;
                    break;
            }
        }
        // public override void AddGlobalHints(BossModule module, GlobalHints hints)
        // {
        //           hints.Add($"doubleA: {doubleA}! doubleB: {doubleB}! rotA: {rotationA}! rotB: {rotationB}!\nRemaining hits:{_remainingHits}");       
        // }
    }
    class Earthbreak : Components.LocationTargetedAOEs
    {
        public Earthbreak() : base(ActionID.MakeSpell(AID.Earthbreak2), 5) { }
    }
    class DeadlyHold : Components.SingleTargetCast
    {
        public DeadlyHold() : base(ActionID.MakeSpell(AID.DeadlyHold)) { }
    }
    class TailSwing : Components.SelfTargetedAOEs
    {
        public TailSwing() : base(ActionID.MakeSpell(AID.TailSwing), new AOEShapeCircle(13)) { } 
    }
    class CriticalBite : Components.SelfTargetedAOEs
    {
        public CriticalBite() : base(ActionID.MakeSpell(AID.CriticalBite), new AOEShapeCone(10,60.Degrees())) { } 
    }
    class PluckAndPrune : Components.SelfTargetedAOEs
    {
        public PluckAndPrune() : base(ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(7)) { } 
    }
    class TearyTwirl : Components.SelfTargetedAOEs
    {
        public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(7)) { } 
    }
    class HeirloomScream : Components.SelfTargetedAOEs
    {
        public HeirloomScream() : base(ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(7)) { } 
    }
    class PungentPirouette : Components.SelfTargetedAOEs
    {
        public PungentPirouette() : base(ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(7)) { } 
    }
    class Pollen : Components.SelfTargetedAOEs
    {
        public Pollen() : base(ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(7)) { } 
    }
    class PithekosStates : StateMachineBuilder
    {
        public PithekosStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<QuadrupleHammerCW>()
            .ActivateOnEnter<QuadrupleHammerCCW>()
            .ActivateOnEnter<DoubleSlammer>()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<CriticalBite>()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<Earthbreak>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.GymnasticEggplant).All(e => e.IsDead) && module.Enemies(OID.GymnasticQueen).All(e => e.IsDead) && module.Enemies(OID.GymnasticOnion).All(e => e.IsDead) && module.Enemies(OID.GymnasticGarlic).All(e => e.IsDead) && module.Enemies(OID.GymnasticTomato).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 909, NameID = 12019)]
    public class Pithekos : BossModule
    {
        public Pithekos(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) {}

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.GymnasticEggplant))
                Arena.Actor(s, ArenaColor.Danger, false);
            foreach (var s in Enemies(OID.GymnasticTomato))
                Arena.Actor(s, ArenaColor.Danger, false);
            foreach (var s in Enemies(OID.GymnasticQueen))
                Arena.Actor(s, ArenaColor.Danger, false);
            foreach (var s in Enemies(OID.GymnasticGarlic))
                Arena.Actor(s, ArenaColor.Danger, false);
            foreach (var s in Enemies(OID.GymnasticOnion))
                Arena.Actor(s, ArenaColor.Danger, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.GymnasticOnion => 7,
                    OID.GymnasticEggplant => 6,
                    OID.GymnasticGarlic => 5,
                    OID.GymnasticTomato => 4,
                    OID.GymnasticQueen => 3,
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
