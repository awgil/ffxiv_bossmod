using System.Linq;

namespace BossMod.MaskedCarnivale.Stage06.Act1
{
    public enum OID : uint
    {
        Boss = 0x25CD, //R=2.53
        Mandragora = 0x2700, //R0.3
    };

    public enum AID : uint
    {
    TearyTwirl = 14693, // 2700->self, 3,0s cast, range 6+R circle
    DemonEye = 14691, // 25CD->self, 5,0s cast, range 50+R circle
    Attack = 6499, // 2700/25CD->player, no cast, single-target
    ColdStare = 14692, // 25CD->self, 2,5s cast, range 40+R 90-degree cone
    };

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Get blinded by the Teary Twirl AOE by the mandragoras.\nBlindness makes you immune to all the gaze attacks.");
        } 
    }   
    class Stage01States : StateMachineBuilder
    {
        public Stage01States(Stage01 module) : base(module)
        {
            TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Mandragora).All(e => e.IsDead);
        }
    }

    public class Stage01 : BossModule
    {
        public Stage01(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Mandragora).Any(e => e.InCombat); }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
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
                    OID.Boss => 1,
                    OID.Mandragora => 0,
                    _ => 0
                };
            }
        }
    }
}
