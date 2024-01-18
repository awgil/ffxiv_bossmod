namespace BossMod.MaskedCarnivale.Stage01.Act1.ArenaSlime
{
    public enum OID : uint
    {
        Boss = 0x25BD, //R=1.5
        Dullahan = 0x25BE, //R1.5
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
class Stage01SlimeStates : StateMachineBuilder
    {
        public Stage01SlimeStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<IronJustice>()
            .DeactivateOnEnter<Hints>();
        }
    }

public class Stage01Slime : BossModule
    {
        public Stage01Slime(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Dullahan))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
