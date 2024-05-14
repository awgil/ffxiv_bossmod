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
    BallisticMissileAOE = 4771, // Helper->self, 4.0s cast, range 4 circle

    ChthonicHush = 4327, // Boss->self, no cast, range 12+R ?-degree cone
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
    Icon_382 = 382, // Helper
    Stack = 93, // player
}
class Paradox(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Paradox), 5);
class HoodSwing(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.HoodSwing), new AOEShapeCone(12, 22.5f.Degrees()));
class Petrifaction(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Petrifaction));
class Ka(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Ka), new AOEShapeCone(45, 30.Degrees()));
class BallisticMissileAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BallisticMissileAOE), new AOEShapeCircle(4));
class GaseousBomb(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.GaseousBomb), 5, 2);

class D062HarmachisStates : StateMachineBuilder
{
    public D062HarmachisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Paradox>()
            //.ActivateOnEnter<HoodSwing>()
            .ActivateOnEnter<Petrifaction>()
            .ActivateOnEnter<Ka>()
            .ActivateOnEnter<GaseousBomb>()
            .ActivateOnEnter<BallisticMissileAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3821)]
public class D062Harmachis(WorldState ws, Actor primary) : BossModule(ws, primary, new(248, 272), new ArenaBoundsCircle(20));
