// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouSatyros
{
    public enum OID : uint
    {
        Boss = 0x3D2D, //R=7.5
        BossAdd = 0x3D2E, //R=2.08
        BossHelper = 0x233C,
        StormsGrip = 0x3D2F, //R=1.0
        BonusAdds_Lyssa = 0x3D4E, //R=3.75, bonus loot adds
        BonusAdds_Lampas = 0x3D4D, //R=2.001, bonus loot adds
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss/BossAdd->player, no cast, single-target
        AutoAttack2 = 870, // BonusAdd_Lyssa->player, no cast, single-target
        StormWingA = 32220, // Boss->self, 5,0s cast, single-target
        StormWingB = 32219, // Boss->self, 5,0s cast, single-target
        StormWing2 = 32221, // BossHelper->self, 5,0s cast, range 40 90-degree cone
        DreadDive = 32218, // Boss->player, 5,0s cast, single-target
        FlashGale = 32222, // Boss->location, 3,0s cast, range 6 circle
        unknown = 32199, // 3D2F->self, no cast, single-target
        WindCutter = 32227, // 3D2F->self, no cast, range 4 circle
        BigHorn = 32226, // BossAdd->player, no cast, single-target
        Wingblow = 32224, // Boss->self, 4,0s cast, single-target
        Wingblow2 = 32225, // BossHelper->self, 4,0s cast, range 15 circle

        HeavySmash = 32317, // BossAdd->location, 3,0s cast, range 6 circle
        Telega = 9630, // BonusAdds->self, no cast, single-target, bonus add disappear
    };

    class HeavySmash : Components.LocationTargetedAOEs
    {
        public HeavySmash() : base(ActionID.MakeSpell(AID.HeavySmash), 6) { }
    }

    class StormWing : Components.SelfTargetedAOEs
    {
        public StormWing() : base(ActionID.MakeSpell(AID.StormWing2), new AOEShapeCone(40, 45.Degrees())) { }
    }

    class FlashGale : Components.LocationTargetedAOEs
    {
        public FlashGale() : base(ActionID.MakeSpell(AID.FlashGale), 6) { }
    }

    class WindCutter : Components.PersistentVoidzone
    {
        public WindCutter() : base(4, m => m.Enemies(OID.StormsGrip)) { }
    }

    class Wingblow : Components.SelfTargetedAOEs
    {
        public Wingblow() : base(ActionID.MakeSpell(AID.Wingblow2), new AOEShapeCircle(15)) { }
    }

    class DreadDive : Components.SingleTargetCast
    {
        public DreadDive() : base(ActionID.MakeSpell(AID.DreadDive)) { }
    }

    class SatyrosStates : StateMachineBuilder
    {
        public SatyrosStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<StormWing>()
                .ActivateOnEnter<FlashGale>()
                .ActivateOnEnter<WindCutter>()
                .ActivateOnEnter<Wingblow>()
                .ActivateOnEnter<DreadDive>()
                .ActivateOnEnter<HeavySmash>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdds_Lyssa).All(e => e.IsDead) && module.Enemies(OID.BonusAdds_Lampas).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 909, NameID = 12003)]
    public class Satyros : BossModule
    {
        public Satyros(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object);
            foreach (var s in Enemies(OID.BonusAdds_Lyssa))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.BonusAdds_Lampas))
                Arena.Actor(s, ArenaColor.Vulnerable);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BonusAdds_Lampas => 4,
                    OID.BonusAdds_Lyssa => 3,
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
