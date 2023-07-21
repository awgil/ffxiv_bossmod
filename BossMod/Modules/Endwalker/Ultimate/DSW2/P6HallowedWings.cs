using BossMod.RealmReborn.Dungeon.D13CastrumMeridianum.D132MagitekVanguardF1;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P6HallowedWings : Components.GenericAOEs
    {
        private AOEInstance? _aoe;

        private static AOEShapeRect _shape = new(50, 11);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var offset = (AID)spell.Action.ID switch
            {
                AID.HallowedWingsLN or AID.HallowedWingsLF => _shape.HalfWidth,
                AID.HallowedWingsRN or AID.HallowedWingsRF => -_shape.HalfWidth,
                _ => 0
            };
            if (offset != 0)
                _aoe = new(_shape, caster.Position + offset * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, spell.FinishAt.AddSeconds(1.2f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HallowedWingsAOELeft or AID.HallowedWingsAOERight)
            {
                ++NumCasts;
                _aoe = null;
            }
        }
    }

    // TODO: show hints earlier (when activated at nidhogg pos)
    class P6CauterizeN : Components.SelfTargetedAOEs
    {
        public P6CauterizeN() : base(ActionID.MakeSpell(AID.CauterizeN), new AOEShapeRect(80, 11)) { }
    }

    class P6HallowedPlume : Components.GenericBaitAway
    {
        private Actor? _caster;
        private bool _far;

        private static AOEShapeCircle _shape = new(10);

        public P6HallowedPlume() : base(ActionID.MakeSpell(AID.HallowedPlume), centerAtTarget: true) { }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (_caster != null)
            {
                var players = module.Raid.WithoutSlot().SortedByRange(_caster.Position);
                var targets = _far ? players.TakeLast(2) : players.Take(2);
                foreach (var t in targets)
                    CurrentBaits.Add(new(_caster, t, _shape));
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_caster != null)
                hints.Add($"Tankbuster {(_far ? "far" : "near")}");
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            bool? far = (AID)spell.Action.ID switch
            {
                AID.HallowedWingsLN or AID.HallowedWingsRN => false,
                AID.HallowedWingsLF or AID.HallowedWingsRF => true,
                _ => null
            };
            if (far != null)
            {
                ForbiddenPlayers = module.Raid.WithSlot().WhereActor(p => p.Role != Role.Tank).Mask();
                _caster = caster;
                _far = far.Value;
            }
        }
    }
}
