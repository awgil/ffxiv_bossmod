namespace BossMod.Endwalker.Ultimate.DSW1
{
    // 'heavensblaze' (full party stack except tank) + 'holy shield bash' / 'holy bladedance' (stun + aoe tankbuster)
    class Heavensblaze : BossComponent
    {
        private Actor? _danceSource;
        private Actor? _danceTarget;
        private Actor? _blazeTarget;

        private static AOEShapeCone _danceAOE = new(16, 45.Degrees());
        private static float _blazeRadius = 4;

        public Heavensblaze()
        {
            Tether(TetherID.HolyBladedance, (_, source, target) => { _danceSource = source; _danceTarget = target; });
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_blazeTarget != null)
            {
                // during blaze cast, dance target is stunned, so don't bother providing any hints for him
                // others need to remain stacked and out of dance aoe
                if (actor == _danceTarget)
                    return;

                if (actor != _blazeTarget && !actor.Position.InCircle(_blazeTarget.Position, _blazeRadius))
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
                    arena.Actor(p, p == _danceTarget || p == _blazeTarget ? ArenaColor.Danger : p.Position.InCircle(_blazeTarget.Position, _blazeRadius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
                arena.AddCircle(_blazeTarget.Position, _blazeRadius, ArenaColor.Safe);
            }
            else if (_danceSource != null && _danceTarget != null)
            {
                foreach (var p in module.Raid.WithoutSlot())
                    arena.Actor(p, p == _danceTarget ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
                arena.AddLine(_danceSource.Position, _danceTarget.Position, ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.Heavensblaze))
                _blazeTarget = module.WorldState.Actors.Find(actor.CastInfo.TargetID);
        }
    }
}
