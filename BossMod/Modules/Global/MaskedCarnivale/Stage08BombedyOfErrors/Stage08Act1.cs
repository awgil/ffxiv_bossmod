namespace BossMod.Global.MaskedCarnivale.Stage08.Act1;

public enum OID : uint
{
    Boss = 0x2708, //R=0.6
    Bomb = 0x2709, //R=1.2
    Snoll = 0x270A, //R=0.9
}

public enum AID : uint
{
    SelfDestruct = 14687, // 2708->self, no cast, range 10 circle
    HypothermalCombustion = 14689, // 270A->self, no cast, range 6 circle
    SelfDestruct2 = 14688, // 2709->self, no cast, range 6 circle
}

class Selfdetonations(BossModule module) : BossComponent(module)
{
    private const string hint = "In bomb explosion radius!";

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Module.PrimaryActor.IsDead)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(Module.PrimaryActor.Position, 10, 0xFF000000, 2);
            Arena.AddCircle(Module.PrimaryActor.Position, 10, ArenaColor.Danger);
        }
        foreach (var p in Module.Enemies(OID.Bomb).Where(x => !x.IsDead))
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(p.Position, 6, 0xFF000000, 2);
            Arena.AddCircle(p.Position, 6, ArenaColor.Danger);
        }
        foreach (var p in Module.Enemies(OID.Snoll).Where(x => !x.IsDead))
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(p.Position, 6, 0xFF000000, 2);
            Arena.AddCircle(p.Position, 6, ArenaColor.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Module.PrimaryActor.IsDead && actor.Position.InCircle(Module.PrimaryActor.Position, 10))
            hints.Add(hint);
        foreach (var p in Module.Enemies(OID.Bomb).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 6))
                hints.Add(hint);
        foreach (var p in Module.Enemies(OID.Snoll).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 6))
                hints.Add(hint);
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("For this stage the spell Flying Sardine to interrupt the Progenitrix in Act 2\nis highly recommended. Hit the Cherry Bomb from a safe distance\nwith anything but fire damage to set of a chain reaction to win this act.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Hit the Cherry Bomb from a safe distance to win this act.");
    }
}

class Stage08Act1States : StateMachineBuilder
{
    public Stage08Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Hints2>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Bomb).All(e => e.IsDead) && module.Enemies(OID.Snoll).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 618, NameID = 8140, SortOrder = 1)]
public class Stage08Act1 : BossModule
{
    public Stage08Act1(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
        ActivateComponent<Selfdetonations>();
    }

    protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Bomb).Any(e => e.InCombat) || Enemies(OID.Snoll).Any(e => e.InCombat); }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Bomb))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Snoll))
            Arena.Actor(s, ArenaColor.Enemy);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 1,
                OID.Snoll or OID.Bomb => 0,
                _ => 0
            };
        }
    }
}
