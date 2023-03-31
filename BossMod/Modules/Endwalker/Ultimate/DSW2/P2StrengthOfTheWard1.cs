using System.Collections.Generic;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // spreads
    class P2StrengthOfTheWard1LightningStorm : Components.UniformStackSpread
    {
        public P2StrengthOfTheWard1LightningStorm() : base(0, 5) { }

        public override void Init(BossModule module)
        {
            AddSpreads(module.Raid.WithoutSlot(true));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.LightningStormAOE)
                Spreads.Clear();
        }
    }

    // charges
    class P2StrengthOfTheWard1SpiralThrust : Components.GenericAOEs
    {
        private List<Actor> _knights = new();

        private static AOEShapeRect _shape = new(52, 8);

        public P2StrengthOfTheWard1SpiralThrust() : base(ActionID.MakeSpell(AID.SpiralThrust), "GTFO from charge aoe!") { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var k in _knights)
                yield return new(_shape, k.Position, k.Rotation); // TODO: activation
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id == 0x1E43 && (OID)actor.OID is OID.SerVellguine or OID.SerPaulecrain or OID.SerIgnasse)
                _knights.Add(actor);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction)
            {
                _knights.Remove(caster);
                ++NumCasts;
            }
        }
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

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id == 0x1E43 && (OID)actor.OID == OID.SerGuerrique)
            {
                _aoe = new(new AOEShapeCircle(_impactRadiusIncrement), actor.Position, default, module.WorldState.CurrentTime.AddSeconds(8.2f));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HeavyImpactHit1 or AID.HeavyImpactHit2 or AID.HeavyImpactHit3 or AID.HeavyImpactHit4 or AID.HeavyImpactHit5)
            {
                if (++NumCasts < 5)
                {
                    var inner = _impactRadiusIncrement * NumCasts;
                    _aoe = new(new AOEShapeDonut(inner, inner + _impactRadiusIncrement), caster.Position, default, module.WorldState.CurrentTime.AddSeconds(1.9f));
                }
                else
                {
                    _aoe = null;
                }
            }
        }
    }
}
