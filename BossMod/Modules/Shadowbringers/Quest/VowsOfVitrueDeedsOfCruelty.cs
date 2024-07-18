﻿namespace BossMod.Shadowbringers.Quest.VowsOfVitrueDeedsOfCruelty;

public enum OID : uint
{
    Boss = 0x2C85, // R6.000, x1
    TerminusEstVisual = 0x2C98, // R1.000, x3
    BossHelper = 0x233C, // R0.500, x15, 523 type
    SigniferPraetorianus = 0x2C9A, // R0.500, x0 (spawn during fight), the adds on the catwalk that just rain down Fire II
    LembusPraetorianus = 0x2C99, // R2.400, x0 (spawn during fight), two large magitek ships
    MagitekBit = 0x2C9C, // R0.600, x0 (spawn during fight)
}

public enum AID : uint
{
    LoadData = 18786, // Boss->self, 3.0s cast, single-target
    AutoAttack = 870, // Boss/LembusPraetorianus->player, no cast, single-target
    MagitekRayRightArm = 18783, // Boss->self, 3.2s cast, range 45+R width 8 rect
    MagitekRayLeftArm = 18784, // Boss->self, 3.2s cast, range 45+R width 8 rect
    SystemError = 18785, // Boss->self, 1.0s cast, single-target
    AngrySalamander = 18787, // Boss->self, 3.0s cast, range 40+R width 6 rect
    FireII = 18959, // SigniferPraetorianus->location, 3.0s cast, range 5 circle
    TerminusEstBossCast = 18788, // Boss->self, 3.0s cast, single-target
    TerminusEstLocationHelper = 18889, // BossHelper->self, 4.0s cast, range 3 circle
    TerminusEstVisual = 18789, // TerminusEstVisual->self, 1.0s cast, range 40+R width 4 rect
    HorridRoar = 18779, // 2CC5->location, 2.0s cast, range 6 circle, this is your own attack. It spawns an aoe at the location of any enemy it initally hits
    GarleanFire = 4007, // LembusPraetorianus->location, 3.0s cast, range 5 circle
    MagitekBit = 18790, // Boss->self, no cast, single-target
    MetalCutterCast = 18793, // Boss->self, 6.0s cast, single-target
    MetalCutter = 18794, // BossHelper->self, 6.0s cast, range 30+R 20-degree cone
    AtomicRayCast = 18795, // Boss->self, 6.0s cast, single-target
    AtomicRay = 18796, // BossHelper->location, 6.0s cast, range 10 circle
    MagitekRayBit = 18791, // MagitekBit->self, 6.0s cast, range 50+R width 2 rect
    SelfDetonate = 18792, // MagitekBit->self, 7.0s cast, range 40+R circle, enrage if bits are not killed before cast
}

class MagitekRayRightArm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayRightArm), new AOEShapeRect(45, 4));
class MagitekRayLeftArm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayLeftArm), new AOEShapeRect(45, 4));
class AngrySalamander(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AngrySalamander), new AOEShapeRect(40, 3));
class TerminusEstRects(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(40, 2);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TerminusEstLocationHelper)
        {
            _aoes.AddRange(
            [
                new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)),
                new(_shape, caster.Position, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell)),
                new(_shape, caster.Position, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell))
            ]);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TerminusEstVisual)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
class TerminusEstCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TerminusEstLocationHelper), new AOEShapeCircle(3));
class FireII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FireII), 5);
class GarleanFire(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GarleanFire), 5);
class MetalCutter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MetalCutter), new AOEShapeCone(30, 10.Degrees()));
class MagitekRayBits(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRayBit), new AOEShapeRect(50, 1));
class AtomicRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AtomicRay), new AOEShapeCircle(10));
class SelfDetonate(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SelfDetonate), "Enrage if bits are not killed before cast");
class VowsOfVirtueDeedsOfCrueltyStates : StateMachineBuilder
{
    public VowsOfVirtueDeedsOfCrueltyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekRayRightArm>()
            .ActivateOnEnter<MagitekRayLeftArm>()
            .ActivateOnEnter<AngrySalamander>()
            .ActivateOnEnter<TerminusEstCircle>()
            .ActivateOnEnter<TerminusEstRects>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<GarleanFire>()
            .ActivateOnEnter<MetalCutter>()
            .ActivateOnEnter<MagitekRayBits>()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<SelfDetonate>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69218, NameID = 9189)]
public class VowsOfVirtueDeedsOfCruelty(WorldState ws, Actor primary) : BossModule(ws, primary, new(240, 230), new ArenaBoundsSquare(20));
