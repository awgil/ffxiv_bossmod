// CONTRIB: made by malediktus, not checked
namespace BossMod.Endwalker.TreasureHunt.LuckyFace
{
    public enum OID : uint
    {
        Boss = 0x377F, // R3,240
        BossHelper = 0x233C, // R0,500
        ExcitingQueen = 0x380C, // R0,840, spawn during fight (icon 5)
        ExcitingTomato = 0x380B, // R0,840, spawn during fight (icon 4)
        ExcitingGarlic = 0x380A, // R0,840, spawn during fight (icon 3)
        ExcitingEgg = 0x3809, // R0,840, spawn during fight (icon 2)
        ExcitingOnion = 0x3808, // R0,840, spawn during fight (icon 1)
    };

    public enum AID : uint
    {
        AutoAttack = 27980, // Boss->player, no cast, single-target
        FakeLeftInTheDark = 27989, // Boss->self, 4,0s cast, range 20 180-degree cone
        LeftInTheDark = 27990, // BossHelper->self, 4,0s cast, range 20 180-degree cone
        HeartOnFireII = 28001, // BossHelper->location, 3,0s cast, range 6 circle
        FakeHeartOnFireII = 27988, // Boss->self, 3,0s cast, single-target
        HeartOnFireIV = 27981, // Boss->player, 5,0s cast, single-target
        FakeQuakeInYourBoots = 27997, // Boss->self, 4,0s cast, range 10 circle
        QuakeInYourBoots = 27998, // BossHelper->self, 4,0s cast, range 10 circle
        MerryGoRound1 = 27983, // Boss->self, 3,0s cast, single-target, boss animation
        FakeLeftInTheDark2 = 27993, // Boss->self, 4,0s cast, range 20 180-degree cone
        LeftInTheDark2 = 27994, // BossHelper->self, 4,0s cast, range 20 180-degree cone
        MerryGoRound2 = 27986, // Boss->self, no cast, single-target, boss animation
        FakeQuakeMeAway = 27999, // Boss->self, 4,0s cast, range 10-20 donut
        QuakeMeAway = 28000, // BossHelper->self, 4,0s cast, range 10-20 donut
        MerryGoRound3 = 27984, // Boss->self, 3,0s cast, single-target
        FakeRightInTheDark = 27991, // Boss->self, 4,0s cast, range 20 180-degree cone
        RightInTheDark1 = 27992, // BossHelper->self, 4,0s cast, range 20 180-degree cone
        MerryGoRound4 = 27985, // Boss->self, no cast, single-target, boss animation
        applyspreadmarkers = 28045, // Boss->self, no cast, single-target
        FakeQuakeInYourBoots2 = 28189, // BossHelper->self, 4,0s cast, range 10-20 donut
        QuakeInYourBoots2 = 28090, // Boss->self, 4,0s cast, range 10 circle
        HeartOnFireIII = 28002, // BossHelper->player, 5,0s cast, range 6 circle
        TempersFlare = 27982, // Boss->self, 5,0s cast, range 60 circle
        FakeRightInTheDark2 = 27995, // Boss->self, 4,0s cast, range 20 180-degree cone
        RightInTheDark2 = 27996, // BossHelper->self, 4,0s cast, range 20 180-degree cone
        FakeQuakeMeAway2 = 28091, // Boss->self, 4,0s cast, range 10-20 donut
        QuakeMeAway2 = 28190, // BossHelper->self, 4,0s cast, range 10 circle
        MerryGoRound5 = 27987, // Boss->self, no cast, single-target, boss animation
        unknown2 = 28145, // Boss->self, no cast, single-target, probably death animation since it was cast after death
        PluckAndPrune = 6449, // 3809->self, 3,5s cast, range 6+R circle
        TearyTwirl = 6448, // 3808->self, 3,5s cast, range 6+R circle
        Telega = 9630, // 380C->self, no cast, single-target
        HeirloomScream = 6451, // 380B->self, 3,5s cast, range 6+R circle
        PungentPirouette = 6450, // 380A->self, 3,5s cast, range 6+R circle
        Pollen = 6452, // 380C->self, 3,5s cast, range 6+R circle
    };

