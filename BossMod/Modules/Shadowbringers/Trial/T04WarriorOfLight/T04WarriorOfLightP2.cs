/*
 * The phase 2 boss is a different actor than the phase 1 boss.
 * There is a checkpoint midway through the fight where party can restart
 * if they die after phase 2 boss has been damaged.
 *
 * This module lets us change the primary actor to the phase 2 boss while
 * re-using all the components from phase 1. Without this then the module does
 * not load if party wipes after reaching phase 2.
 */
// import the components from phase 1 of the fight.
using BossMod.Shadowbringers.Trial.T04WarriorOfLightP1;

namespace BossMod.Shadowbringers.Trial.T04WarriorOfLightP2;

[SkipLocalsInit]
sealed class T04WarriorOfLightP2States : StateMachineBuilder
{
    public T04WarriorOfLightP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TerrorUnleashed>()
            .ActivateOnEnter<SolemnConfiteor>()
            .ActivateOnEnter<ElementStayMove>()
            .ActivateOnEnter<CoruscantSaberDonut>()
            .ActivateOnEnter<CoruscantSaberCircle>()
            .ActivateOnEnter<Twincast>()
            .ActivateOnEnter<AbsoluteHoly>()
            .ActivateOnEnter<TheBitterEnd>()
            .ActivateOnEnter<RadiantBraver>()
            .ActivateOnEnter<RadiantBraverCleave>()
            .ActivateOnEnter<RadiantDesperado>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<ImbuedCoruscance>()
            .ActivateOnEnter<ImbuedCoruscanceDonut>()
            .ActivateOnEnter<ElddragonDive>()
            .ActivateOnEnter<BrimstoneEarth>()
            .ActivateOnEnter<BrimstoneEarthGrow>()
            .ActivateOnEnter<SwordOfLight>()
            .ActivateOnEnter<SuitonSan>()
            .ActivateOnEnter<RadiantMeteor>()
            .ActivateOnEnter<UltimateCrossover>()
            .ActivateOnEnter<Ascendance>()
            .ActivateOnEnter<KatonSan>()
            .ActivateOnEnter<PerfectDecimation>()
            .ActivateOnEnter<FlareBreath>()
            .ActivateOnEnter<FlareBreathAOE>()
            .ActivateOnEnter<DelugeOfDeath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(T04WarriorOfLightP2States),
    ConfigType = null, // replace null with typeof(WarriorOfLightConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.WarriorOfLightP2,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Shadowbringers,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 738u,
    NameID = 9462u,
    SortOrder = 2,
    PlanLevel = 0)]

[SkipLocalsInit]
public sealed class T04WarriorOfLightP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
