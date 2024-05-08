namespace BossMod.Global.MaskedCarnivale.Stage11.Act1;

public enum OID : uint
{
    Boss = 0x2718, //R=1.2
}

public enum AID : uint
{
    Fulmination = 14583, // 2718->self, 23.0s cast, range 60 circle
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("These bombs start self-destruction on combat start. Pull them together\nwith Sticky Tongue and attack them with anything to interrupt them.\nThey are weak against wind and strong against fire.");
    }
}

class Stage11Act1States : StateMachineBuilder
{
    public Stage11Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 621, NameID = 2280, SortOrder = 1)]
public class Stage11Act1 : BossModule
{
    public Stage11Act1(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
