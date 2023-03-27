using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    // counts cone casts; do we also care about shockwaves?..
    // TODO: refactor - split into proteans & baited puddles
    class PureOfHeart : Components.CastCounter
    {
        private List<Actor> _skyblindCasters = new();
        private Actor? _boss;
        private BitMask _skyblindPlayers;
        private BitMask _coneTargets;

        private static AOEShapeCone _brightwingAOE = new(18, 15.Degrees()); // TODO: verify angle
        private static float _skyblindRadius = 3;

        public PureOfHeart() : base(ActionID.MakeSpell(AID.Brightwing)) { }

        public override void Init(BossModule module)
        {
            _boss = module.Enemies(OID.SerCharibert).FirstOrDefault();
        }

        public override void Update(BossModule module)
        {
            _coneTargets = _boss != null && NumCasts < 8 ? module.Raid.WithSlot().SortedByRange(_boss.Position).Take(2).Mask() : new();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_boss == null)
                return;

            if (_coneTargets[slot])
            {
                var dir = Angle.FromDirection(actor.Position - _boss.Position);
                if (module.Raid.WithoutSlot().Exclude(actor).Any(p => _brightwingAOE.Check(p.Position, _boss.Position, dir)))
                    hints.Add("Aim cone away from others!");
            }
            else
            {
                if (module.Raid.WithSlot().IncludedInMask(_coneTargets).Any(sa => _brightwingAOE.Check(actor.Position, _boss.Position, Angle.FromDirection(sa.Item2.Position - _boss.Position))))
                    hints.Add("GTFO from cone!");
            }

            if (_skyblindPlayers[slot] && module.Raid.WithSlot().ExcludedFromMask(_skyblindPlayers).InRadius(actor.Position, _skyblindRadius).Any())
                hints.Add("GTFO from raid!");

            if (_skyblindCasters.Any(c => actor.Position.InCircle(c.CastInfo!.LocXZ, _skyblindRadius)))
                hints.Add("GTFO from puddle!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _skyblindPlayers[playerSlot] ? PlayerPriority.Danger : _coneTargets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_boss != null)
            {
                foreach (var (_, p) in module.Raid.WithSlot().IncludedInMask(_coneTargets))
                {
                    _brightwingAOE.Draw(arena, _boss.Position, Angle.FromDirection(p.Position - _boss.Position));
                }
            }

            foreach (var c in _skyblindCasters)
            {
                arena.ZoneCircle(c.CastInfo!.LocXZ, _skyblindRadius, ArenaColor.AOE);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (_, player) in module.Raid.WithSlot().IncludedInMask(_skyblindPlayers))
            {
                arena.AddCircle(player.Position, _skyblindRadius, ArenaColor.Danger);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Skyblind)
            {
                _skyblindPlayers.Set(module.Raid.FindSlot(actor.InstanceID));
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Skyblind)
            {
                _skyblindPlayers.Clear(module.Raid.FindSlot(actor.InstanceID));
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Skyblind)
                _skyblindCasters.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Skyblind)
                _skyblindCasters.Remove(caster);
        }
    }
}
