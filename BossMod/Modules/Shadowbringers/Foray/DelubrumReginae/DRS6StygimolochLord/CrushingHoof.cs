using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6StygimolochLord
{
    class CrushingHoof : Components.GenericAOEs
    {
        private AOEInstance? _aoe;

        public CrushingHoof() : base(ActionID.MakeSpell(AID.CrushingHoofAOE)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.CrushingHoof)
                _aoe = new(new AOEShapeCircle(25), spell.LocXZ, activation: spell.NPCFinishAt.AddSeconds(1));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
                _aoe = null;
        }
    }
}
