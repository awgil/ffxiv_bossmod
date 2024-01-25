// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage05
{
    public enum OID : uint
    {
        Boss = 0x25CC, //R=5.0
    };

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("These turtles have very high defenses.\nBring 1000 Needles or Doom to defeat them.\nAlternatively you can remove their buff with Eerie Soundwave.");
        }
    }

    class Stage05States : StateMachineBuilder
    {
        public Stage05States(BossModule module) : base(module)
        {
            TrivialPhase()
            .DeactivateOnEnter<Hints>();
        }
    }

    [ModuleInfo(CFCID = 615, NameID = 8089)]
    public class Stage05 : BossModule
    {
        public Stage05(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
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
