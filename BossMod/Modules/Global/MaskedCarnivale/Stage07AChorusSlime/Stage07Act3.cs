namespace BossMod.Global.MaskedCarnivale.Stage07.Act3;

public enum OID : uint
{
    Boss = 0x2706, //R=5.0
    Slime = 0x2707 //R=0.8
}

public enum AID : uint
{
    LowVoltage = 14710, // Boss->self, 12.0s cast, range 30+R circle - can be line of sighted by barricade
    Detonation = 14696, // Slime->self, no cast, range 6+R circle
    Object130 = 14711 // Boss->self, no cast, range 30+R circle - instant kill if you do not line of sight the towers when they die
}

sealed class LowVoltage(BossModule module) : Components.CastLineOfSightAOEComplex(module, (uint)AID.LowVoltage, Layouts.Layout2CornersBlockers, riskyWithSecondsLeft: 99d);

sealed class SlimeExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Actor> slimes = [with(6)];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Slime)
        {
            slimes.Add(actor);
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.Slime)
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
        hints.Add("Pull or push the Lava Slimes to the towers and then hit the slimes\nfrom a distance to set off the explosions. The towers create a damage\npulse every 12s and a deadly explosion when they die. Take cover.");
    }
}

sealed class Stage07Act3States : StateMachineBuilder
{
    public Stage07Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LowVoltage>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage07Act3.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 617, NameID = 8095, SortOrder = 3)]
public sealed class Stage07Act3 : BossModule
{
    public Stage07Act3(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.Layout2Corners)
    {
        ActivateComponent<Hints>();
        ActivateComponent<SlimeExplosion>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Slime];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
        Arena.Actors(Enemies((uint)OID.Slime));
    }
}
