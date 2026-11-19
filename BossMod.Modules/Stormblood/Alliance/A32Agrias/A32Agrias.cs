#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Stormblood.Alliance.A32Agrias;

public enum OID : uint
{
    Boss = 0x261F, // R3.800, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    _Gen_EphemeralKnight = 0x2620, // R3.800, x8
    _Gen_Halidom = 0x2626, // R1.000, x0 (spawn during fight)
    _Gen_ShieldKnight = 0x2624, // R1.250, x0 (spawn during fight)
    _Gen_SwordKnight = 0x2623, // R1.250, x0 (spawn during fight)
    _Gen_EmblazonedShield = 0x2625, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss/2623->player, no cast, single-target
    _Weaponskill_DivineLight = 14439, // Boss->self, 4.0s cast, range 60 circle
    _Weaponskill_ThunderSlash = 14438, // Boss->self/player, 4.0s cast, range 60 ?-degree cone
    _Spell_JudgmentBlade = 14423, // Boss->self, 8.0s cast, range 60 circle
    _Spell_CleansingFlame = 14437, // Helper->player, 5.0s cast, range 6 circle
    _Weaponskill_CleansingFlame = 14436, // Boss->self, 5.0s cast, single-target
    _Weaponskill_CleansingStrike = 14420, // Boss->self, 6.0s cast, single-target
    _Weaponskill_ = 15058, // Helper->self, no cast, range 60 circle
    _Weaponskill_1 = 14417, // 2626->player, no cast, single-target
    _Weaponskill_Consecration = 14416, // Boss->self, 4.0s cast, single-target
    _Weaponskill_NorthswainsStrike = 14419, // 2620->self, 15.0s cast, range 60 width 6 rect
    _Weaponskill_NorthswainsStrike1 = 14982, // Boss->self, 15.0s cast, single-target
    _Weaponskill_HallowedBolt = 14426, // Boss->self, 4.0s cast, single-target
    _Spell_HallowedBolt = 14427, // Helper->self, 4.0s cast, range 15 circle
    _Spell_HallowedBolt1 = 14428, // Helper->self, 4.0s cast, range 15-30 donut
    _Weaponskill_2 = 14429, // Boss->self, no cast, single-target
    _Spell_Attack = 14431, // 2624->player, no cast, single-target
    _Weaponskill_MortalBlow = 14430, // 2623->self, 5.0s cast, range 30 circle
    _Weaponskill_HeavenlyJudgment = 14432, // Boss->self, 4.0s cast, single-target
    _Spell_HeavenlyJudgment = 14433, // Helper->self, 8.0s cast, range 60 circle
    _Weaponskill_DivineRuination = 14424, // Boss->self, 8.0s cast, single-target
    _Spell_DivineRuination = 14425, // Helper->self, no cast, range 60 width 6 rect
}

public enum SID : uint
{
    _Gen_Shieldbearer = 1733, // none->player, extra=0x0
    HeavenlyShield = 1735,
    _Gen_DamageUp = 1781, // none->Boss/2623, extra=0x0
    _Gen_VulnerabilityUp = 202, // Boss/Helper/2623->player, extra=0x1/0x2/0x3
    _Gen_Stun = 149, // none->player, extra=0x0
    _Gen_Invincibility = 1570, // none->player, extra=0x0
    _Gen_Doom = 210, // none->player, extra=0x0
    _Gen_VulnerabilityDown = 350, // none->2626/2625, extra=0x0
    _Gen_Swordbearer = 1734, // none->player, extra=0x0
    _Gen_Fetters = 667, // none->player, extra=0xEC4
    _Gen_TheEcho = 42, // none->player, extra=0x0
    _Gen_ReducedRates = 364, // none->player, extra=0x1E
    _Gen_MeatAndMead = 360, // none->player, extra=0xA
    _Gen_Jackpot = 902, // none->player, extra=0xA
    _Gen_TheHeatOfBattle = 365, // none->player, extra=0xF
    _Gen_Invincibility1 = 775, // none->2624, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0126trg_t1h = 25, // player->self
    _Gen_Icon_target_ae_s5f = 139, // player->self
    _Gen_Icon_m0285trg_b0c = 165, // player->self
    _Gen_Icon_lockon5_t0h = 23, // player->self
    _Gen_Icon_m0196trg_a0h = 8, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_subbly_mgc01f = 14, // 2625->2624
    _Gen_Tether_chn_light01f = 2, // player->Boss
}

class DivineLight(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_DivineLight);
class ThunderSlash(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 45.Degrees()), (uint)IconID._Gen_Icon_m0126trg_t1h, AID._Weaponskill_ThunderSlash, 5.1f, damageType: AIHints.PredictedDamageType.Tankbuster);
class CleansingFlame(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_CleansingFlame, 6);
class NorthswainsStrike(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_NorthswainsStrike, new AOEShapeRect(60, 3))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(a => a with { Risky = a.Activation < WorldState.FutureTime(2) });
}
class Halidom(BossModule module) : Components.AddsPointless(module, (uint)OID._Gen_Halidom);
class EmblazonedShield(BossModule module) : Components.AddsPointless(module, (uint)OID._Gen_EmblazonedShield);
class ShieldKnight(BossModule module) : Components.Adds(module, (uint)OID._Gen_ShieldKnight, 1);
class SwordKnight(BossModule module) : Components.Adds(module, (uint)OID._Gen_SwordKnight, 1);
class ShieldKnightInvincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID._Gen_Invincibility1);
class HallowedBolt1(BossModule module) : Components.StandardAOEs(module, AID._Spell_HallowedBolt, 15);
class HallowedBolt2(BossModule module) : Components.StandardAOEs(module, AID._Spell_HallowedBolt1, new AOEShapeDonut(15, 30));
class DivineRuination(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 3), (uint)IconID._Gen_Icon_m0196trg_a0h, AID._Spell_DivineRuination, 8.2f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!CurrentBaits.Any(b => b.Target == actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

// intermission jail is r19.5 centered at (300, -50)
class A32AgriasStates : StateMachineBuilder
{
    public A32AgriasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DivineLight>()
            .ActivateOnEnter<ThunderSlash>()
            .ActivateOnEnter<HeavenlyShield>()
            .ActivateOnEnter<CleansingFlame>()
            .ActivateOnEnter<NorthswainsStrike>()
            .ActivateOnEnter<Halidom>()
            .ActivateOnEnter<ShieldKnight>()
            .ActivateOnEnter<ShieldKnightInvincibility>()
            .ActivateOnEnter<SwordKnight>()
            .ActivateOnEnter<EmblazonedShield>()
            .ActivateOnEnter<HallowedBolt1>()
            .ActivateOnEnter<HallowedBolt2>()
            .ActivateOnEnter<DivineRuination>()
            .Raw.Update = () => module.PrimaryActor is { HPMP.CurHP: 1, IsTargetable: false };
    }
}

//[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7931)]
//public class A32Agrias(ModuleInit init) : BossModule(init, new(600, -54), new ArenaBoundsCircle(29.5f));

