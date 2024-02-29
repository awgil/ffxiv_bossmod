// CONTRIB: made by malediktus, not checked
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouStyphnolobion
{
    public enum OID : uint
    {
        Boss = 0x3D37, //R=5.3
        BossAdd = 0x3D38, //R=2.53
        BossHelper = 0x233C,
        GymnasticGarlic = 0x3D51, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
        GymnasticQueen = 0x3D53, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
        GymnasticEggplant = 0x3D50, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
        GymnasticOnion = 0x3D4F, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
        GymnasticTomato = 0x3D52, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
        BonusAdds_Lyssa = 0x3D4E, //R=3.75
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        AutoAttack2 = 872, // BossAdd->player, no cast, single-target
        EarthQuakerCombo = 32199, // Boss->self, no cast, single-target
        EarthQuaker = 32247, // Boss->self, 3,5s cast, single-target
        EarthQuaker1 = 32248, // BossHelper->self, 4,0s cast, range 10 circle
        EarthQuaker2 = 32249, // BossHelper->self, 6,0s cast, range 10-20 donut
        Rake = 32245, // Boss->player, 5,0s cast, single-target
        EarthShaker = 32250, // Boss->self, 3,5s cast, single-target
        EarthShaker2 = 32251, // BossHelper->players, 4,0s cast, range 60 30-degree cone
        StoneIII = 32252, // Boss->self, 2,5s cast, single-target
        StoneIII2 = 32253, // BossHelper->location, 3,0s cast, range 6 circle
        BeakSnap = 32254, // BossAdd->player, no cast, single-target
        Tiiimbeeer = 32246, // Boss->self, 5,0s cast, range 50 circle

        PluckAndPrune = 32302, // GymnasticEggplant->self, 3,5s cast, range 7 circle
        Pollen = 32305, // GymnasticQueen->self, 3,5s cast, range 7 circle
        HeirloomScream = 32304, // GymnasticTomato->self, 3,5s cast, range 7 circle
        PungentPirouette = 32303, // GymnasticGarlic->self, 3,5s cast, range 7 circle
        TearyTwirl = 32301, // GymnasticOnion->self, 3,5s cast, range 7 circle
        Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
        HeavySmash = 32317, // 3D4E->location, 3,0s cast, range 6 circle
    };

    class Rake : Components.SingleTargetCast
    {
        public Rake() : base(ActionID.MakeSpell(AID.Rake)) { }
    }

    class Tiiimbeeer : Components.RaidwideCast
    {
        public Tiiimbeeer() : base(ActionID.MakeSpell(AID.Tiiimbeeer)) { }
    }

    class StoneIII : Components.LocationTargetedAOEs
    {
        public StoneIII() : base(ActionID.MakeSpell(AID.StoneIII2), 6) { }
    }

    class EarthShaker : Components.BaitAwayCast
    {
        public EarthShaker() : base(ActionID.MakeSpell(AID.EarthShaker2), new AOEShapeCone(60, 15.Degrees())) { }
    }

    class EarthQuaker : Components.ConcentricAOEs
    {
        private static AOEShape[] _shapes = { new AOEShapeCircle(10), new AOEShapeDonut(10, 20) };

        public EarthQuaker() : base(_shapes) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.EarthQuaker)
                AddSequence(module.Bounds.Center, spell.NPCFinishAt.AddSeconds(0.5f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (Sequences.Count > 0)
            {
                var order = (AID)spell.Action.ID switch
                {
                    AID.EarthQuaker1 => 0,
                    AID.EarthQuaker2 => 1,
                    _ => -1
                };
                AdvanceSequence(order, module.Bounds.Center, module.WorldState.CurrentTime.AddSeconds(1.95f));
            }
        }
    }

    class PluckAndPrune : Components.SelfTargetedAOEs
    {
        public PluckAndPrune() : base(ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(7)) { }
    }

    class TearyTwirl : Components.SelfTargetedAOEs
    {
        public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(7)) { }
    }

    class HeirloomScream : Components.SelfTargetedAOEs
    {
        public HeirloomScream() : base(ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(7)) { }
    }

    class PungentPirouette : Components.SelfTargetedAOEs
    {
        public PungentPirouette() : base(ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(7)) { }
    }

    class Pollen : Components.SelfTargetedAOEs
    {
        public Pollen() : base(ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(7)) { }
    }

    class HeavySmash : Components.LocationTargetedAOEs
    {
        public HeavySmash() : base(ActionID.MakeSpell(AID.HeavySmash), 6) { }
    }

    class StyphnolobionStates : StateMachineBuilder
    {
        public StyphnolobionStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Rake>()
                .ActivateOnEnter<Tiiimbeeer>()
                .ActivateOnEnter<StoneIII>()
                .ActivateOnEnter<EarthShaker>()
                .ActivateOnEnter<EarthQuaker>()
                .ActivateOnEnter<PluckAndPrune>()
                .ActivateOnEnter<TearyTwirl>()
                .ActivateOnEnter<HeirloomScream>()
                .ActivateOnEnter<PungentPirouette>()
                .ActivateOnEnter<Pollen>()
                .ActivateOnEnter<HeavySmash>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdds_Lyssa).All(e => e.IsDead) && module.Enemies(OID.GymnasticEggplant).All(e => e.IsDead) && module.Enemies(OID.GymnasticQueen).All(e => e.IsDead) && module.Enemies(OID.GymnasticOnion).All(e => e.IsDead) && module.Enemies(OID.GymnasticGarlic).All(e => e.IsDead) && module.Enemies(OID.GymnasticTomato).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 909, NameID = 12012)]
    public class Styphnolobion : BossModule
    {
        public Styphnolobion(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object);
            foreach (var s in Enemies(OID.GymnasticEggplant))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.GymnasticTomato))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.GymnasticQueen))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.GymnasticGarlic))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.GymnasticOnion))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.BonusAdds_Lyssa))
                Arena.Actor(s, ArenaColor.Vulnerable);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.GymnasticOnion => 7,
                    OID.GymnasticEggplant => 6,
                    OID.GymnasticGarlic => 5,
                    OID.GymnasticTomato => 4,
                    OID.GymnasticQueen or OID.BonusAdds_Lyssa => 3,
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
