namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

// raidwide is slightly delayed
class Elegeia(BossModule module) : Components.CastCounter(module, AID.Elegeia);

class Telomania(BossModule module) : Components.CastCounter(module, AID.TelomaniaLast);

class UltimateFate(BossModule module) : Components.CastCounter(module, AID.EnrageAOE);

// TODO: proper tankbuster component...
class Hubris(BossModule module) : Components.CastCounter(module, AID.HubrisAOE);

// TODO: proper stacks component
class Eironeia(BossModule module) : Components.CastCounter(module, AID.EironeiaAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 846, NameID = 10448, PlanLevel = 90)]
public class Ex3Endsinger(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
