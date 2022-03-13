using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.P1S
{
    using static BossModule;

    // state related to [aether]flails mechanics
    class Flails : Component
    {
        public enum Zone { Left, Right, Inner, Outer, UnknownCircle }

        public int NumCasts { get; private set; } = 0;
        private P1S _module;
        private List<Actor> _weaponsBall;
        private List<Actor> _weaponsChakram;
        private AOEShape? _first;
        private AOEShape? _second;
        private bool _detectSecond;
        private bool _showSecond;

        public Flails(P1S module, Zone first, Zone second)
        {
            _module = module;
            _weaponsBall = module.Enemies(OID.FlailI);
            _weaponsChakram = module.Enemies(OID.FlailO);
            _first = ShapeForZone(first);
            _second = ShapeForZone(second);
            _detectSecond = second == Zone.UnknownCircle;
            _showSecond = _first is AOEShapeCone != _second is AOEShapeCone;
        }

        public override void Update()
        {
            // currently the best way i've found to determine secondary aetherflail attack if first attack is a cone is to watch spawned npcs
            // these can appear few frames later...
            if (_detectSecond && _weaponsBall.Count + _weaponsChakram.Count > 0)
            {
                _detectSecond = false;
                if (_weaponsBall.Count > 0 && _weaponsChakram.Count > 0)
                {
                    Service.Log($"[P1S] Failed to determine second aetherflail: there are {_weaponsBall.Count} balls and {_weaponsChakram.Count} chakrams");
                }
                else
                {
                    _second = ShapeForZone(_weaponsBall.Count > 0 ? Zone.Inner : Zone.Outer);
                }
            }
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_first?.Check(actor.Position, _module.PrimaryActor) ?? false)
                hints.Add("Hit by first flail!");
            if (_showSecond && _second != null && _second.Check(actor.Position, _module.PrimaryActor))
                hints.Add("Hit by second flail!");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            _first?.Draw(arena, _module.PrimaryActor);
            if (_showSecond)
                _second?.Draw(arena, _module.PrimaryActor);
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
                    _first = null;
                    _showSecond = true;
                    break;
                case AID.GaolerFlailR2:
                case AID.GaolerFlailL2:
                case AID.GaolerFlailI2:
                case AID.GaolerFlailO2:
                    ++NumCasts;
                    _second = null;
                    break;
            }
        }

        private static AOEShape? ShapeForZone(Zone zone)
        {
            return zone switch
            {
                Zone.Left => new AOEShapeCone(60, 3 * MathF.PI / 4, MathF.PI / 2),
                Zone.Right => new AOEShapeCone(60, 3 * MathF.PI / 4, -MathF.PI / 2),
                Zone.Inner => new AOEShapeCircle(P1S.InnerCircleRadius),
                Zone.Outer => new AOEShapeDonut(P1S.InnerCircleRadius, 60),
                _ => null
            };
        }
    }
}