    public enum SID : uint
    {
        Revolutionary = 2905, // Boss->Boss, extra=0x0
        Paralysis = 17, // 380A->player, extra=0x0
        Slow = 9, // 380A->player, extra=0x0
        Heavy = 14, // 380A->player, extra=0x32

    };
    public enum IconID : uint
    {
        tankbuster = 218,
        spreadmarker = 194,
    };
    class LeftInTheDark1 : Components.SelfTargetedAOEs
    {
        public LeftInTheDark1() : base(ActionID.MakeSpell(AID.LeftInTheDark), new AOEShapeCone(20,90.Degrees())) { } 
    }
    class LeftInTheDark2 : Components.SelfTargetedAOEs
    {
        public LeftInTheDark2() : base(ActionID.MakeSpell(AID.LeftInTheDark2), new AOEShapeCone(20,90.Degrees())) { } 
    }
    class RightInTheDark1 : Components.SelfTargetedAOEs
    {
        public RightInTheDark1() : base(ActionID.MakeSpell(AID.RightInTheDark1), new AOEShapeCone(20,90.Degrees())) { } 
    }
    class RightInTheDark2 : Components.SelfTargetedAOEs
    {
        public RightInTheDark2() : base(ActionID.MakeSpell(AID.RightInTheDark2), new AOEShapeCone(20,90.Degrees())) { } 
    }
    class QuakeInYourBoots1 : Components.SelfTargetedAOEs
    {
        public QuakeInYourBoots1() : base(ActionID.MakeSpell(AID.QuakeInYourBoots), new AOEShapeCircle(10)) { } 
    }
    class QuakeInYourBoots2 : Components.SelfTargetedAOEs
    {
        public QuakeInYourBoots2() : base(ActionID.MakeSpell(AID.QuakeInYourBoots2), new AOEShapeCircle(10)) { } 
    }
    class QuakeMeAway1 : Components.SelfTargetedAOEs
    {
        public QuakeMeAway1() : base(ActionID.MakeSpell(AID.QuakeMeAway), new AOEShapeDonut(10,20)) { } 
    }
    class QuakeMeAway2 : Components.SelfTargetedAOEs
    {
        public QuakeMeAway2() : base(ActionID.MakeSpell(AID.QuakeMeAway2), new AOEShapeCircle(10)) { } 
    }
    class HeartOnFireII : Components.LocationTargetedAOEs
    {
        public HeartOnFireII() : base(ActionID.MakeSpell(AID.HeartOnFireII), 6) {}
    }
    class HeartOnFireIV : Components.SingleTargetCast
    {
        public HeartOnFireIV() : base(ActionID.MakeSpell(AID.HeartOnFireIV)) { }
    }
    class HeartOnFireIII : Components.UniformStackSpread
    {
        public HeartOnFireIII() : base(0, 6, alwaysShowSpreads: true) { }
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if(iconID == (uint)IconID.spreadmarker)
                AddSpread(actor);
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.HeartOnFireIII)
                Spreads.Clear();
        }
    }
    class TempersFlare : Components.RaidwideCast
    {
        public TempersFlare() : base(ActionID.MakeSpell(AID.TempersFlare)) { }
    }
    class PluckAndPrune : Components.SelfTargetedAOEs
    {
        public PluckAndPrune() : base(ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(6.84f)) { } 
    }
    class TearyTwirl : Components.SelfTargetedAOEs
    {
        public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(6.84f)) { } 
    }
    class HeirloomScream : Components.SelfTargetedAOEs
    {
        public HeirloomScream() : base(ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(6.84f)) { } 
    }
    class PungentPirouette : Components.SelfTargetedAOEs
    {
        public PungentPirouette() : base(ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(6.84f)) { } 
    }
    class Pollen : Components.SelfTargetedAOEs
    {
        public Pollen() : base(ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(6.84f)) { } 
    }
    class LuckyFaceStates : StateMachineBuilder
    {
        public LuckyFaceStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<LeftInTheDark1>()
            .ActivateOnEnter<LeftInTheDark2>()
            .ActivateOnEnter<RightInTheDark1>()
            .ActivateOnEnter<RightInTheDark2>()
            .ActivateOnEnter<QuakeInYourBoots1>()
            .ActivateOnEnter<QuakeInYourBoots2>()
            .ActivateOnEnter<QuakeMeAway1>()
            .ActivateOnEnter<QuakeMeAway2>()
            .ActivateOnEnter<TempersFlare>()
            .ActivateOnEnter<HeartOnFireII>()
            .ActivateOnEnter<HeartOnFireIII>()
            .ActivateOnEnter<HeartOnFireIV>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>();
        }
    }
    [ModuleInfo(CFCID = 819, NameID = 10831)]
    public class LuckyFace : BossModule
    {
        public LuckyFace(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, -460), 20)) {}

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.ExcitingEgg))
                Arena.Actor(s, ArenaColor.Object, false);
            foreach (var s in Enemies(OID.ExcitingTomato))
                Arena.Actor(s, ArenaColor.Object, false);
            foreach (var s in Enemies(OID.ExcitingQueen))
                Arena.Actor(s, ArenaColor.Object, false);
            foreach (var s in Enemies(OID.ExcitingGarlic))
                Arena.Actor(s, ArenaColor.Object, false);
            foreach (var s in Enemies(OID.ExcitingOnion))
                Arena.Actor(s, ArenaColor.Object, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.ExcitingOnion => 6,
                    OID.ExcitingTomato => 5,
                    OID.ExcitingGarlic => 4,
                    OID.ExcitingEgg => 3,
                    OID.ExcitingQueen => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
