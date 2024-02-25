// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouTriton
{
    public enum OID : uint
    {
        Boss = 0x3D30, //R=6.08
        BossAdd = 0x3D31, //R=2.2
        BossHelper = 0x233C,
        Bubble = 0x3D32, //R=1.0
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss/BossAdd->player, no cast, single-target
        PelagicCleaver = 32230, // Boss->self, 3,5s cast, range 40 60-degree cone
        AquaticLance = 32231, // Boss->self, 4,0s cast, range 13 circle
        FoulWaters = 32229, // Boss->location, 3,0s cast, range 3 circle, AOE + spawns bubble
        Riptide = 32233, // Bubble->self, 1,0s cast, range 5 circle, pulls into bubble, dist 30 between centers
        WateryGrave = 32234, // Bubble->self, no cast, range 2 circle, voidzone, imprisons player until status runs out
        NavalRam = 32232, // BossAdd->player, no cast, single-target
        ProtolithicPuncture = 32228, // Boss->player, 5,0s cast, single-target
    };

    class PelagicCleaver : Components.SelfTargetedAOEs
    {
        public PelagicCleaver() : base(ActionID.MakeSpell(AID.PelagicCleaver), new AOEShapeCone(40, 30.Degrees())) { }
    }

    class FoulWaters : Components.PersistentVoidzoneAtCastTarget
    {
        public FoulWaters() : base(5, ActionID.MakeSpell(AID.FoulWaters), m => m.Enemies(OID.Bubble), 0) { }
    }

    class AquaticLance : Components.SelfTargetedAOEs
    {
        public AquaticLance() : base(ActionID.MakeSpell(AID.AquaticLance), new AOEShapeCircle(13)) { }
    }

    class ProtolithicPuncture : Components.SingleTargetCast
    {
        public ProtolithicPuncture() : base(ActionID.MakeSpell(AID.ProtolithicPuncture)) { }
    }

    class TritonStates : StateMachineBuilder
    {
        public TritonStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<PelagicCleaver>()
                .ActivateOnEnter<FoulWaters>()
                .ActivateOnEnter<AquaticLance>()
                .ActivateOnEnter<ProtolithicPuncture>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 909, NameID = 12006)]
    public class Triton : BossModule
    {
        public Triton(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
