namespace BossMod.Stormblood.Foray.NM.Daphne;

public enum OID : uint
{
    Boss = 0x2744, // R6.875, x1
    Tentacle = 0x276E, // R7.200, x0 (spawn during fight)
    Helper1 = 0x276A, // R0.500, x0 (spawn during fight)
    Helper2 = 0x276B, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    SpellwindCast = 15031, // Boss->self, 4.0s cast, single-target
    SpellwindAOE = 15032, // Helper1->location, no cast, range 40 circle
    Upburst = 15025, // Tentacle->self, 3.5s cast, range 8 circle
    RoilingReach = 15029, // Boss->self, 4.5s cast, range 32 width 7 cross
    Wallop = 15027, // Tentacle->self, 4.0s cast, range 50 width 7 rect
    ChillingGlare = 15030, // Boss->self, 4.0s cast, range 40 circle
}

class Spellwind(BossModule module) : Components.RaidwideCast(module, AID.SpellwindCast);
class Upburst(BossModule module) : Components.StandardAOEs(module, AID.Upburst, new AOEShapeCircle(8));
class RoilingReach(BossModule module) : Components.StandardAOEs(module, AID.RoilingReach, new AOEShapeCross(32, 3.5f));
class Wallop(BossModule module) : Components.StandardAOEs(module, AID.Wallop, new AOEShapeRect(50, 3.5f));
class ChillingGlare(BossModule module) : Components.CastGaze(module, AID.ChillingGlare);

class DaphneStates : StateMachineBuilder
{
    public DaphneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spellwind>()
            .ActivateOnEnter<Upburst>()
            .ActivateOnEnter<RoilingReach>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<ChillingGlare>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1417, Contributors = "xan", SortOrder = 5)]
public class Daphne(WorldState ws, Actor primary) : BossModule(ws, primary, new(207.8475f, -736.8179f), new ArenaBoundsCircle(80, MapResolution: 1));
