// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretBasket
{
    public enum OID : uint
    {
        Boss = 0x302D, //R=2.34
        BossAdd = 0x302E, //R=1.05
        BossHelper = 0x233C,
        SecretQueen = 0x3021, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
        SecretGarlic = 0x301F, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
        SecretTomato = 0x3020, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
        SecretOnion = 0x301D, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
        SecretEgg = 0x301E, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    };

    public enum AID : uint
    {
        AutoAttack2 = 872, // Boss/BossAdd/SecretQueen->player, no cast, single-target
        unknown = 21698, // Boss->self, no cast, single-target
        HeavyStrike = 21723, // Boss->self, no cast, single-target
        HeavyStrike1 = 21724, // BossHelper->self, 4,0s cast, range 6+R 270-degree cone, donut segment
        HeavyStrike2 = 21725, // BossHelper->self, 4,0s cast, range 12+R 270-degree cone, donut segment
        HeavyStrike3 = 21726, // BossHelper->self, 4,9s cast, range 18+R 270-degree cone, donut segment
        PollenCorona = 21722, // Boss->self, 3,0s cast, range 8 circle
        StraightPunch = 21721, // Boss->player, 4,0s cast, single-target
        Leafcutter = 21732, // BossAdd->self, 3,0s cast, range 15 width 4 rect
        EarthCrusher = 21727, // Boss->self, 3,0s cast, single-target
        EarthCrusher2 = 21728, // BossHelper->self, 4,0s cast, range 10-20 donut
        SomersaultSlash = 21731, // BossAdd->player, no cast, single-target
        Earthquake = 21729, // Boss->self, 4,0s cast, single-target
        Earthquake2 = 21730, // BossHelper->self, no cast, range 20 circle

        Pollen = 6452, // 2A0A->self, 3,5s cast, range 6+R circle
        TearyTwirl = 6448, // 2A06->self, 3,5s cast, range 6+R circle
        HeirloomScream = 6451, // 2A09->self, 3,5s cast, range 6+R circle
        PluckAndPrune = 6449, // 2A07->self, 3,5s cast, range 6+R circle
        PungentPirouette = 6450, // 2A08->self, 3,5s cast, range 6+R circle
        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    };

    class Earthquake : Components.RaidwideCast
    {
        public Earthquake() : base(ActionID.MakeSpell(AID.Earthquake)) { }
    }

    class HeavyStrike1 : Components.SelfTargetedAOEs
    {
        public HeavyStrike1() : base(ActionID.MakeSpell(AID.HeavyStrike1), new AOEShapeDonutSector(1, 6.5f, 135.Degrees()))
        {
            Color = ArenaColor.Danger;
        }
    }

    class HeavyStrike2 : Components.SelfTargetedAOEs
    {
        public HeavyStrike2() : base(ActionID.MakeSpell(AID.HeavyStrike2), new AOEShapeDonutSector(6.5f, 12.5f, 135.Degrees())) { }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastFinished(module, caster, spell);
            if ((AID)spell.Action.ID == AID.HeavyStrike1)
                Color = ArenaColor.Danger;
            else
                Color = ArenaColor.AOE;
        }

    }

    class HeavyStrike3 : Components.SelfTargetedAOEs
    {
        public HeavyStrike3() : base(ActionID.MakeSpell(AID.HeavyStrike3), new AOEShapeDonutSector(12.5f, 18.5f, 135.Degrees())) { }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            base.OnCastFinished(module, caster, spell);
            if ((AID)spell.Action.ID == AID.HeavyStrike2)
                Color = ArenaColor.Danger;
            else
                Color = ArenaColor.AOE;
        }
    }

    class PollenCorona : Components.SelfTargetedAOEs
    {
        public PollenCorona() : base(ActionID.MakeSpell(AID.PollenCorona), new AOEShapeCircle(8)) { }
    }

    class StraightPunch : Components.SingleTargetCast
    {
        public StraightPunch() : base(ActionID.MakeSpell(AID.StraightPunch)) { }
    }

    class Leafcutter : Components.SelfTargetedAOEs
    {
        public Leafcutter() : base(ActionID.MakeSpell(AID.Leafcutter), new AOEShapeRect(15, 2)) { }
    }

    class EarthCrusher : Components.SelfTargetedAOEs
    {
        public EarthCrusher() : base(ActionID.MakeSpell(AID.EarthCrusher2), new AOEShapeDonut(10, 20)) { }
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

    class BasketStates : StateMachineBuilder
    {
        public BasketStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Earthquake>()
                .ActivateOnEnter<HeavyStrike1>()
                .ActivateOnEnter<HeavyStrike2>()
                .ActivateOnEnter<HeavyStrike3>()
                .ActivateOnEnter<PollenCorona>()
                .ActivateOnEnter<StraightPunch>()
                .ActivateOnEnter<Leafcutter>()
                .ActivateOnEnter<EarthCrusher>()
                .ActivateOnEnter<PluckAndPrune>()
                .ActivateOnEnter<TearyTwirl>()
                .ActivateOnEnter<HeirloomScream>()
                .ActivateOnEnter<PungentPirouette>()
                .ActivateOnEnter<Pollen>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.SecretEgg).All(e => e.IsDead) && module.Enemies(OID.SecretQueen).All(e => e.IsDead) && module.Enemies(OID.SecretOnion).All(e => e.IsDead) && module.Enemies(OID.SecretGarlic).All(e => e.IsDead) && module.Enemies(OID.SecretTomato).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9784)]
    public class Basket : BossModule
    {
        public Basket(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
