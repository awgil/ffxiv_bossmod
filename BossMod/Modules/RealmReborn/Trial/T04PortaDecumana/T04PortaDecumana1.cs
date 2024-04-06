namespace BossMod.RealmReborn.Trial.T04PortaDecumana.Phase1;

public enum OID : uint
{
    Boss = 0x38FB, // x1
    UltimaGaruda = 0x38FC, // x1
    UltimaTitan = 0x38FD, // x1
    UltimaIfrit = 0x38FF, // x1
    GraniteGaol = 0x38FE, // spawn during fight
    Helper = 0x233C, // x15
};

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

    //_Gen_ = 28988, // Helper->player, no cast, single-target, visual ???
    //_Gen_ = 28511, // Helper->self, no cast, ??? (cast during transition)
    //_Gen_ = 28992, // Helper->self, no cast, single-target, ??? (cast during transition)
    TransitionFinish1 = 28994, // Boss->self, no cast, single-target, visual (lose buff)
    TransitionFinish2 = 28993, // Boss->self, no cast, single-target, visual (lose buff)
    TransitionFinish3 = 28995, // Boss->self, no cast, single-target, visual (lose buff)
};

public enum SID : uint
{
    Invincibility = 325, // none->Boss, extra=0x0
    VortexBarrier = 3012, // Boss->Boss, extra=0x0
};

class EarthenFury : Components.RaidwideCast
{
    public EarthenFury() : base(ActionID.MakeSpell(AID.EarthenFuryAOE)) { }
}

class Geocrush : Components.SelfTargetedAOEs
{
    public Geocrush() : base(ActionID.MakeSpell(AID.Geocrush), new AOEShapeCircle(25)) { } // TODO: verify falloff...
}

class Landslide1 : Components.SelfTargetedAOEs
{
    public Landslide1() : base(ActionID.MakeSpell(AID.Landslide1), new AOEShapeRect(40, 3)) { }
}

class Landslide2 : Components.SelfTargetedAOEs
{
    public Landslide2() : base(ActionID.MakeSpell(AID.Landslide2), new AOEShapeRect(40, 3)) { }
}

class WeightOfTheLand : Components.SelfTargetedAOEs
{
    public WeightOfTheLand() : base(ActionID.MakeSpell(AID.WeightOfTheLand), new AOEShapeCircle(6)) { }
}

class AerialBlast : Components.RaidwideCast
{
    public AerialBlast() : base(ActionID.MakeSpell(AID.AerialBlastAOE)) { }
}

class EyeOfTheStorm : Components.SelfTargetedAOEs
{
    public EyeOfTheStorm() : base(ActionID.MakeSpell(AID.EyeOfTheStormAOE), new AOEShapeDonut(12.5f, 25)) { }
}

class MistralShriek : Components.SelfTargetedAOEs
{
    public MistralShriek() : base(ActionID.MakeSpell(AID.MistralShriek), new AOEShapeCircle(23)) { }
}

class Hellfire : Components.RaidwideCast
{
    public Hellfire() : base(ActionID.MakeSpell(AID.HellfireAOE)) { }
}

class RadiantPlume : Components.SelfTargetedAOEs
{
    public RadiantPlume() : base(ActionID.MakeSpell(AID.RadiantPlumeAOE), new AOEShapeCircle(8)) { }
}

class VulcanBurst : Components.KnockbackFromCastTarget
{
    public VulcanBurst() : base(ActionID.MakeSpell(AID.VulcanBurst), 15) { }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(module.Bounds.Center, module.Bounds.HalfSize - Distance), Casters[0].CastInfo!.NPCFinishAt);
    }
}

class T04PortaDecumana1States : StateMachineBuilder
{
    public T04PortaDecumana1States(BossModule module) : base(module)
    {
        TrivialPhase()
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
public class T04PortaDecumana1 : BossModule
{
    public T04PortaDecumana1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-772, -600), 20)) { }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        if (PrimaryActor.FindStatus(SID.Invincibility) != null || PrimaryActor.FindStatus(SID.VortexBarrier) != null)
            hints.PotentialTargets.RemoveAll(e => e.Actor == PrimaryActor);
    }
}
