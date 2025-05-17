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
    MistralShriek = 14611, // Boss->self, 7.0s cast, range 30 circle
    MistralSong = 14616, // Boss->self, 3.5s cast, range 20 150-degree cone
    MiniSupercell = 14588, // Helper->Noctis, no cast, single-target, linestack target
    MiniSupercell2 = 14612, // Boss->self, 5.0s cast, range 45 width 6 rect, line stack, knockback 50, away from source
    GravitationalForce = 14614, // Boss->self, 3.5s cast, single-target
    GravitationalForce2 = 14615, // Helper->location, 3.5s cast, range 5 circle
    Vortex = 14677, // Helper->self, no cast, range 50 circle
    Vortex2 = 14620, // Helper->self, no cast, range 50 circle
    Vortex3 = 14622, // Helper->self, no cast, range 50 circle
    Vortex4 = 14623, // Helper->self, no cast, range 50 circle
    Microburst = 14619, // Boss->self, 17.3s cast, range 25 circle
    GustFront = 14617, // Boss->self, no cast, single-target, dorito stack
    GustFront2 = 14618, // Helper->player/Noctis, no cast, single-target
    WickedTornado = 14613, // Boss->self, 3.5s cast, range 8-20 donut
    MistralGaol = 14621, // Boss->self, 5.0s cast, range 6 circle, quick time event starts
    Microburst2 = 14624, // Boss->self, no cast, range 25 circle, quick time event failed (enrage)
    warpstrike = 14597, //duty action for player
}

class GustFront(BossModule module) : Components.UniformStackSpread(module, 1.2f, 0)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GustFront)
            AddStack(Module.Enemies(OID.Noctis).FirstOrDefault()!);
        if ((AID)spell.Action.ID == AID.GustFront2)
            Stacks.Clear();
    }
}

class Microburst(BossModule module) : Components.StandardAOEs(module, AID.Microburst, new AOEShapeCircle(18))
{
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.Microburst)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if ((AID)spell.Action.ID == AID.Microburst)
            casting = false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add($"Keep using duty action on the {Module.Enemies(OID.Monolith).FirstOrDefault()!.Name}s to stay out of the AOE!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (casting && actor.Position.AlmostEqual(Module.PrimaryActor.Position, 15))
            hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.warpstrike), Module.Enemies(OID.Monolith).FirstOrDefault()!, ActionQueue.Priority.High);
    }
}

class MistralShriek(BossModule module) : Components.StandardAOEs(module, AID.MistralShriek, new AOEShapeCircle(30))
{
    private bool casting;
    private DateTime done;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.MistralShriek)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if ((AID)spell.Action.ID == AID.MistralShriek)
            casting = false;
        done = WorldState.CurrentTime;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add($"Use duty action to teleport to the {Module.Enemies(OID.Monolith).FirstOrDefault()!.Name} at the opposite side of Garuda!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (casting)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.warpstrike), Module.Enemies(OID.Monolith).FirstOrDefault(p => !p.Position.AlmostEqual(Module.PrimaryActor.Position, 5))!, ActionQueue.Priority.High);
        if (WorldState.CurrentTime > done && WorldState.CurrentTime < done.AddSeconds(2))
            hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.warpstrike), Module.PrimaryActor, ActionQueue.Priority.High);
    }
}

class MistralSong(BossModule module) : Components.StandardAOEs(module, AID.MistralSong, new AOEShapeCone(20, 75.Degrees()));
class WickedTornado(BossModule module) : Components.StandardAOEs(module, AID.WickedTornado, new AOEShapeDonut(8, 20));

// TODO: create and use generic 'line stack' component
class MiniSupercell(BossModule module) : Components.GenericBaitAway(module)
{
    private Actor? target;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MiniSupercell)
        {
            target = WorldState.Actors.Find(spell.MainTargetID);
            CurrentBaits.Add(new(Module.PrimaryActor, target!, new AOEShapeRect(45, 3)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MiniSupercell2)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0 && actor != target)
            hints.AddForbiddenZone(ShapeContains.InvertedRect(Module.PrimaryActor.Position, (target!.Position - Module.PrimaryActor.Position).Normalized(), 45, 0, 3));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count > 0)
        {
            if (!actor.Position.InRect(Module.PrimaryActor.Position, (target!.Position - Module.PrimaryActor.Position).Normalized(), 45, 0, 3))
                hints.Add("Stack!");
            else
                hints.Add("Stack!", false);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var bait in CurrentBaits)
            bait.Shape.Draw(Arena, BaitOrigin(bait), bait.Rotation, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }
}

class MiniSupercellKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.MiniSupercell2, 50, shape: new AOEShapeRect(45, 3), stopAtWall: true);

class GravitationalForce(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.GravitationalForce2, m => m.Enemies(OID.GravityVoidzone), 0);
class MistralGaol(BossModule module) : Components.CastHint(module, AID.MistralGaol, "Prepare for Quick Time Event (spam buttons when it starts)");

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
public class Garuda(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(22))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Noctis))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.Monolith))
            Arena.Actor(s, ArenaColor.Object);
    }
}
