// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage12.Act1
{
    public enum OID : uint
    {
        Boss = 0x271A, //R=0.8
    };

    public enum AID : uint
    {
        Seedvolley = 14750, // 271A->player, no cast, single-target
    };

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("For this stage Ice Spikes and Bomb Toss are recommended spells.\nUse Ice Spikes to instantly kill roselets once they become aggressive.\nHydnora in act 2 is weak against water and strong against earth spells.");
        }
    }

    class Stage12Act1States : StateMachineBuilder
    {
        public Stage12Act1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>();
        }
    }

    [ModuleInfo(CFCID = 622, NameID = 8103)]
    public class Stage12Act1 : BossModule
    {
        public Stage12Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
