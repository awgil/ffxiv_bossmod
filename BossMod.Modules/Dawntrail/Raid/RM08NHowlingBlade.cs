namespace BossMod.Dawntrail.Raid.RM08NHowlingBlade;

public enum OID : uint
{
    Boss = 0x4722, // R5.005, x1
    Helper = 0x233C, // R0.500, x40, Helper type
    HowlingBladeShadow = 0x4723, // R4.235, x5
    MoonlitShadow = 0x4724, // R5.005, x4
    WolfOfWind1 = 0x4725, // R1.500, x1
    GleamingFang = 0x4726, // R1.400, x8
    WolfOfStone1 = 0x4732, // R1.500, x1
    WolfOfWind2 = 0x47B0, // R1.500, x1
    WolfOfStone2 = 0x47B1, // R1.500, x1
    WolfOfWind3 = 0x4849, // R1.500, x5
    WolfOfStone3 = 0x484A, // R1.500, x4
}

public enum AID : uint
{
    AutoAttack = 42221, // Boss->player, no cast, single-target
    AutoAttackStone = 42224, // WolfOfStone1->player, no cast, single-target
    AutoAttackWind = 42223, // WolfOfWind1->player, no cast, single-target
    Teleport = 41835, // Boss->location, no cast, single-target

    ExtraplanarPursuit = 41870, // Boss->self, 1.6+2.4s cast, single-target, visual (raidwide)
    ExtraplanarPursuitAOE = 42830, // Helper->self, 4.0s cast, range 40 circle, raidwide
    GreatDivide = 41869, // Boss->players, 5.0s cast, range 60 width 6 rect, shared tankbuster
    Heavensearth = 41865, // Boss->self, 5.0s cast, single-target, visual (stack)
    HeavensearthAOE1 = 41866, // Helper->players, 5.3s cast, range 6 circle stack
    HeavensearthAOE2 = 41859, // Helper->players, 5.0s cast, range 6 circle stack

    WolvesReign1 = 43373, // Boss->self, 5.1s cast, single-target, visual (clone circles + jumps)
    WolvesReign2 = 43374, // Boss->self, 5.1s cast, single-target, visual
    WolvesReign3 = 43279, // Boss->self, 4.6s cast, single-target, visual
    WolvesReign4 = 43280, // Boss->self, 4.6s cast, single-target, visual
    WolvesReignJump1 = 43375, // Boss->location, no cast, single-target
    WolvesReignJump2 = 43296, // Boss->location, no cast, single-target
    WolvesReignTeleport1 = 43383, // Boss->location, 2.0+0.5s cast, single-target
    WolvesReignTeleport2 = 41841, // Boss->location, 1.0+0.5s cast, single-target
    WolvesReignConeBoss1 = 43385, // Boss->self, 2.5+0.5s cast, single-target
    WolvesReignConeBoss2 = 41842, // Boss->self, 1.5+0.5s cast, single-target
    WolvesReignClone1 = 43376, // HowlingBladeShadow->self, 6.3s cast, single-target
    WolvesReignClone2 = 43377, // HowlingBladeShadow->self, 6.4s cast, single-target
    WolvesReignClone3 = 43378, // HowlingBladeShadow->self, 6.5s cast, single-target
    WolvesReignClone4 = 43299, // HowlingBladeShadow->self, 5.8s cast, single-target
    WolvesReignClone5 = 43300, // HowlingBladeShadow->self, 5.9s cast, single-target
    WolvesReignClone6 = 43301, // HowlingBladeShadow->self, 6.0s cast, single-target
    WolvesReignCircle1 = 43380, // Helper->self, 6.7s cast, range 6 circle
    WolvesReignCircle2 = 43381, // Helper->self, 6.8s cast, range 6 circle
    WolvesReignCircle3 = 43382, // Helper->self, 6.9s cast, range 6 circle
    WolvesReignCircle4 = 43379, // Helper->self, 7.0s cast, range 6 circle
    WolvesReignCircle5 = 43302, // Helper->self, 6.2s cast, range 6 circle
    WolvesReignCircle6 = 43303, // Helper->self, 6.3s cast, range 6 circle
    WolvesReignCircle7 = 43304, // Helper->self, 6.4s cast, range 6 circle
    WolvesReignCircle8 = 43311, // Helper->self, 6.5s cast, range 6 circle
    WolvesReignRect1 = 43384, // Helper->self, 2.5s cast, range 36 width 10 rect
    WolvesReignRect2 = 43368, // Helper->self, 1.5s cast, range 28 width 10 rect
    WolvesReignCone1 = 43386, // Helper->self, 3.0s cast, range 40 120-degree cone
    WolvesReignCone2 = 42928, // Helper->self, 2.0s cast, range 40 120-degree cone

