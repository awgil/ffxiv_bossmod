namespace BossMod.Dawntrail.Dungeon.D09Yuweyawata.D092OverseerKanilokka;

public enum OID : uint
{
    Boss = 0x464A, // R2.340, x1
    PreservedSoul = 0x464B, // R2.500, x24
    Helper = 0x233C, // R0.500, x16, Helper type
}

public enum AID : uint
{
    AutoAttack = 40659, // Boss->player, no cast, single-target
    DarkSouls = 40658, // Boss->player, 5.0s cast, single-target tankbuster
    FreeSpirits = 40639, // Boss->self, 4.0+1.0s cast, single-target, visual (raidwide)
    FreeSpiritsAOE = 40640, // Helper->self, 5.0s cast, range 20 circle, raidwide
    Soulweave1 = 40641, // PreservedSoul->self, 2.5s cast, range ?-32 donut
    Soulweave2 = 40642, // PreservedSoul->self, 2.5s cast, range ?-32 donut
    PhantomFlood = 40643, // Boss->self, 3.7+1.3s cast, single-target, visual (persistent donut)
    PhantomFloodAOE = 40644, // Helper->self, 5.0s cast, range 5-20 donut
    DarkII = 40654, // Boss->self, 4.5+0.5s cast, single-target, visual (pizzas)
    DarkIIRepeat = 40655, // Boss->self, no cast, single-target, visual (second hit)
    DarkIIAOE1 = 40656, // Helper->self, 5.0s cast, range 35 30-degree cone
    DarkIIAOE2 = 40657, // Helper->self, 7.5s cast, range 35 30-degree cone
    TelltaleTears = 40649, // Helper->player, 5.0s cast, range 5 circle spread
    LostHope = 40645, // Boss->self, 3.0s cast, range 20 circle, apply misdirection
    Necrohazard = 40646, // Boss->self, 15.0s cast, range 20 circle with ? falloff
    Bloodburst = 40647, // Boss->self, 5.0s cast, range 45 circle, raidwide
    SoulDouse = 40651, // Helper->players, 5.0s cast, range 6 circle stack
}

public enum IconID : uint
{
    DarkSouls = 218, // player->self
    TelltaleTears = 558, // player->self
    SoulDouse = 62, // player->self
}

class BoundsChange(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 7)
            return;
        ArenaBounds? bounds = state switch
        {
            0x00020001 => D092OverseerKanilokka.StandardBounds,
            0x00200010 => D092OverseerKanilokka.SmallBounds,
            0x00800040 => D092OverseerKanilokka.ComplexBounds1,
            0x02000100 => D092OverseerKanilokka.ComplexBounds2,
            0x00080004 => D092OverseerKanilokka.InitialBounds,
            _ => null
        };
        if (bounds != null)
            Module.Arena.Bounds = bounds;
    }
}

class DarkSouls(BossModule module) : Components.SingleTargetCast(module, AID.DarkSouls);
class FreeSpirits(BossModule module) : Components.StandardAOEs(module, AID.FreeSpiritsAOE, new AOEShapeDonut(15, 20));

class Soulweave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];

    private static readonly AOEShapeDonut _shape = new(28, 32); // TODO: verify inner radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(_shape, c.CastInfo!.LocXZ, default, Module.CastFinishAt(c.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Soulweave1 or AID.Soulweave2)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Soulweave1 or AID.Soulweave2)
            _casters.Remove(caster);
    }
}

class PhantomFlood(BossModule module) : Components.StandardAOEs(module, AID.PhantomFloodAOE, new AOEShapeDonut(5, 20));

class DarkII(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(35, 15.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(6);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkIIAOE1 or AID.DarkIIAOE2)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkIIAOE1 or AID.DarkIIAOE2)
            _aoes.RemoveAll(aoe => aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
    }
}

class TelltaleTears : Components.SpreadFromCastTargets
{
    public TelltaleTears(BossModule module) : base(module, AID.TelltaleTears, 5)
    {
        ExtraAISpreadThreshold = 0;
    }
}

class Necrohazard(BossModule module) : Components.StandardAOEs(module, AID.Necrohazard, new AOEShapeCircle(18)) // TODO: verify falloff
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count > 0)
            hints.AddSpecialMode(AIHints.SpecialMode.Misdirection, default);
    }
}

class Bloodburst(BossModule module) : Components.RaidwideCast(module, AID.Bloodburst);
class SoulDouse(BossModule module) : Components.StackWithCastTargets(module, AID.SoulDouse, 6, 4);

class D092OverseerKanilokkaStates : StateMachineBuilder
{
    public D092OverseerKanilokkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BoundsChange>()
            .ActivateOnEnter<DarkSouls>()
            .ActivateOnEnter<FreeSpirits>()
            .ActivateOnEnter<Soulweave>()
            .ActivateOnEnter<PhantomFlood>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<TelltaleTears>()
            .ActivateOnEnter<Necrohazard>()
            .ActivateOnEnter<Bloodburst>()
            .ActivateOnEnter<SoulDouse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13634)]
public class D092OverseerKanilokka(WorldState ws, Actor primary) : BossModule(ws, primary, new(116, -66), InitialBounds)
{
    public static readonly ArenaBoundsCircle InitialBounds = new(20);
    public static readonly ArenaBoundsCustom StandardBounds = BuildCircularBounds(15);
    public static readonly ArenaBoundsCustom SmallBounds = BuildCircularBounds(5);
    public static readonly ArenaBoundsCustom ComplexBounds1 = BuildComplexBounds1();
    public static readonly ArenaBoundsCustom ComplexBounds2 = BuildComplexBounds2();

