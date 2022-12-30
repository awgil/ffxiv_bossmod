using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3QueensGuard
{
    class CoatOfArms : BossComponent
    {
        public List<Actor> Wards = new();
        private Dictionary<ulong, Angle> _activeWards = new();
        private Dictionary<ulong, Angle> _imminentWards = new();

        public bool Active => _activeWards.Count > 0;

        public override void Init(BossModule module)
        {
            Wards = module.Enemies(OID.AetherialWard);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var targetWard = Wards.Find(w => w.InstanceID == actor.TargetID);
            if (targetWard == null)
                return;

            Angle wardDir;
            if (_activeWards.TryGetValue(actor.TargetID, out wardDir) || _imminentWards.TryGetValue(actor.TargetID, out wardDir))
            {
                var attackDir = (actor.Position - targetWard.Position).Normalized();
                var forbiddenDir = (targetWard.Rotation + wardDir).ToDirection();
                if (Math.Abs(attackDir.Dot(forbiddenDir)) > 0.7071067f)
                {
                    hints.Add("Attack target from unshielded side!");
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var w in Wards.Where(w => w.IsTargetable && !w.IsDead))
            {
                arena.Actor(w, ArenaColor.Enemy, true);
                Angle wardDir;
                if (_activeWards.TryGetValue(w.InstanceID, out wardDir) || _imminentWards.TryGetValue(w.InstanceID, out wardDir))
                {
                    DrawWard(arena, w.Position, w.Rotation + wardDir);
                    DrawWard(arena, w.Position, w.Rotation + wardDir + 180.Degrees());
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.DirectionalParry)
            {
                _imminentWards.Remove(actor.InstanceID);
                _activeWards[actor.InstanceID] = (status.Extra == 0xC ? 90 : 0).Degrees();
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.DirectionalParry)
            {
                _activeWards.Remove(actor.InstanceID);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.CoatOfArmsFB:
                    _imminentWards[caster.InstanceID] = 0.Degrees();
                    break;
                case AID.CoatOfArmsLR:
                    _imminentWards[caster.InstanceID] = 90.Degrees();
                    break;
            }
        }

        private void DrawWard(MiniArena arena, WPos pos, Angle dir)
        {
            arena.PathArcTo(pos, 1.5f, (dir - 45.Degrees()).Rad, (dir + 45.Degrees()).Rad);
            arena.PathStroke(false, ArenaColor.Danger);
        }
    }
}
