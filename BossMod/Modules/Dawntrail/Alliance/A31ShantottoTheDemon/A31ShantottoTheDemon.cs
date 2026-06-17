namespace BossMod.Dawntrail.Alliance.A31Shantotto;

public enum OID : uint
{
    Boss = 0x4D9F, // R4.900, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    LargeSpecimen = 0x4DDC, // R2.400, x2, Helper type
    LeyLines = 0x4DDD, // R1.000, x21
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    FlarePlay = 50215, // Boss->self, 5.0s cast, range 78 circle
    VidohunirCast = 50213, // Boss->player, 5.0+1.0s cast, single-target
    Vidohunir = 50214, // Helper->players, 6.0s cast, range 5 circle
    EmpiricalResearchCast = 50206, // Boss->self, 3.0s cast, single-target
    EmpiricalResearch = 50208, // Helper->self, 3.8s cast, range 80 width 12 rect
    SuperiorStoneIIRaidwide = 50193, // Boss->self, 4.0s cast, range 60 circle
    SuperiorStoneIICast = 50194, // Helper->self, 4.8s cast, range 21 width 13 rect
    SuperiorStoneIIInstant = 51025, // Helper->self, no cast, range 21 width 13 rect
    GroundbreakingQuakeCast = 50195, // Boss->self, 8.0s cast, single-target
    GroundbreakingQuake = 50196, // Helper->self, 9.0s cast, range 30 width 12 rect
    PainfulPressure = 50197, // Helper->self, no cast, ???
    Jump = 50192, // Boss->location, no cast, single-target
    DiagrammaticDoorway = 50200, // Boss->self, 3.0s cast, single-target
    DaintyStep = 50205, // Boss->location, no cast, single-target
    CircumscribedFireSlow = 50201, // Boss->self, 7.0s cast, range 6-70 donut
    CircumscribedFireFast = 50202, // Boss->self, 1.0s cast, range 6-70 donut
    LocalizedBlizzard = 50203, // Boss->self, 2.2s cast, range 10 circle
    ThunderAndError = 50204, // Helper->players, 5.0s cast, range 5 circle
    MeteoricRhyme = 50182, // Boss->self, 3.0s cast, single-target
    SmallSpecimen = 50184, // Helper->location, 4.0s cast, range 6 circle
    StardustSpecimen = 50185, // Helper->players, 6.0s cast, range 6 circle
    LargeSpecimen = 50186, // 4DDC->self, 6.0s cast, range 50 circle
    Unk1 = 50183, // Boss->location, no cast, range 30 circle
    Jump2 = 50648, // Boss->location, no cast, single-target
    Shockwave = 50187, // Helper->self, 10.5s cast, range 48 width 60 rect
    FallingRubbleTiny = 50188, // Helper->self, 4.5s cast, range 8 circle
    FallingRubbleSmall = 50189, // Helper->self, 4.5s cast, range 12 circle
    FallingRubbleLarge = 50190, // Helper->self, 4.5s cast, range 25 width 6 rect
    FallingRubbleHuge = 50191, // Helper->self, 4.5s cast, range 35 width 10 rect
    AeroDynamicsCast = 50198, // Boss->self, 3.0s cast, single-target
    AeroDynamicsKB1 = 50199, // Helper->self, no cast, range 48 width 60 rect
    AeroDynamicsKB2 = 50382, // Helper->self, no cast, range 48 width 60 rect
    FinalExamCast = 50210, // Boss->player, 4.2+0.8s cast, single-target
    FinalExamFirst = 50211, // Helper->players, 5.0s cast, range 6 circle
    FinalExamRest = 50212, // Helper->players, no cast, range 6 circle
}

public enum SID : uint
{
    Unk1 = 2552, // Boss->Boss/4DDC, extra=0x475
    EasterlyWinds = 5398, // none->player, extra=0x0
    WesterlyWinds = 5399, // none->player, extra=0x0
    Unk2 = 2160, // none->4D66, extra=0x3931
}

public enum IconID : uint
{
    TankbusterShare = 570, // player->self
    Spread = 558, // player->self
    Share = 318, // player->self
    Countdown = 713, // player/4D66->self
    ShareMulti = 305, // player->self
}

public enum TetherID : uint
{
    Red = 382, // 4DDD->4DDD
    PurpleCircle = 383, // 4DDD->4DDD
    Purple = 384, // 4DDD->4DDD
}

