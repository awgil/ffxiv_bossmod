using System.Linq;

namespace BossMod.MaskedCarnivale.Stage01
{
    public enum OID : uint
    {
        Boss = 0x25BE, //R=1.5
        Slime = 0x25BD, //R1.5
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
        public IronJustice() : base(ActionID.MakeSpell(AID.IronJustice), new AOEShapeCone(8,60.Degrees())) { } 
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
        private static bool IsDead(Actor? actor) => actor == null || actor.IsDestroyed || actor.IsDead;
        public Stage01States(Stage01 module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<IronJustice>()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => IsDead(module.Dullahan()) && IsDead(module.Slime());
        }
    }

public class Stage01 : BossModule
    {
        public Actor? _Slime;
        public Actor? Dullahan() => PrimaryActor;
        public Actor? Slime() => _Slime;
        public Stage01(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Slime).Any(e => e.InCombat); }
        protected override void UpdateModule()
        {
            _Slime ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Slime).FirstOrDefault() : null;
        }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Slime))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
