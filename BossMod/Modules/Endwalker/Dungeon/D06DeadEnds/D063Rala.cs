namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D063Rala;

public enum OID : uint
{
    Boss = 0x34C7, // R=4.75
    Helper = 0x233C,
    GoldenWings = 0x34C8, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Benevolence1 = 25945, // Boss->self, 5.0s cast, single-target
    BenevolenceStack = 25946, // Helper->players, 5.4s cast, range 6 circle //Stack mechanic
    LamellarLight1 = 25939, // Helper->self, 6.0s cast, range 15 circle
    LamellarLight2 = 25942, // GoldenWings->self, 3.0s cast, single-target
    LamellarLight3 = 25951, // Helper->self, 3.0s cast, range 40 width 4 rect //LineAOE
    Lifesbreath = 25940, // Boss->self, 4.0s cast, range 50 width 10 rect
    LovingEmbraceLeft = 25943, // Boss->self, 7.0s cast, range 45 180-degree cone //Cleave
    LovingEmbraceRight = 25944, // Boss->self, 7.0s cast, range 45 180-degree cone //Cleave
    Pity = 25949, // Boss->player, 5.0s cast, single-target //Tankbuster
    Prance1 = 25937, // Boss->location, 5.0s cast, single-target
    Prance2 = 25938, // Boss->location, no cast, single-target
    StillEmbrace1 = 25947, // Boss->self, 5.0s cast, single-target
    StillEmbraceSpread = 25948, // Helper->player, 5.4s cast, range 6 circle //Spread mechanic
    Unknown = 25941, // Boss->location, no cast, single-target
    WarmGlow = 25950, // Boss->self, 5.0s cast, range 40 circle //Raidwide
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper->player, extra=0x1
    UnknownStatus = 2056, // none->34C8, extra=0x16C
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon218 = 218, // player
    Icon62 = 62, // player
    Icon139 = 139, // player
}
class LamellarLight1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LamellarLight1), new AOEShapeCircle(15));
class Lifesbreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Lifesbreath), new AOEShapeRect(50, 5));
class LamellarLight3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LamellarLight3), new AOEShapeRect(40, 2));
class StillEmbraceSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.StillEmbraceSpread), 6);
class BenevolenceStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BenevolenceStack), 6, 8);

class LovingEmbraceLeft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LovingEmbraceLeft), new AOEShapeCone(45, 90.Degrees()));
class LovingEmbraceRight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LovingEmbraceRight), new AOEShapeCone(45, 90.Degrees()));

class Pity(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Pity));
class WarmGlow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WarmGlow));

class D063RalaStates : StateMachineBuilder
{
    public D063RalaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LamellarLight1>()
            .ActivateOnEnter<LamellarLight3>()
            .ActivateOnEnter<Lifesbreath>()
            .ActivateOnEnter<StillEmbraceSpread>()
            .ActivateOnEnter<BenevolenceStack>()
            .ActivateOnEnter<LovingEmbraceLeft>()
            .ActivateOnEnter<LovingEmbraceRight>()
            .ActivateOnEnter<Pity>()
            .ActivateOnEnter<WarmGlow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10316)]
public class D063Rala(WorldState ws, Actor primary) : BossModule(ws, primary, new(-380, -135), new ArenaBoundsCircle(20));
