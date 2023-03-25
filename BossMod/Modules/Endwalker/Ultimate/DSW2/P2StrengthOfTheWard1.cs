using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // spreads
    class P2StrengthOfTheWard1LightningStorm : Components.StackSpread
    {
        public P2StrengthOfTheWard1LightningStorm() : base(0, 5) { }

        public override void Init(BossModule module)
        {
            SpreadTargets.AddRange(module.Raid.WithoutSlot(true));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.LightningStormAOE)
                SpreadTargets.Clear();
        }
    }

    // charges
    class P2StrengthOfTheWard1SpiralThrust : Components.GenericAOEs
    {
        private List<Actor> _knights = new();

        private static AOEShapeRect _shape = new(52, 8);

        public P2StrengthOfTheWard1SpiralThrust() : base(ActionID.MakeSpell(AID.SpiralThrust), "GTFO from charge aoe!") { }

        public override void Init(BossModule module)
        {
            _knights.AddRange(module.Enemies(OID.SerVellguine));
            _knights.AddRange(module.Enemies(OID.SerPaulecrain));
            _knights.AddRange(module.Enemies(OID.SerIgnasse));
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (NumCasts == 0)
                foreach (var k in _knights.Where(k => IsKnightInChargePosition(module, k)))
                    yield return new(_shape, k.Position, k.Rotation); // TODO: activation
        }

        private bool IsKnightInChargePosition(BossModule module, Actor knight) => Utils.AlmostEqual((knight.Position - module.Bounds.Center).LengthSq(), 23 * 23, 5);
    }

    // rings
    class P2StrengthOfTheWard1HeavyImpact : Components.GenericAOEs
    {
        private static float _impactRadiusIncrement = 6;

        private AOEInstance? _aoe;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void Init(BossModule module)
        {
            var source = module.Enemies(OID.SerGuerrique).FirstOrDefault();
            if (source != null)
                _aoe = new(new AOEShapeCircle(_impactRadiusIncrement), source.Position); // TODO: activation
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HeavyImpactHit1 or AID.HeavyImpactHit2 or AID.HeavyImpactHit3 or AID.HeavyImpactHit4 or AID.HeavyImpactHit5)
            {
                if (++NumCasts < 5)
                {
                    var inner = _impactRadiusIncrement * NumCasts;
                    _aoe = new(new AOEShapeDonut(inner, inner + _impactRadiusIncrement), caster.Position); // TODO: activation
                }
                else
                {
                    _aoe = null;
                }
            }
        }
    }
}
