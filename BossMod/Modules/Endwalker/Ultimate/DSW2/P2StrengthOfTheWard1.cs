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
    class P2StrengthOfTheWard1HeavyImpact : HeavyImpact
    {
        public P2StrengthOfTheWard1HeavyImpact() : base(8.2f) { }
    }
}
