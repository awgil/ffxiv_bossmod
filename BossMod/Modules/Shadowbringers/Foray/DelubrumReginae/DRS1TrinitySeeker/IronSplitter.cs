using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker
{
    class IronSplitter : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        public IronSplitter() : base(ActionID.MakeSpell(AID.IronSplitter)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                var distance = (caster.Position - module.Bounds.Center).Length();
                if (distance is <3 or >9 and <11 or >17 and <19) // tiles
                {
                    _aoes.Add(new(new AOEShapeCircle(4), module.Bounds.Center, new(), spell.FinishAt));
                    _aoes.Add(new(new AOEShapeDonut(8, 12), module.Bounds.Center, new(), spell.FinishAt));
                    _aoes.Add(new(new AOEShapeDonut(16, 20), module.Bounds.Center, new(), spell.FinishAt));
                }
                else
                {
                    _aoes.Add(new(new AOEShapeDonut(4, 8), module.Bounds.Center, new(), spell.FinishAt));
                    _aoes.Add(new(new AOEShapeDonut(12, 16), module.Bounds.Center, new(), spell.FinishAt));
                    _aoes.Add(new(new AOEShapeDonut(20, 25), module.Bounds.Center, new(), spell.FinishAt));
                }
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _aoes.Clear();
            }
        }
    }
}
