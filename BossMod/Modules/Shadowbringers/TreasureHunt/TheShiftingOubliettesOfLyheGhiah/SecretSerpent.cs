// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretSerpent
{
    public enum OID : uint
    {
        Boss = 0x3025, //R=5.29
        BossAdd = 0x3026, //R=3.45
        BossHelper = 0x233C,
        SecretQueen = 0x3021, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
        SecretGarlic = 0x301F, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
        SecretTomato = 0x3020, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
        SecretOnion = 0x301D, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
        SecretEgg = 0x301E, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss/BossAdd->player, no cast, single-target
        AutoAttack2 = 872, // BonusAdds->player, no cast, single-target
        Douse = 21701, // Boss->location, 3,0s cast, range 8 circle
        Drench = 21700, // Boss->self, 3,0s cast, range 10+R 90-degree cone
        FangsEnd = 21699, // Boss->player, 4,0s cast, single-target
        Drench2 = 22771, // BossAdd->self, 3,0s cast, range 10+R 90-degree cone
        ScaleRipple = 21702, // Boss->self, 2,5s cast, range 8 circle

        Pollen = 6452, // 2A0A->self, 3,5s cast, range 6+R circle
        TearyTwirl = 6448, // 2A06->self, 3,5s cast, range 6+R circle
        HeirloomScream = 6451, // 2A09->self, 3,5s cast, range 6+R circle
        PluckAndPrune = 6449, // 2A07->self, 3,5s cast, range 6+R circle
        PungentPirouette = 6450, // 2A08->self, 3,5s cast, range 6+R circle
        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    };

    class Douse : Components.LocationTargetedAOEs
    {
        public Douse() : base(ActionID.MakeSpell(AID.Douse), 8) { }
    }

    class FangsEnd : Components.SingleTargetCast
    {
        public FangsEnd() : base(ActionID.MakeSpell(AID.FangsEnd)) { }
    }

    class Drench : Components.SelfTargetedAOEs
    {
        public Drench() : base(ActionID.MakeSpell(AID.Drench), new AOEShapeCone(15.29f, 45.Degrees())) { }
    }

    class Drench2 : Components.SelfTargetedAOEs
    {
        public Drench2() : base(ActionID.MakeSpell(AID.Drench2), new AOEShapeCone(13.45f, 45.Degrees())) { }
    }

    class ScaleRipple : Components.SelfTargetedAOEs
    {
        public ScaleRipple() : base(ActionID.MakeSpell(AID.ScaleRipple), new AOEShapeCircle(8)) { }
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

    class SerpentStates : StateMachineBuilder
    {
        public SerpentStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Douse>()
                .ActivateOnEnter<FangsEnd>()
                .ActivateOnEnter<Drench>()
                .ActivateOnEnter<Drench2>()
                .ActivateOnEnter<ScaleRipple>()
                .ActivateOnEnter<PluckAndPrune>()
                .ActivateOnEnter<TearyTwirl>()
                .ActivateOnEnter<HeirloomScream>()
                .ActivateOnEnter<PungentPirouette>()
                .ActivateOnEnter<Pollen>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.SecretEgg).All(e => e.IsDead) && module.Enemies(OID.SecretQueen).All(e => e.IsDead) && module.Enemies(OID.SecretOnion).All(e => e.IsDead) && module.Enemies(OID.SecretGarlic).All(e => e.IsDead) && module.Enemies(OID.SecretTomato).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9776)]
    public class Serpent : BossModule
    {
        public Serpent(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