    BeckonMoonlight1 = 43387, // Boss->self, 4.0s cast, single-target, visual (moonlit shadows)
    BeckonMoonlight2 = 41845, // Boss->self, 3.0s cast, single-target, visual (moonlit shadows)
    MoonlitShadowTeleport1 = 43390, // MoonlitShadow->location, no cast, single-target
    MoonlitShadowTeleport2 = 43391, // MoonlitShadow->location, no cast, single-target
    MoonlitShadowTeleport3 = 41950, // MoonlitShadow->location, no cast, single-target
    MoonlitShadowTeleport4 = 41951, // MoonlitShadow->location, no cast, single-target
    MoonbeamsBite1 = 43388, // MoonlitShadow->self, 6.0s cast, range 40 width 20 rect
    MoonbeamsBite2 = 43389, // MoonlitShadow->self, 6.0s cast, range 40 width 20 rect
    MoonbeamsBite3 = 41847, // MoonlitShadow->self, 5.0s cast, range 40 width 20 rect
    MoonbeamsBite4 = 41846, // MoonlitShadow->self, 5.0s cast, range 40 width 20 rect

    Shadowchase1 = 43392, // Boss->self, 4.0s cast, single-target, visual (clone lines)
    Shadowchase2 = 41843, // Boss->self, 4.0s cast, single-target, visual (clone lines)
    ShadowchaseAOE1 = 43393, // HowlingBladeShadow->self, 4.0s cast, range 40 width 8 rect
    ShadowchaseAOE2 = 41844, // HowlingBladeShadow->self, 3.0s cast, range 40 width 8 rect
    BareFangs = 41860, // Boss->self, 3.0s cast, single-target, visual (charges)
    FangedCharge = 41861, // GleamingFang->self, 4.0s cast, range 46 width 6 rect
    TerrestrialRage = 41862, // Boss->self, 3.0s cast, single-target, visual (puddles)
    TargetedQuakeVisual = 42885, // Boss->self, no cast, single-target
    TargetedQuake = 41864, // Helper->location, 4.0s cast, range 4 circle

    TacticalPack = 41852, // Boss->self, 3.0s cast, single-target, visual (adds)
    TacticalPackVisual = 41853, // Boss->self, no cast, single-target
    Gust = 41858, // Helper->player, 5.0s cast, range 5 circle spread
    WealOfStoneAdd1 = 41894, // WolfOfStone1->self, 5.0s cast, single-target
    WealOfStoneAdd2 = 43400, // WolfOfStone3->self, 2.5s cast, single-target
    WealOfStoneAdd3 = 43404, // WolfOfStone3->self, 2.5s cast, single-target
    WealOfStone1 = 41895, // Helper->self, 5.0s cast, range 40 width 6 rect
    WealOfStone2 = 43401, // Helper->self, 2.5s cast, range 40 width 6 rect
    WealOfStone3 = 43405, // Helper->self, 2.5s cast, range 40 width 6 rect
    GrowlingWindAdd = 41892, // WolfOfWind1->self, 5.0s cast, single-target
    GrowlingWind = 41893, // Helper->self, 5.0s cast, range 40 width 6 rect
    RoaringWindAdd = 43396, // WolfOfWind3->self, 2.5s cast, single-target
    RoaringWind = 43397, // Helper->self, 2.5s cast, range 40 width 8 rect
    RavenousSaber1 = 42819, // Helper->self, 3.4s cast, range 40 circle, raidwide
    RavenousSaber2 = 42820, // Helper->self, 3.6s cast, range 40 circle, raidwide
    RavenousSaber3 = 42821, // Helper->self, 3.9s cast, range 40 circle, raidwide
    RavenousSaber4 = 43516, // Helper->self, 6.0s cast, range 40 circle, raidwide
    RavenousSaber5 = 41855, // Helper->self, 7.3s cast, range 40 circle, raidwide