    private static ArenaBoundsCustom BuildCircularBounds(float radius) => new(InitialBounds.Radius, InitialBounds.Clipper.Simplify(new(CurveApprox.Circle(radius, 0.05f))));

    private static ArenaBoundsCustom BuildComplexBounds1()
    {
        var remove = new PolygonClipper.Operand();
        remove.AddContour([new(+6.9f, -18.7f), new(+4.8f, -15.2f), new(+1.5f, -13.8f), new(0, -12.4f), new(+1.3f, -10.6f), new(+3.7f, -6.2f), new(+3.6f, -3.2f),
            new(+4.2f, -2.2f), new(+6.6f, -1.5f), new(+8.2f, -2.1f), new(+11.1f, -5.6f), new(+15.9f, -4.8f), new(+17.2f, -0.5f), new(+20, 0), new(+25, -25)]);
        remove.AddContour([new(+19.8f, +2.8f), new(+15.9f, +1.6f), new(+13.5f, -1.9f), new(+11.8f, -1.6f), new(+10.4f, +0.6f), new(+6.9f, +2.3f), new(+3.9f, +3.0f),
            new(+2.7f, +4.1f), new(+1.7f, +4.5f), new(+0.2f, +7.3f), new(-4.7f, +10.4f), new(-4.5f, +12.4f), new(-1.0f, +14.9f), new(-2.2f, +19.8f), new(+25, +25)]);
        remove.AddContour([new(-5.2f, +19.3f), new(-4.4f, +16.0f), new(-8.8f, +12.5f), new(-8.2f, +8.2f), new(-3.7f, +6.0f), new(-3.3f, +3.6f), new(-4.4f, +2.2f),
            new(-4.9f, +0.2f), new(-7.1f, -0.5f), new(-9.3f, -3.9f), new(-11.4f, -3.1f), new(-14.2f, -1.0f), new(-17.5f, -1.5f), new(-19.6f, -3.9f), new(-25, +25)]);
        remove.AddContour([new(-18.8f, -6.6f), new(-17.4f, -4.5f), new(-15.1f, -4.1f), new(-12.7f, -6.9f), new(-8.0f, -7.0f), new(-5.4f, -4.0f), new(-3.2f, -3.7f),
            new(-2.0f, -4.4f), new(0, -6.8f), new(-3.0f, -11.6f), new(-2.3f, -14.9f), new(+1.2f, -17.9f), new(+4.0f, -19.6f), new(-25, -25)]);
        return new(InitialBounds.Radius, InitialBounds.Clipper.Difference(new(CurveApprox.Circle(19.5f, 0.05f)), remove));
    }

    private static ArenaBoundsCustom BuildComplexBounds2()
    {
        var remove = new PolygonClipper.Operand();
        remove.AddContour([new(+1.4f, -25), new(+1.4f, -19.8f), new(+1.4f, -19.0f), new(+3.5f, -17.8f), new(+4.2f, -17.2f), new(+4.6f, -14.7f), new(+4.0f, -12.8f), new(+2.4f, -11.8f),
            new(+1.2f, -11.4f), new(-1.2f, -11.0f), new(-1.7f, -10.5f), new(-1.7f, -9.0f), new(+2.4f, -6.1f), new(+2.7f, -4.1f), new(+4.9f, +0.6f), new(+6.4f, +4.0f), new(+6.1f, +5.5f),
            new(+8.5f, +6.9f), new(+13.1f, +3.5f), new(+15.6f, +3.5f), new(+17.5f, +5.4f), new(+17.6f, +8.6f), new(+17.9f, +8.9f), new(+25, +8.9f), new(+25, -25)]);
        remove.AddContour([new(+16.6f, +25), new(+16.6f, +11.1f), new(+15.6f, +10.6f), new(+15.6f, +8.3f), new(+15.2f, +7.2f), new(+12.8f, +6.9f), new(+11.9f, +8.3f), new(+10.9f, +9.1f), new(+8.7f, +10.1f),
            new(+7.9f, +10.0f), new(+3.8f, +8.1f), new(+3.6f, +7.2f), new(+3.6f, +4.7f), new(+2.9f, +3.9f), new(-2.7f, +4.1f), new(-4.1f, +4.1f), new(-5.7f, +3.7f), new(-7.2f, +3.6f), new(-8.8f, +4.7f),
            new(-9.1f, +6.2f), new(-9.4f, +9.4f), new(-10.0f, +10.3f), new(-12.4f, +12.0f), new(-15.2f, +10.4f), new(-16.6f, +11.2f), new(-16.6f, +25)]);
        remove.AddContour([new(-25, +8.7f), new(-17.9f, +8.7f), new(-15.9f, +7.6f), new(-13.2f, +8.9f), new(-12.0f, +8.1f), new(-12.1f, +6.8f), new(-12.8f, +5.6f), new(-12.3f, +3.6f), new(-11.3f, +1.6f),
            new(-10.4f, +0.8f), new(-8.8f, +0.5f), new(-5.3f, +0.6f), new(-5.0f, +0.3f), new(-2.7f, -4.1f), new(-2.6f, -6.4f), new(-5.1f, -8.8f), new(-5.1f, -10.0f), new(-4.8f, -11.5f),
            new(-3.9f, -12.9f), new(-1.3f, -13.7f), new(0, -13.8f), new(+1.1f, -14.1f), new(+1.5f, -14.9f), new(+1.7f, -16.3f), new(-1.1f, -18.1f), new(-1.4f, -19.9f), new(-1.4f, -25), new(-25, -25)]);
        return new(InitialBounds.Radius, InitialBounds.Clipper.Union(new(InitialBounds.Clipper.Difference(new(CurveApprox.Circle(19.5f, 0.05f)), remove)), new(CurveApprox.Circle(5, 0.05f))));
    }
}
