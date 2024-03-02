// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Trials.T08Asura
{
    public enum OID : uint
    {
        Boss = 0x409B, //R=5.0
        Helper = 0x233C,
        Helper2 = 0x40A9,
        PhantomAsura = 0x40F8, //R=5.0
        AsuraImage = 0x40A2, //R=45.0

    }

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        LowerRealm = 36001, // Boss->self, 5,0s cast, range 40 circle, raidwide
        Teleport = 35966, // Boss->location, no cast, single-target, boss teleports mid
        AsuriChakra = 35994, // Boss->self, 6,0s cast, range 5 circle
        Chakra1 = 35995, // Helper->self, 6,0s cast, range 6-8 donut
        Chakra2 = 35996, // Helper->self, 6,0s cast, range 9-11 donut
        Chakra4 = 35997, // Helper->self, 6,0s cast, range 12-14 donut
        Chakra3 = 35998, // Helper->self, 6,0s cast, range 15-17 donut
        Chakra5 = 35999, // Helper->self, 6,0s cast, range 18-20 donut
        CuttingJewel = 36000, // Boss->players, 5,0s cast, range 4 circle, tankbuster
        Ephemerality = 35990, // Boss->self, 5,0s cast, range 20 circle, raidwide
        Laceration = 35991, // PhantomAsura->self, 1,0s cast, range 9 circle
        Divinity = 36008, // Boss->self, no cast, single-target
        Divinity2 = 36009, // AsuraImage->self, no cast, single-target
        DivineAwakening = 35967, // Boss->self, no cast, single-target
        DivineAwakening2 = 35968, // AsuraImage->self, no cast, single-target
        IconographyPedestalPurge = 35969, // Boss->self, 5,0s cast, range 10 circle
        IconicExecution = 36017, // Boss->self, 4,0s cast, single-target
        IconicExecution2 = 36018, // Boss->self, 3,0s cast, single-target
        PedestalPurge = 35970, // AsuraImage->self, 4,0s cast, range 30 circle
        IconographyWheelOfDeincarnation = 35971, // Boss->self, 5,0s cast, range 8-40 donut
        WheelOfDeincarnation = 35972, // AsuraImage->self, 4,0s cast, range 15-96 donut
        IconographyBladewise = 35973, // Boss->self, 5,0s cast, range 50 width 6 rect
        Bladewise = 35974, // AsuraImage->self, 4,0s cast, range 100 width 28 rect
        RemoveStatus = 36010, // AsuraImage->self, no cast, single-target, removes status 2552 (unknown effect) from itself
        KhadgaTelegraph1 = 36011, // Helper->self, 2,0s cast, range 20 180-degree cone
        KhadgaTelegraph2 = 36013, // Helper->self, 2,0s cast, range 20 180-degree cone
        KhadgaTelegraph3 = 36012, // Helper->self, 2,0s cast, range 20 180-degree cone
        SixBladedKhadga = 35976, // Boss->self, 13,0s cast, single-target
        Khadga1 = 35977, // Boss->self, no cast, range 20 180-degree cone
        Khadga2 = 35981, // Boss->self, no cast, range 20 180-degree cone
        Khadga3 = 35980, // Boss->self, no cast, range 20 180-degree cone
        Khadga4 = 35978, // Boss->self, no cast, range 20 180-degree cone
        Khadga5 = 35982, // Boss->self, no cast, range 20 180-degree cone
        Khadga6 = 35979, // Boss->self, no cast, range 20 180-degree cone
        ManyFaces1 = 35983, // Boss->self, 4,0s cast, single-target
        ManyFaces2 = 36014, // Boss->self, no cast, single-target
        TheFaceOfWrathA = 35984, // Boss->self, 8,0s cast, single-target
        TheFaceOfWrathB = 35986, // Boss->self, 8,0s cast, single-target
        TheFaceOfWrath = 36022, // Helper->self, no cast, single-target
        FaceMechanicWrath = 36015, // Helper->self, 8,0s cast, range 20 180-degree cone
        FaceMechanicDelight = 36016, // Helper->self, 8,0s cast, range 20 180-degree cone
        TheFaceOfDelightA = 35989, // Boss->self, 8,0s cast, single-target
        TheFaceOfDelightB = 35987, // Boss->self, 8,0s cast, single-target
        TheFaceOfDelight = 36023, // Helper->self, no cast, single-target
        TheFaceOfDelightSnapshot = 36007, // Helper->self, no cast, range 20 180-degree cone
        TheFaceOfWrathSnapshot = 36006, // Helper->self, no cast, range 20 180-degree cone
        MyriadAspects = 36019, // Boss->self, 3,0s cast, single-target
        MyriadAspects1 = 36020, // Helper->self, 4,0s cast, range 40 30-degree cone
        MyriadAspects2 = 36021, // Helper->self, 6,0s cast, range 40 30-degree cone
        Bladescatter = 35992, // Boss->self, 5,0s cast, single-target
        Scattering = 35993, // Helper->self, 3,0s cast, range 20 width 6 rect
        OrderedChaos = 36002, // Boss->self, no cast, single-target
        OrderedChaos2 = 36003, // Helper->player, 5,0s cast, range 5 circle
    };

    public enum IconID : uint
    {
        Tankbuster = 342, // player
        Khadga1 = 454, // Helper, icon 1
        Khadga2 = 455, // Helper, icon 2
        Khadga3 = 456, // Helper, icon 3
        Khadga4 = 457, // Helper, icon 4
        Khadga5 = 458, // Helper, icon 5
        Khadga6 = 459, // Helper, icon 6
        Spreadmarker = 139, // player
    };

    class LowerRealm : Components.RaidwideCast
    {
        public LowerRealm() : base(ActionID.MakeSpell(AID.LowerRealm)) { }
        public override void Update(BossModule module) //deathwall appears after 1st cast
        {
            if (NumCasts > 0)
                module.Arena.Bounds = new ArenaBoundsCircle(new(100, 100), 19);
        }
    }

    class Ephemerality : Components.RaidwideCast
    {
        public Ephemerality() : base(ActionID.MakeSpell(AID.Ephemerality)) { }
    }

    class AsuriChakra : Components.SelfTargetedAOEs
    {
        public AsuriChakra() : base(ActionID.MakeSpell(AID.AsuriChakra), new AOEShapeCircle(5)) { }
    }

    class Chakra1 : Components.SelfTargetedAOEs
    {
        public Chakra1() : base(ActionID.MakeSpell(AID.Chakra1), new AOEShapeDonut(6, 8)) { }
    }

    class Chakra2 : Components.SelfTargetedAOEs
    {
        public Chakra2() : base(ActionID.MakeSpell(AID.Chakra2), new AOEShapeDonut(9, 11)) { }
    }

    class Chakra3 : Components.SelfTargetedAOEs
    {
        public Chakra3() : base(ActionID.MakeSpell(AID.Chakra3), new AOEShapeDonut(12, 14)) { }
    }

    class Chakra4 : Components.SelfTargetedAOEs
    {
        public Chakra4() : base(ActionID.MakeSpell(AID.Chakra4), new AOEShapeDonut(15, 17)) { }
    }

    class Chakra5 : Components.SelfTargetedAOEs
    {
        public Chakra5() : base(ActionID.MakeSpell(AID.Chakra4), new AOEShapeDonut(18, 20)) { }
    }

    class CuttingJewel : Components.BaitAwayCast
    {
        public CuttingJewel() : base(ActionID.MakeSpell(AID.CuttingJewel), new AOEShapeCircle(4), true) { }
    }

    class CuttingJewelHint : Components.SingleTargetCast
    {
        public CuttingJewelHint() : base(ActionID.MakeSpell(AID.CuttingJewel)) { }
    }

    class Laceration : Components.GenericAOEs
    {
        private static readonly AOEShapeCircle circle = new (9);
        private DateTime _activation;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in module.Enemies(OID.PhantomAsura))
                if (_activation != default)
                    yield return new(circle, c.Position, activation: _activation);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id == 0x11D6)
                _activation = module.WorldState.CurrentTime.AddSeconds(5); //actual time is 5-7s delay, but the AOEs end up getting casted at the same time, so we take the earliest time
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Laceration)
                _activation = default;
        }
    }

    class IconographyPedestalPurge : Components.SelfTargetedAOEs
    {
        public IconographyPedestalPurge() : base(ActionID.MakeSpell(AID.IconographyPedestalPurge), new AOEShapeCircle(10)) { }
    }

    class PedestalPurge : Components.SelfTargetedAOEs
    {
        public PedestalPurge() : base(ActionID.MakeSpell(AID.PedestalPurge), new AOEShapeCircle(30)) { }
    }

    class IconographyWheelOfDeincarnation : Components.SelfTargetedAOEs
    {
        public IconographyWheelOfDeincarnation() : base(ActionID.MakeSpell(AID.IconographyWheelOfDeincarnation), new AOEShapeDonut(8, 40)) { }
    }

    class WheelOfDeincarnation : Components.SelfTargetedAOEs
    {
        public WheelOfDeincarnation() : base(ActionID.MakeSpell(AID.WheelOfDeincarnation), new AOEShapeDonut(15, 96)) { }
    }

    class IconographyBladewise : Components.SelfTargetedAOEs
    {
        public IconographyBladewise() : base(ActionID.MakeSpell(AID.IconographyBladewise), new AOEShapeRect(50, 3)) { }
    }

    class Bladewise : Components.SelfTargetedAOEs
    {
        public Bladewise() : base(ActionID.MakeSpell(AID.Bladewise), new AOEShapeRect(100, 14)) { }
    }

    class SixBladedKhadga : Components.GenericAOEs
    {
        private List<ActorCastInfo> _spell = new();
        private DateTime _start;
        private static readonly AOEShapeCone Cone = new(20, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_spell.Count > 0 && NumCasts == 0)
                yield return new(Cone, module.PrimaryActor.Position, _spell[0].Rotation, _start.AddSeconds(12.9f), ArenaColor.Danger);
            if (_spell.Count > 1 && NumCasts == 0)
                yield return new(Cone, module.PrimaryActor.Position, _spell[1].Rotation, _start.AddSeconds(14.9f));
            if (_spell.Count > 1 && NumCasts == 1)
                yield return new(Cone, module.PrimaryActor.Position, _spell[1].Rotation, _start.AddSeconds(14.9f), ArenaColor.Danger);
            if (_spell.Count > 2 && NumCasts == 1)
                yield return new(Cone, module.PrimaryActor.Position, _spell[2].Rotation, _start.AddSeconds(17));
            if (_spell.Count > 3 && NumCasts == 2)
                yield return new(Cone, module.PrimaryActor.Position, _spell[2].Rotation, _start.AddSeconds(17), ArenaColor.Danger);
            if (_spell.Count > 3 && NumCasts == 2)
                yield return new(Cone, module.PrimaryActor.Position, _spell[3].Rotation, _start.AddSeconds(19));
            if (_spell.Count > 4 && NumCasts == 3)
                yield return new(Cone, module.PrimaryActor.Position, _spell[3].Rotation, _start.AddSeconds(19), ArenaColor.Danger);
            if (_spell.Count > 4 && NumCasts == 3)
                yield return new(Cone, module.PrimaryActor.Position, _spell[4].Rotation, _start.AddSeconds(21.1f));
            if (_spell.Count > 5 && NumCasts == 4)
                yield return new(Cone, module.PrimaryActor.Position, _spell[4].Rotation, _start.AddSeconds(21.1f), ArenaColor.Danger);
            if (_spell.Count > 5 && NumCasts == 4)
                yield return new(Cone, module.PrimaryActor.Position, _spell[5].Rotation, _start.AddSeconds(23.2f));
            if (_spell.Count > 5 && NumCasts == 5)
                yield return new(Cone, module.PrimaryActor.Position, _spell[5].Rotation, _start.AddSeconds(23.2f), ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.KhadgaTelegraph1 or AID.KhadgaTelegraph2 or AID.KhadgaTelegraph3)
            {
                _spell.Add(spell);
                if (_start == default)
                    _start = module.WorldState.CurrentTime;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.Khadga1 or AID.Khadga2 or AID.Khadga3 or AID.Khadga4 or AID.Khadga5 or AID.Khadga6)
            {
                ++NumCasts;
                if (NumCasts == 6)
                {
                    NumCasts = 0;
                    _start = default;
                    _spell.Clear();
                }
            }
        }
    }

    class MyriadAspects : Components.GenericAOEs
    {
        private static readonly AOEShapeCone cone = new (40, 15.Degrees());
        private DateTime _activation1;
        private DateTime _activation2;
        private List<ActorCastInfo> _spell1 = new();
        private List<ActorCastInfo> _spell2 = new();

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (NumCasts < 6 && _spell1.Count > 0)
                foreach (var c in _spell1)
                    yield return new(cone, module.Bounds.Center, c.Rotation, activation: _activation1);
            if (NumCasts >= 6 && _spell2.Count > 0)
                foreach (var c in _spell2)
                    yield return new(cone, module.Bounds.Center, c.Rotation, activation: _activation2);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.MyriadAspects1)
            {
                _activation1 = spell.NPCFinishAt;
                _spell1.Add(spell);
            }
            if ((AID)spell.Action.ID == AID.MyriadAspects2)
            {
                _activation2 = spell.NPCFinishAt;
                _spell2.Add(spell);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.MyriadAspects1 or AID.MyriadAspects2)
                ++NumCasts;
                if (NumCasts == 12)
                {
                    NumCasts = 0;
                    _spell1.Clear();
                    _spell2.Clear();
                }
        }
    }

    class Scattering : Components.SelfTargetedAOEs
    {
        public Scattering() : base(ActionID.MakeSpell(AID.Scattering), new AOEShapeRect(20, 3)) { }
    }

    class OrderedChaos : Components.UniformStackSpread
    {
        public OrderedChaos() : base(0, 5, alwaysShowSpreads: true) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Spreadmarker)
                AddSpread(actor);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.OrderedChaos2)
                Spreads.Clear();
        }
    }

    class ManyFaces : Components.GenericAOEs
    {
        private static readonly AOEShapeCone cone = new (20, 90.Degrees());
        private DateTime _activation;
        private bool delight;
        private bool wrath;
        private Angle _rotationWrath;
        private Angle _rotationDelight;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (delight)
                yield return new(cone, module.Bounds.Center, _rotationDelight, activation: _activation);
            if (wrath)
                yield return new(cone, module.Bounds.Center, _rotationWrath, activation: _activation);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.TheFaceOfDelightA or AID.TheFaceOfDelightB)
                delight = true;
            if ((AID)spell.Action.ID is AID.TheFaceOfWrathA or AID.TheFaceOfWrathB)
                wrath = true;
            if ((AID)spell.Action.ID == AID.FaceMechanicWrath)
            {
                _activation = spell.NPCFinishAt;
                _rotationDelight = spell.Rotation;
            }
            if ((AID)spell.Action.ID == AID.FaceMechanicDelight)
            {
                _activation = spell.NPCFinishAt;
                _rotationWrath = spell.Rotation;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.TheFaceOfDelightSnapshot or AID.TheFaceOfWrathSnapshot)
            {
                delight = false;
                wrath = false;
            }
        }
    }

    class T08AsuraStates : StateMachineBuilder
    {
        public T08AsuraStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Ephemerality>()
                .ActivateOnEnter<LowerRealm>()
                .ActivateOnEnter<AsuriChakra>()
                .ActivateOnEnter<Chakra1>()
                .ActivateOnEnter<Chakra2>()
                .ActivateOnEnter<Chakra3>()
                .ActivateOnEnter<Chakra4>()
                .ActivateOnEnter<Chakra5>()
                .ActivateOnEnter<CuttingJewel>()
                .ActivateOnEnter<CuttingJewelHint>()
                .ActivateOnEnter<Laceration>()
                .ActivateOnEnter<IconographyPedestalPurge>()
                .ActivateOnEnter<PedestalPurge>()
                .ActivateOnEnter<IconographyWheelOfDeincarnation>()
                .ActivateOnEnter<WheelOfDeincarnation>()
                .ActivateOnEnter<IconographyBladewise>()
                .ActivateOnEnter<Bladewise>()
                .ActivateOnEnter<SixBladedKhadga>()
                .ActivateOnEnter<MyriadAspects>()
                .ActivateOnEnter<Scattering>()
                .ActivateOnEnter<OrderedChaos>()
                .ActivateOnEnter<ManyFaces>();
        }
    }

    [ModuleInfo(CFCID = 944, NameID = 12351)]
    public class T08Asura : BossModule
    {
        public T08Asura(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
    }
}
