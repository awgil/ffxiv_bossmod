namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D062Harmachis;

public enum OID : uint
{
    Boss = 0xE9A, // R2.000-5.300, x1, Depends on which form they're in
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BallisticMissileB = 4334, // Boss->self, 3.0s cast, single-target , done
    BallisticMissileH = 4335, // Helper->self, no cast, ???
    BallisticMissileVisual = 4771, // Helper->self, 4.0s cast, range 4 circle 

    WeighingOfTheHeartNaga = 3790, // Boss->self, 3.0s cast, single-target (Snake form, Gaze -> x2 CircleofFlames)
    WeighingOfTheHeartMachina = 3792, // Boss->self, 3.0s cast, single-target (2 Player Enum -> Stack)
    WeighingOfTheHeartCobra = 4328, // Boss->self, 3.0s cast, single-target (Damage up form + Cleave)
    WeighingOfTheHeartFormSphinx = 5007, // Helper->self, no cast, single-target 

    ChthonicHush = 4327, // Boss->self, no cast, range 12+R (R=5.3) 120-degree cone
    CircleOfFlames = 4332, // Boss->player, no cast, range 5 circle
    GaseousBomb = 4336, // Boss->player, no cast, range 5 circle
    HoodSwing = 4329, // Boss->self, no cast, range 8+R ?-degree cone
    InertiaStream = 4333, // Boss->player, no cast, single-target
    Ka = 4326, // Boss->self, 3.0s cast, range 40+R 60-degree cone
    Paradox = 4325, // Helper->location, 3.0s cast, range 5 circle
    Petrifaction = 4331, // Boss->self, 3.0s cast, range 60 circle
    RiddleOfTheSphinx = 4324, // Boss->self, 3.0s cast, single-target
    SteelScales = 4330, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    Transfiguration = 705, // Boss->Boss, extra=0x1D/0x1E/0x1F
    DamageUp = 443, // Boss->Boss, extra=0x1
    Poison = 2104, // Boss->player, extra=0x0
    Bind = 2518 // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Enumeration = 382, // Helper, 2 person enum
    Stack = 93 // player
}

class BallisticMissile(BossModule module) : Components.UniformStackSpread(module, 4, 0, 2, 2)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bind)
            AddStack(actor, WorldState.FutureTime(6.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BallisticMissileH)
            Stacks.Clear();
    }
}

class ChthonicHush(BossModule module) : Components.Cleave(module, AID.ChthonicHush, new AOEShapeCone(13.3f, 60.Degrees()))
{
    private readonly GasousBomb _stack1 = module.FindComponent<GasousBomb>()!;
    private readonly BallisticMissile _stack2 = module.FindComponent<BallisticMissile>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_stack1.ActiveStacks.Any() && !_stack2.ActiveStacks.Any())
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_stack1.ActiveStacks.Any() && !_stack2.ActiveStacks.Any())
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_stack1.ActiveStacks.Any() && !_stack2.ActiveStacks.Any())
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class CircleofFlame(BossModule module) : Components.UniformStackSpread(module, 0, 5, 0, 0, true)
{
    public int NumCasts { get; private set; }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Petrifaction && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            foreach (var actor in WorldState.Party.WithoutSlot())
            {
                AddSpread(actor);
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {

        if ((AID)spell.Action.ID is AID.CircleOfFlames)
            ++NumCasts;

        if ((AID)spell.Action.ID is AID.CircleOfFlames && NumCasts >= 2)
        {
            // reset for next usage of mechanic, thanks xan for the tip on how tf to fix this/dealing with me asking questions
            NumCasts = 0;
            Spreads.Clear();
        }
    }
}

class FormNaga(BossModule module) : Components.RaidwideCast(module, AID.WeighingOfTheHeartNaga, "Naga Form, Gaze -> x2 Spread AOE's");
class FormMachina(BossModule module) : Components.RaidwideCast(module, AID.WeighingOfTheHeartMachina, "Machina form, 2 Player Enum/Stack -> Party Stack");
class FormCobra(BossModule module) : Components.RaidwideCast(module, AID.WeighingOfTheHeartCobra, "Cobra form, damage up + cleave");
class GasousBomb(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.GaseousBomb, 5, 4.1f, 4, 4);
class Ka(BossModule module) : Components.StandardAOEs(module, AID.Ka, new AOEShapeCone(45, 30.Degrees()));
class Paradox(BossModule module) : Components.StandardAOEs(module, AID.Paradox, 5);
class Petrifaction(BossModule module) : Components.CastGaze(module, AID.Petrifaction);

class D062HarmachisStates : StateMachineBuilder
{
    public D062HarmachisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BallisticMissile>()
            .ActivateOnEnter<GasousBomb>()
            .ActivateOnEnter<Paradox>()
            .ActivateOnEnter<Petrifaction>()
            .ActivateOnEnter<Ka>()
            .ActivateOnEnter<ChthonicHush>()
            .ActivateOnEnter<CircleofFlame>()
            .ActivateOnEnter<FormNaga>()
            .ActivateOnEnter<FormMachina>()
            .ActivateOnEnter<FormCobra>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3821)]
public class D062Harmachis(WorldState ws, Actor primary) : BossModule(ws, primary, new(248, 272), new ArenaBoundsCircle(20));
