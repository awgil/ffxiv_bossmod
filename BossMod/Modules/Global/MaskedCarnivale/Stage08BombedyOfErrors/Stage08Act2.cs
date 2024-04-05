namespace BossMod.Global.MaskedCarnivale.Stage08.Act2;

public enum OID : uint
{
    Boss = 0x270B, //R=3.75
    Bomb = 0x270C, //R=0.6
    Snoll = 0x270D, //R=0.9
};

public enum AID : uint
{
    Attack = 6499, // 270C/270B->player, no cast, single-target
    SelfDestruct = 14730, // 270C->self, no cast, range 6 circle
    HypothermalCombustion = 14731, // 270D->self, no cast, range 6 circle
    Sap = 14708, // 270B->location, 5,0s cast, range 8 circle
    Burst = 14680, // 270B->self, 6,0s cast, range 50 circle
};

class Sap : Components.LocationTargetedAOEs
{
    public Sap() : base(ActionID.MakeSpell(AID.Sap), 8) { }
}

class Burst : Components.CastHint
{
    public Burst() : base(ActionID.MakeSpell(AID.Burst), "Interrupt or wipe!") { }
}

class Selfdetonations : BossComponent
{
    private static readonly string hint = "In bomb explosion radius!";
    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var p in module.Enemies(OID.Bomb).Where(x => !x.IsDead))
        {
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddCircle(p.Position, 10, 0xFF000000, 2);
            arena.AddCircle(p.Position, 10, ArenaColor.Danger);
        }
        foreach (var p in module.Enemies(OID.Snoll).Where(x => !x.IsDead))
        {
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddCircle(p.Position, 6, 0xFF000000, 2);
            arena.AddCircle(p.Position, 6, ArenaColor.Danger);
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        foreach (var p in module.Enemies(OID.Bomb).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 10))
                hints.Add(hint);
        foreach (var p in module.Enemies(OID.Snoll).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 6))
                hints.Add(hint);
    }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add("Clever activation of cherry bombs will freeze the Progenitrix.\nInterrupt its burst skill or wipe. The Progenitrix is weak to wind spells.");
    }
}

class Stage08Act2States : StateMachineBuilder
{
    public Stage08Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Sap>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Bomb).All(e => e.IsDead) && module.Enemies(OID.Snoll).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 618, NameID = 8098, SortOrder = 2)]
public class Stage08Act2 : BossModule
{
    public Stage08Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
    {
        ActivateComponent<Hints>();
        ActivateComponent<Layout2Corners>();
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
