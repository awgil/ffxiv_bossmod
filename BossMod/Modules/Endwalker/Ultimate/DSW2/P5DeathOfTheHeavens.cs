namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P5DeathOfTheHeavensHeavyImpact : HeavyImpact
    {
        public P5DeathOfTheHeavensHeavyImpact() : base(10.5f) { }
    }

    class P5DeathOfTheHeavensGaze : DragonsGaze
    {
        public P5DeathOfTheHeavensGaze() : base(OID.BossP5) { }
    }

    // TODO: make more meaningful somehow
    class P5DeathOfTheHeavensDooms : BossComponent
    {
        public BitMask Dooms;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Dooms[slot])
                hints.Add("Doom", false);
        }

        // note: we could also use status, but it appears slightly later
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.Deathstorm)
                foreach (var t in spell.Targets)
                    Dooms.Set(module.Raid.FindSlot(t.ID));
        }
    }

    class P5DeathOfTheHeavensLightningStorm : Components.UniformStackSpread
    {
        public P5DeathOfTheHeavensLightningStorm() : base(0, 5) { }

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
}
