namespace BossMod.Global.MaskedCarnivale.Stage13.Act1;

public enum OID : uint
{
    Boss = 0x26F5, //R=1.4
    Vodoriga = 0x26F6, //R=1.2
};

public enum AID : uint
{
    Attack = 6497, // Boss/Vodoriga->player, no cast, single-target
    Mow = 14879, // Boss->self, 3,0s cast, range 6+R 120-degree cone
};

class Mow : Components.SelfTargetedAOEs
{
    public Mow() : base(ActionID.MakeSpell(AID.Mow), new AOEShapeCone(7.4f, 60.Degrees())) { }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add("The first act is trivial, almost anything will work.\nFor act 2 having Flying Sardine is recommended.");
    }
}

class Stage13Act1States : StateMachineBuilder
{
    public Stage13Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Mow>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Vodoriga).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 623, NameID = 8104, SortOrder = 1)]
public class Stage13Act1 : BossModule
{
    public Stage13Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
    {
        ActivateComponent<Hints>();
    }

    protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Vodoriga).Any(e => e.InCombat); }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Vodoriga))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
