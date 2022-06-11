using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // second part of the mechanic - icons spread / rest stack, voidzones, charge to tethers, towers
    class P2StrengthOfTheWard2 : BossModule.Component
    {
        public bool LeapsDone { get; private set; }
        public bool ChargeDone { get; private set; }
        public bool RageDone { get; private set; }
        public bool TowersDone { get; private set; }
        private BitMask _playersWithIcons;
        private Angle _dirToBoss;
        private List<Actor> _chargeSources = new();
        private List<Actor> _voidzones = new();
        private List<Actor> _towers = new();

        private static float _leapRadius = 24;
        private static float _chargeHalfWidth = 4;
        private static float _rageRadius = 8;
        private static float _towerRadius = 3;
        private static AOEShapeCircle _voidzoneAOE = new(10);

        public override void Init(BossModule module)
        {
            _chargeSources.AddRange(module.Enemies(OID.SerAdelphel));
            _chargeSources.AddRange(module.Enemies(OID.SerJanlenoux));

            Vector3 offset = new();
            foreach (var s in _chargeSources)
                offset += s.Position - module.Arena.WorldCenter;
            _dirToBoss = Angle.FromDirection(offset) + Angle.Radians(MathF.PI);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_voidzones.Any(v => _voidzoneAOE.Check(actor.Position, v.CastInfo!.Location, new())))
                hints.Add($"GTFO from voidzone aoe!");

            bool isTank = actor.Role == Role.Tank;
            bool isLeapTarget = _playersWithIcons[slot];
            if (!LeapsDone)
            {
                if (isLeapTarget && module.Raid.WithoutSlot().InRadiusExcluding(actor, _leapRadius).Any())
                    hints.Add("GTFO from others!");
                if (!isLeapTarget && module.Raid.WithSlot().IncludedInMask(_playersWithIcons).InRadius(actor.Position, _leapRadius).Any())
                    hints.Add("GTFO from marked player!");
            }

            if (!RageDone && !isLeapTarget)
            {
                hints.Add("Stack", module.Raid.WithSlot().ExcludedFromMask(_playersWithIcons).OutOfRadius(actor.Position, _rageRadius).Any());
            }

            if (LeapsDone && RageDone && _towers.Count > 0 && !isTank)
            {
                hints.Add("Soak tower", !_towers.InRadius(actor.Position, _towerRadius).Any());
            }

            if (!ChargeDone)
            {
                var tetherSource = _chargeSources.Find(s => s.Tether.Target == actor.InstanceID);
                if (isTank)
                {
                    if (tetherSource == null)
                        hints.Add("Grab tether!");
                    else if (ChargeHitsNonTanks(module, tetherSource, actor))
                        hints.Add("Move away from raid!");
                }
                else
                {
                    if (tetherSource != null)
                        hints.Add("Pass tether!");
                    else if (IsInChargeAOE(module, actor))
                        hints.Add("GTFO from tanks!");
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var v in _voidzones)
            {
                _voidzoneAOE.Draw(arena, v.CastInfo!.Location, new());
            }

            foreach (var source in _chargeSources)
            {
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target != null)
                {
                    var dir = target.Position - source.Position;
                    var len = dir.Length();
                    dir /= len;
                    arena.ZoneQuad(source.Position, dir, len, 0, _chargeHalfWidth, arena.ColorAOE);
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            bool pcIsLeapTarget = _playersWithIcons[pcSlot];
            foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
            {
                arena.Actor(player, pcIsLeapTarget ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }

            if (pcIsLeapTarget && !LeapsDone)
            {
                DrawSafeSpot(arena, _dirToBoss + Angle.Radians(MathF.PI / 2));
                DrawSafeSpot(arena, _dirToBoss + Angle.Radians(MathF.PI));
                DrawSafeSpot(arena, _dirToBoss - Angle.Radians(MathF.PI / 2));
            }
            if (!pcIsLeapTarget && !RageDone)
            {
                DrawSafeSpot(arena, _dirToBoss);
                // draw stack radius around player
                arena.AddCircle(pc.Position, _rageRadius, arena.ColorSafe);
            }

            if (!LeapsDone)
            {
                foreach (var (_, player) in module.Raid.WithSlot().IncludedInMask(_playersWithIcons))
                {
                    arena.AddCircle(player.Position, _leapRadius, arena.ColorDanger);
                }
            }

            // draw tethers
            foreach (var source in _chargeSources)
            {
                module.Arena.Actor(source, module.Arena.ColorDanger);
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target != null)
                    module.Arena.AddLine(source.Position, target.Position, module.Arena.ColorDanger);
            }

            if (LeapsDone && ChargeDone)
            {
                foreach (var t in _towers)
                {
                    arena.AddCircle(t.CastInfo!.Location, _towerRadius, arena.ColorSafe);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.DimensionalCollapseAOE:
                    _voidzones.Add(actor);
                    break;
                case AID.Conviction1AOE:
                    _towers.Add(actor);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.DimensionalCollapseAOE:
                    _voidzones.Remove(actor);
                    break;
                case AID.Conviction1AOE:
                    _towers.Remove(actor);
                    TowersDone = true;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (!info.IsSpell())
                return;
            switch ((AID)info.Action.ID)
            {
                case AID.SkywardLeap:
                    LeapsDone = true;
                    break;
                case AID.HolyShieldBash:
                    _chargeSources.RemoveAll(a => a.InstanceID == info.CasterID);
                    ChargeDone = true;
                    break;
                case AID.DragonsRageAOE:
                    RageDone = true;
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            if ((IconID)iconID == IconID.SkywardLeap)
            {
                _playersWithIcons.Set(module.Raid.FindSlot(actorID));
            }
        }

        private bool ChargeHitsNonTanks(BossModule module, Actor source, Actor target)
        {
            var dir = target.Position - source.Position;
            var len = dir.Length();
            dir /= len;
            return module.Raid.WithoutSlot().Any(p => p.Role != Role.Tank && GeometryUtils.PointInRect(p.Position - source.Position, dir, len, 0, _chargeHalfWidth));
        }

        private bool IsInChargeAOE(BossModule module, Actor player)
        {
            foreach (var source in _chargeSources)
            {
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target == null)
                    continue;
                var dir = target.Position - source.Position;
                var len = dir.Length();
                dir /= len;
                if (GeometryUtils.PointInRect(player.Position - source.Position, dir, len, 0, _chargeHalfWidth))
                    return true;
            }
            return false;
        }

        private void DrawSafeSpot(MiniArena arena, Angle dir)
        {
            arena.AddCircle(arena.WorldCenter + 20 * dir.ToDirection(), 2, arena.ColorSafe);
        }
    }
}
