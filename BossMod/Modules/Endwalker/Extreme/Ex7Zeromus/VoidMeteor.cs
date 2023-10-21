using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex7Zeromus
{
    class MeteorImpactProximity : Components.SelfTargetedAOEs
    {
        public MeteorImpactProximity() : base(ActionID.MakeSpell(AID.MeteorImpactProximity), new AOEShapeCircle(10)) { } // TODO: verify falloff
    }

    class MeteorImpactCharge : BossComponent
    {
        struct PlayerState
        {
            public Actor? TetherSource;
            public int Order;
            public bool Stretched;
            public bool NonClipping;
            public List<(WPos, WPos, WPos)>? DangerZone;
        }

        public int NumCasts { get; private set; }
        private int _numTethers;
        private List<WPos> _meteors = new();
        private PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];

        private static float _radius = 2;
        private static int _ownThickness = 2;
        private static int _otherThickness = 1;
        private static bool _drawShadows = true;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (SourceIfActive(slot) != null)
            {
                if (!_playerStates[slot].NonClipping)
                    hints.Add("Avoid other meteors!");
                if (!_playerStates[slot].Stretched)
                    hints.Add("Stretch the tether!");
            }

            if (IsClippedByOthers(module, actor))
                hints.Add("GTFO from charges!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return SourceIfActive(playerSlot) != null ? PlayerPriority.Interesting : PlayerPriority.Normal;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_drawShadows && SourceIfActive(pcSlot) is var source && source != null)
            {
                ref var state = ref _playerStates.AsSpan()[pcSlot];
                state.DangerZone ??= arena.Bounds.ClipAndTriangulate(new Clip2D().Union(_meteors.Select(m => BuildShadowPolygon(source.Position, m))));
                arena.Zone(state.DangerZone, ArenaColor.AOE);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var m in _meteors)
                arena.AddCircle(m, _radius, ArenaColor.Object);

            foreach (var (slot, target) in module.Raid.WithSlot(true))
            {
                if (SourceIfActive(slot) is var source && source != null)
                {
                    var thickness = slot == pcSlot ? _ownThickness : _otherThickness;
                    if (thickness > 0)
                    {
                        var norm = (target.Position - source.Position).Normalized().OrthoL() * 2;
                        var rot = Angle.FromDirection(target.Position - source.Position);
                        arena.PathArcTo(target.Position, 2, (rot + 90.Degrees()).Rad, (rot - 90.Degrees()).Rad);
                        arena.PathLineTo(source.Position - norm);
                        arena.PathLineTo(source.Position + norm);
                        arena.PathStroke(true, _playerStates[slot].NonClipping ? ArenaColor.Safe : ArenaColor.Danger, thickness);
                        arena.AddLine(source.Position, target.Position, _playerStates[slot].Stretched ? ArenaColor.Safe : ArenaColor.Danger, thickness);
                    }
                }
            }

            // circle showing approximate min stretch distance; for second order, we might be forced to drop meteor there and die to avoid wipe
            if (SourceIfActive(pcSlot) is var pcSource && pcSource != null)
                arena.AddCircle(pcSource.Position, 26, ArenaColor.Danger);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.MeteorImpactProximity:
                    _meteors.Add(caster.Position);
                    break;
                case AID.MeteorImpactChargeNormal:
                case AID.MeteorImpactChargeClipping:
                    _meteors.Add(spell.TargetXZ);
                    ++NumCasts;
                    var (closestSlot, closestPlayer) = module.Raid.WithSlot(true).Closest(spell.TargetXZ);
                    if (closestPlayer != null)
                        _playerStates[closestSlot].TetherSource = null;
                    break;
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if ((TetherID)tether.ID is TetherID.VoidMeteorCloseClipping or TetherID.VoidMeteorCloseGood or TetherID.VoidMeteorStretchedClipping or TetherID.VoidMeteorStretchedGood && module.Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            {
                if (_playerStates[slot].TetherSource == null)
                    _playerStates[slot].Order = _numTethers++;
                _playerStates[slot].TetherSource = source;
                _playerStates[slot].Stretched = (TetherID)source.Tether.ID is TetherID.VoidMeteorStretchedClipping or TetherID.VoidMeteorStretchedGood;
                _playerStates[slot].NonClipping = (TetherID)source.Tether.ID is TetherID.VoidMeteorCloseGood or TetherID.VoidMeteorStretchedGood;
            }
        }

        private Actor? SourceIfActive(int slot) => NumCasts switch
        {
            < 4 => _playerStates[slot].Order < 4 ? _playerStates[slot].TetherSource : null,
            < 8 => _playerStates[slot].Order < 8 ? _playerStates[slot].TetherSource : null,
            _ => null
        };

        private IEnumerable<WPos> BuildShadowPolygon(WPos source, WPos meteor)
        {
            var toMeteor = meteor - source;
            var dirToMeteor = Angle.FromDirection(toMeteor);
            var halfAngle = Angle.Asin(_radius * 2 / toMeteor.Length());
            // intersection point is at dirToMeteor -+ halfAngle relative to source; relative to meteor, it is (dirToMeteor + 180) +- (90 - halfAngle)
            var dirFromMeteor = dirToMeteor + 180.Degrees();
            var halfAngleFromMeteor = 90.Degrees() - halfAngle;
            foreach (var p in CurveApprox.CircleArc(meteor, _radius * 2, dirFromMeteor + halfAngleFromMeteor, dirFromMeteor - halfAngleFromMeteor, 0.2f))
                yield return p;
            yield return source + 100 * (dirToMeteor + halfAngle).ToDirection();
            yield return source + 100 * (dirToMeteor - halfAngle).ToDirection();
        }

        private bool IsClipped(WPos source, WPos target, WPos position) => position.InCircle(target, _radius) || position.InRect(source, target - source, _radius);

        private bool IsClippedByOthers(BossModule module, Actor player)
        {
            foreach (var (i, p) in module.Raid.WithSlot(true).Exclude(player))
                if (SourceIfActive(i) is var src && src != null && IsClipped(src.Position, p.Position, player.Position))
                    return true;
            return false;
        }
    }

    class MeteorImpactExplosion : Components.SelfTargetedAOEs
    {
        public MeteorImpactExplosion() : base(ActionID.MakeSpell(AID.MeteorImpactExplosion), new AOEShapeCircle(10)) { }
    }
}
