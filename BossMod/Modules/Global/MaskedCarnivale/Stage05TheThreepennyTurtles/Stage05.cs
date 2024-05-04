namespace BossMod.Global.MaskedCarnivale.Stage05;

public enum OID : uint
{
    Boss = 0x25CC, //R=5.0
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 615, NameID = 8089)]
public class Stage05 : BossModule
{
    public Stage05(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
