namespace BossMod.Dawntrail.Dungeon.D08TenderValley.D080YokHuyAttestant;

public enum OID : uint
{
    Boss = 0x4253,
    YokHuyOrb = 0x424F,
    YokHuyAltar = 0x4251,
    YokHuyAltar2 = 0x448D
}

public enum AID : uint
{
    AutoAttack = 872,

    SunToss = 38539,
    BoulderToss = 38540,
    TectonicShift = 39221
}

class SunToss(BossModule module) : Components.StandardAOEs(module, AID.SunToss, 6);
class BoulderToss(BossModule module) : Components.SingleTargetDelayableCast(module, AID.BoulderToss);
class TectonicShift(BossModule module) : Components.StandardAOEs(module, AID.TectonicShift, 8);

class D080YokHuyAttestantStates : StateMachineBuilder
{
    public D080YokHuyAttestantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SunToss>()
            .ActivateOnEnter<BoulderToss>()
            .ActivateOnEnter<TectonicShift>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CerQ", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12801, SortOrder = -1)]
public class D080YokHuyAttestant(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130, -475.25f), new ArenaBoundsRect(17.5f, 24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.YokHuyOrb), ArenaColor.Object);
        Arena.Actors(Enemies(OID.YokHuyAltar), ArenaColor.Object);
        Arena.Actors(Enemies(OID.YokHuyAltar2), ArenaColor.Object);
    }
}
