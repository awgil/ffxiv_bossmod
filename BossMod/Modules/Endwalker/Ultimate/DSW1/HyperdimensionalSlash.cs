using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    class HyperdimensionalSlash : BossModule.Component
    {
        public int NumCasts { get; private set; }
        private List<Actor> _laserTargets = new();
        private Angle _coneDir;
        private List<(Vector3 Pos, Actor? Source)> _tears = new();
        private BitMask _riskyTears;

        private static float _linkRadius = 9; // TODO: verify
        private static AOEShapeRect _aoeLaser = new(70, 4);
        private static AOEShapeCone _aoeCone = new(40, Angle.Radians(MathF.PI / 3));

        public override void Update(BossModule module)
        {
            _tears.Clear();
            foreach (var tear in module.Enemies(OID.AetherialTear))
                _tears.Add((tear.Position, null));
            foreach (var target in _laserTargets)
                _tears.Add((TearPosition(module, target), target));

            _riskyTears.Reset();
            for (int i = 0; i < _tears.Count; ++i)
            {
                for (int j = i + 1; j < _tears.Count; ++j)
                {
                    if (GeometryUtils.PointInCircle(_tears[i].Pos - _tears[j].Pos, _linkRadius))
                    {
                        _riskyTears.Set(i);
                        _riskyTears.Set(j);
                    }
                }
            }

            var coneTarget = module.Raid.WithoutSlot().MinBy(a => (a.Position - module.Arena.WorldCenter).LengthSquared());
            _coneDir = coneTarget != null ? Angle.FromDirection(coneTarget.Position - module.Arena.WorldCenter) : new();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_laserTargets.Count == 0)
                return;

            int tearIndex = _tears.FindIndex(t => t.Source == actor);
            if (tearIndex >= 0)
            {
                // make sure actor's tear placement is good
                if (_riskyTears[tearIndex])
                    hints.Add("Aim away from other tears!");
                if (GeometryUtils.PointInCircle(actor.Position - _tears[tearIndex].Pos, _linkRadius))
                    hints.Add("Stay closer to center!");
            }

            // make sure actor is not clipped by any lasers
            if (_laserTargets.Exclude(actor).Any(target => _aoeLaser.Check(actor.Position, module.Arena.WorldCenter, Angle.FromDirection(target.Position - module.Arena.WorldCenter))))
                hints.Add("GTFO from laser aoe!");

            // make sure actor is either not hit by cone (if is target of a laser) or is hit by a cone (otherwise)
            bool hitByCone = _aoeCone.Check(actor.Position, module.Arena.WorldCenter, _coneDir);
            if (tearIndex >= 0 && hitByCone)
                hints.Add("GTFO from cone aoe!");
            else if (tearIndex < 0 && !hitByCone)
                hints.Add("Stack with others!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_laserTargets.Count == 0)
                return;

            foreach (var t in _laserTargets)
                _aoeLaser.Draw(arena, arena.WorldCenter, Angle.FromDirection(t.Position - arena.WorldCenter));
            _aoeCone.Draw(arena, arena.WorldCenter, _coneDir);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            for (int i = 0; i < _tears.Count; ++i)
                arena.AddCircle(_tears[i].Pos, _linkRadius, _riskyTears[i] ? arena.ColorDanger : arena.ColorSafe);

            if (_laserTargets.Contains(pc))
                arena.AddLine(arena.WorldCenter, TearPosition(module, pc), arena.ColorDanger);
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.HyperdimensionalSlashAOERest))
            {
                _laserTargets.Clear();
                ++NumCasts;
            }
        }

        public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            if ((IconID)iconID == IconID.HyperdimensionalSlash)
            {
                var actor = module.WorldState.Actors.Find(actorID);
                if (actor != null)
                    _laserTargets.Add(actor);
            }
        }

        private Vector3 TearPosition(BossModule module, Actor target)
        {
            return module.Arena.ClampToBounds(module.Arena.WorldCenter + 50 * Vector3.Normalize(target.Position - module.Arena.WorldCenter));
        }
    }
}
