using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker
{
    class MercyFourfold : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();
        private List<AOEInstance?> _safezones = new();
        private static AOEShapeCone _shapeAOE = new(50, 90.Degrees());
        private static AOEShapeCone _shapeSafe = new(50, 45.Degrees());

        public MercyFourfold() : base(ActionID.MakeSpell(AID.MercyFourfoldAOE)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoes.Count > 0)
                yield return _aoes[0];
            if (_safezones.Count > 0 && _safezones[0] != null)
                yield return _safezones[0]!.Value;
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID != SID.Mercy)
                return;

            var dirOffset = status.Extra switch
            {
                0xF7 => -45.Degrees(),
                0xF8 => -135.Degrees(),
                0xF9 => 45.Degrees(),
                0xFA => 135.Degrees(),
                _ => 0.Degrees()
            };
            if (dirOffset == default)
                return;

            var dir = actor.Rotation + dirOffset;
            if (_aoes.Count > 0)
            {
                // see whether there is a safezone for two contiguous aoes
                var mid = dir.ToDirection() + _aoes.Last().Rotation.ToDirection(); // length should be either ~sqrt(2) or ~0
                if (mid.LengthSq() > 1)
                    _safezones.Add(new(_shapeSafe, actor.Position, Angle.FromDirection(-mid), new(), ArenaColor.SafeFromAOE, false));
                else
                    _safezones.Add(null);
            }

            var activationDelay = 15 - 1.3f * _aoes.Count;
            _aoes.Add(new(_shapeAOE, actor.Position, dir, module.WorldState.CurrentTime.AddSeconds(activationDelay)));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
            {
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                if (_safezones.Count > 0)
                    _safezones.RemoveAt(0);
            }
        }
    }
}
