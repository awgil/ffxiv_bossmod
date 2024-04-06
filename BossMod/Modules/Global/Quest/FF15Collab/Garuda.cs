namespace BossMod.Global.Quest.FF15Collab.Garuda;

public enum OID : uint
{
    Boss = 0x257A, //R=1.7
    Helper = 0x233C,
    Monolith = 0x2654, //R=2.3
    Noctis = 0x2651,
    GravityVoidzone = 0x1E91C1,
    Turbulence = 0x2653, //cage just before quicktime event
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    MistralShriek = 14611, // Boss->self, 7,0s cast, range 30 circle
    MistralSong = 14616, // Boss->self, 3,5s cast, range 20 150-degree cone
    MiniSupercell = 14588, // Helper->Noctis, no cast, single-target, linestack target
    MiniSupercell2 = 14612, // Boss->self, 5,0s cast, range 45 width 6 rect, line stack, knockback 50, away from source
    GravitationalForce = 14614, // Boss->self, 3,5s cast, single-target
    GravitationalForce2 = 14615, // Helper->location, 3,5s cast, range 5 circle
    Vortex = 14677, // Helper->self, no cast, range 50 circle
    Vortex2 = 14620, // Helper->self, no cast, range 50 circle
    Vortex3 = 14622, // Helper->self, no cast, range 50 circle
    Vortex4 = 14623, // Helper->self, no cast, range 50 circle
    Microburst = 14619, // Boss->self, 17,3s cast, range 25 circle
    GustFront = 14617, // Boss->self, no cast, single-target, dorito stack
    GustFront2 = 14618, // Helper->player/Noctis, no cast, single-target
    WickedTornado = 14613, // Boss->self, 3,5s cast, range 8-20 donut
    MistralGaol = 14621, // Boss->self, 5,0s cast, range 6 circle, quick time event starts
    Microburst2 = 14624, // Boss->self, no cast, range 25 circle, quick time event failed (enrage)
    warpstrike = 14597, //duty action for player
};

class GustFront : Components.UniformStackSpread
{
    public GustFront() : base(1.2f, 0) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GustFront)
            AddStack(module.Enemies(OID.Noctis).FirstOrDefault()!);
        if ((AID)spell.Action.ID == AID.GustFront2)
            Stacks.Clear();
    }
}

class Microburst : Components.SelfTargetedAOEs
{
    private bool casting;

    public Microburst() : base(ActionID.MakeSpell(AID.Microburst), new AOEShapeCircle(18)) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(module, caster, spell);
        if ((AID)spell.Action.ID == AID.Microburst)
            casting = true;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(module, caster, spell);
        if ((AID)spell.Action.ID == AID.Microburst)
            casting = false;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (casting)
            hints.Add($"Keep using duty action on the {module.Enemies(OID.Monolith).FirstOrDefault()!.Name}s to stay out of the AOE!");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (casting && actor.Position.AlmostEqual(module.PrimaryActor.Position, 15))
            hints.PlannedActions.Add((ActionID.MakeSpell(AID.warpstrike), module.Enemies(OID.Monolith).FirstOrDefault()!, 1, false));
    }
}

class MistralShriek : Components.SelfTargetedAOEs
{
    private bool casting;
    private DateTime done;

    public MistralShriek() : base(ActionID.MakeSpell(AID.MistralShriek), new AOEShapeCircle(30)) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(module, caster, spell);
        if ((AID)spell.Action.ID == AID.MistralShriek)
            casting = true;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(module, caster, spell);
        if ((AID)spell.Action.ID == AID.MistralShriek)
            casting = false;
        done = module.WorldState.CurrentTime;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (casting)
            hints.Add($"Use duty action to teleport to the {module.Enemies(OID.Monolith).FirstOrDefault()!.Name} at the opposite side of Garuda!");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (casting)
            hints.PlannedActions.Add((ActionID.MakeSpell(AID.warpstrike), module.Enemies(OID.Monolith).Where(p => !p.Position.AlmostEqual(module.PrimaryActor.Position, 5)).FirstOrDefault()!, 1, false));
        if (module.WorldState.CurrentTime > done && module.WorldState.CurrentTime < done.AddSeconds(2))
            hints.PlannedActions.Add((ActionID.MakeSpell(AID.warpstrike), module.PrimaryActor, 1, false));
    }
}

class MistralSong : Components.SelfTargetedAOEs
{
    public MistralSong() : base(ActionID.MakeSpell(AID.MistralSong), new AOEShapeCone(20, 75.Degrees())) { }
}

class WickedTornado : Components.SelfTargetedAOEs
{
    public WickedTornado() : base(ActionID.MakeSpell(AID.WickedTornado), new AOEShapeDonut(8, 20)) { }
}

// TODO: create and use generic 'line stack' component
class MiniSupercell : Components.GenericBaitAway
{
    private Actor? target;

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MiniSupercell)
        {
            target = module.WorldState.Actors.Find(spell.MainTargetID);
            CurrentBaits.Add(new(module.PrimaryActor, target!, new AOEShapeRect(45, 3)));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MiniSupercell2)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0 && actor != target)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(module.PrimaryActor.Position, (target!.Position - module.PrimaryActor.Position).Normalized(), 45, 0, 3));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (CurrentBaits.Count > 0)
        {
            if (!actor.Position.InRect(module.PrimaryActor.Position, (target!.Position - module.PrimaryActor.Position).Normalized(), 45, 0, 3))
                hints.Add("Stack!");
            else
                hints.Add("Stack!", false);
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var bait in CurrentBaits)
            bait.Shape.Draw(arena, BaitOrigin(bait), bait.Rotation, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena) { }
}

class MiniSupercellKB : Components.KnockbackFromCastTarget
{
    public MiniSupercellKB() : base(ActionID.MakeSpell(AID.MiniSupercell2), 50, shape: new AOEShapeRect(45, 3))
    {
        StopAtWall = true;
    }
}

class GravitationalForce : Components.PersistentVoidzoneAtCastTarget
{
    public GravitationalForce() : base(5, ActionID.MakeSpell(AID.GravitationalForce2), m => m.Enemies(OID.GravityVoidzone), 0) { }
}

class MistralGaol : Components.CastHint
{
    public MistralGaol() : base(ActionID.MakeSpell(AID.MistralGaol), "Prepare for Quick Time Event (spam buttons when it starts)") { }
}

class GarudaStates : StateMachineBuilder
{
    public GarudaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MistralShriek>()
            .ActivateOnEnter<GustFront>()
            .ActivateOnEnter<MistralSong>()
            .ActivateOnEnter<GravitationalForce>()
            .ActivateOnEnter<MiniSupercell>()
            .ActivateOnEnter<MiniSupercellKB>()
            .ActivateOnEnter<Microburst>()
            .ActivateOnEnter<WickedTornado>()
            .ActivateOnEnter<MistralGaol>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68696, NameID = 7893)] // also: CFC 646
public class Garuda : BossModule
{
    public Garuda(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 22)) { }
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Noctis))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.Monolith))
            Arena.Actor(s, ArenaColor.Object);
    }
}
