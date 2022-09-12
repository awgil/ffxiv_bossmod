using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows line-of-sight cones for arbitrary origin and blocking shapes
    public abstract class GenericLineOfSightAOE : CastCounter
    {
        public float MaxRange { get; private init; }
        public DateTime NextExplosion;
        public WPos? Origin { get; private set; } // inactive if null
        public List<(WPos Center, float Radius)> Blockers { get; private set; } = new();
        public List<(float Distance, Angle Dir, Angle HalfWidth)> Visibility { get; private set; } = new();

        public GenericLineOfSightAOE(ActionID aid, float maxRange = 1000) : base(aid)
        {
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
                    Visibility.Add((dist + b.Radius, Angle.FromDirection(toBlock), Angle.Asin(b.Radius / dist)));
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Origin == null || !actor.Position.InCircle(Origin.Value, MaxRange))
                return;
            if (Visibility.Any(v => !actor.Position.InCircle(Origin.Value, v.Distance) && actor.Position.InCone(Origin.Value, v.Dir, v.HalfWidth)))
                return;
            hints.Add("Hide behind obstacle!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, AIHints hints)
        {
            if (Origin != null)
            {
                hints.RestrictedZones.Add((new AOEShapeDonut(MaxRange, 1000), Origin.Value, new(), NextExplosion));
                foreach (var v in Visibility)
                    hints.RestrictedZones.Add((new AOEShapeDonutSector(v.Distance, MaxRange + 10, v.HalfWidth), Origin.Value, v.Dir, NextExplosion));
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (Origin != null)
            {
                arena.ZoneDonut(Origin.Value, MaxRange, 1000, ArenaColor.SafeFromAOE);
                foreach (var v in Visibility)
                    arena.ZoneCone(Origin.Value, v.Distance, 1000, v.Dir, v.HalfWidth, ArenaColor.SafeFromAOE);
            }
        }
    }

    // simple line-of-sight aoe that happens at the end of the cast
    public class CastLineOfSightAOE : GenericLineOfSightAOE
    {
        public uint BlockOID { get; private init; }
        private List<Actor> _blockers = new();
        private List<Actor> _casters = new();
        public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.FinishAt);

        public CastLineOfSightAOE(ActionID aid, uint blockOID, float maxRange = 1000) : base(aid, maxRange)
        {
            BlockOID = blockOID;
        }

        public override void Init(BossModule module)
        {
            _blockers = module.Enemies(BlockOID);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Add(caster);
                Modify(ActiveCaster?.Position ?? null, _blockers.Select(b => (b.Position, b.HitboxRadius)));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Remove(caster);
                Modify(ActiveCaster?.Position ?? null, _blockers.Select(b => (b.Position, b.HitboxRadius)));
            }
        }
    }
}
