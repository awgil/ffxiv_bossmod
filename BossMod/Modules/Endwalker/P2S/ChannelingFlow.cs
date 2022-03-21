using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P2S
{
    using static BossModule;

    // state related to channeling [over]flow mechanics
    class ChannelingFlow : Component
    {
        private (Vector3, DateTime)[] _arrows = new (Vector3, DateTime)[PartyState.MaxSize];

        private static float _typhoonHalfWidth = 2.5f;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            Actor? partner = null;
            if (SlotActive(module, slot))
            {
                int numPartners = 0, numClipped = 0;
                var partnerDir = -_arrows[slot].Item1;
                float minDistance = 50;
                foreach (var (otherSlot, otherActor) in ActorsHitBy(module, slot, actor))
                {
                    if (_arrows[otherSlot].Item1 == partnerDir)
                    {
                        minDistance = MathF.Min(minDistance, Vector3.Dot(actor.Position - otherActor.Position, partnerDir));
                        ++numPartners;
                        partner = otherActor;
                    }
                    else
                    {
                        ++numClipped;
                    }
                }

                if (numPartners == 0)
                    hints.Add("Aim to hit partner!");
                if (numPartners > 1 || numClipped > 0)
                    hints.Add("Avoid clipping irrelevant players!");
                if (minDistance < 20) // TODO: verify min range
                    hints.Add("Too close to partner!");
            }

            if (ActiveArrows(module).Any(pd => pd.Item1 != actor && pd.Item1 != partner && GeometryUtils.PointInRect(actor.Position - pd.Item1.Position, pd.Item2, 50, 0, _typhoonHalfWidth)))
                hints.Add("GTFO from imminent flow!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (player, dir) in ActiveArrows(module))
            {
                arena.ZoneQuad(player.Position, dir, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            var dir = (SID)actor.Statuses[index].ID switch
            {
                SID.MarkFlowN => -Vector3.UnitZ,
                SID.MarkFlowS =>  Vector3.UnitZ,
                SID.MarkFlowW => -Vector3.UnitX,
                SID.MarkFlowE =>  Vector3.UnitX,
                _ => Vector3.Zero
            };
            if (dir != Vector3.Zero)
            {
                var slot = module.WorldState.Party.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    _arrows[slot] = (dir, actor.Statuses[index].ExpireAt);
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, int index)
        {
            var s = actor.Statuses[index];
            if ((SID)s.ID is SID.MarkFlowN or SID.MarkFlowS or SID.MarkFlowW or SID.MarkFlowE)
            {
                var slot = module.WorldState.Party.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    _arrows[slot] = (Vector3.Zero, new());
            }
        }

        private bool SlotActive(BossModule module, int slot)
        {
            var (dir, expire) = _arrows[slot];
            return dir != Vector3.Zero && (expire - module.WorldState.CurrentTime).TotalSeconds < 13;
        }

        private IEnumerable<(Actor, Vector3)> ActiveArrows(BossModule module)
        {
            return module.Raid.WithSlot().WhereSlot(slot => SlotActive(module, slot)).Select(ia => (ia.Item2, _arrows[ia.Item1].Item1));
        }

        private IEnumerable<(int, Actor)> ActorsHitBy(BossModule module, int slot, Actor actor)
        {
            return module.Raid.WithSlot().Exclude(slot).WhereActor(a => GeometryUtils.PointInRect(a.Position - actor.Position, _arrows[slot].Item1, 50, 0, _typhoonHalfWidth));
        }
    }
}
