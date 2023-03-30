namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1ProteanWaveTornado : Components.GenericBaitAway
    {
        private static AOEShapeCone _shape = new(40, 15.Degrees());

        public P1ProteanWaveTornado(bool enableHints) : base(ActionID.MakeSpell(AID.ProteanWaveTornadoInvis))
        {
            EnableHints = enableHints;
        }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            foreach (var tornado in module.Enemies(OID.LiquidRage))
            {
                var target = module.Raid.WithoutSlot().Closest(tornado.Position);
                if (target != null)
                    CurrentBaits.Add(new(tornado, target, _shape));
            }
        }
    }

    class P1ProteanWaveTornadoVisCast : Components.SelfTargetedAOEs
    {
        public P1ProteanWaveTornadoVisCast() : base(ActionID.MakeSpell(AID.ProteanWaveTornadoVis), new AOEShapeCone(40, 15.Degrees())) { }
    }

    class P1ProteanWaveTornadoVisBait : P1ProteanWaveTornado
    {
        public P1ProteanWaveTornadoVisBait() : base(false) { }
    }

    class P1ProteanWaveTornadoInvis : P1ProteanWaveTornado
    {
        public P1ProteanWaveTornadoInvis() : base(true) { }
    }
}
