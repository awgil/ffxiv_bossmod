using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // second part of the mechanic - icons spread / rest stack, voidzones, charge to tethers, towers
    class P2StrengthOfTheWard2 : BossComponent
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
        private static AOEShapeCircle _voidzoneAOE = new(9);

        public override void Init(BossModule module)
        {
            _chargeSources.AddRange(module.Enemies(OID.SerAdelphel));
            _chargeSources.AddRange(module.Enemies(OID.SerJanlenoux));

            WDir offset = new();
            foreach (var s in _chargeSources)
                offset += s.Position - module.Bounds.Center;
            _dirToBoss = Angle.FromDirection(offset) + 180.Degrees();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_voidzones.Any(v => _voidzoneAOE.Check(actor.Position, v.CastInfo!.LocXZ)))
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
                hints.Add("Stack", module.Raid.WithSlot().ExcludedFromMask(_playersWithIcons).OutOfRadius(actor.Position, _rageRadius).Any(a => !isTank || a.Item2.Role != Role.Tank));
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
                _voidzoneAOE.Draw(arena, v.CastInfo!.LocXZ);
            }

            foreach (var source in _chargeSources)
            {
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target != null)
                {
                    arena.ZoneRect(source.Position, target.Position, _chargeHalfWidth, ArenaColor.AOE);
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            bool pcIsLeapTarget = _playersWithIcons[pcSlot];
            foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
            {
                arena.Actor(player, pcIsLeapTarget ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
            }

            if (pcIsLeapTarget && !LeapsDone)
            {
                DrawSafeSpot(arena, _dirToBoss + 90.Degrees());
                DrawSafeSpot(arena, _dirToBoss + 180.Degrees());
                DrawSafeSpot(arena, _dirToBoss - 90.Degrees());
            }
            if (!pcIsLeapTarget && !RageDone)
            {
                DrawSafeSpot(arena, _dirToBoss);
                // draw stack radius around player
                arena.AddCircle(pc.Position, _rageRadius, ArenaColor.Safe);
            }

            if (!LeapsDone)
            {
                foreach (var (_, player) in module.Raid.WithSlot().IncludedInMask(_playersWithIcons))
                {
                    arena.AddCircle(player.Position, _leapRadius, ArenaColor.Danger);
                }
            }

            // draw tethers
            foreach (var source in _chargeSources)
            {
                module.Arena.Actor(source, ArenaColor.Danger, true);
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target != null)
                    module.Arena.AddLine(source.Position, target.Position, ArenaColor.Danger);
            }

            if (LeapsDone && ChargeDone)
            {
                foreach (var t in _towers)
                {
                    arena.AddCircle(t.CastInfo!.LocXZ, _towerRadius, ArenaColor.Safe);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DimensionalCollapseAOE:
                    _voidzones.Add(caster);
                    break;
                case AID.Conviction1AOE:
                    _towers.Add(caster);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DimensionalCollapseAOE:
                    _voidzones.Remove(caster);
                    break;
                case AID.Conviction1AOE:
                    _towers.Remove(caster);
                    TowersDone = true;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.SkywardLeap:
                    LeapsDone = true;
                    break;
                case AID.HolyShieldBash:
                    _chargeSources.Remove(caster);
                    ChargeDone = true;
                    break;
                case AID.DragonsRageAOE:
                    RageDone = true;
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if ((IconID)iconID == IconID.SkywardLeap)
            {
                _playersWithIcons.Set(module.Raid.FindSlot(actor.InstanceID));
            }
        }

        private bool ChargeHitsNonTanks(BossModule module, Actor source, Actor target)
        {
            var dir = target.Position - source.Position;
            var len = dir.Length();
            dir /= len;
            return module.Raid.WithoutSlot().Any(p => p.Role != Role.Tank && p.Position.InRect(source.Position, dir, len, 0, _chargeHalfWidth));
        }

        private bool IsInChargeAOE(BossModule module, Actor player)
        {
            foreach (var source in _chargeSources)
            {
                var target = module.WorldState.Actors.Find(source.Tether.Target);
                if (target != null && player.Position.InRect(source.Position, target.Position - source.Position, _chargeHalfWidth))
                    return true;
            }
            return false;
        }

        private void DrawSafeSpot(MiniArena arena, Angle dir)
        {
            arena.AddCircle(arena.Bounds.Center + 20 * dir.ToDirection(), 2, ArenaColor.Safe);
        }
    }
}
