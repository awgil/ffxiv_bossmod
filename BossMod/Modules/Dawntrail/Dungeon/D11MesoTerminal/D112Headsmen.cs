using BossMod.Components;

namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D112Headsmen;

public enum OID : uint
{
    Helper    = 0x233C, // R0.500, x?, Helper type
    PestilentHeadsman = 0x4893, // R2.720, x?
    HoodedHeadsman    = 0x49E1, // R1.000, x?
    BloodyHeadsman   = 0x4890, // R2.720, x?
    Boss      = 0x4891, // R2.720, x?
    RavenousHeadsman  = 0x4892, // R2.720, x?
    SwordOfJustice    = 0x4894, // R1.000, x?
    Gen_Hellmaker         = 0x48D2, // R2.500, x?
}

public enum AID : uint
{
    Ability_LawlessPursuit         = 43577, // 4892/4893/4890/4891->self, 3.0s cast, single-target
    Spell_HeadSplittingRoar        = 43578, // 4892/4893/4890/4891->self, 5.0s cast, single-target
    Spell_HeadSplittingRoar1       = 43579, // 49E1->self, 5.5s cast, range 60 circle
    Ability_                       = 43580, // 4892/4893/4890/4891->location, no cast, single-target
    Ability_ShacklesOfFate         = 43581, // 4891/4892/4893/4890->player, 4.0s cast, single-target
    Ability_ShacklesOfFate1        = 43582, // 233C->player, 1.0s cast, single-target
    Ability_Dismemberment          = 43586, // 4892/4893/4891/4890->self, 3.0s cast, single-target
    AutoAttack_                    = 43600, // 4892/4893->player, no cast, single-target
    AutoAttack_1                   = 43598, // 4891->player, no cast, single-target
    AutoAttack_2                   = 43599, // 4890->player, no cast, single-target
    Ability_Dismemberment1         = 43587, // 233C->self, 6.0s cast, range 16 width 4 rect
    Ability_PealOfJudgment         = 43593, // 4892/4893/4891/4890->self, 3.0s cast, single-target
    Ability_PealOfJudgment1        = 43594, // 233C->self, no cast, range 2 width 4 rect
    Weaponskill_ChoppingBlock      = 43595, // 4892/4893/4890->self, 3.5s cast, range 6 circle
    Weaponskill_ExecutionWheel     = 43596, // 4891->self, 3.5s cast, range ?-9 donut
    Ability_FlayingFlail           = 43591, // 4892/4893/4891/4890->self, 3.0s cast, single-target
    Ability_FlayingFlail1          = 43592, // 233C->location, 5.0s cast, range 5 circle
    Weaponskill_DeathPenalty       = 43588, // 4890->player, 5.0s cast, single-target
    Ability_WillBreaker            = 44856, // 4891->player, 7.0s cast, single-target
    Weaponskill_RelentlessTorment  = 43589, // 4891->player, 5.0s cast, single-target
    Weaponskill_RelentlessTorment1 = 43590, // 233C->player, no cast, single-target
    Ability_1                      = 43584, // 4892/4893/4890/4891->self, no cast, single-target
}

public enum SID : uint
{
    Gen_CellBlockA   = 4542, // none->player, extra=0x0
    Gen_CellBlockB   = 4543, // none->player, extra=0x0
    Gen_CellBlockG   = 4544, // none->player, extra=0x0
    Gen_CellBlockO   = 4545, // none->player, extra=0x0
    Gen_GuardOnDutyA = 4546, // none->_Gen_BloodyHeadsman1, extra=0x0
    Gen_GuardOnDutyB = 4547, // none->Boss, extra=0x0
    Gen_GuardOnDutyG = 4548, // none->_Gen_RavenousHeadsman, extra=0x0
    Gen_GuardOnDutyO = 4549, // none->_Gen_PestilentHeadsman, extra=0x0
}

class _Spell_HeadSplittingRoar1(BossModule      module) : RaidwideCast(module, AID.Spell_HeadSplittingRoar1);
class _Ability_Dismemberment1(BossModule        module) : StandardAOEs(module, AID.Ability_Dismemberment1, new AOEShapeRect(16f, 2f));
class _Weaponskill_ChoppingBlock(BossModule     module) : StandardAOEs(module, AID.Weaponskill_ChoppingBlock, 6f);
class _Weaponskill_ExecutionWheel(BossModule    module) : StandardAOEs(module, AID.Weaponskill_ExecutionWheel, new AOEShapeDonut(5, 9));
class _Ability_FlayingFlail1(BossModule         module) : StandardAOEs(module, AID.Ability_FlayingFlail1, 5f);

class SwordOfJustice(BossModule module) : GenericAOEs(module)
{
    public List<Actor>       Swords = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (Actor swords in Swords)
        {
            yield return new AOEInstance(new AOEShapeRect(4f, 2f), swords.Position, Rotation: swords.Rotation);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SwordOfJustice)
        {
            Swords.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.SwordOfJustice)
        {
            Swords.Remove(actor);
        }
    }
}

class LawlessPursuit(BossModule module) : GenericAOEs(module)
{
    private readonly Dictionary<Actor, Actor?> bosses = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (bosses.TryGetValue(actor, out var bs) && bs is { IsDeadOrDestroyed: false })
        {
            return [new AOEInstance(new AOEShapeDonut(8f, 100f), bs.Position)];
        }

        foreach (var statuses in actor.Statuses)
        {
            var id = statuses.ID - (uint)SID.Gen_CellBlockA;
            if (id < 4)
            {
                var bossesAll = new[] { OID.BloodyHeadsman, OID.PestilentHeadsman, OID.RavenousHeadsman, OID.Boss }.SelectMany(oid => Module.Enemies(oid));
                bosses[actor] = bossesAll.FirstOrDefault(a => a.FindStatus(SID.Gen_GuardOnDutyA + id).HasValue);
            }
        }
        return [];
    }
}

class D112HeadsmenStates : StateMachineBuilder
{
    public D112HeadsmenStates(BossModule module) : base(module)
    {
        TrivialPhase().
            ActivateOnEnter<LawlessPursuit>().
            ActivateOnEnter<_Spell_HeadSplittingRoar1>().
            ActivateOnEnter<_Ability_Dismemberment1>().
            ActivateOnEnter<_Weaponskill_ChoppingBlock>().
            ActivateOnEnter<_Weaponskill_ExecutionWheel>().
            ActivateOnEnter<_Ability_FlayingFlail1>().
            ActivateOnEnter<SwordOfJustice>().Raw.Update = () => (module as D112Headsmen)?.bossActor?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028, NameID = 14049)]
public class D112Headsmen(WorldState ws, Actor primary) : BossModule(ws, primary, new(60f, -256f), new ArenaBoundsRect(28f, 20f))
{
    public Actor? bossActor;

    protected override void UpdateModule()
    {
        base.UpdateModule();
        if (bossActor?.IsDeadOrDestroyed ?? true)
        {
            bossActor = new[] { OID.BloodyHeadsman, OID.PestilentHeadsman, OID.RavenousHeadsman, OID.Boss }.SelectMany(Enemies).FirstOrDefault(ba => !ba.IsDeadOrDestroyed);
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.BloodyHeadsman),   ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.RavenousHeadsman), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.PestilentHeadsman), ArenaColor.Enemy);

        Arena.Actors(Enemies(OID.Helper), ArenaColor.Danger);
        Arena.Actors(Enemies(OID.SwordOfJustice), ArenaColor.Danger);
    }
}
