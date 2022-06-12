using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A4Naldthal
{
    // TODO: refactor, it's quite hacky...
    class OnceAboveEverBelow : BossModule.Component
    {
        private class Instance
        {
            public WPos Start;
            public WDir Dir;
            public int Casts;

            public Instance(WPos start, WDir dir)
            {
                Start = start;
                Dir = dir;
            }
        }

        private List<Instance> _active = new();
        private int _restCount;

        private static float _advance = 6;
        private static AOEShapeCircle _aoe = new(6);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (ActiveAOEs().Any(c => _aoe.Check(actor.Position, c)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in ActiveAOEs())
                _aoe.Draw(arena, c);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.EverfireFirst) || actor.CastInfo!.IsSpell(AID.OnceBurnedFirst))
            {
                WDir dir = MathF.Abs(actor.Position.X - module.Bounds.Center.X) < 1 ? new(1, 0) : new(0, 1);
                _active.Add(new(actor.Position, dir));
                _active.Add(new(actor.Position, -dir));
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.EverfireFirst) || actor.CastInfo!.IsSpell(AID.OnceBurnedFirst))
            {
                foreach (var inst in _active.Where(i => i.Start == actor.Position))
                {
                    ++inst.Casts;
                }
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (_active.Count > 0 && info!.IsSpell(AID.EverfireRest) || info!.IsSpell(AID.OnceBurnedRest))
            {
                if (++_restCount > 24)
                {
                    _active.Clear();
                    _restCount = 0;
                    return;
                }

                var caster = module.WorldState.Actors.Find(info.CasterID);
                if (caster != null)
                {
                    var dir = caster.Rotation.ToDirection();
                    var normal = dir.OrthoL();
                    var inst = _active.Find(i => WDir.Dot(i.Dir, dir) > 0.9f && MathF.Abs(WDir.Dot(caster.Position - i.Start, normal)) < 1);
                    if (inst != null)
                    {
                        ++inst.Casts;
                    }
                }
            }
        }

        private IEnumerable<WPos> ActiveAOEs()
        {
            foreach (var inst in _active)
            {
                for (int i = inst.Casts; i <= 4; ++i)
                {
                    yield return inst.Start + i * _advance * inst.Dir;
                }
            }
        }
    }
}
