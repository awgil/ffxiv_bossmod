namespace BossMod.RealmReborn.Dungeon.D03Copperbell.D033Gyges;

public enum OID : uint
{
    Boss = 0x38C9,
    Helper = 0x233C, // x5
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast
    GiganticSwing = 28762, // Boss->self, 6.0s cast, range 4-40 donut aoe
    GiganticSmash = 28760, // Boss->location, 6.0s cast, range 10 aoe
    GiganticBlast = 28761, // Helper->self, 6.0s cast, range 8 aoe
    GrandSlam = 28764, // Boss->player, 5.0s cast, tankbuster
    ColossalSlam = 28763, // Boss->self, 4.0s cast, range 40 60-degree cone aoe
}

class GiganticSwing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GiganticSwing), new AOEShapeDonut(4, 40));
class GiganticSmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GiganticSmash), 10);
class GiganticBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GiganticBlast), new AOEShapeCircle(8));
class GrandSlam(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.GrandSlam));
class ColossalSlam(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.ColossalSlam), new AOEShapeCone(40, 30.Degrees()));

class D033GygesStates : StateMachineBuilder
{
    public D033GygesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GiganticSwing>()
            .ActivateOnEnter<GiganticSmash>()
            .ActivateOnEnter<GiganticBlast>()
            .ActivateOnEnter<GrandSlam>()
            .ActivateOnEnter<ColossalSlam>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 3, NameID = 101)]
public class D033Gyges(WorldState ws, Actor primary) : BossModule(ws, primary, new(-100.42f, 6.67f), new ArenaBoundsCircle(20));