class FlarePlay(BossModule module) : Components.RaidwideCast(module, AID.FlarePlay);
class Vidohunir(BossModule module) : Components.CastSharedTankbuster(module, AID.Vidohunir, 5);
class EmpiricalResearch(BossModule module) : Components.StandardAOEs(module, AID.EmpiricalResearch, new AOEShapeRect(80, 6));
class SuperiorStoneIIRaidwide(BossModule module) : Components.RaidwideCast(module, AID.SuperiorStoneIIRaidwide);
class SuperiorStoneIIRect(BossModule module) : Components.StandardAOEs(module, AID.SuperiorStoneIICast, new AOEShapeRect(21, 6.5f));
class GroundbreakingQuake(BossModule module) : Components.StandardAOEs(module, AID.GroundbreakingQuake, new AOEShapeRect(30, 6));
class SuperiorStoneArena(BossModule module) : Components.CastCounter(module, AID.SuperiorStoneIICast)
{
    readonly RelSimplifiedComplexPolygon _rect = new(CurveApprox.Rect(new WDir(24, 0), new WDir(0, 30)));
    RelSimplifiedComplexPolygon _current = new(CurveApprox.Rect(new WDir(24, 0), new WDir(0, 30)));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            List<WDir> curve = [new WDir(-6.5f, 0).Rotate(spell.Rotation), new WDir(-6.5f, 21).Rotate(spell.Rotation), new WDir(6.5f, 21).Rotate(spell.Rotation), new WDir(6.5f, 0).Rotate(spell.Rotation)];
            var poly = new RelSimplifiedComplexPolygon(curve.Select(c => c + (caster.Position - Arena.Center)));
            _current = Arena.Bounds.Clipper.Difference(new(_current), new(poly));
            Arena.Bounds = new ArenaBoundsCustom(30, _current);
        }

        if ((AID)spell.Action.ID == AID.PainfulPressure)
        {
            _current = _rect;
            Arena.Bounds = new ArenaBoundsRect(24, 30);
        }
    }
}

