namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D10PalaceDeathgaze;

public enum OID : uint
{
    Boss = 0x1692, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    AeroBlast = 6420, // Boss->self, 3.0s cast, range 40+R circle
    Bombination = 6418, // Boss->self, 3.0s cast, range 6+R circle
    Lumisphere = 6419, // Boss->location, 3.0s cast, range 6 circle
    Stormwind = 6417, // Boss->self, 3.0s cast, range 12+R 90-degree cone
    Whipcrack = 6416, // Boss->player, no cast, single-target
}

class AeroBlast(BossModule module) : Components.RaidwideCast(module, AID.AeroBlast);
class Bombination(BossModule module) : Components.StandardAOEs(module, AID.Bombination, new AOEShapeCircle(12));
class Lumisphere(BossModule module) : Components.StandardAOEs(module, AID.Lumisphere, 6);
class Stormwind(BossModule module) : Components.StandardAOEs(module, AID.Stormwind, new AOEShapeCone(18, 45.Degrees()));

class D10PalaceDeathgazeStates : StateMachineBuilder
{
    public D10PalaceDeathgazeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AeroBlast>()
            .ActivateOnEnter<Bombination>()
            .ActivateOnEnter<Lumisphere>()
            .ActivateOnEnter<Stormwind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 174, NameID = 4986)]
public class D10PalaceDeathgaze(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -220), new ArenaBoundsCircle(24));
