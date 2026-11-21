namespace BossMod.Stormblood.Alliance.A32Agrias;

public enum OID : uint
{
    Boss = 0x261F, // R3.800, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    EphemeralKnight = 0x2620, // R3.800, x8
    SwordKnight = 0x2623, // R1.250, x0 (spawn during fight)
    ShieldKnight = 0x2624, // R1.250, x0 (spawn during fight)
    EmblazonedShield = 0x2625, // R1.000, x0 (spawn during fight)
    Halidom = 0x2626, // R1.000, x0 (spawn during fight)
    CloudOfLigma = 0x2778, // R2.000, x0 (spawn during fight)
    TaintlessSoul = 0x2621, // R1.500, x0 (spawn during fight)
    TaintedSoul = 0x2622, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/2623->player, no cast, single-target
    DivineLight = 14439, // Boss->self, 4.0s cast, range 60 circle
    ThunderSlash = 14438, // Boss->self/player, 4.0s cast, range 60 90?-degree cone
    JudgmentBlade = 14423, // Boss->self, 8.0s cast, range 60 circle
    CleansingFlameCast = 14436, // Boss->self, 5.0s cast, single-target
    CleansingFlame = 14437, // Helper->player, 5.0s cast, range 6 circle
    CleansingStrike = 14420, // Boss->self, 6.0s cast, single-target
    Jail = 15058, // Helper->self, no cast, range 60 circle
    Unk2 = 14429, // Boss->self, no cast, single-target
    DarkFlare = 14421, // 2621->self, 8.0s cast, range 40 circle
    Imprison = 14417, // 2626->player, no cast, single-target
    Consecration = 14416, // Boss->self, 4.0s cast, single-target
    NorthswainsStrikeCast = 14982, // Boss->self, 15.0s cast, single-target
    NorthswainsStrike = 14419, // 2620->self, 15.0s cast, range 60 width 6 rect
    HallowedBoltCast = 14426, // Boss->self, 4.0s cast, single-target
    HallowedBoltCircle = 14427, // Helper->self, 4.0s cast, range 15 circle
    HallowedBoltDonut = 14428, // Helper->self, 4.0s cast, range 15-30 donut
    KnightAttack = 14431, // 2624->player, no cast, single-target
    MortalBlow = 14430, // 2623->self, 5.0s cast, range 30 circle
    HeavenlyJudgmentVisual = 14432, // Boss->self, 4.0s cast, single-target
    HeavenlyJudgment = 14433, // Helper->self, 8.0s cast, range 60 circle
    DivineRuinationCast = 14424, // Boss->self, 8.0s cast, single-target
    DivineRuination = 14425, // Helper->self, no cast, range 60 width 6 rect
}

public enum SID : uint
{
    Shieldbearer = 1733, // none->player, extra=0x0
    Swordbearer = 1734, // none->player, extra=0x0
    HeavenlyShield = 1735,
    DamageUp = 1781, // none->Boss/2623, extra=0x0
    Stun = 149, // none->player, extra=0x0
    InvincibilityTeleport = 1570, // none->player, extra=0x0
    Doom = 210, // none->player, extra=0x0
    VulnerabilityDown = 350, // none->2626/2625, extra=0x0
    Fetters = 667, // none->player, extra=0xEC4
    InvincibilityKnight = 775, // none->2624, extra=0x0
}

public enum IconID : uint
{
    ThunderSlash = 25, // player->self
    Spread = 139, // player->self
    Jail = 165, // player->self
    HallowedBolt = 23, // player->self
    DivineRuination = 8, // player->self
}

public enum TetherID : uint
{
    KnightTether = 14, // 2625->2624
    Light = 2, // player->Boss
}

class DivineLight(BossModule module) : Components.RaidwideCast(module, AID.DivineLight);
class ThunderSlash(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 45.Degrees()), (uint)IconID.ThunderSlash, AID.ThunderSlash, 5.1f, damageType: AIHints.PredictedDamageType.Tankbuster);
class CleansingFlame(BossModule module) : Components.SpreadFromCastTargets(module, AID.CleansingFlame, 6);
class NorthswainsStrike(BossModule module) : Components.StandardAOEs(module, AID.NorthswainsStrike, new AOEShapeRect(60, 3))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(a => a with { Risky = a.Activation < WorldState.FutureTime(2) });
}
class Halidom(BossModule module) : Components.AddsPointless(module, (uint)OID.Halidom);
class EmblazonedShield(BossModule module) : Components.AddsPointless(module, (uint)OID.EmblazonedShield);
class ShieldKnight(BossModule module) : Components.Adds(module, (uint)OID.ShieldKnight, 1);
class SwordKnight(BossModule module) : Components.Adds(module, (uint)OID.SwordKnight, 1);
class ShieldKnightInvincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID.InvincibilityKnight);
class HallowedBolt1(BossModule module) : Components.StandardAOEs(module, AID.HallowedBoltCircle, 15);
class HallowedBolt2(BossModule module) : Components.StandardAOEs(module, AID.HallowedBoltDonut, new AOEShapeDonut(15, 30));
class DivineRuination(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 3), (uint)IconID.DivineRuination, AID.DivineRuination, 8.2f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!CurrentBaits.Any(b => b.Target == actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class A32AgriasStates : StateMachineBuilder
{
    public A32AgriasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DivineLight>()
            .ActivateOnEnter<ThunderSlash>()
            .ActivateOnEnter<HeavenlyShield>()
            .ActivateOnEnter<CleansingFlame>()
            .ActivateOnEnter<Jail>()
            .ActivateOnEnter<TaintedSoul>()
            .ActivateOnEnter<TaintlessSoul>()
            .ActivateOnEnter<NorthswainsStrike>()
            .ActivateOnEnter<Halidom>()
            .ActivateOnEnter<ShieldKnight>()
            .ActivateOnEnter<ShieldKnightInvincibility>()
            .ActivateOnEnter<SwordKnight>()
            .ActivateOnEnter<EmblazonedShield>()
            .ActivateOnEnter<HallowedBolt1>()
            .ActivateOnEnter<HallowedBolt2>()
            .ActivateOnEnter<DivineRuination>()
            .Raw.Update = () => Module.PrimaryActor is { HPMP.CurHP: 1, IsTargetable: false };
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7931)]
public class A32Agrias(ModuleInit init) : BossModule(init, new(600, -54), new ArenaBoundsCircle(29.5f));
