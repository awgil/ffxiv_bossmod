// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretKorrigan
{
    public enum OID : uint
    {
        Boss = 0x3022, //R=2.85
        BossAdd = 0x301C, //R=0.84
        BossHelper = 0x233C,
        SecretQueen = 0x3021, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
        SecretGarlic = 0x301F, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
        SecretTomato = 0x3020, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
        SecretOnion = 0x301D, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
        SecretEgg = 0x301E, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss/BossAdd/BonusAdds->player, no cast, single-target
        Hypnotize = 21674, // Boss->self, 4,0s cast, range 40 circle
        LeafDagger = 21675, // Boss->location, 2,5s cast, range 3 circle
        SaibaiMandragora = 21676, // Boss->self, 3,0s cast, single-target
        Ram = 21673, // Boss->player, 3,0s cast, single-target

        Pollen = 6452, // 2A0A->self, 3,5s cast, range 6+R circle
        TearyTwirl = 6448, // 2A06->self, 3,5s cast, range 6+R circle
        HeirloomScream = 6451, // 2A09->self, 3,5s cast, range 6+R circle
        PluckAndPrune = 6449, // 2A07->self, 3,5s cast, range 6+R circle
        PungentPirouette = 6450, // 2A08->self, 3,5s cast, range 6+R circle
        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    };

    class Hypnotize : Components.CastGaze
    {
        public Hypnotize() : base(ActionID.MakeSpell(AID.Hypnotize)) { }
    }

    class Ram : Components.SingleTargetCast
    {
        public Ram() : base(ActionID.MakeSpell(AID.Ram)) { }
    }

    class SaibaiMandragora : Components.CastHint
    {
        public SaibaiMandragora() : base(ActionID.MakeSpell(AID.SaibaiMandragora), "Calls adds") { }
    }

    class LeafDagger : Components.LocationTargetedAOEs
    {
        public LeafDagger() : base(ActionID.MakeSpell(AID.LeafDagger), 3) { }
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

    class KorriganStates : StateMachineBuilder
    {
        public KorriganStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Hypnotize>()
                .ActivateOnEnter<LeafDagger>()
                .ActivateOnEnter<SaibaiMandragora>()
                .ActivateOnEnter<Ram>()
                .ActivateOnEnter<PluckAndPrune>()
                .ActivateOnEnter<TearyTwirl>()
                .ActivateOnEnter<HeirloomScream>()
                .ActivateOnEnter<PungentPirouette>()
                .ActivateOnEnter<Pollen>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.SecretEgg).All(e => e.IsDead) && module.Enemies(OID.SecretQueen).All(e => e.IsDead) && module.Enemies(OID.SecretOnion).All(e => e.IsDead) && module.Enemies(OID.SecretGarlic).All(e => e.IsDead) && module.Enemies(OID.SecretTomato).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9806)]
    public class Korrigan : BossModule
    {
        public Korrigan(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
                    OID.SecretTomato => 4,
                    OID.SecretQueen => 3,
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
