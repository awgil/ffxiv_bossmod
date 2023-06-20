using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P12S1Athena
{
    // TODO: consider using envcontrols instead
    class UnnaturalEnchainment : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shape = new(5, 10, 5);

        public UnnaturalEnchainment() : base(ActionID.MakeSpell(AID.Sample)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.UnnaturalEnchainment)
                _aoes.Add(new(_shape, source.Position, default, module.WorldState.CurrentTime.AddSeconds(8.2f)));
        }
    }
}
