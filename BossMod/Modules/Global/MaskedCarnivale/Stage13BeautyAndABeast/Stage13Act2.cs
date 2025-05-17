namespace BossMod.Global.MaskedCarnivale.Stage13.Act2;

public enum OID : uint
{
    Boss = 0x26F8, // R=2.0
    Succubus = 0x26F7, //R=1.0
    Helper = 0x233C, //R=0.5
}

public enum AID : uint
{
    Attack = 6497, // 26F8/26F7->player, no cast, single-target
    VoidFireII = 14880, // 26F8->location, 3.0s cast, range 5 circle
    VoidAero = 14881, // 26F8->self, 3.0s cast, range 40+R width 8 rect
    DarkSabbath = 14951, // 26F8->self, 3.0s cast, range 60 circle, gaze
    DarkMist = 14884, // 26F8->self, 3.0s cast, range 8+R circle
    CircleOfBlood = 15043, // 26F8->self, 3.0s cast, single-target
    CircleOfBlood2 = 14887, // 233C->self, 3.0s cast, range 10-20 donut
    VoidFireIV = 14888, // 26F8->location, 4.0s cast, range 10 circle
    VoidFireIV2 = 14886, // 26F8->self, no cast, single-target
    VoidFireIV3 = 14889, // 233C->location, 3.0s cast, range 6 circle
    SummonDarkness = 14885, // 26F8->self, no cast, single-target, summons succubus add
    BeguilingMist = 15045, // 26F7->self, 7.0s cast, range 50+R circle, interruptable, applies hysteria
    FatalAllure = 14952, // 26F8->self, no cast, range 50+R circle, attract, applies terror
    BloodRain = 14882, // 26F8->location, 3.0s cast, range 50 circle
}

class VoidFireII(BossModule module) : Components.StandardAOEs(module, AID.VoidFireII, 5);
class VoidFireIV(BossModule module) : Components.StandardAOEs(module, AID.VoidFireIV, 10);
class VoidFireIV3(BossModule module) : Components.StandardAOEs(module, AID.VoidFireIV3, 6);
class VoidAero(BossModule module) : Components.StandardAOEs(module, AID.VoidAero, new AOEShapeRect(42, 4));
class DarkSabbath(BossModule module) : Components.CastGaze(module, AID.DarkSabbath);
class DarkMist(BossModule module) : Components.StandardAOEs(module, AID.DarkMist, new AOEShapeCircle(10));
class CircleOfBlood(BossModule module) : Components.StandardAOEs(module, AID.CircleOfBlood2, new AOEShapeDonut(10, 20));
class BeguilingMist(BossModule module) : Components.CastHint(module, AID.BeguilingMist, "Interrupt or run around uncontrollably!");
class BloodRain(BossModule module) : Components.RaidwideCast(module, AID.BloodRain, "Harmless raidwide unless you failed to kill succubus in time");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Camilla will cast various AOEs and summons adds.\nInterrupt the adds with Flying Sardine and kill them fast.\nIf the add is still alive during the next Black Sabbath, you will be wiped.");
    }
}

class Stage13Act2States : StateMachineBuilder
{
    public Stage13Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidFireII>()
            .ActivateOnEnter<VoidFireIV>()
            .ActivateOnEnter<VoidFireIV3>()
            .ActivateOnEnter<VoidAero>()
            .ActivateOnEnter<DarkSabbath>()
            .ActivateOnEnter<DarkMist>()
            .ActivateOnEnter<CircleOfBlood>()
            .ActivateOnEnter<BeguilingMist>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 623, NameID = 8107, SortOrder = 2)]
public class Stage13Act2 : BossModule
{
    public Stage13Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Succubus))
            Arena.Actor(s, ArenaColor.Object);
    }
}