    TerrestrialTitans = 43316, // Boss->self, 3.0s cast, single-target, visual (pillars)
    TerrestrialTitansAOE = 43317, // Helper->self, 3.0s cast, range 3 circle
    TitanicPursuit = 41851, // Boss->self, no cast, range 40 circle, raidwide
    TitanicPursuitAOE = 42832, // Helper->self, 2.4s cast, range 40 circle
    Towerfall = 43315, // Helper->self, 4.4s cast, range 30 width 6 rect
    TrackingTremors = 42210, // Boss->self, 5.0s cast, single-target, visual (multi-hit stack)
    TrackingTremorsAOE = 42211, // Helper->players, no cast, range 6 circle
}

public enum IconID : uint
{
    GreatDivide = 598, // player
    Heavensearth1 = 62, // player
    Heavensearth2 = 93, // player
    Gust = 376, // player
    TrackingTremors = 316, // player
}

class ExtraplanarPursuit(BossModule module) : Components.RaidwideCast(module, AID.ExtraplanarPursuitAOE);
class GreatDivide(BossModule module) : Components.CastSharedTankbuster(module, AID.GreatDivide, new AOEShapeRect(60, 3));
class Heavensearth1(BossModule module) : Components.StackWithCastTargets(module, AID.HeavensearthAOE1, 6, 4);
class Heavensearth2(BossModule module) : Components.StackWithCastTargets(module, AID.HeavensearthAOE2, 6, 4);
class Gust(BossModule module) : Components.SpreadFromCastTargets(module, AID.Gust, 5);
class TargetedQuake(BossModule module) : Components.StandardAOEs(module, AID.TargetedQuake, new AOEShapeCircle(4));
class WindStoneLines(BossModule module) : Components.GroupedAOEs(module, [AID.GrowlingWind, AID.WealOfStone1, AID.WealOfStone2, AID.WealOfStone3], new AOEShapeRect(40, 3));
class FangedCharge(BossModule module) : Components.StandardAOEs(module, AID.FangedCharge, new AOEShapeRect(46, 3), 2, highlightImminent: true);
class MoonbeamsBite(BossModule module) : Components.GroupedAOEs(module, [AID.MoonbeamsBite1, AID.MoonbeamsBite2, AID.MoonbeamsBite3, AID.MoonbeamsBite4], new AOEShapeRect(40, 10), 2, true);
class ShadowchaseRoaringWind(BossModule module) : Components.GroupedAOEs(module, [AID.ShadowchaseAOE1, AID.ShadowchaseAOE2, AID.RoaringWind], new AOEShapeRect(40, 4));
class TerrestrialTitans(BossModule module) : Components.StandardAOEs(module, AID.TerrestrialTitansAOE, new AOEShapeCircle(3));
class TitanicPursuit(BossModule module) : Components.RaidwideCast(module, AID.TitanicPursuitAOE);
class Towerfall(BossModule module) : Components.StandardAOEs(module, AID.Towerfall, new AOEShapeRect(30, 3));
class RavenousSaber(BossModule module) : Components.CastCounterMulti(module, [AID.RavenousSaber1, AID.RavenousSaber2, AID.RavenousSaber3, AID.RavenousSaber4, AID.RavenousSaber5])
{
    private readonly List<DateTime> _hits = [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_hits.Count > 0)
            hints.Add($"Raidwide x{_hits.Count}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_hits.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _hits[0], AIHints.PredictedDamageType.Raidwide, _hits.Count, _hits[^1]);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            _hits.Add(Module.CastFinishAt(spell));
            _hits.Sort();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (WatchedActions.Contains(spell.Action) && _hits.Count > 0)
            _hits.RemoveAt(0);
    }
}
class WolvesReignCircle(BossModule module) : Components.GroupedAOEs(module, [AID.WolvesReignCircle1, AID.WolvesReignCircle2, AID.WolvesReignCircle3, AID.WolvesReignCircle4,
    AID.WolvesReignCircle5, AID.WolvesReignCircle6, AID.WolvesReignCircle7, AID.WolvesReignCircle8], new AOEShapeCircle(6));
