using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A1Byregot
{
    class Reproduce : BossComponent
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

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_active.Any(e => ImminentAOEs(module, e).Any(c => _aoe.Check(actor.Position, c))))
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
                    _aoe.Draw(arena, c);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow)
                _active.Add(new(caster.Position, (AID)spell.Action.ID == AID.CloudToGroundFast));
        }

        public override void OnEventCast(BossModule module, Actor caster, CastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.CloudToGroundFast or AID.CloudToGroundSlow or AID.CloudToGroundFastAOE or AID.CloudToGroundSlowAOE)
            {
                int index = _active.FindIndex(item => MathF.Abs(item.Next.Z - caster.Position.Z) < 1);
                if (index == -1)
                {
                    module.ReportError(this, $"Failed to find entry for {spell.CasterID:X}");
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
