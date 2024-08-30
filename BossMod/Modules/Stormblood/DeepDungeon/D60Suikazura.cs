namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D60Suikazura;

public enum OID : uint
{
    Boss = 0x23E4, // R2.500, x1
    AccursedCane = 0x23E5, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Firewalker = 11925, // Boss->self, 3.0s cast, range 10 90-degree cone
    InfiniteAnguish = 11930, // AccursedCane->self, 3.0s cast, range 12 circle
    FireII = 11927, // Boss->location, 3.5s cast, range 5 circle
    Topple = 11926, // Boss->self, 3.0s cast, range 3+R circle
    SearingChain = 11929, // AccursedCane->self, 3.0s cast, range 60+R width 4 rect
    AncientFlare = 11928, // Boss->self, 5.0s cast, range 50 circle
}

class Firewalker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Firewalker), new AOEShapeCone(10, 45.Degrees()));
class InfiniteAnguish(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InfiniteAnguish), new AOEShapeDonut(6.5f, 12));
class FireII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FireII), 5);
class Topple(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Topple), new AOEShapeCircle(5.5f));
class SearingChain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SearingChain), new AOEShapeRect(61, 2));
class AncientFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AncientFlare), "Raidwide, watch your feet after it goes off");

class D60SuikazuraStates : StateMachineBuilder
{
    public D60SuikazuraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Firewalker>()
            .ActivateOnEnter<InfiniteAnguish>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<Topple>()
            .ActivateOnEnter<SearingChain>()
            .ActivateOnEnter<AncientFlare>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 545, NameID = 7487)]
public class D60Suikazura(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(25f));

