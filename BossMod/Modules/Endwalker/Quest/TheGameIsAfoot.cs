namespace BossMod.Endwalker.Quest.TheGameIsAfoot;

public enum OID : uint
{
    Boss = 0x4037,
    Helper = 0x233C,
}

public enum AID : uint
{
    WindUnbound = 34883, // Boss->self, 5.0s cast, range 40 circle
    SnatchMorsel = 34884, // Boss->402D, 5.0s cast, single-target
    PeckingFlurry = 34886, // Boss->self, 4.0s cast, range 40 circle
    FallingRock = 34888, // Helper->location, 5.0s cast, range 6 circle
    StickySpit = 34890, // Helper->player, 5.0s cast, range 6 circle
    Swoop = 35717, // Boss->location, 5.0s cast, width 16 rect charge
    FurlingFlapping = 34893, // Helper->players/402D/402E, 5.0s cast, range 8 circle
    DeadlySwoop = 35888, // Boss->location, 30.0s cast, width 16 rect charge
}

class PeckingFlurry(BossModule module) : Components.RaidwideCast(module, AID.PeckingFlurry);
class WindUnbound(BossModule module) : Components.RaidwideCast(module, AID.WindUnbound);
class SnatchMorsel(BossModule module) : Components.SingleTargetCast(module, AID.SnatchMorsel, "Wukbuster");
class FallingRock(BossModule module) : Components.StandardAOEs(module, AID.FallingRock, 6, maxCasts: 8);
class StickySpit(BossModule module) : Components.StackWithCastTargets(module, AID.StickySpit, 6);
class Swoop(BossModule module) : Components.ChargeAOEs(module, AID.Swoop, 8);
class FurlingFlapping(BossModule module) : Components.SpreadFromCastTargets(module, AID.FurlingFlapping, 8);
class DeadlySwoop(BossModule module) : Components.ChargeAOEs(module, AID.DeadlySwoop, 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Raid.WithoutSlot().Count() == 3)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class GiantColibriStates : StateMachineBuilder
{
    public GiantColibriStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PeckingFlurry>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<SnatchMorsel>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<StickySpit>()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<DeadlySwoop>()
            .ActivateOnEnter<FurlingFlapping>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70288, NameID = 12499)]
public class GiantColibri(WorldState ws, Actor primary) : BossModule(ws, primary, new(425, -440), new ArenaBoundsCircle(15));
