namespace BossMod.Shadowbringers.Quest.AFeastOfLies;

public enum OID : uint
{
    Boss = 0x295A,
    Helper = 0x233C,
}

public enum AID : uint
{
    UnceremoniousBeheading = 16274, // Boss->self, 4.0s cast, range 10 circle
    KatunCycle = 16275, // Boss->self, 4.0s cast, range 5-40 donut
    MercilessRight = 16278, // Boss->self, 4.0s cast, single-target
    MercilessRight1 = 16283, // 29FB->self, 3.8s cast, range 40 120-degree cone
    MercilessRight2 = 16284, // 29FE->self, 4.2s cast, range 40 120-degree cone
    Evisceration = 16277, // Boss->self, 4.5s cast, range 40 120-degree cone
    HotPursuit = 16291, // Boss->self, 2.5s cast, single-target
    HotPursuit1 = 16285, // 29E6->location, 3.0s cast, range 5 circle
    NexusOfThunder = 16280, // Boss->self, 2.5s cast, single-target
    NexusOfThunder1 = 16276, // 29E6->self, 4.3s cast, range 45 width 5 rect
    LivingFlame = 16294, // Boss->self, 3.0s cast, single-target
    Spiritcall = 16292, // Boss->self, 3.0s cast, range 40 circle
    Burn = 16290, // 29C2->self, 4.5s cast, range 8 circle
    RisingThunder = 16293, // Boss->self, 3.0s cast, single-target
    Electrocution = 16286, // 295B->self, 10.0s cast, range 6 circle
    ShatteredSky = 17191, // Boss->self, 4.0s cast, single-target
    ShatteredSky1 = 16282, // 29E6->self, 0.5s cast, range 40 circle
    NexusOfThunder2 = 16296, // 29E6->self, 6.3s cast, range 45 width 5 rect
    MercilessLeft = 16279, // Boss->self, 4.0s cast, single-target
    MercilessLeft1 = 16298, // 29FC->self, 3.8s cast, range 40 120-degree cone
    MercilessLeft2 = 16297, // 29FD->self, 4.2s cast, range 40 120-degree cone
}

class UnceremoniousBeheading(BossModule module) : Components.StandardAOEs(module, AID.UnceremoniousBeheading, new AOEShapeCircle(10));
class KatunCycle(BossModule module) : Components.StandardAOEs(module, AID.KatunCycle, new AOEShapeDonut(5, 40));
class MercilessRight(BossModule module) : Components.StandardAOEs(module, AID.MercilessRight1, new AOEShapeCone(40, 60.Degrees()));
class MercilessRight1(BossModule module) : Components.StandardAOEs(module, AID.MercilessRight2, new AOEShapeCone(40, 60.Degrees()));
class MercilessLeft(BossModule module) : Components.StandardAOEs(module, AID.MercilessLeft1, new AOEShapeCone(40, 60.Degrees()));
class MercilessLeft1(BossModule module) : Components.StandardAOEs(module, AID.MercilessLeft2, new AOEShapeCone(40, 60.Degrees()));
class Evisceration(BossModule module) : Components.StandardAOEs(module, AID.Evisceration, new AOEShapeCone(40, 60.Degrees()));
class HotPursuit(BossModule module) : Components.StandardAOEs(module, AID.HotPursuit1, 5);
class NexusOfThunder(BossModule module) : Components.StandardAOEs(module, AID.NexusOfThunder1, new AOEShapeRect(45, 2.5f));
class NexusOfThunder1(BossModule module) : Components.StandardAOEs(module, AID.NexusOfThunder2, new AOEShapeRect(45, 2.5f));
class Burn(BossModule module) : Components.StandardAOEs(module, AID.Burn, new AOEShapeCircle(8), maxCasts: 5);
class Spiritcall(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Spiritcall, 20, stopAtWall: true);

class Electrocution(BossModule module) : Components.StandardAOEs(module, AID.Electrocution, new AOEShapeCircle(6))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 12)
        {
            var enemy = hints.PotentialTargets.Where(x => x.Actor.OID == 0x295B).MinBy(e => actor.DistanceToHitbox(e.Actor));
            foreach (var e in hints.PotentialTargets)
                e.Priority = e == enemy ? 1 : 0;
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

class SerpentHead(BossModule module) : Components.Adds(module, 0x29E8, 1);

class RanjitStates : StateMachineBuilder
{
    public RanjitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<UnceremoniousBeheading>()
            .ActivateOnEnter<KatunCycle>()
            .ActivateOnEnter<MercilessRight>()
            .ActivateOnEnter<MercilessRight1>()
            .ActivateOnEnter<MercilessLeft>()
            .ActivateOnEnter<MercilessLeft1>()
            .ActivateOnEnter<Evisceration>()
            .ActivateOnEnter<HotPursuit>()
            .ActivateOnEnter<NexusOfThunder>()
            .ActivateOnEnter<NexusOfThunder1>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<Electrocution>()
            .ActivateOnEnter<Spiritcall>()
            .ActivateOnEnter<SerpentHead>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69167, NameID = 8374)]
public class Ranjit(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 18), new ArenaBoundsCircle(15));
