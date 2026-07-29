namespace BossMod.Global.MaskedCarnivale.Stage07.Act2;

public enum OID : uint
{
    Boss = 0x2705, //R=1.6
    Sprite = 0x2704 //R=0.8
}

public enum AID : uint
{
    Detonation = 14696, // Boss->self, no cast, range 6+R circle
    Blizzard = 14709 // Sprite->player, 1.0s cast, single-target
}

sealed class SlimeExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Actor> slimes = [with(4)];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Boss)
        {
            slimes.Add(actor);
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.Boss)
        {
            slimes.Remove(actor);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = slimes.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(slimes[i].Position, 7.6f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = slimes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (actor.Position.InCircle(slimes[i].Position, 7.6f))
            {
                hints.Add("In slime explosion radius!");
                return;
            }
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Pull or push the Lava Slimes to the Ice Sprites and then hit the slimes\nfrom a distance to set of the explosions.");
    }
}

sealed class Stage07Act2States : StateMachineBuilder
{
    public Stage07Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => AllDeadOrDestroyed(Stage07Act2.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 617, NameID = 8094, SortOrder = 2)]
public sealed class Stage07Act2 : BossModule
{
    public Stage07Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.Layout4Quads)
    {
        ActivateComponent<Hints>();
        ActivateComponent<SlimeExplosion>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Sprite];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
        Arena.Actors(Enemies((uint)OID.Sprite));
    }
}
