using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to fledgling flight & death toll mechanics
    class FledglingFlight : Component
    {
        public bool PlacementDone { get; private set; } = false;
        public bool CastsDone { get; private set; } = false;
        private List<(Actor, Angle)> _sources = new(); // actor + rotation
        private int[] _playerDeathTollStacks = new int[8];
        private int[] _playerAOECount = new int[8];

        private static Angle _coneHalfAngle = Angle.Radians(MathF.PI / 4);
        private static float _eyePlacementOffset = 10;

        public override void Update(BossModule module)
        {
            if (_sources.Count == 0)
                return;

            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                _playerDeathTollStacks[i] = player.FindStatus((uint)SID.DeathsToll)?.Extra ?? 0; // TODO: use status events here...
                _playerAOECount[i] = _sources.Where(srcRot => GeometryUtils.PointInCone(player.Position - srcRot.Item1.Position, srcRot.Item2, _coneHalfAngle)).Count();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_sources.Count == 0)
                return;

            var eyePos = GetEyePlacementPosition(module, slot, actor);
            if (eyePos != null && !GeometryUtils.PointInCircle(actor.Position - eyePos.Value, 5))
            {
                hints.Add("Get closer to eye placement position!");
            }

            if (_playerAOECount[slot] < _playerDeathTollStacks[slot])
            {
                hints.Add($"Enter more aoes ({_playerAOECount[slot]}/{_playerDeathTollStacks[slot]})!");
            }
            else if (_playerAOECount[slot] > _playerDeathTollStacks[slot])
            {
                hints.Add($"GTFO from eyes ({_playerAOECount[slot]}/{_playerDeathTollStacks[slot]})!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_sources.Count == 0)
                return;

            // draw all players
            foreach ((int i, var player) in module.Raid.WithSlot())
                arena.Actor(player, _playerAOECount[i] != _playerDeathTollStacks[i] ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);

            var eyePos = GetEyePlacementPosition(module, pcSlot, pc);
            if (eyePos != null)
                arena.AddCircle(eyePos.Value, 1, arena.ColorSafe);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach ((var source, var dir) in _sources)
            {
                arena.ZoneIsoscelesTri(source.Position, dir, _coneHalfAngle, 50, arena.ColorAOE);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AshenEye))
            {
                if (!PlacementDone)
                {
                    PlacementDone = true;
                    _sources.Clear();
                }
                _sources.Add((actor, actor.Rotation));
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AshenEye))
            {
                _sources.RemoveAll(x => x.Item1 == actor);
                CastsDone = _sources.Count == 0;
            }
        }

        public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            if (iconID >= 296 && iconID <= 299)
            {
                if (PlacementDone)
                {
                    module.ReportError(this, $"Unexpected icon after eyes started casting");
                    return;
                }

                var actor = module.WorldState.Actors.Find(actorID);
                if (actor != null)
                {
                    float dir = iconID switch
                    {
                        296 => MathF.PI / 2, // E
                        297 => 3 * MathF.PI / 2, // W
                        298 => 0, // S
                        299 => MathF.PI, // N
                        _ => 0
                    };
                    _sources.Add((actor, Angle.Radians(dir)));
                }
            }
        }

        private Vector3? GetEyePlacementPosition(BossModule module, int slot, Actor player)
        {
            if (PlacementDone)
                return null;

            (var src, Angle rot) = _sources.Find(srcRot => srcRot.Item1 == player);
            if (src == null)
                return null;

            var offset = rot.ToDirection() * _eyePlacementOffset;
            return _playerDeathTollStacks[slot] > 0 ? module.Arena.WorldCenter - offset : module.Arena.WorldCenter + offset;
        }
    }
}
