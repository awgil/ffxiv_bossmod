using System.Linq;
using BossMod.Components;

namespace BossMod.MaskedCarnivale.Stage06.Act2
{
    public enum OID : uint
    {
        Boss = 0x26FF, //R=2.53
        Eye = 0x25CE, //R1.35
        Mandragora = 0x2701, //R0.3
    };

    public enum AID : uint
    {
        TearyTwirl = 14693, // 2700->self, 3,0s cast, range 6+R circle
        DemonEye = 14691, // 25CD->self, 5,0s cast, range 50+R circle
        Attack = 6499, // 2700/25CD->player, no cast, single-target
        ColdStare = 14692, // 25CD->self, 2,5s cast, range 40+R 90-degree cone
        Stone = 14695, // 25CE->player, 1,0s cast, single-target
        DreadGaze = 14694, // 25CE->self, 3,0s cast, range 6+R ?-degree cone

    };
    class TearyTwirl : StackWithCastTargets
    {
        public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), 6) { }
    }
    class DreadGaze : SelfTargetedAOEs
    {
        public DreadGaze() : base(ActionID.MakeSpell(AID.DreadGaze), new AOEShapeCone(6,45.Degrees())) { } 
    }
    class Stage06Act2States : StateMachineBuilder
    {
        public Stage06Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<DreadGaze>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Mandragora).All(e => e.IsDead) && module.Enemies(OID.Eye).All(e => e.IsDead);
        }
    }

    public class Stage06Act2 : BossModule
    {
        public Stage06Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25)) {}
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Mandragora).Any(e => e.InCombat) || Enemies(OID.Eye).Any(e => e.InCombat); }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Eye))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Mandragora))
                Arena.Actor(s, ArenaColor.Object, false);
        }
        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
        base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss or OID.Eye => 1,
                    OID.Mandragora => 0,
                    _ => 0
                };
            }
        }
    }
}
