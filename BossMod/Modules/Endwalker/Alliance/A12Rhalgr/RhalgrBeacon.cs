using System.Collections.Generic;

namespace BossMod.Endwalker.Alliance.A12Rhalgr
{
    class RhalgrBeaconAOE : Components.SelfTargetedAOEs
    {
        public RhalgrBeaconAOE() : base(ActionID.MakeSpell(AID.RhalgrsBeaconAOE), new AOEShapeCircle(10)) { }
    }

    class RhalgrBeaconShock : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCircle _shape = new(8);

        public RhalgrBeaconShock() : base(ActionID.MakeSpell(AID.Shock)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.LightningOrb)
                _aoes.Add(new(_shape, actor.Position, default, module.WorldState.CurrentTime.AddSeconds(13)));
        }
    }

    // TODO: this is a knockback 50, ignores immunities - but need to clamp to correct fingers
    // there are two possible source locations ([-10.12, 268.50] and [-24.12, 266.50]), two potential fingers for each - one of them is sometimes covered by lightning aoes
    class RhalgrBeaconKnockback : Components.CastCounter
    {
        public RhalgrBeaconKnockback() : base(ActionID.MakeSpell(AID.RhalgrsBeaconKnockback)) { }
    }
}
