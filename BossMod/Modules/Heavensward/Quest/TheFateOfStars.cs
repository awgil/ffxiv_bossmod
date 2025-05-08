namespace BossMod.Heavensward.Quest.TheFateOfStars;

public enum OID : uint
{
    Boss = 0x161E,
    Helper = 0x233C,
    MagitekTurretI = 0x161F, // R0.600, x0 (spawn during fight)
    MagitekTurretII = 0x1620, // R0.600, x0 (spawn during fight)
    TerminusEst = 0x1621, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    MagitekSlug = 6026, // Boss->self, 2.5s cast, range 60+R width 4 rect
    AetherochemicalGrenado = 6031, // 1620->location, 3.0s cast, range 8 circle
    SelfDetonate = 6032, // 161F/1620->self, 5.0s cast, range 40+R circle
    MagitekSpread = 6027, // Boss->self, 3.0s cast, range 20+R 240-degree cone
}

class MagitekSlug(BossModule module) : Components.StandardAOEs(module, AID.MagitekSlug, new AOEShapeRect(60, 2));
class AetherochemicalGrenado(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalGrenado, 8);
class SelfDetonate(BossModule module) : Components.CastHint(module, AID.SelfDetonate, "Kill turret before detonation!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PriorityTargets)
            if (h.Actor.CastInfo?.Action == WatchedAction)
                h.Priority = 5;
    }
}
class MagitekSpread(BossModule module) : Components.StandardAOEs(module, AID.MagitekSpread, new AOEShapeCone(20.55f, 120.Degrees()));

class RegulaVanHydrusStates : StateMachineBuilder
{
    public RegulaVanHydrusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekSlug>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MagitekSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67824, NameID = 3818)]
public class RegulaVanHydrus(WorldState ws, Actor primary) : BossModule(ws, primary, new(230, 79), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
