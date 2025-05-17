using BossMod;

namespace BossMod.Endwalker.Criterion.C01ASS.C010Udumbara;

public enum OID : uint
{
    NBoss = 0x3AD3, // R4.000, x1
    NSapria = 0x3AD4, // R1.440, x2
    SBoss = 0x3ADC, // R4.000, x1
    SSapria = 0x3ADD, // R1.440, x2
}

public enum AID : uint
{
    AutoAttack = 31320, // NBoss/NSapria/SBoss/SSapria->player, no cast, single-target
    NHoneyedLeft = 31067, // NBoss->self, 4.0s cast, range 30 180-degree cone
    NHoneyedRight = 31068, // NBoss->self, 4.0s cast, range 30 180-degree cone
    NHoneyedFront = 31069, // NBoss->self, 4.0s cast, range 30 120-degree cone
    NBloodyCaress = 31071, // NSapria->self, 3.0s cast, range 12 120-degree cone
    SHoneyedLeft = 31091, // SBoss->self, 4.0s cast, range 30 180-degree cone
    SHoneyedRight = 31092, // SBoss->self, 4.0s cast, range 30 180-degree cone
    SHoneyedFront = 31093, // SBoss->self, 4.0s cast, range 30 120-degree cone
    SBloodyCaress = 31095, // SSapria->self, 3.0s cast, range 12 120-degree cone
}

class HoneyedLeft(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(30, 90.Degrees()));
class NHoneyedLeft(BossModule module) : HoneyedLeft(module, AID.NHoneyedLeft);
class SHoneyedLeft(BossModule module) : HoneyedLeft(module, AID.SHoneyedLeft);

class HoneyedRight(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(30, 90.Degrees()));
class NHoneyedRight(BossModule module) : HoneyedRight(module, AID.NHoneyedRight);
class SHoneyedRight(BossModule module) : HoneyedRight(module, AID.SHoneyedRight);

class HoneyedFront(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(30, 60.Degrees()));
class NHoneyedFront(BossModule module) : HoneyedFront(module, AID.NHoneyedFront);
class SHoneyedFront(BossModule module) : HoneyedFront(module, AID.SHoneyedFront);

class BloodyCaress(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(12, 60.Degrees()));
class NBloodyCaress(BossModule module) : BloodyCaress(module, AID.NBloodyCaress);
class SBloodyCaress(BossModule module) : BloodyCaress(module, AID.SBloodyCaress);

class C010UdumbaraStates : StateMachineBuilder
{
    public C010UdumbaraStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NHoneyedLeft>(!savage)
            .ActivateOnEnter<NHoneyedRight>(!savage)
            .ActivateOnEnter<NHoneyedFront>(!savage)
            .ActivateOnEnter<NBloodyCaress>(!savage)
            .ActivateOnEnter<SHoneyedLeft>(savage)
            .ActivateOnEnter<SHoneyedRight>(savage)
            .ActivateOnEnter<SHoneyedFront>(savage)
            .ActivateOnEnter<SBloodyCaress>(savage)
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(savage ? OID.SSapria : OID.NSapria).All(a => a.IsDead);
    }
}
class C010NUdumbaraStates(BossModule module) : C010UdumbaraStates(module, false);
class C010SUdumbaraStates(BossModule module) : C010UdumbaraStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11511, SortOrder = 3)]
public class C010NUdumbara(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.NSapria), ArenaColor.Enemy);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11511, SortOrder = 3)]
public class C010SUdumbara(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SSapria), ArenaColor.Enemy);
    }
}
