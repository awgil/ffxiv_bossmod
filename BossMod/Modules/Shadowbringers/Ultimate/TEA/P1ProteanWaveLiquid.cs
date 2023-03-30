using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1ProteanWaveLiquidVisBoss : Components.SelfTargetedAOEs
    {
        public P1ProteanWaveLiquidVisBoss() : base(ActionID.MakeSpell(AID.ProteanWaveLiquidVisBoss), new AOEShapeCone(40, 15.Degrees())) { }
    }

    class P1ProteanWaveLiquidVisHelper : Components.SelfTargetedAOEs
    {
        public P1ProteanWaveLiquidVisHelper() : base(ActionID.MakeSpell(AID.ProteanWaveLiquidVisHelper), new AOEShapeCone(40, 15.Degrees())) { }
    }

    // single protean ("shadow") that fires in the direction the boss is facing
    class P1ProteanWaveLiquidInvisFixed : Components.GenericAOEs
    {
        private Actor? _source;

        private static AOEShapeCone _shape = new(40, 15.Degrees());

        public P1ProteanWaveLiquidInvisFixed() : base(ActionID.MakeSpell(AID.ProteanWaveLiquidInvisBoss)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_source != null)
                yield return new(_shape, _source.Position, _source.Rotation);
        }

        public override void Init(BossModule module)
        {
            _source = module.Enemies(OID.BossP1).FirstOrDefault();
        }
    }

    // proteans baited on 4 closest targets
    class P1ProteanWaveLiquidInvisBaited : Components.GenericBaitAway
    {
        private Actor? _source;

        private static AOEShapeCone _shape = new(40, 15.Degrees());

        public P1ProteanWaveLiquidInvisBaited() : base(ActionID.MakeSpell(AID.ProteanWaveLiquidInvisHelper)) { }

        public override void Init(BossModule module)
        {
            _source = module.Enemies(OID.BossP1).FirstOrDefault();
        }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (_source != null)
                foreach (var target in module.Raid.WithoutSlot().SortedByRange(_source.Position).Take(4))
                    CurrentBaits.Add(new(_source, target, _shape));
        }
    }
}
