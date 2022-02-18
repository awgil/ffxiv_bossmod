using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.P3S
{
    using static BossModule;

    // state related to fledgling flight & death toll mechanics
    class FledglingFlight : Component
    {
        public bool PlacementDone { get; private set; } = false;
        public bool CastsDone { get; private set; } = false;
        private P3S _module;
        private List<(WorldState.Actor, float)> _sources = new(); // actor + rotation
        private int[] _playerDeathTollStacks = new int[8];
        private int[] _playerAOECount = new int[8];

        private static float _coneHalfAngle = MathF.PI / 8; // not sure about this
        private static float _eyePlacementOffset = 10;

        public FledglingFlight(P3S module)
        {
            _module = module;
        }

        public override void Update()
        {
            if (_sources.Count == 0)
                return;

            foreach ((int i, var player) in _module.RaidMembers.WithSlot())
            {
                _playerDeathTollStacks[i] = player.FindStatus((uint)SID.DeathsToll)?.Extra ?? 0; // TODO: use status events here...
                _playerAOECount[i] = _sources.Where(srcRot => GeometryUtils.PointInCone(player.Position - srcRot.Item1.Position, srcRot.Item2, _coneHalfAngle)).Count();
            }
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_sources.Count == 0)
                return;

            var eyePos = GetEyePlacementPosition(slot, actor);
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

        public override void DrawArenaForeground(MiniArena arena)
        {
            var pc = _module.Player();
            if (_sources.Count == 0 || pc == null)
                return;

            // draw all players
            foreach ((int i, var player) in _module.RaidMembers.WithSlot())
                arena.Actor(player, _playerAOECount[i] != _playerDeathTollStacks[i] ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);

            var eyePos = GetEyePlacementPosition(_module.PlayerSlot, pc);
            if (eyePos != null)
                arena.AddCircle(eyePos.Value, 1, arena.ColorSafe);
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            foreach ((var source, var dir) in _sources)
            {
                arena.ZoneIsoscelesTri(source.Position, dir, _coneHalfAngle, 50, arena.ColorAOE);
            }
        }

        public override void OnCastStarted(WorldState.Actor actor)
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

        public override void OnCastFinished(WorldState.Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AshenEye))
            {
                _sources.RemoveAll(x => x.Item1 == actor);
                CastsDone = _sources.Count == 0;
            }
        }

        public override void OnEventIcon(uint actorID, uint iconID)
        {
            if (iconID >= 296 && iconID <= 299)
            {
                if (PlacementDone)
                {
                    Service.Log($"[P3S] [FledglingFlight] Unexpected icon after eyes started casting");
                    return;
                }

                var actor = _module.WorldState.FindActor(actorID);
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
                    _sources.Add((actor, dir));
                }
            }
        }

        private Vector3? GetEyePlacementPosition(int slot, WorldState.Actor player)
        {
            if (PlacementDone)
                return null;

            (var src, float rot) = _sources.Find(srcRot => srcRot.Item1 == player);
            if (src == null)
                return null;

            var offset = GeometryUtils.DirectionToVec3(rot) * _eyePlacementOffset;
            return _playerDeathTollStacks[slot] > 0 ? _module.Arena.WorldCenter - offset : _module.Arena.WorldCenter + offset;
        }
    }
}
