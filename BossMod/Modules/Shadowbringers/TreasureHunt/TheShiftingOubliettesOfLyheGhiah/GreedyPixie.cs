// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.GreedyPixie
{
    public enum OID : uint
    {
        Boss = 0x3018, //R=1.6
        BossAdd = 0x3019, //R=1.8
        PixieDouble = 0x304C, //R=1.6
        PixieDouble2 = 0x304D, //R=1.6
        BossHelper = 0x233C,
        SecretQueen = 0x3021, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
        SecretGarlic = 0x301F, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
        SecretTomato = 0x3020, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
        SecretOnion = 0x301D, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
        SecretEgg = 0x301E, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
        BonusAdd_TheKeeperOfTheKeys = 0x3034, // R3.230
        BonusAdd_FuathTrickster = 0x3033, // R0.750
    };

    public enum AID : uint
    {
        AutoAttack = 23185, // Boss->player, no cast, single-target
        AutoAttack2 = 872, // Adds->player, no cast, single-target
        WindRune = 21686, // Boss->self, 3,0s cast, range 40 width 8 rect
        SongRune = 21684, // Boss->location, 3,0s cast, range 6 circle
        StormRune = 21682, // Boss->self, 4,0s cast, range 40 circle
        BushBash = 22779, // PixieDouble2->self, 7,0s cast, range 12 circle
        BushBash2 = 21683, // Boss->self, 5,0s cast, range 12 circle
        NatureCall = 22780, // PixieDouble->self, 7,0s cast, range 30 120-degree cone, turns player into a plant
        NatureCall2 = 21685, // Boss->self, 5,0s cast, range 30 120-degree cone, turns player into a plant

        Pollen = 6452, // 2A0A->self, 3,5s cast, range 6+R circle
        TearyTwirl = 6448, // 2A06->self, 3,5s cast, range 6+R circle
        HeirloomScream = 6451, // 2A09->self, 3,5s cast, range 6+R circle
        PluckAndPrune = 6449, // 2A07->self, 3,5s cast, range 6+R circle
        PungentPirouette = 6450, // 2A08->self, 3,5s cast, range 6+R circle
        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
        Mash = 21767, // 3034->self, 3,0s cast, range 13 width 4 rect
        Inhale = 21770, // 3034->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
        Spin = 21769, // 3034->self, 4,0s cast, range 11 circle
        Scoop = 21768, // 3034->self, 4,0s cast, range 15 120-degree cone
    };

    class Windrune : Components.SelfTargetedAOEs
    {
        public Windrune() : base(ActionID.MakeSpell(AID.WindRune), new AOEShapeRect(40, 4)) { }
    }

    class SongRune : Components.LocationTargetedAOEs
    {
        public SongRune() : base(ActionID.MakeSpell(AID.SongRune), 6) { }
    }

    class StormRune : Components.RaidwideCast
    {
        public StormRune() : base(ActionID.MakeSpell(AID.StormRune)) { }
    }

    class BushBash : Components.SelfTargetedAOEs
    {
        public BushBash() : base(ActionID.MakeSpell(AID.BushBash), new AOEShapeCircle(12)) { }
    }

    class BushBash2 : Components.SelfTargetedAOEs
    {
        public BushBash2() : base(ActionID.MakeSpell(AID.BushBash2), new AOEShapeCircle(12)) { }
    }

    class NatureCall : Components.SelfTargetedAOEs
    {
        public NatureCall() : base(ActionID.MakeSpell(AID.NatureCall), new AOEShapeCone(30, 60.Degrees())) { }
    }

    class NatureCall2 : Components.SelfTargetedAOEs
    {
        public NatureCall2() : base(ActionID.MakeSpell(AID.NatureCall), new AOEShapeCone(30, 60.Degrees())) { }
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

    class Spin : Components.SelfTargetedAOEs
    {
        public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11)) { }
    }

    class Mash : Components.SelfTargetedAOEs
    {
        public Mash() : base(ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2)) { }
    }

    class Scoop : Components.SelfTargetedAOEs
    {
        public Scoop() : base(ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees())) { }
    }

    class GreedyPixieStates : StateMachineBuilder
    {
        public GreedyPixieStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Windrune>()
                .ActivateOnEnter<StormRune>()
                .ActivateOnEnter<SongRune>()
                .ActivateOnEnter<BushBash>()
                .ActivateOnEnter<BushBash2>()
                .ActivateOnEnter<NatureCall>()
                .ActivateOnEnter<NatureCall2>()
                .ActivateOnEnter<PluckAndPrune>()
                .ActivateOnEnter<TearyTwirl>()
                .ActivateOnEnter<HeirloomScream>()
                .ActivateOnEnter<PungentPirouette>()
                .ActivateOnEnter<Pollen>()
                .ActivateOnEnter<Spin>()
                .ActivateOnEnter<Mash>()
                .ActivateOnEnter<Scoop>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.SecretEgg).All(e => e.IsDead) && module.Enemies(OID.SecretQueen).All(e => e.IsDead) && module.Enemies(OID.SecretOnion).All(e => e.IsDead) && module.Enemies(OID.SecretGarlic).All(e => e.IsDead) && module.Enemies(OID.SecretTomato).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_TheKeeperOfTheKeys).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_FuathTrickster).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9797)]
    public class GreedyPixie : BossModule
    {
        public GreedyPixie(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object);
            foreach (var s in Enemies(OID.SecretEgg))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.SecretTomato))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.SecretQueen))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.SecretGarlic))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.SecretOnion))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.BonusAdd_TheKeeperOfTheKeys))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.BonusAdd_FuathTrickster))
                Arena.Actor(s, ArenaColor.Vulnerable);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.SecretOnion => 7,
                    OID.SecretEgg => 6,
                    OID.SecretGarlic => 5,
                    OID.SecretTomato or OID.BonusAdd_FuathTrickster => 4,
                    OID.SecretQueen or OID.BonusAdd_TheKeeperOfTheKeys => 3,
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
