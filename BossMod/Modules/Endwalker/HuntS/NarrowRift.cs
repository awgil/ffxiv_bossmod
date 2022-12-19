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
        ContinualMeddlingBL = 27329, // Boss->self, 4.0s cast, range 60 circle, applies about face/left face debuffs
        ContinualMeddlingBR = 27330, // Boss->self, 4.0s cast, range 60 circle, applies about face/right face debuffs
        // also meddling: 27325
        EmptyRefrainCircleFirst = 27331, // Boss->self, 12.0s cast, range 10 circle
        EmptyRefrainDonutFirst = 27332, // Boss->self, 13.5s cast, range 6-40 donut
        EmptyRefrainCircleSecond = 27335, // Boss->self, 1.0s cast, range 10 circle
        EmptyRefrainDonutSecond = 27337, // Boss->self, 1.0s cast, range 6-40 donut
    };

    public enum SID : uint
    {
        ForwardMarch = 1958, // Boss->player, extra=0x0
        AboutFace = 1959, // Boss->player, extra=0x0
        LeftFace = 1960, // Boss->player, extra=0x0
        RightFace = 1961, // Boss->player, extra=0x0
        ForcedMarch = 1257, // Boss->player, extra=1 (forward) / 2 (backward) / 4 (left) / ? (right)
    };

    class EmptyPromise : Components.GenericAOEs
    {
        private List<AOEShape> _pendingShapes = new();

        private static AOEShapeCircle _shapeCircle = new(10);
        private static AOEShapeDonut _shapeDonut = new(6, 40);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_pendingShapes.Count > 0)
                yield return new(_pendingShapes.First(), module.PrimaryActor.Position, new(), module.PrimaryActor.CastInfo?.FinishAt ?? module.WorldState.CurrentTime); // TODO: activation
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

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_activation != new DateTime())
                yield return new(_shape, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
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

    class ContinualMeddling : BossComponent
    {
        private static float _marchSpeed = 6;
        private static float _marchDuration = 2;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (module.PrimaryActor.CastInfo != null && module.PrimaryActor.CastInfo.IsSpell() && (AID)module.PrimaryActor.CastInfo.Action.ID is AID.ContinualMeddlingFR or AID.ContinualMeddlingFL or AID.ContinualMeddlingBL or AID.ContinualMeddlingBR)
            {
                hints.Add("Apply march debuffs");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var pos = pc.Position;
            var dir = pc.Rotation;
            foreach (var march in MarchDirections(module, pc))
            {
                dir += march.dir;
                var dest = pos + dir.ToDirection() * _marchSpeed * march.duration;
                arena.AddLine(pos, dest, ArenaColor.Danger);
                arena.Actor(dest, pc.Rotation, ArenaColor.Danger);
                pos = dest;
            }
        }

        private List<(Angle dir, float duration, TimeSpan start)> MarchDirections(BossModule module, Actor actor)
        {
            List<(Angle dir, float duration, TimeSpan start)> res = new();
            foreach (var s in actor.Statuses)
            {
                switch ((SID)s.ID)
                {
                    case SID.ForwardMarch:
                        res.Add((0.Degrees(), _marchDuration, s.ExpireAt - module.WorldState.CurrentTime));
                        break;
                    case SID.AboutFace:
                        res.Add((180.Degrees(), _marchDuration, s.ExpireAt - module.WorldState.CurrentTime));
                        break;
                    case SID.LeftFace:
                        res.Add((90.Degrees(), _marchDuration, s.ExpireAt - module.WorldState.CurrentTime));
                        break;
                    case SID.RightFace:
                        res.Add((-90.Degrees(), _marchDuration, s.ExpireAt - module.WorldState.CurrentTime));
                        break;
                    case SID.ForcedMarch:
                        // note: as soon as player starts marching, he turns to desired direction...
                        // TODO: would be nice to use non-interpolated rotation here...
                        res.Add((0.Degrees(), (float)(s.ExpireAt - module.WorldState.CurrentTime).TotalSeconds, new()));
                        break;
                }
            }
            res.SortBy(e => e.start);
            return res;
        }
    }

    class NarrowRiftStates : StateMachineBuilder
    {
        public NarrowRiftStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<EmptyPromise>()
                .ActivateOnEnter<VanishingRay>()
                .ActivateOnEnter<ContinualMeddling>();
        }
    }

    public class NarrowRift : SimpleBossModule
    {
        public NarrowRift(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
