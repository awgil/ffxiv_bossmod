namespace BossMod.Endwalker.Dungeon.D10FellCourt.D101EvilDreamers;

public enum OID : uint
{
    EvilDreamer4 = 0x3966, // R2.400-4.800, x19
    EvilDreamer2 = 0x3967, // R2.400, x0-10 (spawn during fight)
    EvilDreamer1 = 0x3988, // R2.400-7.200, x1
    EvilDreamer3 = 0x3968, // R3.600, x0-7
}

public enum AID : uint
{
    AutoAttack = 30246, // EvilDreamer1/EvilDreamer3/EvilDreamer4->player, no cast, single-target
    UnknownAbility = 29623, // SmallerBoss->location, no cast, single-target
    UniteMare1 = 29621, // SmallerBoss->self, 11.0s cast, range 6 circle
    UniteMare2 = 29622, // SmallerBoss->self, 11.0s cast, range 6 circle
    UniteMare3 = 29628, // EvilDreamer1->self, 10.0s cast, range 12 circle
    DarkVision2 = 29627, // EvilDreamer4->self, 15.0s cast, range 41 width 5 rect
    DarkVision1 = 29624, // EvilDreamer4->self, 8.0s cast, range 40 width 5 rect
    UnknownAbility2 = 29629, // EvilDreamer4->location, no cast, single-target
    VoidGravity = 29626, // EvilDreamer4->player, 6.0s cast, range 6 circle
}

public enum SID : uint
{
    AreaOfInfluenceUp = 1749, // none->SmallerBoss, extra=0xC
    HPBoost = 483, // none->SmallerBoss, extra=0x0
}

public enum TetherID : uint
{
    Tether14 = 14, // SmallerBoss->SmallerBoss
}

class UniteMare1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UniteMare1), new AOEShapeCircle(6));
class UniteMare2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UniteMare2), new AOEShapeCircle(6));
class UniteMare3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UniteMare3), new AOEShapeCircle(12));
class DarkVision1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkVision1), new AOEShapeRect(40, 2.5f));
class DarkVision2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkVision2), new AOEShapeRect(41, 2.5f));
class VoidGravity(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidGravity), 6, 4);

class D101EvilDreamersStates : StateMachineBuilder
{
    public D101EvilDreamersStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<UniteMare1>()
            .ActivateOnEnter<UniteMare2>()
            .ActivateOnEnter<UniteMare3>()
            .ActivateOnEnter<DarkVision1>()
            .ActivateOnEnter<DarkVision2>()
            .ActivateOnEnter<VoidGravity>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.EvilDreamer1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11382)] // 11383
public class D101EvilDreamers(WorldState ws, Actor primary) : BossModule(ws, primary, new(168, 90), new ArenaBoundsCircle(20))
{
    private Actor? _evilDreamer2;
    private Actor? _evilDreamer3;
    private Actor? _evilDreamer4;

    public Actor? EvilDreamer1() => PrimaryActor;
    public Actor? EvilDreamer2() => _evilDreamer2;
    public Actor? EvilDreamer3() => _evilDreamer3;
    public Actor? EvilDreamer4() => _evilDreamer4;

    protected override void UpdateModule()
    {
        _evilDreamer2 ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.EvilDreamer2).FirstOrDefault() : null;
        _evilDreamer3 ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.EvilDreamer3).FirstOrDefault() : null;
        _evilDreamer4 ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.EvilDreamer4).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_evilDreamer2, ArenaColor.Enemy);
        Arena.Actor(_evilDreamer3, ArenaColor.Enemy);
        Arena.Actor(_evilDreamer4, ArenaColor.Enemy);
    }
}
