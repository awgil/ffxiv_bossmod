namespace BossMod.Heavensward.Dungeon.D06Aetherochemical.D062Harmachis;

public enum OID : uint
{
    Boss = 0xE9A, // R2.000-5.300, x1
    Helper = 0x1B2, // R0.500, x3
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BallisticMissile1 = 4334, // Boss->self, 3.0s cast, single-target
    BallisticMissile2 = 4335, // Helper->self, no cast, ???
    BallisticMissileVisual = 4771, // Helper->self, 4.0s cast, range 4 circle

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

    WeighingOfTheHeart1 = 3790, // Boss->self, 3.0s cast, single-target
    WeighingOfTheHeart2 = 3792, // Boss->self, 3.0s cast, single-target
    WeighingOfTheHeart3 = 4328, // Boss->self, 3.0s cast, single-target
    WeighingOfTheHeartSphinxForm = 5007, // Helper->self, no cast, single-target
}

public enum SID : uint
{
    Transfiguration = 705, // Boss->Boss, extra=0x1D/0x1E/0x1F
    DamageUp = 443, // Boss->Boss, extra=0x1
    Poison = 2104, // Boss->player, extra=0x0
    Bind = 2518, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Enumeration = 382, // Helper
    Stack = 93, // player
}

class Paradox(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Paradox), 5);
class ChthonicHush(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.ChthonicHush), new AOEShapeCone(13.3f, 60.Degrees()));
class Petrifaction(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Petrifaction));
class Ka(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Ka), new AOEShapeCone(45, 30.Degrees()));
class GaseousBomb(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, ActionID.MakeSpell(AID.GaseousBomb), 5, 4.1f);
class BallisticMissile(BossModule module) : Components.UniformStackSpread(module, 4, 0, 2, 2)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bind)
            AddStack(actor, Module.WorldState.FutureTime(6.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BallisticMissile2)
            Stacks.Clear();
    }
}

class D062HarmachisStates : StateMachineBuilder
{
    public D062HarmachisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Paradox>()
            .ActivateOnEnter<ChthonicHush>()
            .ActivateOnEnter<Petrifaction>()
            .ActivateOnEnter<Ka>()
            .ActivateOnEnter<GaseousBomb>()
            .ActivateOnEnter<BallisticMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3821)]
public class D062Harmachis(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(248, 272), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(228, 272), 20, 1.8f, 90.Degrees()), new Rectangle(new(268.25f, 272), 20, 2, 90.Degrees())];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);
}
