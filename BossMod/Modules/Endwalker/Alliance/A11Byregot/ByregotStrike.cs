namespace BossMod.Endwalker.Alliance.A11Byregot
{
    class ByregotStrike : BossComponent
    {
        private Actor? _jumpCaster;
        private Actor? _knockbackCaster;
        private Angle? _coneRotation;

        private static AOEShapeCircle _aoeJump = new(8);
        private static AOEShapeCone _aoeCone = new(90, 15.Degrees());
        private static float _knockbackDistance = 18;

        public bool Done => _knockbackCaster == null;

        public override void Update(BossModule module)
        {
            if (_coneRotation != null && _jumpCaster != null)
                _coneRotation = _jumpCaster.Rotation;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_jumpCaster != null && _aoeJump.Check(actor.Position, _jumpCaster.CastInfo!.LocXZ))
            {
                hints.Add("GTFO from aoe!");
            }

            if (_coneRotation != null && _knockbackCaster != null)
            {
                if (_aoeCone.Check(actor.Position, _knockbackCaster.Position, _coneRotation.Value) ||
                    _aoeCone.Check(actor.Position, _knockbackCaster.Position, _coneRotation.Value + 90.Degrees()) ||
                    _aoeCone.Check(actor.Position, _knockbackCaster.Position, _coneRotation.Value + 180.Degrees()) ||
                    _aoeCone.Check(actor.Position, _knockbackCaster.Position, _coneRotation.Value - 90.Degrees()))
                {
                    hints.Add("GTFO from cones!");
                }
            }

            if (_knockbackCaster != null)
            {
                var adjPos = Components.Knockback.AwayFromSource(actor.Position, _knockbackCaster, _knockbackDistance);
                if (!module.Bounds.Contains(adjPos))
                {
                    hints.Add("About to be knocked into wall!");
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_jumpCaster != null)
            {
                _aoeJump.Draw(arena, _jumpCaster.CastInfo!.LocXZ);
            }

            if (_coneRotation != null && _knockbackCaster != null)
            {
                _aoeCone.Draw(arena, _knockbackCaster.Position, _coneRotation.Value);
                _aoeCone.Draw(arena, _knockbackCaster.Position, _coneRotation.Value + 90.Degrees());
                _aoeCone.Draw(arena, _knockbackCaster.Position, _coneRotation.Value + 180.Degrees());
                _aoeCone.Draw(arena, _knockbackCaster.Position, _coneRotation.Value - 90.Degrees());
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_knockbackCaster != null)
            {
                var adjPos = Components.Knockback.AwayFromSource(pc.Position, _knockbackCaster, _knockbackDistance);
                if (adjPos != pc.Position)
                {
                    arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
                    arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ByregotStrike:
                    _jumpCaster = caster;
                    break;
                case AID.ByregotStrikeWithCone:
                    _jumpCaster = caster;
                    _coneRotation = caster.Rotation;
                    break;
                case AID.ByregotStrikeKnockback:
                    _knockbackCaster = caster;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (_jumpCaster == caster)
            {
                _jumpCaster = null;
            }
            if (_knockbackCaster == caster)
            {
                _knockbackCaster = null;
                _coneRotation = null;
            }
        }
    }
}
