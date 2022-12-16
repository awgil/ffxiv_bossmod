using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.HuntS.NarrowRift
{
    public enum OID : uint
    {
        Boss = 0x35DB, // R6.000, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        EmptyPromiseCircle = 27487, // Boss->self, 5.0s cast, range 10 circle
        EmptyPromiseDonut = 27488, // Boss->self, 5.0s cast, range 6-40 donut
        VanishingRayStart = 27333, // Boss->self, no cast, range 1 circle, visual
        VanishingRay = 27516, // Boss->self, no cast, range 50 width 8 rect
        VanishingRayEnd = 27519, // Boss->self, no cast, single-target, visual
        ContinualMeddlingFR = 27327, // Boss->self, 4.0s cast, range 60 circle, applies forward march/right face debuffs
        ContinualMeddlingFL = 27328, // Boss->self, 4.0s cast, range 60 circle, applies forward march/left face debuffs
        ContinualMeddlingBR = 27330, // Boss->self, 4.0s cast, range 60 circle, applies about face/right face debuffs
        // also meddling: 27325, 27329
        EmptyRefrainCircleFirst = 27331, // Boss->self, 12.0s cast, range 10 circle
        EmptyRefrainDonutFirst = 27332, // Boss->self, 13.5s cast, range 6-40 donut
        EmptyRefrainCircleSecond = 27335, // Boss->self, 1.0s cast, range 10 circle
        EmptyRefrainDonutSecond = 27337, // Boss->self, 1.0s cast, range 6-40 donut
    };

    class EmptyPromise : Components.GenericAOEs
    {
        private List<AOEShape> _pendingShapes = new();

        private static AOEShapeCircle _shapeCircle = new(10);
        private static AOEShapeDonut _shapeDonut = new(6, 40);

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_pendingShapes.Count > 0)
                yield return (_pendingShapes.First(), module.PrimaryActor.Position, new(), module.PrimaryActor.CastInfo?.FinishAt ?? module.WorldState.CurrentTime);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster != module.PrimaryActor)
                return;
            switch ((AID)spell.Action.ID)
            {
                case AID.EmptyPromiseCircle:
                    _pendingShapes.Add(_shapeCircle);
                    break;
                case AID.EmptyPromiseDonut:
                    _pendingShapes.Add(_shapeDonut);
                    break;
                case AID.EmptyRefrainCircleFirst:
                    _pendingShapes.Add(_shapeCircle);
                    _pendingShapes.Add(_shapeDonut);
                    break;
                case AID.EmptyRefrainDonutFirst:
                    _pendingShapes.Add(_shapeDonut);
                    _pendingShapes.Add(_shapeCircle);
                    break;
            };
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster == module.PrimaryActor && _pendingShapes.Count > 0)
                _pendingShapes.RemoveAt(0);
        }
    }

    class VanishingRay : Components.GenericAOEs
    {
        private DateTime _activation;
        private static AOEShapeRect _shape = new(50, 4);

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_activation != new DateTime())
                yield return (_shape, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (caster != module.PrimaryActor)
                return;
            switch ((AID)spell.Action.ID)
            {
                case AID.VanishingRayStart:
                    _activation = module.WorldState.CurrentTime.AddSeconds(4);
                    break;
                case AID.VanishingRay:
                    _activation = new();
                    break;
            }
        }
    }

    // TODO: continual meddling
    class NarrowRiftStates : StateMachineBuilder
    {
        public NarrowRiftStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<EmptyPromise>()
                .ActivateOnEnter<VanishingRay>();
        }
    }

    public class NarrowRift : SimpleBossModule
    {
        public NarrowRift(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
