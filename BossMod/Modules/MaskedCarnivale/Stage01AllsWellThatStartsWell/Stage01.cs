using System.Linq;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage01
{
    public enum OID : uint
    {
        Boss = 0x25BE, //R=1.5
        Slime = 0x25BD, //R=1.5
    };

    public enum AID : uint
    {
        AutoAttack = 6499, // 25BD->player, no cast, single-target
        FluidSpread = 14198, // 25BD->player, no cast, single-target
        AutoAttack2 = 6497, // 25BE->player, no cast, single-target
        IronJustice = 14199, // 25BE->self, 2,5s cast, range 8+R 120-degree cone
    };

    class IronJustice : Components.SelfTargetedAOEs
    {
        public IronJustice() : base(ActionID.MakeSpell(AID.IronJustice), new AOEShapeCone(9.5f, 60.Degrees())) { }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("This stage is trivial.\nUse whatever skills you have to defeat these opponents.");
        }
    }

    class Stage01States : StateMachineBuilder
    {
        public Stage01States(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<IronJustice>()
                .DeactivateOnEnter<Hints>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Slime).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 610, NameID = 8077)]
    public class Stage01 : BossModule
    {
        public Stage01(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Slime).Any(e => e.InCombat); }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Slime))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
