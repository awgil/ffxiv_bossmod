namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D50EddaBlackbosom;

public enum OID : uint
{
    Boss = 0x16C6, // R1.500, x1
    DemonButler = 0x16E9, // R1.200, x4
    GargoyleSteward = 0x16E8, // R2.300, x4
    // going to assume these are the seals that get's placed/activated on the arena when you fuck up the mechanic? Doesn't appear on the base map so
    Actor1ea13a = 0x1EA13A, // R2.000, x1, EventObj type
    Actor1ea13b = 0x1EA13B, // R2.000, x1, EventObj type
    Actor1ea139 = 0x1EA139, // R2.000, x1, EventObj type
    Actor1ea137 = 0x1EA137, // R2.000, x1, EventObj type
    Actor1ea13c = 0x1EA13C, // R2.000, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1ea138 = 0x1EA138, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    BlackHoneymoon = 6402, // Boss->location, 3.0s cast, range 40 circle
    ColdFeet = 6403, // Boss->self, 3.0s cast, range 40 circle // gaze mechanic
    DarkHarvest = 6400, // Boss->player, 2.0s cast, single-target
    Desolation = 6404, // GargoyleSteward->self, 4.0s cast, range 55+R width 6 rect
    InHealthCircle = 6398, // Boss->self, 4.5s cast, range 16 circle
    InHealthDonut = 6399, // Boss->self, 4.5s cast, range 50+R circle // actually a donut, inner is ~2.5f
    TerrorEye = 6405, // DemonButler->location, 4.0s cast, range 6 circle
}

class BlackHoneymoon(BossModule module) : Components.RaidwideCast(module, AID.BlackHoneymoon);
class ColdFeet(BossModule module) : Components.CastGaze(module, AID.ColdFeet);
class DarkHarvest(BossModule module) : Components.SingleTargetCast(module, AID.DarkHarvest, "Tankbuster");
class Desolation(BossModule module) : Components.StandardAOEs(module, AID.Desolation, new AOEShapeRect(57.3f, 3));
class InHeathCircle(BossModule module) : Components.StandardAOEs(module, AID.InHealthCircle, new AOEShapeCircle(16));
class InHeathDonut(BossModule module) : Components.StandardAOEs(module, AID.InHealthDonut, new AOEShapeDonut(2.5f, 50));
class TerrorEye(BossModule module) : Components.StandardAOEs(module, AID.TerrorEye, 6);

class D50EddaBlackbosomStates : StateMachineBuilder
{
    public D50EddaBlackbosomStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlackHoneymoon>()
            .ActivateOnEnter<ColdFeet>()
            .ActivateOnEnter<DarkHarvest>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<InHeathCircle>()
            .ActivateOnEnter<InHeathDonut>()
            .ActivateOnEnter<TerrorEye>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 178, NameID = 5038)]
public class D50EddaBlackbosom(WorldState ws, Actor primary) : BossModule(ws, primary, new(300, 374), new ArenaBoundsCircle(24));
