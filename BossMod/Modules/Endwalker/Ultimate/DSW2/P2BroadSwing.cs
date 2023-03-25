namespace BossMod.Endwalker.Ultimate.DSW2
{
    // TODO: refactor (generic-aoe, use cast rotation)
    class P2BroadSwing : Components.CastCounter
    {
        private Actor? _caster;
        private bool _lr;

        private static AOEShapeCone _aoe = new(40, 60.Degrees());

        public P2BroadSwing() : base(ActionID.MakeSpell(AID.BroadSwingAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_caster == null)
                return;
            bool inAOE = NumCasts switch
            {
                0 => _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(0)) || _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(1)),
                1 => _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(1)) || _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(2)),
                2 => _aoe.Check(actor.Position, _caster.Position, _caster.Rotation + AngleOffset(2)),
                _ => false
            };
            if (inAOE)
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_caster == null)
                return;
            switch (NumCasts)
            {
                case 0:
                    DrawZone(arena, _caster, 0, ArenaColor.Danger);
                    DrawZone(arena, _caster, 1, ArenaColor.AOE);
                    break;
                case 1:
                    DrawZone(arena, _caster, 1, ArenaColor.Danger);
                    DrawZone(arena, _caster, 2, ArenaColor.AOE);
                    break;
                case 2:
                    DrawZone(arena, _caster, 2, ArenaColor.Danger);
                    break;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.BroadSwingRL:
                    _caster = caster;
                    _lr = false;
                    break;
                case AID.BroadSwingLR:
                    _caster = caster;
                    _lr = true;
                    break;
            }
        }

        private Angle AngleOffset(int order)
        {
            var dir = order switch
            {
                0 => -60.Degrees(),
                1 =>  60.Degrees(),
                _ => 180.Degrees()
            };
            return _lr ? -dir : dir;
        }

        private void DrawZone(MiniArena arena, Actor caster, int order, uint color)
        {
            var dir = caster.Rotation + AngleOffset(order);
            arena.ZoneCone(caster.Position, 0, _aoe.Radius, dir, _aoe.HalfAngle, color);
        }
    }
}
