using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P5WrathOfTheHeavensSkywardLeap : Components.UniformStackSpread
    {
        public P5WrathOfTheHeavensSkywardLeap() : base(0, 24, alwaysShowSpreads: true, raidwideOnResolve: false) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.SkywardLeapP5)
                AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(6.4f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SkywardLeapP5)
                Spreads.Clear();
        }
    }

    class P5WrathOfTheHeavensSpiralPierce : Components.BaitAwayTethers
    {
        public P5WrathOfTheHeavensSpiralPierce() : base(new AOEShapeRect(50, 8), (uint)TetherID.SpiralPierce, ActionID.MakeSpell(AID.SpiralPierce)) { }
    }

    class P5WrathOfTheHeavensTwistingDive : Components.SelfTargetedAOEs
    {
        public P5WrathOfTheHeavensTwistingDive() : base(ActionID.MakeSpell(AID.TwistingDive), new AOEShapeRect(60, 5)) { }
    }

    class P5WrathOfTheHeavensChainLightning : Components.UniformStackSpread
    {
        public BitMask Targets { get; private set; }

        public void ShowSpreads(BossModule module, float delay) => AddSpreads(module.Raid.WithSlot(true).IncludedInMask(Targets).Actors(), module.WorldState.CurrentTime.AddSeconds(delay));

        public P5WrathOfTheHeavensChainLightning() : base(0, 5) { }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return Targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        // note: this happens about a second before statuses appear
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ChainLightning:
                    foreach (var t in spell.Targets)
                        Targets.Set(module.Raid.FindSlot(t.ID));
                    break;
                case AID.ChainLightningAOE:
                    Spreads.Clear();
                    break;
            }
        }
    }

    class P5WrathOfTheHeavensTwister : Components.GenericAOEs
    {
        private List<WPos> _predicted = new();
        private List<Actor> _voidzones = new();

        private static AOEShapeCircle _shape = new(2); // TODO: verify radius

        public bool Active => _voidzones.Count > 0;

        public P5WrathOfTheHeavensTwister() : base(default, "GTFO from twister!") { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var p in _predicted)
                yield return new(_shape, p); // TODO: activation
            foreach (var p in _voidzones)
                yield return new(_shape, p.Position);
        }

        public override void Init(BossModule module)
        {
            _predicted.AddRange(module.Raid.WithoutSlot().Select(a => a.Position));
            _voidzones = module.Enemies(OID.VoidzoneTwister);
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.VoidzoneTwister)
                _predicted.Clear();
        }
    }

    class P5WrathOfTheHeavensCauterizeBait : Components.GenericBaitAway
    {
        private static AOEShapeRect _shape = new(48, 10);

        public P5WrathOfTheHeavensCauterizeBait() : base(alwaysDrawOtherBaits: true) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Cauterize)
            {
                foreach (var s in module.Enemies(OID.Darkscale))
                    CurrentBaits.Add(new(s, actor, _shape));
                foreach (var s in module.Enemies(OID.Vidofnir))
                    CurrentBaits.Add(new(s, actor, _shape));
            }
        }
    }

    class P5WrathOfTheHeavensCauterize1 : Components.SelfTargetedAOEs
    {
        public P5WrathOfTheHeavensCauterize1() : base(ActionID.MakeSpell(AID.Cauterize1), new AOEShapeRect(48, 10)) { }
    }

    class P5WrathOfTheHeavensCauterize2 : Components.SelfTargetedAOEs
    {
        public P5WrathOfTheHeavensCauterize2() : base(ActionID.MakeSpell(AID.Cauterize2), new AOEShapeRect(48, 10)) { }
    }

    class P5WrathOfTheHeavensAscalonsMercyRevealed : Components.BaitAwayEveryone
    {
        public P5WrathOfTheHeavensAscalonsMercyRevealed() : base(new AOEShapeCone(50, 15.Degrees()), ActionID.MakeSpell(AID.AscalonsMercyRevealedAOE)) { }

        public override void Init(BossModule module)
        {
            if (module.Enemies(OID.BossP5).FirstOrDefault() is var source && source != null)
                SetSource(module, source);
        }
    }

    // TODO: detect baiter
    class P5WrathOfTheHeavensLiquidHeaven : Components.PersistentVoidzoneAtCastTarget
    {
        public P5WrathOfTheHeavensLiquidHeaven() : base(6, ActionID.MakeSpell(AID.LiquidHeaven), m => m.Enemies(OID.VoidzoneLiquidHeaven).Where(z => z.EventState != 7), 1.1f) { }
    }

    // TODO: detect baiter
    class P5WrathOfTheHeavensAltarFlare : Components.LocationTargetedAOEs
    {
        public P5WrathOfTheHeavensAltarFlare() : base(ActionID.MakeSpell(AID.AltarFlareAOE), 8) { }
    }

    class P5WrathOfTheHeavensEmptyDimension : Components.SelfTargetedAOEs
    {
        private WPos _predicted;

        public bool KnowPosition => _predicted != default;

        public P5WrathOfTheHeavensEmptyDimension() : base(ActionID.MakeSpell(AID.EmptyDimension), new AOEShapeDonut(6, 70)) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (Casters.Count == 0 && KnowPosition)
                arena.AddCircle(_predicted, 6, ArenaColor.Safe, 2);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.SerGrinnaux && id == 0x1E43)
                _predicted = actor.Position;
        }
    }
}
