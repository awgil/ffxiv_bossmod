namespace BossMod.Endwalker.Criterion.C01ASS.C010Armor;

public enum OID : uint
{
    NBoss = 0x3AD8, // R2.500, x1
    SBoss = 0x3AE1, // R2.500, x1
}

public enum AID : uint
{
    AutoAttack = 31109, // Boss->player, no cast, single-target
    NDominionSlash = 31082, // Boss->self, 3.5s cast, range 12 90-degree cone aoe
    NInfernalWeight = 31083, // Boss->self, 5.0s cast, raidwide
    NHellsNebula = 31084, // Boss->self, 4.0s cast, raidwide set hp to 1
    SDominionSlash = 31106, // Boss->self, 3.5s cast, range 12 90-degree cone aoe
    SInfernalWeight = 31107, // Boss->self, 5.0s cast, raidwide
    SHellsNebula = 31108, // Boss->self, 4.0s cast, raidwide set hp to 1
}

class DominionSlash(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(12, 45.Degrees()));
class NDominionSlash(BossModule module) : DominionSlash(module, AID.NDominionSlash);
class SDominionSlash(BossModule module) : DominionSlash(module, AID.SDominionSlash);

class InfernalWeight(BossModule module, AID aid) : Components.RaidwideCast(module, ActionID.MakeSpell(aid), "Raidwide with slow");
class NInfernalWeight(BossModule module) : InfernalWeight(module, AID.NInfernalWeight);
class SInfernalWeight(BossModule module) : InfernalWeight(module, AID.SInfernalWeight);

class HellsNebula(BossModule module, AID aid) : Components.CastHint(module, ActionID.MakeSpell(aid), "Reduce hp to 1");
class NHellsNebula(BossModule module) : HellsNebula(module, AID.NHellsNebula);
class SHellsNebula(BossModule module) : HellsNebula(module, AID.SHellsNebula);

class C010ArmorStates : StateMachineBuilder
{
    public C010ArmorStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NDominionSlash>(!savage)
            .ActivateOnEnter<NInfernalWeight>(!savage)
            .ActivateOnEnter<NHellsNebula>(!savage)
            .ActivateOnEnter<SDominionSlash>(savage)
            .ActivateOnEnter<SInfernalWeight>(savage)
            .ActivateOnEnter<SHellsNebula>(savage);
    }
}
class C010NArmorStates(BossModule module) : C010ArmorStates(module, false);
class C010SArmorStates(BossModule module) : C010ArmorStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11515, SortOrder = 6)]
public class C010NArmor(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11515, SortOrder = 6)]
public class C010SArmor(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
