using System.Linq;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage04.Act1
{
    public enum OID : uint
    {
        Boss = 0x25C8, //R=1.65
        Bat = 0x25D2, //R=0.4
    };

    public enum AID : uint
    {
        AutoAttack = 6497, // 25C8->player, no cast, single-target
        AutoAttack2 = 6499, // 25D2->player, no cast, single-target
        BloodDrain = 14360, // 25D2->player, no cast, single-target
        SanguineBite = 14361, // 25C8->self, no cast, range 3+R width 2 rect
    };

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Trivial stage. Enemies here are weak to lightning and fire.\nIn Act 2 the Ram's Voice and Ultravibration combo can be useful.\nFlying Sardine for interrupts can be beneficial.");
        }
    }

    class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Bats are weak to lightning.\nThe wolf is weak to fire.");
        }
    }

    class Stage04Act1States : StateMachineBuilder
    {
        public Stage04Act1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Hints2>()
                .DeactivateOnEnter<Hints>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Bat).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 614, NameID = 8086)]
    public class Stage04Act1 : BossModule
    {
        public Stage04Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Bat).Any(e => e.InCombat); }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Bat))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
