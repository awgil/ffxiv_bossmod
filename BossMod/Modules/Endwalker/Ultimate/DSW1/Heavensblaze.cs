using System;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    // 'heavensblaze' (full party stack except tank) + 'holy shield bash' / 'holy bladedance' (stun + aoe tankbuster)
    class Heavensblaze : BossModule.Component
    {
        private Actor? _danceSource;
        private Actor? _danceTarget;
        private Actor? _blazeTarget;

        private static AOEShapeCone _danceAOE = new(16, Angle.Radians(MathF.PI / 4));
        private static float _blazeRadius = 4;

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_blazeTarget != null)
            {
                // during blaze cast, dance target is stunned, so don't bother providing any hints for him
                // others need to remain stacked and out of dance aoe
                if (actor == _danceTarget)
                    return;

                if (actor != _blazeTarget && !GeometryUtils.PointInCircle(actor.Position - _blazeTarget.Position, _blazeRadius))
                    hints.Add("Stack!");

                // don't bother adding hints for dance aoe, you're probably dead if you get there anyway...
            }
            else if (_danceSource != null && _danceTarget != null)
            {
                // TODO: consider adding pass/take tether hint (how to select tether tank, should it always be adelphel's?)
                // TODO: consider adding invuln hint for tether tank?..
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_danceSource != null && _danceTarget != null)
            {
                _danceAOE.Draw(arena, _danceSource.Position, Angle.FromDirection(_danceTarget.Position - _danceSource.Position));
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_blazeTarget != null)
            {
                foreach (var p in module.Raid.WithoutSlot())
                    arena.Actor(p, p == _danceTarget || p == _blazeTarget ? arena.ColorDanger : GeometryUtils.PointInCircle(p.Position - _blazeTarget.Position, _blazeRadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                arena.AddCircle(_blazeTarget.Position, _blazeRadius, arena.ColorSafe);
            }
            else if (_danceSource != null && _danceTarget != null)
            {
                foreach (var p in module.Raid.WithoutSlot())
                    arena.Actor(p, p == _danceTarget ? arena.ColorDanger : arena.ColorPlayerGeneric);
                arena.AddLine(_danceSource.Position, _danceTarget.Position, arena.ColorDanger);
            }
        }

        public override void OnTethered(BossModule module, Actor actor)
        {
            _danceSource = actor;
            _danceTarget = module.WorldState.Actors.Find(actor.Tether.Target);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.Heavensblaze))
                _blazeTarget = module.WorldState.Actors.Find(actor.CastInfo.TargetID);
        }
    }
}
