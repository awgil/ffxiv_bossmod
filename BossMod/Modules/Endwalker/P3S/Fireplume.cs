using System;
using System.Numerics;

namespace BossMod.P3S
{
    using static BossModule;

    // state related to 'single' and 'multi' fireplumes (normal or parts of gloryplume)
    class Fireplume : Component
    {
        private P3S _module;
        private Vector3? _singlePos = null;
        private float _multiStartingDirection;
        private int _multiStartedCasts = 0;
        private int _multiFinishedCasts = 0;

        private static float _singleRadius = 15;
        private static float _multiRadius = 10;
        private static float _multiPairOffset = 15;

        public Fireplume(P3S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_singlePos != null && GeometryUtils.PointInCircle(actor.Position - _singlePos.Value, _singleRadius))
            {
                hints.Add("GTFO from plume!");
            }

            if (_multiStartedCasts > _multiFinishedCasts)
            {
                if (_multiFinishedCasts > 0 && GeometryUtils.PointInCircle(actor.Position - _module.Arena.WorldCenter, _multiRadius) ||
                    _multiFinishedCasts < 8 && InPair(_multiStartingDirection + MathF.PI / 4, actor) ||
                    _multiFinishedCasts < 6 && InPair(_multiStartingDirection - MathF.PI / 2, actor) ||
                    _multiFinishedCasts < 4 && InPair(_multiStartingDirection - MathF.PI / 4, actor) ||
                    _multiFinishedCasts < 2 && InPair(_multiStartingDirection, actor))
                {
                    hints.Add("GTFO from plume!");
                }
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            if (_singlePos != null)
            {
                arena.ZoneCircle(_singlePos.Value, _singleRadius, arena.ColorAOE);
            }

            if (_multiStartedCasts > _multiFinishedCasts)
            {
                if (_multiFinishedCasts > 0) // don't draw center aoe before first explosion, it's confusing - but start drawing it immediately after first explosion, to simplify positioning
                    arena.ZoneCircle(arena.WorldCenter, _multiRadius, _multiFinishedCasts >= 6 ? arena.ColorDanger : arena.ColorAOE);

                // don't draw more than two next pairs
                if (_multiFinishedCasts < 8)
                    DrawPair(arena, _multiStartingDirection + MathF.PI / 4, _multiStartedCasts > 6 && _multiFinishedCasts >= 4);
                if (_multiFinishedCasts < 6)
                    DrawPair(arena, _multiStartingDirection - MathF.PI / 2, _multiStartedCasts > 4 && _multiFinishedCasts >= 2);
                if (_multiFinishedCasts < 4)
                    DrawPair(arena, _multiStartingDirection - MathF.PI / 4, _multiStartedCasts > 2);
                if (_multiFinishedCasts < 2)
                    DrawPair(arena, _multiStartingDirection, true);
            }
        }

        public override void OnCastStarted(Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.ExperimentalFireplumeSingleAOE:
                case AID.ExperimentalGloryplumeSingleAOE:
                    _singlePos = actor.Position;
                    break;
                case AID.ExperimentalFireplumeMultiAOE:
                case AID.ExperimentalGloryplumeMultiAOE:
                    if (_multiStartedCasts++ == 0)
                    {
                        var offset = actor.Position - _module.Arena.WorldCenter;
                        _multiStartingDirection = MathF.Atan2(offset.X, offset.Z);
                    }
                    break;
            }
        }

        public override void OnCastFinished(Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.ExperimentalFireplumeSingleAOE:
                case AID.ExperimentalGloryplumeSingleAOE:
                    _singlePos = null;
                    break;
                case AID.ExperimentalFireplumeMultiAOE:
                case AID.ExperimentalGloryplumeMultiAOE:
                    ++_multiFinishedCasts;
                    break;
            }
        }

        private bool InPair(float direction, Actor actor)
        {
            var offset = _multiPairOffset * GeometryUtils.DirectionToVec3(direction);
            return GeometryUtils.PointInCircle(actor.Position - _module.Arena.WorldCenter - offset, _multiRadius)
                || GeometryUtils.PointInCircle(actor.Position - _module.Arena.WorldCenter + offset, _multiRadius);
        }

        private void DrawPair(MiniArena arena, float direction, bool imminent)
        {
            var offset = _multiPairOffset * GeometryUtils.DirectionToVec3(direction);
            arena.ZoneCircle(arena.WorldCenter + offset, _multiRadius, imminent ? arena.ColorDanger : arena.ColorAOE);
            arena.ZoneCircle(arena.WorldCenter - offset, _multiRadius, imminent ? arena.ColorDanger : arena.ColorAOE);
        }
    }
}
