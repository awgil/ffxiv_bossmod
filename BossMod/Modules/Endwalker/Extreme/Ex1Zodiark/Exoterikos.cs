using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex1Zodiark
{
    // state related to exoterikos, trimorphos exoterikos and triple esoteric ray mechanics
    class Exoterikos : BossComponent
    {
        private List<(Actor, AOEShape)> _sources= new();

        private static AOEShapeRect _aoeSquare = new(21, 21);
        private static AOEShapeCone _aoeTriangle = new(47, 30.Degrees());
        private static AOEShapeRect _aoeRay = new(42, 7);

        public bool Done => _sources.Count == 0;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveSources(module).Any(actShape => actShape.Item2.Check(actor.Position, actShape.Item1)))
                hints.Add("GTFO from exo aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (src, shape) in ActiveSources(module))
                shape.Draw(arena, src);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            // tethers appear much earlier than cast start, but not for all sigils
            var target = module.WorldState.Actors.Find(tether.Target);
            if (source != module.PrimaryActor || target == null)
                return;

            var shape = ShapeForSigil(target);
            if (shape != null)
                _sources.Add((target, shape));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var shape = ShapeForSigil(caster);
            if (shape != null && !_sources.Any(actShape => actShape.Item1 == caster))
                _sources.Add((caster, shape));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            _sources.RemoveAll(actShape => actShape.Item1 == caster);
        }

        private AOEShape? ShapeForSigil(Actor sigil)
        {
            return (OID)sigil.OID switch
            {
                OID.ExoSquare => _aoeSquare,
                OID.ExoTri => _aoeTriangle,
                OID.ExoGreen => _aoeRay,
                _ => null
            };
        }

        private IEnumerable<(Actor, AOEShape)> ActiveSources(BossModule module)
        {
            bool hadSideSquare = false; // we don't show multiple side-squares, since that would cover whole arena and be useless
            DateTime lastRay = new(); // we only show first rays, otherwise triple rays would cover whole arena and be useless
            foreach (var (actor, shape) in _sources)
            {
                if (shape == _aoeSquare && MathF.Abs(actor.Position.X - module.Bounds.Center.X) > 10)
                {
                    if (hadSideSquare)
                        continue;
                    hadSideSquare = true;
                }
                else if (shape == _aoeRay)
                {
                    if (lastRay != default && (actor.CastInfo == null || (actor.CastInfo.FinishAt - lastRay).TotalSeconds > 2))
                        continue;
                    lastRay = actor.CastInfo?.FinishAt ?? new();
                }
                yield return (actor, shape);
            }
        }
    }
}