class WolvesReignRect1(BossModule module) : Components.StandardAOEs(module, AID.WolvesReignRect1, new AOEShapeRect(36, 5));
class WolvesReignRect2(BossModule module) : Components.StandardAOEs(module, AID.WolvesReignRect2, new AOEShapeRect(28, 5));

class WolvesReignCone(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCone _shape = new(40, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WolvesReignTeleport1:
            case AID.WolvesReignTeleport2:
                _aoe = new(_shape, spell.LocXZ, Angle.FromDirection(Module.Center - spell.LocXZ), Module.CastFinishAt(spell, (AID)spell.Action.ID == AID.WolvesReignTeleport1 ? 5.1f : 4.1f));
                break;
            case AID.WolvesReignCone1:
            case AID.WolvesReignCone2:
                _aoe = new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WolvesReignCone1 or AID.WolvesReignCone2)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}

class TrackingTremors(BossModule module) : Components.StackWithIcon(module, (uint)IconID.TrackingTremors, AID.TrackingTremorsAOE, 6, 6, 4, 8, 5)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != StackAction)
            return;
        if (++NumCasts % 5 == 0)
            Stacks.Clear();
        else
            for (var i = 0; i < Stacks.Count; ++i)
                Stacks[i] = Stacks[i] with { Activation = WorldState.FutureTime(1.1f), Hits = 5 - NumCasts % 5 };
    }
}

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _shrink;
    private readonly List<WPos> _pillars = [];
    private bool _small;

    private static readonly AOEShapeDonut _donut = new(12, 17);
    private static readonly WPos[] _pillarPositions = [new(100, 88.5f), new(109.917f, 105.619f), new(90.075f, 105.619f), new(109.959f, 94.25f), new(100, 111.5f), new(90.041f, 94.25f)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_shrink);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            if (state == 0x00200010)
            {
                _shrink = new(_donut, Arena.Center, default, WorldState.FutureTime(11.2f));
            }
            else if (state == 0x00020001)
            {
                _shrink = null;
                _small = true;
                Arena.Bounds = new ArenaBoundsCircle(12);
            }
        }
        else if (index is >= 0x08 and <= 0x19)
        {
            if (state == 0x00020001)
            {
                _pillars.Add(_pillarPositions[(index - 0x08) / 3]);
                if (_pillars.Count == 3)
                    UpdateBounds();
            }
            else if (state == 0x00200004 && _pillars.Count > 0)
            {
                _pillars.Clear();
                UpdateBounds();
            }
        }
    }

    private void UpdateBounds()
    {
        var radius = _small ? 12 : 17;
        if (_pillars.Count == 0)
        {
            Arena.Bounds = new ArenaBoundsCircle(radius);
            return;
        }
        var clipper = new PolygonClipper();
        PolygonClipper.Operand pillars = new();
        foreach (var p in _pillars)
            pillars.AddContour(CurveApprox.Circle(3.5f, 0.1f).Select(c => c + (p - Arena.Center)));
        Arena.Bounds = new ArenaBoundsCustom(radius, clipper.Difference(new(CurveApprox.Circle(radius, 0.1f)), pillars));
    }
}

