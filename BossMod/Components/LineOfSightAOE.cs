using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows line-of-sight cones for arbitrary origin and blocking shapes
    public abstract class GenericLineOfSightAOE : CastCounter
    {
        public DateTime NextExplosion;
        public bool BlockersImpassable;
        public float MaxRange { get; private set; }
        public WPos? Origin { get; private set; } // inactive if null
        public List<(WPos Center, float Radius)> Blockers { get; private set; } = new();
        public List<(float Distance, Angle Dir, Angle HalfWidth)> Visibility { get; private set; } = new();

        public GenericLineOfSightAOE(ActionID aid, float maxRange, bool blockersImpassable) : base(aid)
        {
            BlockersImpassable = blockersImpassable;
            MaxRange = maxRange;
        }

        public void Modify(WPos? origin, IEnumerable<(WPos Center, float Radius)> blockers)
        {
            Origin = origin;
            Blockers.Clear();
            Blockers.AddRange(blockers);
            Visibility.Clear();
            if (origin != null)
            {
                foreach (var b in Blockers)
                {
                    var toBlock = b.Center - origin.Value;
                    var dist = toBlock.Length();
                    Visibility.Add((dist + b.Radius, Angle.FromDirection(toBlock), b.Radius < dist ? Angle.Asin(b.Radius / dist) : 90.Degrees()));
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Origin != null
                && actor.Position.InCircle(Origin.Value, MaxRange)
                && !Visibility.Any(v => !actor.Position.InCircle(Origin.Value, v.Distance) && actor.Position.InCone(Origin.Value, v.Dir, v.HalfWidth)))
            {
                hints.Add("Hide behind obstacle!");
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (Origin != null)
            {
                // inverse of a union of inverted max-range circle and a bunch of infinite cones minus inner cirles
                var normals = Visibility.Select(v => (v.Distance, (v.Dir + v.HalfWidth).ToDirection().OrthoL(), (v.Dir - v.HalfWidth).ToDirection().OrthoR())).ToArray();
                Func<WPos, float> invertedDistanceToSafe = p =>
                {
                    var off = p - Origin.Value;
                    var distOrigin = off.Length();
                    var distanceToSafe = MaxRange - distOrigin; // this is positive if we're inside max-range
                    foreach (var (minRange, nl, nr) in normals)
                    {
                        var distInnerInv = minRange - distOrigin;
                        var distLeft = off.Dot(nl);
                        var distRight = off.Dot(nr);
                        var distCone = Math.Max(distInnerInv, Math.Max(distLeft, distRight));
                        distanceToSafe = Math.Min(distanceToSafe, distCone);
                    }
                    return -distanceToSafe;
                };
                hints.AddForbiddenZone(invertedDistanceToSafe, NextExplosion);
            }
            if (BlockersImpassable)
            {
                var blockers = Blockers.Select(b => ShapeDistance.Circle(b.Center, b.Radius)).ToArray();
                hints.AddForbiddenZone(p => blockers.Min(b => b(p)));
            }
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
        public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.NPCFinishAt);

        public CastLineOfSightAOE(ActionID aid, float maxRange, bool blockersImpassable) : base(aid, maxRange, blockersImpassable) { }

        public abstract IEnumerable<Actor> BlockerActors(BossModule module);

        public override void Init(BossModule module)
        {
            Refresh(module);
        }

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
