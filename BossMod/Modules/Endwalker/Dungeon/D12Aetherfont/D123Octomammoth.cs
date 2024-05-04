namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D123Octomammoth;

public enum OID : uint
{
    Boss = 0x3EAA, // R=26.0
    MammothTentacle = 0x3EAB, // R=6.0
    Crystals = 0x3EAC, // R=0.5
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 33357, // Boss->player, no cast, single-target
    Breathstroke = 34551, // Boss->self, 16.5s cast, range 35 180-degree cone
    Clearout = 33348, // MammothTentacle->self, 9.0s cast, range 16 120-degree cone
    Octostroke = 33347, // Boss->self, 16.0s cast, single-target
    SalineSpit1 = 33352, // Boss->self, 3.0s cast, single-target
    SalineSpit2 = 33353, // Helper->self, 6.0s cast, range 8 circle
    Telekinesis1 = 33349, // Boss->self, 5.0s cast, single-target
    Telekinesis2 = 33351, // Helper->self, 10.0s cast, range 12 circle
    TidalBreath = 33354, // Boss->self, 10.0s cast, range 35 180-degree cone
    TidalRoar = 33356, // Boss->self, 5.0s cast, range 60 circle
    VividEyes = 33355, // Boss->self, 4.0s cast, range 20-26 donut
    WaterDrop = 34436, // Helper->player, 5.0s cast, range 6 circle
    WallopVisual = 33350, // Boss->self, no cast, single-target, visual, starts tentacle wallops
    Wallop = 33346, // MammothTentacle->self, 3.0s cast, range 22 width 8 rect
}

class Border(BossModule module) : BossComponent(module)
{
    private const float _platformOffset = 25;
    private const float _platformRadius = 8;
    private static readonly Angle[] _platformDirections = [-90.Degrees(), -45.Degrees(), 0.Degrees(), 45.Degrees(), 90.Degrees()];
    private static readonly WDir[] _platformCenters = _platformDirections.Select(d => _platformOffset * d.ToDirection()).ToArray();

    private const float _bridgeInner = 22;
    private const float _bridgeOuter = 26;
    private static readonly Angle _offInner = DirToPointAtDistance(_bridgeInner);
    private static readonly Angle _offOuter = DirToPointAtDistance(_bridgeOuter);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(p =>
        {
            // union of platforms
            var res = _platformCenters.Select(off => ShapeDistance.Circle(Module.Center + off, _platformRadius)(p)).Min();
            // union of bridges
            for (int i = 1; i < 5; ++i)
                res = Math.Min(res, ShapeDistance.Rect(Module.Center + _platformCenters[i - 1], Module.Center + _platformCenters[i], 3)(p));
            // invert
            return -res;
        });

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw platforms
        foreach (var c in _platformCenters)
            Arena.AddCircle(Module.Center + c, _platformRadius, ArenaColor.Border);

        // draw bridges
        for (int i = 1; i < 5; ++i)
        {
            DrawBridgeLine(_platformDirections[i - 1], _platformDirections[i], _offInner, _bridgeInner);
            DrawBridgeLine(_platformDirections[i - 1], _platformDirections[i], _offOuter, _bridgeOuter);
        }
    }

    private static Angle DirToPointAtDistance(float d) => Angle.Acos((_platformOffset * _platformOffset + d * d - _platformRadius * _platformRadius) / (2 * _platformOffset * d));

    private void DrawBridgeLine(Angle from, Angle to, Angle offset, float distance)
    {
        var p1 = Module.Center + distance * (from + offset).ToDirection();
        var p2 = Module.Center + distance * (to - offset).ToDirection();
        Arena.AddLine(p1, p2, ArenaColor.Border);
    }
}

class Wallop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wallop), new AOEShapeRect(22, 4));
class VividEyes(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VividEyes), new AOEShapeDonut(20, 26));
class Clearout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Clearout), new AOEShapeCone(16, 60.Degrees()));
class TidalBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TidalBreath), new AOEShapeCone(35, 90.Degrees()));
class Breathstroke(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Breathstroke), new AOEShapeCone(35, 90.Degrees()));
class TidalRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TidalRoar));
class WaterDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WaterDrop), 6);
class SalineSpit(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SalineSpit2), new AOEShapeCircle(8));
class Telekinesis(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Telekinesis2), new AOEShapeCircle(12));

class D123OctomammothStates : StateMachineBuilder
{
    public D123OctomammothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<VividEyes>()
            .ActivateOnEnter<WaterDrop>()
            .ActivateOnEnter<TidalRoar>()
            .ActivateOnEnter<TidalBreath>()
            .ActivateOnEnter<Telekinesis>()
            .ActivateOnEnter<Breathstroke>()
            .ActivateOnEnter<SalineSpit>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12334)]
class D123Octomammoth : BossModule
{
    public D123Octomammoth(WorldState ws, Actor primary) : base(ws, primary, new(-370, -368), new ArenaBoundsCircle(33.3f))
    {
        ActivateComponent<Border>();
    }
}
