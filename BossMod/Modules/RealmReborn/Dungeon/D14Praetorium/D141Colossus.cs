namespace BossMod.RealmReborn.Dungeon.D14Praetorium.D141Colossus;

public enum OID : uint
{
    Boss = 0x3872, // x1
    Helper = 0x233C, // x8
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    CeruleumVent = 28474, // Boss->self, 5.0s cast, raidwide
    Teleport = 28467, // Boss->location, no cast, single-target, teleport
    PrototypeLaserAlpha = 28468, // Boss->self, 5.0s cast, single-target, visual
    IronKissAlpha1 = 28469, // Helper->location, 7.0s cast, range 6 circle aoe (inner set)
    IronKissAlpha2 = 28470, // Helper->location, 9.0s cast, range 6 circle aoe (outer set)
    PrototypeLaserBeta = 28471, // Boss->self, 5.0s cast, single-target, visual
    IronKissBeta = 28472, // Helper->player, 5.0s cast, range 5 circle spread
    GrandSword = 28473, // Boss->self, 5.0s cast, range 25 90-degree cone aoe
}

class CeruleumVent(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CeruleumVent));
class PrototypeLaserAlpha1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.IronKissAlpha1), 6);
class PrototypeLaserAlpha2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.IronKissAlpha2), 6);
class PrototypeLaserBeta(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.IronKissBeta), 5, false);
class GrandSword(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GrandSword), new AOEShapeCone(25, 45.Degrees()));

class D141ColossusStates : StateMachineBuilder
{
    public D141ColossusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CeruleumVent>()
            .ActivateOnEnter<PrototypeLaserAlpha1>()
            .ActivateOnEnter<PrototypeLaserAlpha2>()
            .ActivateOnEnter<PrototypeLaserBeta>()
            .ActivateOnEnter<GrandSword>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 16, NameID = 2134)]
public class D141Colossus(WorldState ws, Actor primary) : BossModule(ws, primary, new(192, 0), new ArenaBoundsCircle(15));
