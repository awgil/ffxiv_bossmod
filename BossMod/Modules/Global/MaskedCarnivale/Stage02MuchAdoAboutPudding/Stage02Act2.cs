namespace BossMod.Global.MaskedCarnivale.Stage02.Act2;

public enum OID : uint
{
    Boss = 0x25C1, //R1.8
    Flan = 0x25C5, //R1.8
    Licorice = 0x25C3, //R=1.8

}

public enum AID : uint
{
    Water = 14271, // 25C5->player, 1.0s cast, single-target
    Stone = 14270, // 25C3->player, 1.0s cast, single-target
    Blizzard = 14267, // 25C1->player, 1.0s cast, single-target
    GoldenTongue = 14265, // 25C5/25C3/25C1->self, 5.0s cast, single-target
}

class GoldenTongue(BossModule module) : Components.CastHint(module, AID.GoldenTongue, "Can be interrupted, increases its magic damage.");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Gelato is weak to fire spells.\nFlan is weak to lightning spells.\nLicorice is weak to water spells.");
    }
}

class Stage02Act2States : StateMachineBuilder
{
    public Stage02Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GoldenTongue>()
            .ActivateOnEnter<Hints>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Flan).All(e => e.IsDead) && module.Enemies(OID.Licorice).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 612, NameID = 8079, SortOrder = 2)]
public class Stage02Act2(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
{
    protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Flan).Any(e => e.InCombat) || Enemies(OID.Licorice).Any(e => e.InCombat); }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Flan))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Licorice))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
