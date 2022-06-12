using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Alliance.A1Byregot
{
    class Reproduce : BossModule.Component
    {
        private class Entry
        {
            public WPos Next;
            public bool Fast;

            public Entry(WPos next, bool fast)
            {
                Next = next;
                Fast = fast;
            }
        }

        private List<Entry> _active = new();

        private static AOEShapeCircle _aoe = new(7);
        private static WDir _advance = new(-8.5f, 0);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_active.Any(e => ImminentAOEs(module, e).Any(c => _aoe.Check(actor.Position, c, new()))))
            {
                hints.Add("GTFO from aoe!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var e in _active)
            {
                foreach (var c in ImminentAOEs(module, e))
                {
                    _aoe.Draw(arena, c, new());
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell() && (AID)actor.CastInfo.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow)
                _active.Add(new(actor.Position, (AID)actor.CastInfo.Action.ID == AID.CloudToGroundFast));
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell() && (AID)info.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow or AID.CloudToGroundFastAOE or AID.CloudToGroundSlowAOE)
            {
                var caster = module.WorldState.Actors.Find(info.CasterID);
                int index = caster != null ? _active.FindIndex(item => MathF.Abs(item.Next.Z - caster.Position.Z) < 1) : -1;
                if (index == -1)
                {
                    module.ReportError(this, $"Failed to find entry for {info.CasterID:X}");
                    return;
                }

                _active[index].Next += _advance;
                if (_active[index].Next.X < module.Bounds.Center.X - module.Bounds.HalfSize)
                    _active.RemoveAt(index);
            }
        }

        private IEnumerable<WPos> ImminentAOEs(BossModule module, Entry e)
        {
            int limit = e.Fast ? 5 : 2;
            var center = e.Next;
            while (limit > 0)
            {
                if (center.X < module.Bounds.Center.X - module.Bounds.HalfSize)
                    yield break;
                yield return center;
                center += _advance;
                --limit;
            }
        }
    }
}
