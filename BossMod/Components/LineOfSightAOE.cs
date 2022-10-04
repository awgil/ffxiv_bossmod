using BossMod.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows line-of-sight cones for arbitrary origin and blocking shapes
    public abstract class GenericLineOfSightAOE : CastCounter
    {
        // shape describing forbidden zone
        // note that origin/rotation args are ignored (rotation doesn't matter, changing origin requires recalculating cached visibility data)
        private class ResultingShape : AOEShape
        {
            public float MaxRange;
            public WPos? Origin;
            public List<(WPos Center, float Radius)> Blockers = new();
            public List<(float Distance, Angle Dir, Angle HalfWidth)> Visibility = new();

            public override bool Check(WPos position, WPos origin, Angle rotation)
            {
                return Origin != null
                    && position.InCircle(Origin.Value, MaxRange)
                    && !Visibility.Any(v => !position.InCircle(Origin.Value, v.Distance) && position.InCone(Origin.Value, v.Dir, v.HalfWidth));
            }

            public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
            {
                yield break; // not supported, do we need it?..
            }

            public override Func<WPos, Map.Coverage> Coverage(Map map, WPos origin, Angle rotation)
            {
                // TODO: important, implement...
                List<Func<WPos, Map.Coverage>> safeZones = new();
                safeZones.Add(map.CoverageDonut(origin, MaxRange, 100));
                foreach (var v in Visibility)
                    safeZones.Add(map.CoverageDonutSector(origin, v.Distance, 100, v.Dir, v.HalfWidth));
                return map.CoverageInvert(map.CoverageUnion(safeZones));
            }

            public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color)
            {
                // not supported...
            }

            public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color)
            {
                // not supported...
            }
        }

        public DateTime NextExplosion;
        public bool BlockersImpassable;
        public float MaxRange => _shape.MaxRange;
        public WPos? Origin => _shape.Origin; // inactive if null
        public IReadOnlyList<(WPos Center, float Radius)> Blockers => _shape.Blockers;
        public IReadOnlyList<(float Distance, Angle Dir, Angle HalfWidth)> Visibility => _shape.Visibility;
        private ResultingShape _shape = new();
        private List<AOEShape> _blockerShapes = new();

        public GenericLineOfSightAOE(ActionID aid, float maxRange, bool blockersImpassable) : base(aid)
        {
            BlockersImpassable = blockersImpassable;
            _shape.MaxRange = maxRange;
        }

        public void Modify(WPos? origin, IEnumerable<(WPos Center, float Radius)> blockers)
        {
            _shape.Origin = origin;
            _shape.Blockers.Clear();
            _shape.Blockers.AddRange(blockers);
            _shape.Visibility.Clear();
            _blockerShapes.Clear();
            if (origin != null)
            {
                foreach (var b in Blockers)
                {
                    var toBlock = b.Center - origin.Value;
                    var dist = toBlock.Length();
                    _shape.Visibility.Add((dist + b.Radius, Angle.FromDirection(toBlock), Angle.Asin(b.Radius / dist)));
                }
            }
            if (BlockersImpassable)
                foreach (var b in Blockers)
                    _blockerShapes.Add(new AOEShapeCircle(b.Radius));
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_shape.Check(actor.Position, new(), new()))
                hints.Add("Hide behind obstacle!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, AIHints hints)
        {
            if (Origin != null)
                hints.ForbiddenZones.Add((_shape, Origin.Value, new(), NextExplosion));
            foreach (var (blocker, shape) in _shape.Blockers.Zip(_blockerShapes))
                hints.ForbiddenZones.Add((shape, blocker.Center, new(), new()));
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            // TODO: reconsider, this looks like shit...
            if (Origin != null)
            {
                arena.ZoneDonut(Origin.Value, MaxRange, 1000, ArenaColor.SafeFromAOE);
                foreach (var v in Visibility)
                    arena.ZoneCone(Origin.Value, v.Distance, 1000, v.Dir, v.HalfWidth, ArenaColor.SafeFromAOE);
            }
        }
    }

    // simple line-of-sight aoe that happens at the end of the cast
    public abstract class CastLineOfSightAOE : GenericLineOfSightAOE
    {
        private List<Actor> _casters = new();
        public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.FinishAt);

        public CastLineOfSightAOE(ActionID aid, float maxRange, bool blockersImpassable) : base(aid, maxRange, blockersImpassable) { }

        public abstract IEnumerable<Actor> BlockerActors(BossModule module);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Add(caster);
                Refresh(module);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Remove(caster);
                Refresh(module);
            }
        }

        private void Refresh(BossModule module)
        {
            var caster = ActiveCaster;
            WPos? position = caster != null ? (module.WorldState.Actors.Find(caster.CastInfo!.TargetID)?.Position ?? caster.CastInfo!.LocXZ) : null;
            Modify(position, BlockerActors(module).Select(b => (b.Position, b.HitboxRadius)));
        }
    }
}