class RM08NHowlingBladeStates : StateMachineBuilder
{
    public RM08NHowlingBladeStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<FangedCharge>()
            .ActivateOnEnter<TargetedQuake>()
            .ActivateOnEnter<WindStoneLines>()
            .ActivateOnEnter<ShadowchaseRoaringWind>()
            .ActivateOnEnter<WolvesReignCircle>()
            .ActivateOnEnter<WolvesReignRect1>()
            .ActivateOnEnter<WolvesReignRect2>()
            .ActivateOnEnter<WolvesReignCone>()
            .ActivateOnEnter<TerrestrialTitans>()
            .ActivateOnEnter<TitanicPursuit>()
            .ActivateOnEnter<Towerfall>();
    }

    private void SinglePhase(uint id)
    {
        ExtraplanarPursuit(id, 7.2f);
        GreatDivide(id + 0x10000, 2.7f);
        WolvesReign(id + 0x20000, 7.4f, true, 1);
        WolvesReign(id + 0x30000, 5.1f, true, 2);
        Heavensearth1(id + 0x40000, 25.3f);
        BeckonMoonlight(id + 0x50000, 9.2f, AID.BeckonMoonlight1, 4, 9.5f);
        Shadowchase(id + 0x60000, 2.7f, AID.Shadowchase1, 7.3f, 0);
        Cast(id + 0x70000, AID.BareFangs, 5.4f, 3);
        Cast(id + 0x80000, AID.TerrestrialRage, 3.2f, 3);
        Heavensearth1(id + 0x90000, 9.2f);
        Cast(id + 0xA0000, AID.TacticalPack, 10.3f, 3, "Adds")
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<Heavensearth2>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Condition(id + 0xA0010, 17.3f, () => AddsResolved() >= 1, "Spreads / stack");
        Condition(id + 0xA0020, 16.2f, () => AddsResolved() >= 2, "Stack / spreads")
            .DeactivateOnExit<Gust>()
            .DeactivateOnExit<Heavensearth2>();
        ComponentCondition<RavenousSaber>(id + 0xA0030, 21, c => c.NumCasts >= 5, "Raidwide x5")
            .ActivateOnEnter<RavenousSaber>()
            .DeactivateOnExit<RavenousSaber>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0xA0040, true, 6.4f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        WolvesReign(id + 0xB0000, 10.3f, false, 3);
        TitansPillars(id + 0xC0000, 2.6f);
        ExtraplanarPursuit(id + 0xD0000, 8.5f);
        GreatDivide(id + 0xE0000, 1.7f);
        Cast(id + 0xF0000, AID.BareFangs, 9.6f, 3);
        WolvesReign(id + 0x100000, 5.7f, false, 4);
        Shadowchase(id + 0x110000, 4.7f, AID.Shadowchase2, 6.2f, 5);
        ExtraplanarPursuit(id + 0x120000, 7.6f);
        TrackingTremors(id + 0x130000, 0.7f);
        BeckonMoonlight(id + 0x140000, 10.2f, AID.BeckonMoonlight2, 3, 8.4f);
        Cast(id + 0x150000, AID.TerrestrialRage, 0.3f, 3);
        ExtraplanarPursuit(id + 0x160000, 8.1f);
        TrackingTremors(id + 0x170000, 0.7f);
        GreatDivide(id + 0x180000, 2);

        SimpleState(id + 0x190000, 120, "Until enrage")
            .ActivateOnEnter<ExtraplanarPursuit>()
            .ActivateOnEnter<GreatDivide>()
            .ActivateOnEnter<Heavensearth1>()
            .ActivateOnEnter<Heavensearth2>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<MoonbeamsBite>()
            .ActivateOnEnter<RavenousSaber>()
            .ActivateOnEnter<TrackingTremors>();
    }

    private void ExtraplanarPursuit(uint id, float delay)
    {
        Cast(id, AID.ExtraplanarPursuit, delay, 1.6f)
            .ActivateOnEnter<ExtraplanarPursuit>();
        ComponentCondition<ExtraplanarPursuit>(id + 0x10, 2.4f, c => c.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<ExtraplanarPursuit>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void GreatDivide(uint id, float delay)
    {
        Cast(id, AID.GreatDivide, delay, 5, "Shared tankbuster")
            .ActivateOnEnter<GreatDivide>()
            .DeactivateOnExit<GreatDivide>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void WolvesReign(uint id, float delay, bool first, int count)
    {
        CastMulti(id, first ? new[] { AID.WolvesReign1, AID.WolvesReign2 } : [AID.WolvesReign3, AID.WolvesReign4], delay, first ? 5.1f : 4.6f);
        ComponentCondition<WolvesReignCircle>(id + 0x10, 1.6f, c => c.NumCasts > (count - 1) * 4, "Circles");
        ComponentCondition<WolvesReignCone>(id + 0x20, first ? 8.2f : 6.5f, c => c.NumCasts >= count, "Cone");
    }

    private void Heavensearth1(uint id, float delay)
    {
        Cast(id, AID.Heavensearth, delay, 5)
            .ActivateOnEnter<Heavensearth1>();
        ComponentCondition<Heavensearth1>(id + 0x10, 0.3f, c => c.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<Heavensearth1>();
    }

    private void BeckonMoonlight(uint id, float delay, AID aid, float castTime, float biteDelay)
    {
        Cast(id, aid, delay, castTime)
            .ActivateOnEnter<MoonbeamsBite>();
        ComponentCondition<MoonbeamsBite>(id + 0x10, biteDelay, c => c.NumCasts >= 1, "Half-room 1");
        ComponentCondition<MoonbeamsBite>(id + 0x20, aid == AID.BeckonMoonlight1 ? 6 : 4.4f, c => c.NumCasts >= 4, "Half-room 4")
            .DeactivateOnExit<MoonbeamsBite>();
    }

    private void Shadowchase(uint id, float delay, AID aid, float linesDelay, int prevCasts)
    {
        Cast(id, aid, delay, 4);
        ComponentCondition<ShadowchaseRoaringWind>(id + 0x10, linesDelay, c => c.NumCasts > prevCasts, "Lines");
    }

    private void TitansPillars(uint id, float delay)
    {
        Cast(id, AID.TerrestrialTitans, delay, 3);
        ComponentCondition<Towerfall>(id + 0x10, 9.6f, c => c.NumCasts >= 3, "Pillars 1");
        ComponentCondition<Towerfall>(id + 0x20, 8.4f, c => c.NumCasts >= 6, "Pillars 2");
        ComponentCondition<Towerfall>(id + 0x30, 8.3f, c => c.NumCasts >= 9, "Pillars 3");
    }

    private void TrackingTremors(uint id, float delay)
    {
        CastStart(id, AID.TrackingTremors, delay)
            .ActivateOnEnter<TrackingTremors>();
        CastEnd(id + 1, 5);
        ComponentCondition<TrackingTremors>(id + 0x10, 1, c => c.NumCasts > 0, "Multi-hit stack start");
        ComponentCondition<TrackingTremors>(id + 0x20, 4.3f, c => c.NumCasts >= 5, "Multi-hit stack end")
            .DeactivateOnExit<TrackingTremors>();
    }

    private int AddsResolved() => (Module.FindComponent<Gust>()?.NumFinishedSpreads > 0 ? 1 : 0) + (Module.FindComponent<Heavensearth2>()?.NumFinishedStacks > 0 ? 1 : 0);
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1025, NameID = 13843)]
public class RM08NHowlingBlade(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(17))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.WolfOfStone1), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.WolfOfWind1), ArenaColor.Enemy);
    }
}
