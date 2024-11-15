namespace BossMod.Shadowbringers.Quest.CourageBornOfFear;
public enum OID : uint
{
    Boss = 0x29E1, // r=0.5
    Helper = 0x233C,
    Andreia = 0x29E0,
    Knight = 0x29E4,
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // 29E4/Boss->player/29DF, no cast, single-target
    _Spell_SanctifiedFire = 17092, // 29E3->player, 2.5s cast, single-target
    _Spell_SanctifiedStoneIII = 17090, // 29E2->player, 3.0s cast, single-target
    _Ability_Quickstep = 4743, // 29E4->location, no cast, ???
    _Ability_ = 17293, // 29E4->self, no cast, single-target
    _Ability_Hastilude = 17171, // 29E4->self, no cast, single-target
    _Weaponskill_Overcome = 17088, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    _Spell_SanctifiedFireII = 17093, // 29E3->29DF, 5.0s cast, range 5 circle
    _Spell_SanctifiedFireII1 = 17188, // 29E3->29DF, no cast, range 5 circle
    _Weaponskill_HolyHeat = 17094, // 29E5->self, no cast, range 6 circle
    _Ability_1 = 4777, // 29E3->self, no cast, single-target
    _Ability_2 = 3269, // 29E3->self, no cast, single-target
    _Weaponskill_MythrilCyclone = 17086, // Boss->self, 4.0s cast, range 50 circle
    _Weaponskill_MythrilCyclone1 = 17087, // 29DD->self, 4.0s cast, range 50 circle
    _Spell_SanctifiedMeltdown = 17324, // 29E3->self, 4.0s cast, single-target
    _Weaponskill_SanctifiedMeltdown = 17323, // 29DD->player/29DF, 5.0s cast, range 6 circle
    _Weaponskill_MythrilCyclone2 = 17207, // 29DD->self, 8.0s cast, range 8-20 donut
    _Weaponskill_UncloudedAscension = 17333, // Boss->self, 4.0s cast, single-target
    _Weaponskill_UncloudedAscension1 = 17335, // 2AD1->self, 5.0s cast, range 10 circle
    _Ability_3 = 17294, // 29E4->self, no cast, single-target
    _AutoAttack_Attack1 = 873, // 29E0->29DF, no cast, single-target
    _Weaponskill_FastBlade = 17225, // 29E4->player/29DF, no cast, single-target
    _Spell_ThePathOfLight = 17229, // 29E4->self, 6.0s cast, single-target
    _Weaponskill_ArrowOfFortitude = 17332, // 29E0->29DF, 2.5s cast, single-target
    _Spell_ThePathOfLight1 = 17230, // 2A3F->self, 5.5s cast, range 15 circle
    _Weaponskill_HeavyShot = 16182, // 29E0->29DF, no cast, single-target
    _Weaponskill_InquisitorsBlade = 17095, // 29E4->self, 5.0s cast, range 40 180-degree cone
    _Ability_Cover = 17228, // 29E4->29E0, no cast, single-target
    _Weaponskill_ShieldBash = 17096, // 29E4->player, no cast, single-target
    _Weaponskill_BodkinVolley = 17083, // 29E0->players, 6.0s cast, range 5 circle
    _Ability_RainOfLight = 17081, // 29E0->self, 3.0s cast, single-target
    _Spell_RainOfLight = 17082, // 29DD->location, 3.0s cast, range 4 circle
    _LimitBreak_ArrowOfFortitude = 17211, // Andreia->self, 4.0s cast, range 30 width 8 rect
    _Weaponskill_BodkinVolley1 = 17189, // Andreia->29DF, 6.0s cast, range 5 circle
}

class ArrowOfFortitude(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._LimitBreak_ArrowOfFortitude), new AOEShapeRect(30, 4));
class BodkinVolley(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_BodkinVolley1), 5, minStackSize: 1);
class RainOfLight(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_RainOfLight), 4);
class ThePathOfLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_ThePathOfLight1), new AOEShapeCircle(15));
class InquisitorsBlade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_InquisitorsBlade), new AOEShapeCone(40, 90.Degrees()));
class MythrilCycloneKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Weaponskill_MythrilCyclone1), 18, stopAtWall: true);
class MythrilCycloneDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_MythrilCyclone2), new AOEShapeDonut(8, 20));
class SanctifiedMeltdown(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Weaponskill_SanctifiedMeltdown), 6);
class UncloudedAscension(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_UncloudedAscension1), new AOEShapeCircle(10));
class Overcome(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Overcome), new AOEShapeCone(8.5f, 60.Degrees()));

class SanctifiedFireII(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), 23, centerAtTarget: true)
{
    private DateTime Timeout = DateTime.MaxValue;

    public override void Update()
    {
        // for some reason, the magus can just forget to cast the two followups, leaving lue-reeq to run around like a moron
        if (WorldState.CurrentTime > Timeout && CurrentBaits.Count > 0)
            Reset();
    }

    private void Reset()
    {
        CurrentBaits.Clear();
        NumCasts = 0;
        Timeout = DateTime.MaxValue;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);
        if (iconID == IID)
            Timeout = WorldState.FutureTime(10);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x29E5 && ++NumCasts >= 3)
            Reset();
    }
}

class FireVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID._Spell_SanctifiedFireII1), m => m.Enemies(0x29E5).Where(e => e.EventState != 7), 0.25f);

class ImmaculateWarriorStates : StateMachineBuilder
{
    public ImmaculateWarriorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overcome>()
            .ActivateOnEnter<SanctifiedFireII>()
            .ActivateOnEnter<FireVoidzone>()
            .ActivateOnEnter<MythrilCycloneDonut>()
            .ActivateOnEnter<MythrilCycloneKB>()
            .ActivateOnEnter<SanctifiedMeltdown>()
            .ActivateOnEnter<UncloudedAscension>()
            .ActivateOnEnter<ThePathOfLight>()
            .ActivateOnEnter<InquisitorsBlade>()
            .ActivateOnEnter<RainOfLight>()
            .ActivateOnEnter<ArrowOfFortitude>()
            .ActivateOnEnter<BodkinVolley>()
            .Raw.Update = () => Module.Enemies(OID.Andreia).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68814, NameID = 8782)]
public class ImmaculateWarrior(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247, 688.5f), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.TargetID == actor.InstanceID ? 1 : 0;
    }
}