class DiagrammaticDoorway(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch ((TetherID)tether.ID)
        {
            case TetherID.Purple:
                if (_predicted.Count == 0)
                    _predicted.Add(new(new AOEShapeDonut(6, 70), source.Position, default, WorldState.FutureTime(10.1f)));
                _predicted.Add(new(new AOEShapeDonut(6, 70), WorldState.Actors.Find(tether.Target)!.Position, default, _predicted[^1].Activation.AddSeconds(3.1f)));
                break;
            case TetherID.PurpleCircle:
                _predicted.Add(new(new AOEShapeDonut(6, 70), WorldState.Actors.Find(tether.Target)!.Position, default, _predicted[^1].Activation.AddSeconds(3.1f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CircumscribedFireSlow or AID.CircumscribedFireFast)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class LocalizedBlizzard(BossModule module) : Components.StandardAOEs(module, AID.LocalizedBlizzard, 10);
class ThunderAndError(BossModule module) : Components.SpreadFromCastTargets(module, AID.ThunderAndError, 5);
class SmallSpecimen(BossModule module) : Components.StandardAOEs(module, AID.SmallSpecimen, 6);
class LargeSpecimen(BossModule module) : Components.ProximityAOEs(module, AID.LargeSpecimen, 15);
class StardustSpecimen(BossModule module) : Components.StackWithCastTargets(module, AID.StardustSpecimen, 6);

class Shockwave(BossModule module) : Components.RaidwideCast(module, AID.Shockwave);

class FallingRubble1(BossModule module) : Components.StandardAOEs(module, AID.FallingRubbleTiny, 8);
class FallingRubble2(BossModule module) : Components.StandardAOEs(module, AID.FallingRubbleSmall, 12);
class FallingRubble3(BossModule module) : Components.StandardAOEs(module, AID.FallingRubbleLarge, new AOEShapeRect(25, 3));
class FallingRubble4(BossModule module) : Components.StandardAOEs(module, AID.FallingRubbleHuge, new AOEShapeRect(35, 5));

class Winds : Components.Knockback
{
    record struct Wind(WDir Direction, DateTime Activation);
    readonly Wind[] _directions = new Wind[PartyState.MaxAllianceSize];

    readonly List<(WDir WindDir, WPos Center, float HalfWidth)> _safeWalls = [];

    public Winds(BossModule module) : base(module, AID.AeroDynamicsKB1)
    {
        CalcWalls();
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var dir = _directions.BoundSafeAt(slot);
        if (dir.Activation > WorldState.CurrentTime)
        {
            var dist = 50f;
            if (HitsWall(_safeWalls, dir.Direction, actor.Position))
                dist = Arena.Bounds.IntersectRay(actor.Position - Arena.Center, dir.Direction);

            yield return new(actor.Position, dist - 0.1f, dir.Activation, null, dir.Direction.ToAngle(), Kind.DirForward);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
            {
                var d = _directions[slot].Direction;
                var walls = _safeWalls.ToList();
                hints.AddForbiddenZone(p => !HitsWall(walls, d, p), src.Activation);
            }
    }

    static bool HitsWall(List<(WDir WindDir, WPos Center, float HalfWidth)> walls, WDir windDir, WPos pos)
    {
        return walls.Any(w => w.WindDir == windDir && pos.InRect(w.Center, w.WindDir * -50, w.HalfWidth));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        WDir d;
        if ((SID)status.ID == SID.EasterlyWinds)
            d = new WDir(-1, 0);
        else if ((SID)status.ID == SID.WesterlyWinds)
            d = new WDir(1, 0);
        else
            return;

        if (Raid.TryFindSlot(actor, out var slot) && slot < _directions.Length)
            _directions[slot] = new(d, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AeroDynamicsKB1:
            case AID.AeroDynamicsKB2:
                NumCasts++;
                Array.Fill(_directions, default);
                break;
            case AID.SuperiorStoneIICast:
                CalcWalls();
                break;
        }
    }

    void CalcWalls()
    {
        _safeWalls.Clear();
        if (Arena.Bounds is not ArenaBoundsCustom poly)
            return;

        foreach (var (a, b) in PolygonUtil.EnumerateEdges(poly.Poly.Parts[0].Vertices))
        {
            var dir = (b - a).OrthoR().Normalized();
            if (MathF.Abs(dir.Z) < 0.1f)
            {
                var clamped = new WDir(dir.X, 0);
                // line segment is on arena edge, no wall
                if (a.X is < -23 or > 23)
                    continue;

                var mid = (a + b) * 0.5f;
                var len = (b - a).Length() * 0.5f;

                _safeWalls.Add((-clamped, mid + Arena.Center, len));
            }
        }
    }
}

class FinalExam(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    public int NumCasts { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FinalExamFirst)
            AddStack(WorldState.Actors.Find(spell.TargetID)!);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FinalExamFirst or AID.FinalExamRest)
        {
            NumCasts++;
            if (NumCasts >= 3)
                Stacks.Clear();
        }
    }
}

class A31ShantottoTheDemonStates : StateMachineBuilder
{
    public A31ShantottoTheDemonStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1)
            .ActivateOnEnter<SuperiorStoneArena>()
            .ActivateOnEnter<SmallSpecimen>()
            .ActivateOnEnter<LargeSpecimen>()
            .ActivateOnEnter<FallingRubble1>()
            .ActivateOnEnter<FallingRubble2>()
            .ActivateOnEnter<FallingRubble3>()
            .ActivateOnEnter<FallingRubble4>()
            .ActivateOnEnter<Winds>();
    }

    void P1(uint id)
    {
        FlarePlay(id, 8.2f);
        Vidohunir(id + 0x100, 4.3f);
        EmpiricalResearch(id + 0x200, 4);
        SuperiorStone1(id + 0x10000, 4.9f);
        EmpiricalResearch(id + 0x11000, 3.3f);
        DiagrammaticDoorway(id + 0x20000, 4.5f, 4);
        MeteoricRhyme(id + 0x30000, 5.2f);
        Shockwave(id + 0x40000, 12.1f);
        SuperiorStone2(id + 0x50000, 17.5f);
        EmpiricalResearch(id + 0x51000, 3.3f);
        DiagrammaticDoorway(id + 0x60000, 5.4f, 6);
        FlarePlay(id + 0x70000, 9.3f);
        FinalExam(id + 0x71000, 5.2f);
        EmpiricalResearch(id + 0x72000, 1.9f);
        Vidohunir(id + 0x73000, 5.6f);

        Timeout(id + 0xFF0000, 10000, "Repeat mechanics until death")
            .ActivateOnEnter<SuperiorStoneIIRaidwide>()
            .ActivateOnEnter<SuperiorStoneIIRect>()
            .ActivateOnEnter<GroundbreakingQuake>()
            .ActivateOnEnter<DiagrammaticDoorway>()
            .ActivateOnEnter<LocalizedBlizzard>()
            .ActivateOnEnter<ThunderAndError>();
    }

    void FlarePlay(uint id, float delay)
    {
        Cast(id, AID.FlarePlay, delay, 5, "Raidwide")
            .ActivateOnEnter<FlarePlay>()
            .DeactivateOnExit<FlarePlay>();
    }

    void Vidohunir(uint id, float delay)
    {
        Cast(id, AID.VidohunirCast, delay, 5)
            .ActivateOnEnter<Vidohunir>();
        ComponentCondition<Vidohunir>(id + 2, 1, v => v.NumCasts > 0, "Shared tankbuster")
            .DeactivateOnExit<Vidohunir>();
    }

    void EmpiricalResearch(uint id, float delay)
    {
        Cast(id, AID.EmpiricalResearchCast, delay, 3)
            .ActivateOnEnter<EmpiricalResearch>();
        ComponentCondition<EmpiricalResearch>(id + 2, 0.8f, r => r.NumCasts > 0, "Laser")
            .DeactivateOnExit<EmpiricalResearch>();
    }

    void SuperiorStone1(uint id, float delay)
    {
        Cast(id, AID.SuperiorStoneIIRaidwide, delay, 4, "Raidwide")
            .ActivateOnEnter<SuperiorStoneIIRaidwide>()
            .ActivateOnEnter<SuperiorStoneIIRect>()
            .DeactivateOnExit<SuperiorStoneIIRaidwide>();

        ComponentCondition<SuperiorStoneIIRect>(id + 0x10, 0.8f, r => r.NumCasts > 0, "Walls")
            .DeactivateOnExit<SuperiorStoneIIRect>();

        Cast(id + 0x100, AID.GroundbreakingQuakeCast, 3.4f, 8)
            .ActivateOnEnter<GroundbreakingQuake>();
        ComponentCondition<GroundbreakingQuake>(id + 0x110, 1, q => q.NumCasts > 0, "Safe rect")
            .DeactivateOnExit<GroundbreakingQuake>();
    }

    void SuperiorStone2(uint id, float delay)
    {
        Cast(id, AID.SuperiorStoneIIRaidwide, delay, 4, "Raidwide")
            .ActivateOnEnter<SuperiorStoneIIRaidwide>()
            .ActivateOnEnter<SuperiorStoneIIRect>()
            .DeactivateOnExit<SuperiorStoneIIRaidwide>();

        ComponentCondition<SuperiorStoneIIRect>(id + 0x10, 0.8f, r => r.NumCasts > 0, "Walls")
            .DeactivateOnExit<SuperiorStoneIIRect>();

        Cast(id + 0x100, AID.AeroDynamicsCast, 1.3f, 3);

        ComponentCondition<Winds>(id + 0x200, 11, w => w.NumCasts > 0, "Knockback")
            .ActivateOnEnter<GroundbreakingQuake>();

        Cast(id + 0x300, AID.GroundbreakingQuakeCast, 0.3f, 8);
        ComponentCondition<GroundbreakingQuake>(id + 0x310, 1, q => q.NumCasts > 0, "Safe rect")
            .DeactivateOnExit<GroundbreakingQuake>();
    }

    void DiagrammaticDoorway(uint id, float delay, int count)
    {
        Cast(id, AID.DiagrammaticDoorway, delay, 3)
            .ActivateOnEnter<DiagrammaticDoorway>();

        ComponentCondition<DiagrammaticDoorway>(id + 0x10, 12f + count * 0.8f, d => d.NumCasts > 0, "Donut 1");
        ComponentCondition<DiagrammaticDoorway>(id + 0x20, 3.2f * (count - 1), d => d.NumCasts >= count, $"Donut {count}")
            .DeactivateOnExit<DiagrammaticDoorway>();

        Cast(id + 0x100, AID.LocalizedBlizzard, 1.1f, 2.2f, "Out")
            .ActivateOnEnter<LocalizedBlizzard>()
            .ActivateOnEnter<ThunderAndError>()
            .DeactivateOnExit<LocalizedBlizzard>();

        ComponentCondition<ThunderAndError>(id + 0x110, 4.3f, t => t.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<ThunderAndError>();
    }

    void MeteoricRhyme(uint id, float delay)
    {
        Cast(id, AID.MeteoricRhyme, delay, 3)
            .ActivateOnEnter<StardustSpecimen>();

        ComponentCondition<SmallSpecimen>(id + 0x10, 7.1f, s => s.NumCasts > 0, "Puddles start");

        ComponentCondition<StardustSpecimen>(id + 0x20, 21.7f, s => s.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<StardustSpecimen>();
    }

    void Shockwave(uint id, float delay)
    {
        // cast by helper
        ComponentCondition<Shockwave>(id, delay, s => s.Active)
            .ActivateOnEnter<Shockwave>();
        Targetable(id + 1, false, 1.7f, "Boss disappears");
        ComponentCondition<Shockwave>(id + 0x10, 8.7f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Shockwave>();
        Targetable(id + 0x20, true, 2.4f, "Boss reappears");
    }

    void FinalExam(uint id, float delay)
    {
        Cast(id, AID.FinalExamCast, delay, 4.2f)
            .ActivateOnEnter<FinalExam>();
        ComponentCondition<FinalExam>(id + 0x10, 0.8f, f => f.NumCasts > 0, "Stack 1");
        ComponentCondition<FinalExam>(id + 0x20, 2.8f, f => f.NumCasts >= 3, "Stack 3")
            .DeactivateOnExit<FinalExam>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117, NameID = 14778)]
public class A31ShantottoTheDemon(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -720), new ArenaBoundsRect(24, 30));
