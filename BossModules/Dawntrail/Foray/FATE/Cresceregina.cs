namespace BossMod.Dawntrail.Foray.FATE.Cresceregina;

public enum OID : uint
{
    Boss = 0x4D63,
    Helper = 0x233C,
    Cresceregina = 0x4EC3, // R0.500, x0 (spawn during fight)
    Cresceregina1 = 0x4EC4, // R0.500, x0 (spawn during fight)
    Cresceregina2 = 0x4EB1, // R0.500, x0 (spawn during fight)
    Cresceregina3 = 0x4D65, // R1.000, x0 (spawn during fight)
    BallOfLevin = 0x4D64, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50539, // Boss->player, no cast, single-target
    HighCaterwaul = 49499, // Cresceregina->self, 3.0s cast, single-target
    RegalFulguration = 49495, // Boss->self, 5.0s cast, range 40 180-degree cone
    RegalFulguration1 = 49494, // Boss->self, 5.0s cast, range 40 180-degree cone
    Thunderbolt = 49500, // 4EB1/4EC3/4EC4->location, 3.5s cast, range 10 circle
    NobleBlaster = 49501, // 4D64->self, 3.5s cast, range 50 width 5 rect
    ThunderboltPuddle = 49502, // 4D65->location, 5.0s cast, range 10 circle
    ThunderboltPuddle1 = 49919, // 4D65->location, 5.5s cast, range 10 circle
    ThunderboltPuddle2 = 49920, // 4D65->location, 6.0s cast, range 10 circle
    ThunderboltPuddle3 = 49921, // 4D65->location, 6.5s cast, range 10 circle
    ThunderboltPuddle4 = 49922, // 4D65->location, 7.0s cast, range 10 circle
    ThunderboltPuddle5 = 49923, // 4D65->location, 7.5s cast, range 10 circle
    ThunderboltPuddle6 = 49924, // 4D65->location, 8.0s cast, range 10 circle
    ThunderboltPuddle7 = 49925, // 4D65->location, 8.5s cast, range 10 circle
    ThunderboltPuddle8 = 49926, // 4D65->location, 9.0s cast, range 10 circle
}

class RegalFulguration(BossModule module) : Components.GroupedAOEs(module, [AID.RegalFulguration, AID.RegalFulguration1], new AOEShapeCone(40, 90.Degrees()));
class Thunderbolt(BossModule module) : Components.StandardAOEs(module, AID.Thunderbolt, 10);
class NobleBlaster(BossModule module) : Components.StandardAOEs(module, AID.NobleBlaster, new AOEShapeRect(50, 2.5f));
class ThunderboltPuddle(BossModule module) : Components.GroupedAOEs(module, [AID.ThunderboltPuddle, AID.ThunderboltPuddle1, AID.ThunderboltPuddle2, AID.ThunderboltPuddle3, AID.ThunderboltPuddle4, AID.ThunderboltPuddle5, AID.ThunderboltPuddle6, AID.ThunderboltPuddle7, AID.ThunderboltPuddle8], new AOEShapeCircle(10), 5, true);

class CrescereginaStates : StateMachineBuilder
{
    public CrescereginaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RegalFulguration>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<NobleBlaster>()
            .ActivateOnEnter<ThunderboltPuddle>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14785)]
public class Cresceregina(WorldState ws, Actor primary) : BossModule(ws, primary, new(140, -708.500f), new ArenaBoundsCircle(40));
