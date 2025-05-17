namespace BossMod.Heavensward.Quest.ASpectacleForTheAges;

public enum OID : uint
{
    Boss = 0x154E,
    Tizona = 0x1552
}

public enum AID : uint
{
    FlamingTizona = 5763, // D25->location, 3.0s cast, range 6 circle
    TheCurse = 5765, // D25->self, 3.0s cast, range 7+R ?-degree cone
}

class FlamingTizona(BossModule module) : Components.StandardAOEs(module, AID.FlamingTizona, 6);
class TheCurse(BossModule module) : Components.StandardAOEs(module, AID.TheCurse, new AOEShapeDonutSector(2, 7, 90.Degrees()));

class Demoralize(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(0x1E9FA8).Where(e => e.EventState != 7));
class Tizona(BossModule module) : Components.Adds(module, (uint)OID.Tizona, 5);

class FlameGeneralAldynnStates : StateMachineBuilder
{
    public FlameGeneralAldynnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlamingTizona>()
            .ActivateOnEnter<TheCurse>()
            .ActivateOnEnter<Demoralize>()
            .ActivateOnEnter<Tizona>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67775, NameID = 4739)]
public class FlameGeneralAldynn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35.75f, -205.5f), new ArenaBoundsCircle(15));
