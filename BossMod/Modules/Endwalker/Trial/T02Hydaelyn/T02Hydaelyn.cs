namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class MousasScorn(BossModule module) : Components.CastSharedTankbuster(module, AID.MousasScorn, 4);
class MousasScornHint(BossModule module) : Components.SingleTargetCast(module, AID.MousasScorn, "Shared Tankbuster");
class HerossSundering(BossModule module) : Components.BaitAwayCast(module, AID.HerossSundering, new AOEShapeCone(40, 45.Degrees()));
class HerossSunderingHint(BossModule module) : Components.SingleTargetCast(module, AID.HerossSundering, "Tankbuster cleave");
class HerossRadiance(BossModule module) : Components.RaidwideCast(module, AID.HerossRadiance);
class MagossRadiance(BossModule module) : Components.RaidwideCast(module, AID.MagossRadiance);
class RadiantHalo(BossModule module) : Components.RaidwideCast(module, AID.RadiantHalo);
class CrystallineStoneIII(BossModule module) : Components.StackWithCastTargets(module, AID.CrystallineStoneIII2, 6, 5);
class CrystallineBlizzardIII(BossModule module) : Components.SpreadFromCastTargets(module, AID.CrystallineBlizzardIII2, 5);
class Beacon(BossModule module) : Components.ChargeAOEs(module, AID.Beacon, 3);
class Beacon2(BossModule module) : Components.StandardAOEs(module, AID.Beacon2, new AOEShapeRect(45, 3), 10);
class HydaelynsRay(BossModule module) : Components.StandardAOEs(module, AID.HydaelynsRay, new AOEShapeRect(45, 15));

class T02HydaelynStates : StateMachineBuilder
{
    public T02HydaelynStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ParhelicCircle>()
            .ActivateOnEnter<MousasScorn>()
            .ActivateOnEnter<Echoes>()
            .ActivateOnEnter<Beacon>()
            .ActivateOnEnter<Beacon2>()
            .ActivateOnEnter<CrystallineStoneIII>()
            .ActivateOnEnter<CrystallineBlizzardIII>()
            .ActivateOnEnter<MousasScornHint>()
            .ActivateOnEnter<HerossSundering>()
            .ActivateOnEnter<HerossSunderingHint>()
            .ActivateOnEnter<HerossRadiance>()
            .ActivateOnEnter<MagossRadiance>()
            .ActivateOnEnter<HydaelynsRay>()
            .ActivateOnEnter<RadiantHalo>()
            .ActivateOnEnter<Lightwave>()
            .ActivateOnEnter<WeaponTracker>()
            .ActivateOnEnter<Exodus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 790, NameID = 10453)]
public class T02Hydaelyn(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.CrystalOfLight))
            Arena.Actor(s, ArenaColor.Object);
    }
}
