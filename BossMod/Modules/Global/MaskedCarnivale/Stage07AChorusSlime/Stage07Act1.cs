namespace BossMod.Global.MaskedCarnivale.Stage07.Act1;

public enum OID : uint
{
    Boss = 0x2703, //R=1.6
    Sprite = 0x2702, //R=0.8
}

public enum AID : uint
{
    Detonation = 14696, // 2703->self, no cast, range 6+R circle
    Blizzard = 14709, // 2702->player, 1.0s cast, single-target
}

class SlimeExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Module.PrimaryActor.IsDead)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(Module.PrimaryActor.Position, 7.6f, 0xFF000000, 2);
            Arena.AddCircle(Module.PrimaryActor.Position, 7.6f, ArenaColor.Danger);
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Module.PrimaryActor.IsDead)
            if (Module.PrimaryActor.Position.InCircle(Module.PrimaryActor.Position, 7.6f))
                hints.Add("In slime explosion radius!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("For this stage the spells Sticky Tongue and Snort are recommended.\nUse them to pull or push Slimes close toIce Sprites.\nThen hit the slime from a distance with anything but fire spells to set of an explosion.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Hit the Lava Slime from a safe distance to win this act.");
    }
}

class Stage07Act1States : StateMachineBuilder
{
    public Stage07Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Hints2>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Sprite).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 617, NameID = 8094, SortOrder = 1)]
public class Stage07Act1 : BossModule
{
    public Stage07Act1(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
        ActivateComponent<SlimeExplosion>();
    }

    protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Sprite).Any(e => e.InCombat); }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Sprite))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
