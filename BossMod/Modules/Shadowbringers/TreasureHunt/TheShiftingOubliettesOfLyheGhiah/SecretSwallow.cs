// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretSwallow
{
    public enum OID : uint
    {
        Boss = 0x302B, //R=4.0
        BossAdd = 0x302C, //R=2.0 
        BossHelper = 0x233C,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // 302B/302C->player, no cast, single-target
        ElectricWhorl = 21720, // 302B->self, 4,5s cast, range 8-60 donut
        Hydrocannon = 21712, // 302B->self, no cast, single-target
        Hydrocannon2 = 21766, // 233C->location, 3,0s cast, range 8 circle
        Ceras = 21716, // 302B->player, 4,0s cast, single-target
        SeventhWave = 21719, // 302B->self, 4,5s cast, range 11 circle
        BodySlam = 21718, // 302B->location, 4,0s cast, range 10 circle
        PrevailingCurrent = 21717, // 302C->self, 3,0s cast, range 22+R width 6 rect
    };

    class ElectricWhorl : Components.SelfTargetedAOEs
    {
        public ElectricWhorl() : base(ActionID.MakeSpell(AID.SeventhWave), new AOEShapeCircle(11)) { }
    }

    class PrevailingCurrent : Components.SelfTargetedAOEs
    {
        public PrevailingCurrent() : base(ActionID.MakeSpell(AID.PrevailingCurrent), new AOEShapeRect(24, 3)) { }
    }

    class SeventhWave : Components.SelfTargetedAOEs
    {
        public SeventhWave() : base(ActionID.MakeSpell(AID.ElectricWhorl), new AOEShapeDonut(8, 60)) { }
    }

    class Hydrocannon : Components.LocationTargetedAOEs
    {
        public Hydrocannon() : base(ActionID.MakeSpell(AID.Hydrocannon2), 8) { }
    }

    class Ceras : Components.SingleTargetCast
    {
        public Ceras() : base(ActionID.MakeSpell(AID.Ceras)) { }
    }

    class BodySlam : Components.LocationTargetedAOEs
    {
        public BodySlam() : base(ActionID.MakeSpell(AID.BodySlam), 10) { }
    }

    class SwallowStates : StateMachineBuilder
    {
        public SwallowStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<ElectricWhorl>()
                .ActivateOnEnter<PrevailingCurrent>()
                .ActivateOnEnter<SeventhWave>()
                .ActivateOnEnter<Hydrocannon>()
                .ActivateOnEnter<Ceras>()
                .ActivateOnEnter<BodySlam>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9782)]
    public class Swallow : BossModule
    {
        public Swallow(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
