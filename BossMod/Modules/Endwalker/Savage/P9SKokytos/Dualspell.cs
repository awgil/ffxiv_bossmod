using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P9SKokytos
{
    class DualspellFire : Components.GenericStackSpread
    {
        private bool _active;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_active)
                hints.Add("Pairs");
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.DualspellIceFire or AID.TwoMindsIceFire)
                _active = true;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            var radius = (AID)spell.Action.ID switch
            {
                AID.DualspellVisualIce => 6,
                AID.DualspellVisualFire => 12,
                _ => 0
            };
            if (_active && radius != 0)
            {
                // assume always targets supports
                foreach (var p in module.Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()))
                    Stacks.Add(new(p, radius, 2, 2, module.WorldState.CurrentTime.AddSeconds(4.5f)));
            }
        }
    }

    class DualspellLightning : Components.GenericBaitAway
    {
        private bool _active;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_active)
                hints.Add("Spread");
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.DualspellIceLightning or AID.TwoMindsIceLightning)
                _active = true;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            var halfWidth = (AID)spell.Action.ID switch
            {
                AID.DualspellVisualIce => 4,
                AID.DualspellVisualLightning => 8,
                _ => 0
            };
            if (_active && halfWidth != 0)
            {
                var shape = new AOEShapeRect(40, halfWidth);
                foreach (var p in module.Raid.WithoutSlot(true))
                    CurrentBaits.Add(new(module.PrimaryActor, p, shape));
            }
        }
    }

    class DualspellIce : Components.GenericAOEs
    {
        public enum Mechanic { None, In, Out }

        private Mechanic _curMechanic;
        private AOEInstance? _aoe;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_curMechanic != Mechanic.None)
                hints.Add(_curMechanic.ToString());
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DualspellVisualIce:
                    SetMechanic(module, Mechanic.In);
                    break;
                case AID.DualspellVisualFire:
                case AID.DualspellVisualLightning:
                    SetMechanic(module, Mechanic.Out);
                    break;
                case AID.DualspellBlizzardOut:
                case AID.DualspellBlizzardIn:
                    ++NumCasts;
                    break;
            }
        }

        private void SetMechanic(BossModule module, Mechanic mechanic)
        {
            _curMechanic = mechanic;
            _aoe = new(new AOEShapeDonut(mechanic == Mechanic.In ? 8 : 14, 40), module.PrimaryActor.Position, default, module.WorldState.CurrentTime.AddSeconds(4.5f));
        }
    }
}
