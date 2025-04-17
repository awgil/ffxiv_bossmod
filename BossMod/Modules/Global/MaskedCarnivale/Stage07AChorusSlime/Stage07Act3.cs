namespace BossMod.Global.MaskedCarnivale.Stage07.Act3;

public enum OID : uint
{
    Boss = 0x2706, //R=5.0
    Slime = 0x2707, //R=0.8
}

public enum AID : uint
{
    LowVoltage = 14710, // 2706->self, 12.0s cast, range 30+R circle - can be line of sighted by barricade
    Detonation = 14696, // 2707->self, no cast, range 6+R circle
    Object130 = 14711, // 2706->self, no cast, range 30+R circle - instant kill if you do not line of sight the towers when they die
}

class LowVoltage(BossModule module) : Components.GenericLineOfSightAOE(module, AID.LowVoltage, 35, true); //TODO: find a way to use the obstacles on the map and draw proper AOEs, this does nothing right now

class SlimeExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Module.Enemies(OID.Slime).Where(x => !x.IsDead))
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(p.Position, 7.6f, 0xFF000000, 2);
            Arena.AddCircle(p.Position, 7.6f, ArenaColor.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var p in Module.Enemies(OID.Slime).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 7.5f))
                hints.Add("In slime explosion radius!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Pull or push the Lava Slimes to the towers and then hit the slimes\nfrom a distance to set off the explosions. The towers create a damage\npulse every 12s and a deadly explosion when they die. Take cover.");
    }
}

class Stage07Act3States : StateMachineBuilder
{
    public Stage07Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LowVoltage>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Slime).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 617, NameID = 8095, SortOrder = 3)]
public class Stage07Act3 : BossModule
{
    public Stage07Act3(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
        ActivateComponent<Layout2Corners>();
        ActivateComponent<SlimeExplosion>();
    }

    protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Slime).Any(e => e.InCombat); }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Slime))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
