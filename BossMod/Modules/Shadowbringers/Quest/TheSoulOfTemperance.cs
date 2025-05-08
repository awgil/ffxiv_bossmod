namespace BossMod.Shadowbringers.Quest.TheSoulOfTemperance;

public enum OID : uint
{
    Boss = 0x29CE,
    BossP2 = 0x29D0,
    Helper = 0x233C,
}

public enum AID : uint
{
    SanctifiedAero1 = 16911, // 2A0C->self, 4.0s cast, range 40+R width 6 rect
    SanctifiedStone = 17322, // 29D0->self, 5.0s cast, single-target
    HolyBlur = 17547, // 2969/29CF/274F/296A/2996->self, 5.0s cast, range 40 circle
    Focus = 17548, // 29CF/296A/2996/2969->players, 5.0s cast, width 4 rect charge
    TemperedVirtue = 15928, // BossP2->self, 6.0s cast, range 15 circle
    WaterAndWine = 15604, // 2AF1->self, 5.0s cast, range 12 circle
    ForceOfRestraint = 15603, // 2AF1->self, 5.0s cast, range 60+R width 4 rect
    SanctifiedHoly1 = 16909, // BossP2->self, 4.0s cast, range 8 circle
    SanctifiedHoly2 = 17604, // 2A0C->location, 4.0s cast, range 6 circle
}

class SanctifiedHoly1(BossModule module) : Components.StandardAOEs(module, AID.SanctifiedHoly1, new AOEShapeCircle(8));
class SanctifiedHoly2(BossModule module) : Components.StandardAOEs(module, AID.SanctifiedHoly2, 6);
class ForceOfRestraint(BossModule module) : Components.StandardAOEs(module, AID.ForceOfRestraint, new AOEShapeRect(60, 2));
class HolyBlur(BossModule module) : Components.RaidwideCast(module, AID.HolyBlur);
class Focus(BossModule module) : Components.BaitAwayChargeCast(module, AID.Focus, 2);
class TemperedVirtue(BossModule module) : Components.StandardAOEs(module, AID.TemperedVirtue, new AOEShapeCircle(15));
class WaterAndWine(BossModule module) : Components.StandardAOEs(module, AID.WaterAndWine, new AOEShapeDonut(6, 12));
class SanctifiedStone(BossModule module) : Components.StackWithCastTargets(module, AID.SanctifiedStone, 5, 1);

class SanctifiedAero(BossModule module) : Components.StandardAOEs(module, AID.SanctifiedAero1, new AOEShapeRect(40.5f, 3));

class Repose(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        static bool SleepProof(Actor a)
        {
            if (a.Statuses.Any(x => x.ID is 1967 or 1968))
                return true;

            return a.PendingStatuses.Any(s => s.StatusId == 3);
        }

        if (WorldState.Actors.FirstOrDefault(x => x.IsTargetable && !x.IsAlly && x.OID != (uint)OID.Boss && !SleepProof(x)) is Actor e)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.Repose), e, ActionQueue.Priority.VeryHigh, castTime: 2.5f);
    }
}

class SophrosyneStates : StateMachineBuilder
{
    public SophrosyneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HolyBlur>()
            .ActivateOnEnter<Focus>()
            .ActivateOnEnter<Repose>()
            .Raw.Update = () => module.Enemies(OID.BossP2).Any(x => x.IsTargetable) || module.WorldState.CurrentCFCID != 673;
        TrivialPhase(1)
            .ActivateOnEnter<SanctifiedAero>()
            .ActivateOnEnter<SanctifiedStone>()
            .ActivateOnEnter<TemperedVirtue>()
            .ActivateOnEnter<WaterAndWine>()
            .ActivateOnEnter<ForceOfRestraint>()
            .ActivateOnEnter<SanctifiedHoly1>()
            .ActivateOnEnter<SanctifiedHoly2>()
            .Raw.Update = () => module.Enemies(OID.BossP2).All(x => x.IsDeadOrDestroyed) || module.WorldState.CurrentCFCID != 673;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68808, NameID = 8777)]
public class Sophrosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(-651.8f, -127.25f), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
