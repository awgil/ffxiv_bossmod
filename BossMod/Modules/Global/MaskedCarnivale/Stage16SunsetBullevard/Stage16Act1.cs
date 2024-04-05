namespace BossMod.Global.MaskedCarnivale.Stage16.Act1;

public enum OID : uint
{
    Boss = 0x26F2, //R=3.2
};

public enum AID : uint
{
    Attack = 6497, // 26F2->player, no cast, single-target
};

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add("The cyclops are very slow, but will instantly kill you, if they catch you.\nKite them or kill them with the self-destruct combo. (Toad Oil->Bristle->\nMoonflute->Swiftcast->Self-destruct) If you don't use the self-destruct\ncombo in act 1, you can bring the Final Sting combo for act 2.\n(Off-guard->Bristle->Moonflute->Final Sting)\nDiamondback is highly recommended in act 2.");
    }
}

class Stage16Act1States : StateMachineBuilder
{
    public Stage16Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 626, NameID = 8112, SortOrder = 1)]
public class Stage16Act1 : BossModule
{
    public Stage16Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
