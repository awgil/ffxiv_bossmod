namespace BossMod.Global.MaskedCarnivale.Stage07.Act1;

public enum OID : uint
{
    Boss = 0x2703, //R=1.6
    Sprite = 0x2702 //R=0.8
}

public enum AID : uint
{
    Detonation = 14696, // Boss->self, no cast, range 6+R circle
    Blizzard = 14709 // Sprite->player, 1.0s cast, single-target
}

sealed class SlimeExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Module.PrimaryActor.IsDead)
        {
            Arena.ZoneCircleOutline(Module.PrimaryActor.Position, 7.6f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Module.PrimaryActor.IsDead)
        {
            if (actor.Position.InCircle(Module.PrimaryActor.Position, 7.6f))
            {
                hints.Add("In slime explosion radius!");
            }
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("For this stage the spells Sticky Tongue and Snort are recommended.\nUse them to pull or push Slimes close to Ice Sprites.\nThen hit the slime from a distance with anything but fire spells to set of an explosion.");
    }
}

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Hit the {Module.PrimaryActor.Name} from a safe distance to win this act.");
    }
}

sealed class Stage07Act1States : StateMachineBuilder
{
    public Stage07Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Hints2>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage07Act1.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 617, NameID = 8094, SortOrder = 1)]
public sealed class Stage07Act1 : BossModule
{
    public Stage07Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
        ActivateComponent<SlimeExplosion>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Sprite];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Sprite));
    }
}
