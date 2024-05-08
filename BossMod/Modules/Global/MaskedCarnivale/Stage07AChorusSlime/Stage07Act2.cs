namespace BossMod.Global.MaskedCarnivale.Stage07.Act2;

public enum OID : uint
{
    Boss = 0x2705, //R=1.6
    Sprite = 0x2704, //R=0.8
}

public enum AID : uint
{
    Detonation = 14696, // 2705->self, no cast, range 6+R circle
    Blizzard = 14709, // 2704->player, 1.0s cast, single-target
}

class SlimeExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Module.Enemies(OID.Boss).Where(x => !x.IsDead))
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(p.Position, 7.6f, 0xFF000000, 2);
            Arena.AddCircle(p.Position, 7.6f, ArenaColor.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var p in Module.Enemies(OID.Boss).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 7.5f))
                hints.Add("In slime explosion radius!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Pull or push the Lava Slimes to the Ice Sprites and then hit the slimes\nfrom a distance to set of the explosions.");
    }
}

class Layout(BossModule module) : Layout4Quads(module);

class Stage07Act2States : StateMachineBuilder
{
    public Stage07Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Sprite).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 617, NameID = 8094, SortOrder = 2)]
public class Stage07Act2 : BossModule
{
    public Stage07Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
        ActivateComponent<Layout>();
        ActivateComponent<SlimeExplosion>();
    }

    protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Sprite).Any(e => e.InCombat); }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Sprite))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
