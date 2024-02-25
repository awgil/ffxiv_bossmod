using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE12BayingOfHounds
{
    public enum OID : uint
    {
        Boss = 0x2E66, // R7.020, x1
        Helper = 0x233C, // R0.500, x14
        Hellsfire = 0x2E67, // R1.000-2.500, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 6497, // Boss->player, no cast, single-target
        Hellclaw = 20534, // Boss->player, 4.0s cast, single-target, tankbuster
        TailBlow = 20535, // Boss->self, 3.0s cast, range 19 90-degree cone aoe
        LavaSpit1 = 20536, // Boss->self, 3.0s cast, single-target, visual (summon hellsfires)
        LavaSpit2 = 20537, // Boss->self, no cast, single-target, visual (summon second set of hellsfires)
        LavaSpitAOE = 20538, // Helper->location, 3.0s cast, range 5 circle
        HellsfireActivate = 19647, // Hellsfire->self, no cast, range ?-50 donut, visual (prepare for activation)
        ScorchingLash = 20553, // Hellsfire->self, 4.0s cast, range 50 width 10 rect aoe
        Hellpounce = 20539, // Boss->location, 4.0s cast, width 10 rect charge aoe, knockback away from source, dist 5 (consider showing?)
        HellpounceSecond = 20540, // Boss->location, 1.0s cast, width 10 rect charge, knockback away from source, dist 5 (consider showing?)
        LionsBreath = 20541, // Boss->self, 4.0s cast, single-target, visual (frontal cone)
        LionsBreathAOE = 20542, // Helper->self, 4.5s cast, range 60 45-degree cone aoe
        DragonsBreath = 20543, // Boss->self, 4,0s cast, single-target, visual (side cones)
        DragonsBreathAOER = 20544, // Helper->self, 4,5s cast, range 60 30-degree cone
        DragonsBreathAOEL = 20545, // Helper->self, 4,5s cast, range 60 30-degree cone
        VoidTornado = 20546, // Boss->self, 4.0s cast, single-target, visual (set hp to 1)
        VoidTornadoAOE = 20547, // Helper->self, no cast, range 30 circle, set hp to 1
        VoidQuake = 20548, // Boss->self, 3.0s cast, single-target, visual (staggered circle/donuts)
        VoidQuakeAOE1 = 20549, // Helper->self, 3.0s cast, range 10 circle aoe
        VoidQuakeAOE2 = 20550, // Helper->self, 3.0s cast, range 10-20 donut aoe
        VoidQuakeAOE3 = 20551, // Helper->self, 3.0s cast, range 20-30 donut aoe
    };

    class Hellclaw : Components.SingleTargetCast
    {
        public Hellclaw() : base(ActionID.MakeSpell(AID.Hellclaw)) { }
    }

    class TailBlow : Components.SelfTargetedAOEs
    {
        public TailBlow() : base(ActionID.MakeSpell(AID.TailBlow), new AOEShapeCone(19, 45.Degrees())) { }
    }

    class LavaSpit : Components.LocationTargetedAOEs
    {
        public LavaSpit() : base(ActionID.MakeSpell(AID.LavaSpitAOE), 5) { }
    }

    class ScorchingLash : Components.SelfTargetedAOEs
    {
        public ScorchingLash() : base(ActionID.MakeSpell(AID.ScorchingLash), new AOEShapeRect(50, 5)) { }
    }

    class Hellpounce : Components.GenericAOEs
    {
        private AOEInstance? _charge;

        public Hellpounce() : base(ActionID.MakeSpell(AID.Hellpounce), "GTFO from charge!") { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_charge != null)
                yield return _charge.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.Hellpounce or AID.HellpounceSecond)
                Activate(caster.Position, spell.LocXZ, spell.NPCFinishAt);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Hellpounce:
                    var offset = spell.LocXZ - module.Bounds.Center;
                    Activate(spell.LocXZ, module.Bounds.Center - offset, module.WorldState.CurrentTime.AddSeconds(3.7f));
                    break;
                case AID.HellpounceSecond:
                    _charge = null;
                    break;
            }
        }

        private void Activate(WPos source, WPos target, DateTime activation)
        {
            var shape = new AOEShapeRect(0, 5);
            shape.SetEndPoint(target, source, new());
            _charge = new(shape, source, activation: activation);
        }
    }

    class LionsBreath : Components.SelfTargetedAOEs
    {
        public LionsBreath() : base(ActionID.MakeSpell(AID.LionsBreathAOE), new AOEShapeCone(60, 45.Degrees())) { }
    }

    class DragonsBreathR : Components.SelfTargetedAOEs
    {
        public DragonsBreathR() : base(ActionID.MakeSpell(AID.DragonsBreathAOER), new AOEShapeCone(60, 36.Degrees(), -10.Degrees())) { } // TODO: verify; there should not be an offset in reality here...
    }

    class DragonsBreathL : Components.SelfTargetedAOEs
    {
        public DragonsBreathL() : base(ActionID.MakeSpell(AID.DragonsBreathAOEL), new AOEShapeCone(60, 36.Degrees(), 10.Degrees())) { } // TODO: verify; there should not be an offset in reality here...
    }

    class VoidTornado : Components.CastHint
    {
        public VoidTornado() : base(ActionID.MakeSpell(AID.VoidTornado), "Set hp to 1") { }
    }

    class VoidQuake : Components.GenericAOEs //this concentric AOE can happen forwards or backwards in order with the same AID as the starter
    {
        private List<(Actor caster, AOEShape shape)> _active = new();

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _active.Take(1).Select(e => new AOEInstance(e.shape, e.caster.Position, e.caster.CastInfo!.Rotation, e.caster.CastInfo.NPCFinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.VoidQuakeAOE1 => new AOEShapeCircle(10),
                AID.VoidQuakeAOE2 => new AOEShapeDonut(10, 20),
                AID.VoidQuakeAOE3 => new AOEShapeDonut(20, 30),
                _ => null
            };
            if (shape != null)
                _active.Add((caster, shape));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            _active.RemoveAll(c => c.caster == caster);
        }
    }

    class CE12BayingOfHoundsStates : StateMachineBuilder
    {
        public CE12BayingOfHoundsStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Hellclaw>()
                .ActivateOnEnter<TailBlow>()
                .ActivateOnEnter<LavaSpit>()
                .ActivateOnEnter<ScorchingLash>()
                .ActivateOnEnter<Hellpounce>()
                .ActivateOnEnter<LionsBreath>()
                .ActivateOnEnter<DragonsBreathR>()
                .ActivateOnEnter<DragonsBreathL>()
                .ActivateOnEnter<VoidTornado>()
                .ActivateOnEnter<VoidQuake>();
        }
    }

    [ModuleInfo(CFCID = 735, DynamicEventID = 2)]
    public class CE12BayingOfHounds : BossModule
    {
        public CE12BayingOfHounds(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(154, 785), 25)) { }
    }
}
