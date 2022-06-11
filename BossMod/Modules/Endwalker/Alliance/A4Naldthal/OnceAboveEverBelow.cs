using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Alliance.A4Naldthal
{
    // TODO: refactor, it's quite hacky...
    class OnceAboveEverBelow : BossModule.Component
    {
        private class Instance
        {
            public Vector3 Start;
            public Vector3 Dir;
            public int Casts;

            public Instance(Vector3 start, Vector3 dir)
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
            if (ActiveAOEs().Any(c => _aoe.Check(actor.Position, c, new())))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in ActiveAOEs())
                _aoe.Draw(arena, c, new());
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.EverfireFirst) || actor.CastInfo!.IsSpell(AID.OnceBurnedFirst))
            {
                Vector3 dir = MathF.Abs(actor.Position.X - module.Arena.WorldCenter.X) < 1 ? Vector3.UnitX : Vector3.UnitZ;
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
                    var normal = new Vector3(dir.Z, 0, -dir.X);
                    var inst = _active.Find(i => Vector3.Dot(i.Dir, dir) > 0.9f && MathF.Abs(Vector3.Dot(caster.Position - i.Start, normal)) < 1);
                    if (inst != null)
                    {
                        ++inst.Casts;
                    }
                }
            }
        }

        private IEnumerable<Vector3> ActiveAOEs()
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
