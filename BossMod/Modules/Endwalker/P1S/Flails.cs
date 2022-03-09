using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod.P1S
{
    using static BossModule;

    // state related to [aether]flails mechanics
    class Flails : Component
    {
        public enum Zone { None, Left, Right, Inner, Outer, UnknownCircle }

        public int NumCasts { get; private set; } = 0;
        private P1S _module;
        private List<Actor> _weaponsBall;
        private List<Actor> _weaponsChakram;
        private Zone _first = Zone.None;
        private Zone _second = Zone.None;
        private bool _showFirst = false;
        private bool _showSecond = false;

        private static float _coneHalfAngle = MathF.PI / 4;

        public Flails(P1S module, Zone first, Zone second)
        {
            _module = module;
            _weaponsBall = module.Enemies(OID.FlailI);
            _weaponsChakram = module.Enemies(OID.FlailO);
            _first = first;
            _second = second;
            _showFirst = true;
            _showSecond = (first == Zone.Left || first == Zone.Right) != (second == Zone.Left || second == Zone.Right);
        }

        public override void Update()
        {
            // currently the best way i've found to determine secondary aetherflail attack if first attack is a cone is to watch spawned npcs
            // these can appear few frames later...
            if (_second == Zone.UnknownCircle && _weaponsBall.Count + _weaponsChakram.Count > 0)
            {
                if (_weaponsBall.Count > 0 && _weaponsChakram.Count > 0)
                {
                    Service.Log($"[P1S] Failed to determine second aetherflail: there are {_weaponsBall.Count} balls and {_weaponsChakram.Count} chakrams");
                }
                else
                {
                    _second = _weaponsBall.Count > 0 ? Zone.Inner : Zone.Outer;
                }
            }
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_showFirst && IsInAOE(actor.Position, _module.PrimaryActor, _first))
                hints.Add("Hit by first flail!");
            if (_showSecond && IsInAOE(actor.Position, _module.PrimaryActor, _second))
                hints.Add("Hit by second flail!");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_showFirst)
                DrawZone(arena, _module.PrimaryActor, _first);
            if (_showSecond)
                DrawZone(arena, _module.PrimaryActor, _second);
        }

        public override void OnCastFinished(Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.GaolerFlailR1:
                case AID.GaolerFlailL1:
                case AID.GaolerFlailI1:
                case AID.GaolerFlailO1:
                    ++NumCasts;
                    _showFirst = false;
                    _showSecond = true;
                    break;
                case AID.GaolerFlailR2:
                case AID.GaolerFlailL2:
                case AID.GaolerFlailI2:
                case AID.GaolerFlailO2:
                    ++NumCasts;
                    _showSecond = false;
                    break;
            }
        }

        private static bool IsInAOE(Vector3 pos, Actor boss, Zone zone)
        {
            return zone switch
            {
                Zone.Left => !GeometryUtils.PointInCone(pos - boss.Position, boss.Rotation - MathF.PI / 2, _coneHalfAngle),
                Zone.Right => !GeometryUtils.PointInCone(pos - boss.Position, boss.Rotation + MathF.PI / 2, _coneHalfAngle),
                Zone.Inner => GeometryUtils.PointInCircle(pos - boss.Position, P1S.InnerCircleRadius),
                Zone.Outer => !GeometryUtils.PointInCircle(pos - boss.Position, P1S.InnerCircleRadius),
                _ => false
            };
        }

        private static void DrawZone(MiniArena arena, Actor boss, Zone zone)
        {
            switch (zone)
            {
                case Zone.Left:
                    arena.ZoneCone(boss.Position, 0, 100, boss.Rotation - MathF.PI / 2 + _coneHalfAngle, boss.Rotation + 3 * MathF.PI / 2 - _coneHalfAngle, arena.ColorAOE);
                    break;
                case Zone.Right:
                    arena.ZoneCone(boss.Position, 0, 100, boss.Rotation + MathF.PI / 2 - _coneHalfAngle, boss.Rotation - 3 * MathF.PI / 2 + _coneHalfAngle, arena.ColorAOE);
                    break;
                case Zone.Inner:
                    arena.ZoneCircle(boss.Position, P1S.InnerCircleRadius, arena.ColorAOE);
                    break;
                case Zone.Outer:
                    arena.ZoneCone(boss.Position, P1S.InnerCircleRadius, 100, 0, 2 * MathF.PI, arena.ColorAOE);
                    break;
            }
        }
    }
}
