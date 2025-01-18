namespace BossMod.Shadowbringers.Quest.TheSoulOfTemperance;

public enum OID : uint
{
    Boss = 0x29CE,
    BossP2 = 0x29D0,
    Helper = 0x233C,
}

public enum AID : uint
{
    _Spell_SanctifiedAeroII = 16907, // Boss->29D7, no cast, single-target
    _Spell_SanctifiedStoneIII = 16906, // Boss->29D7, 2.5s cast, single-target
    _Spell_SanctifiedAero = 16910, // Boss->self, 4.0s cast, single-target
    _Spell_SanctifiedAero1 = 16911, // 2A0C->self, 4.0s cast, range 40+R width 6 rect
    _Spell_SanctifiedStone = 17322, // 29D0->self, 5.0s cast, single-target
    __SanctifiedStone = 17329, // 2A0C->29D7, no cast, range 5 circle
    _Spell_SanctifiedRevival = 16865, // Boss->2969, 2.0s cast, single-target
    _AutoAttack_Attack = 870, // 2969/29CF/274F/2996/296A->29CD, no cast, single-target
    _Weaponskill_HolyBlur = 17547, // 2969/29CF/274F/296A/2996->self, 5.0s cast, range 40 circle
    _Spell_SanctifiedCureII = 16908, // Boss->self/2969/274F/2996/296A/Boss, 2.0s cast, single-target
    _Ability_ = 17581, // 274F/29CF/2996/296A->self, no cast, single-target
    _Weaponskill_Focus = 17548, // 29CF/296A/2996/2969->players, 5.0s cast, width 4 rect charge
    _Spell_TemperedVirtue = 15928, // BossP2->self, 6.0s cast, range 15 circle
    _Spell_WaterAndWine = 15604, // 2AF1->self, 5.0s cast, range 12 circle
    _Ability_1 = 17583, // 29CF->self, no cast, single-target
    _Spell_ForceOfRestraint = 15603, // 2AF1->self, 5.0s cast, range 60+R width 4 rect
    _Spell_SanctifiedHoly = 16909, // BossP2->self, 4.0s cast, range 8 circle
    _Ability_SanctifiedHoly = 17604, // 2A0C->location, 4.0s cast, range 6 circle
}

class SanctifiedHoly1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SanctifiedHoly), new AOEShapeCircle(8));
class SanctifiedHoly2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_SanctifiedHoly), 6);
class ForceOfRestraint(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_ForceOfRestraint), new AOEShapeRect(60, 2));
class HolyBlur(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_HolyBlur));
class Focus(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID._Weaponskill_Focus), 2);
class TemperedVirtue(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_TemperedVirtue), new AOEShapeCircle(15));
class WaterAndWine(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_WaterAndWine), new AOEShapeDonut(6, 12));
class SanctifiedStone(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Spell_SanctifiedStone), 5, 1);

class SanctifiedAero(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SanctifiedAero1), new AOEShapeRect(40.5f, 3));

class Repose(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        static bool SleepProof(Actor a)
        {
            if (a.Statuses.Any(x => x.ID is 1967 or 1968))
                return true;

            if (a.PendingStatuses.Any(s => s.StatusId == 3))
                return true;

            return false;
        }

        if (WorldState.Actors.FirstOrDefault(x => x.IsTargetable && !x.IsAlly && x.OID != (uint)OID.Boss && !SleepProof(x)) is Actor e)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.Repose), e, ActionQueue.Priority.VeryHigh);
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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68808, NameID = 8777)]
public class Sophrosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(-651.8f, -127.25f), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = 0;
    }
}
