namespace BossMod.RealmReborn.Trial.T04PortaDecumana.Phase1;

public enum OID : uint
{
    Boss = 0x38FB, // R=6.0
    UltimaGaruda = 0x38FC, // R=3.4
    UltimaTitan = 0x38FD, // /R=5.25
    UltimaIfrit = 0x38FF, // R=5.0
    GraniteGaol = 0x38FE, // R=1.8
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 29004, // Boss->player, no cast, single-target
    Teleport = 28628, // Boss->location, no cast, single-target

    EarthenFury = 28977, // Boss->self, 5.0s cast, single-target, visual
    EarthenFuryAOE = 28998, // UltimaTitan->self, 5.0s cast, raidwide
    Geocrush = 28999, // UltimaTitan->self, 5.0s cast, raidwide with ? falloff
    Landslide1 = 29000, // UltimaTitan->self, 3.0s cast, range 40 width 6 rect aoe with knockback 15
    Landslide2 = 28981, // Boss->self, 3.0s cast, range 40 width 6 rect aoe with knockback 15
    WeightOfTheLand = 29001, // Helper->self, 3.0s cast, range 6 circle aoe (10x at the same time)
    GraniteInterment = 28987, // Boss->self, 4.0s cast, single-target, visual

    AerialBlast = 28976, // Boss->self, 5.0s cast, single-target, visual
    AerialBlastAOE = 28996, // UltimaGaruda->self, 5.0s cast, raidwide
    EyeOfTheStorm = 28979, // Boss->self, 3.0s cast, single-target, visual
    EyeOfTheStormAOE = 28980, // Helper->self, 3.0s cast, range 12.5-25 donut aoe
    MistralShriek = 28997, // UltimaGaruda->self, 5.0s cast, range 23 circle aoe
    VortexBarrier = 28984, // Boss->self, 4.0s cast, single-target, visual (applies invuln to self)

    Hellfire = 28978, // Boss->self, 5.0s cast, single-target, visual
    HellfireAOE = 29002, // UltimaIfrit->self, 5.0s cast, raidwide
    RadiantPlume = 28982, // Boss->self, 2.2s cast, single-target, visual
    RadiantPlumeAOE = 28983, // Helper->self, 3.0s cast, range 8 circle aoe
    VulcanBurst = 29003, // UltimaIfrit->self, 4.0s cast, raidwide knockback 15

    Transition1 = 28988, // Helper->player, no cast, single-target
    Transition2 = 28992, // Helper->self, no cast, single-target
    Transition3 = 28511, // Helper->self, no cast, ???
    EarthenEternity = 29435, // Boss->self, 33.0s cast, range 35 circle
    HeadsmansWind = 28510, // Boss->self, 28.0s cast, range 35 circle
    RadiantBlaze = 28990, // Boss->self, 32.0s cast, single-target
    RadiantBlaze2 = 28991, // Helper->self, 32.0s cast, range 12 circle
    GraniteSepulchre = 28989, // GraniteGaol->self, 33.0s cast, single-target

    TransitionFinish1 = 28994, // Boss->self, no cast, single-target, visual (lose buff)
    TransitionFinish2 = 28993, // Boss->self, no cast, single-target, visual (lose buff)
    TransitionFinish3 = 28995, // Boss->self, no cast, single-target, visual (lose buff)
}

public enum SID : uint
{
    Invincibility = 325, // none->Boss, extra=0x0
    VortexBarrier = 3012, // Boss->Boss, extra=0x0
}

class EarthenFury(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EarthenFuryAOE));
class Geocrush(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Geocrush), new AOEShapeCircle(25)); // TODO: verify falloff...
class Landslide1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Landslide1), new AOEShapeRect(40, 3));
class Landslide2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Landslide2), new AOEShapeRect(40, 3));
class WeightOfTheLand(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WeightOfTheLand), new AOEShapeCircle(6));
class AerialBlast(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AerialBlastAOE));
class EyeOfTheStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EyeOfTheStormAOE), new AOEShapeDonut(12.5f, 25));
class MistralShriek(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MistralShriek), new AOEShapeCircle(23));
class Hellfire(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HellfireAOE));
class RadiantPlume(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RadiantPlumeAOE), new AOEShapeCircle(8));

class VulcanBurst(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.VulcanBurst), 15, stopAtWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center, Module.Bounds.Radius - Distance), Casters[0].CastInfo!.NPCFinishAt);
    }
}

class MeleeRange(BossModule module) : BossComponent(module) // force melee range for melee rotation solver users
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Service.Config.Get<AutorotationConfig>().Enabled)
            if (!Module.FindComponent<Geocrush>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<Landslide1>()!.ActiveAOEs(slot, actor).Any() &&
            !Module.FindComponent<Landslide2>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<WeightOfTheLand>()!.ActiveAOEs(slot, actor).Any() &&
            !Module.FindComponent<EyeOfTheStorm>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<MistralShriek>()!.ActiveAOEs(slot, actor).Any() &&
            !Module.FindComponent<RadiantPlume>()!.ActiveAOEs(slot, actor).Any())
                if (actor.Role is Role.Melee or Role.Tank)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 3));
    }
}

class T04PortaDecumana1States : StateMachineBuilder
{
    public T04PortaDecumana1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeleeRange>()
            .ActivateOnEnter<EarthenFury>()
            .ActivateOnEnter<Geocrush>()
            .ActivateOnEnter<Landslide1>()
            .ActivateOnEnter<Landslide2>()
            .ActivateOnEnter<WeightOfTheLand>()
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<EyeOfTheStorm>()
            .ActivateOnEnter<MistralShriek>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<RadiantPlume>()
            .ActivateOnEnter<VulcanBurst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 830, NameID = 2137, SortOrder = 1)]
public class T04PortaDecumana1(WorldState ws, Actor primary) : BossModule(ws, primary, new(-772, -600), new ArenaBoundsCircle(19.5f))
{
    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        if (PrimaryActor.FindStatus(SID.Invincibility) != null || PrimaryActor.FindStatus(SID.VortexBarrier) != null)
            hints.PotentialTargets.RemoveAll(e => e.Actor == PrimaryActor);
    }
}
