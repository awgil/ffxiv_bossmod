namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D110Alicanto;

public enum OID : uint
{
    Boss = 0x1818, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Whipcrack = 7128, // Boss->player, no cast, single-target
    Stormwind = 7129, // Boss->self, 3.0s cast, range 12+R 90-degree cone
    Bombination = 7130, // Boss->self, 3.0s cast, range 6+R circle
    Lumisphere = 7131, // Boss->location, 3.0s cast, range 6 circle
    AeroBlast = 7132, // Boss->self, 3.0s cast, range 40+R circle
}

class AeroBlast(BossModule module) : Components.RaidwideCast(module, AID.AeroBlast);
class Bombination(BossModule module) : Components.StandardAOEs(module, AID.Bombination, new AOEShapeCircle(12));
class Lumisphere(BossModule module) : Components.StandardAOEs(module, AID.Lumisphere, 6);
class Stormwind(BossModule module) : Components.StandardAOEs(module, AID.Stormwind, new AOEShapeCone(18, 45.Degrees()));

class D110AlicantoStates : StateMachineBuilder
{
    public D110AlicantoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AeroBlast>()
            .ActivateOnEnter<Bombination>()
            .ActivateOnEnter<Lumisphere>()
            .ActivateOnEnter<Stormwind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 209, NameID = 5371)]
public class D110Alicanto(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -235), new ArenaBoundsCircle(24));
