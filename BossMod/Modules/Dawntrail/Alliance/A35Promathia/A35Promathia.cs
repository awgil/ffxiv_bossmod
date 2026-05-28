namespace BossMod.Dawntrail.Alliance.A35Promathia;

public enum OID : uint
{
    Boss = 0x4DEE, // R8.000, x1
    Helper = 0x233C, // R0.500, x30 (spawn during fight), Helper type
    LinkOfPromathia = 0x4DEF, // R2.240, x10
    MemoryReceptacle = 0x4DF0, // R2.040, x0 (spawn during fight)
    EmptyWanderer = 0x4DF1, // R1.200, x0 (spawn during fight)
    EmptyWeeper = 0x4DF2, // R2.000, x0 (spawn during fight)
    EmptyThinker = 0x4DF3, // R2.300, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45308, // Boss->player, no cast, single-target
    Jump = 50342, // Boss->location, no cast, single-target
    EmptySalvation = 50317, // Boss->location, 5.0s cast, range 25 circle
    FleetingEternity1 = 50318, // Boss->self, 3.5+1.0s cast, single-target
    FleetingEternity2 = 50319, // Boss->self, 3.5+1.0s cast, single-target
    Explosion = 50320, // Helper->self, 5.0s cast, range 16 circle
    WheelOfImpregnabilityCast = 50321, // Boss->self, 2.0+1.0s cast, single-target
    BastionOfTwilightCast = 50322, // Boss->self, 2.0+1.0s cast, single-target
    WheelOfImpregnability = 50323, // Helper->self, no cast, range 13 circle
    BastionOfTwilight = 50324, // Helper->self, no cast, range 13-50 donut
    PestilentPenanceCast = 50330, // Boss->self, 6.4+0.6s cast, single-target
    PestilentPenanceBig = 50331, // Helper->self, 7.0s cast, range 50 width 50 rect
    PestilentPenanceSmall = 50332, // LinkOfPromathia->self, 7.5s cast, range 50 width 5 rect
    CometCast = 50337, // Boss->self, 4.5+0.5s cast, single-target
    Comet = 50338, // Helper->players, 0.5s cast, range 6 circle
    Unk1 = 50345, // Helper->4D66, 1.0s cast, single-target
    FalseGenesisCast = 50343, // Boss->self, 9.5+0.5s cast, single-target
    FalseGenesis = 50344, // Helper->self, 0.5s cast, range 25 circle
    WindsOfPromyvionVisualFirst = 50352, // EmptyThinker->self, 3.9+0.6s cast, single-target
    WindsOfPromyvionVisualRest = 50460, // EmptyThinker->self, no cast, single-target
    WindsOfPromyvionFirst = 50353, // Helper->self, 4.5s cast, range 16 width 6 rect
    WindsOfPromyvionRest = 50354, // Helper->self, 0.6s cast, range 16 width 6 rect
    EmptyBeleaguer = 50351, // EmptyWanderer->self, 6.0s cast, range 6 circle
    AuroralDrape = 50355, // EmptyWeeper->self, 7.0s cast, range 7 width 7 rect
    EmptySeed = 50349, // MemoryReceptacle->self, 5.0s cast, range 10 circle
    DeadlyRebirthEnrageCast = 50346, // Boss->self, 5.7+1.3s cast, single-target
    DeadlyRebirthRaidwide = 50347, // Helper->self, 2.0s cast, range 50 circle
    DeadlyRebirthEnrage = 50348, // Helper->self, 1.3s cast, range 50 circle
    DeadlyRebirthCast = 50694, // Boss->self, 8.0+2.0s cast, single-target
    EarthboundHeaven = 50333, // Boss->self, 2.0+1.0s cast, single-target
    MalevolentBlessingCast1 = 50326, // Boss->self, 5.7+0.8s cast, single-target
    MalevolentBlessingCast2 = 50327, // Boss->self, 5.7+0.8s cast, single-target
    MalevolentBlessingCone = 50328, // Helper->self, 6.5s cast, range 40 23-degree cone
    MalevolentBlessingSide = 50329, // Helper->self, 6.5s cast, range 50 width 50 rect
    InfernalDeliverance = 50334, // Boss->self, 5.5+1.5s cast, single-target
    InfernalDeliveranceTower = 50335, // Helper->self, 7.0s cast, range 4 circle
    InfernalDeliverancePuddle = 50565, // Helper->self, 5.0s cast, range 8 circle
    MeteorCast = 50339, // Boss->self, 4.5+0.5s cast, single-target
    MeteorTank = 50340, // Helper->players, 0.5s cast, range 6 circle
    MeteorSpread = 50341, // Helper->players, 0.5s cast, range 6 circle
}

public enum SID : uint
{
    Unk = 2552, // none->Boss, extra=0x48D/0x458/0x457
    Unk1 = 2056, // none->EmptyWeeper, extra=0x498
    Unk2 = 2160, // none->4D66, extra=0x3931
    Unk3 = 2273, // Boss->Boss, extra=0x226
    Heavy = 1796, // none->player, extra=0x32
    SystemLock = 2578, // none->player, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0
    DownForTheCount = 3908, // Helper->player/4D66, extra=0xEC7
}

public enum IconID : uint
{
    WheelOfImpregnability = 687, // Boss->self
    BastionOfTwilight = 688, // Boss->self
    Tankbuster = 344, // player->self
    TurningRight = 689, // EmptyThinker->self
    TurningLeft = 690, // EmptyThinker->self
    Spread = 466, // player->self
}

public enum TetherID : uint
{
    Unk1 = 427, // Boss->4D66
    Unk2 = 12, // MemoryReceptacle->4D66
}

class EmptySalvation(BossModule module) : Components.RaidwideCast(module, AID.EmptySalvation);

// 0x00010002: puddle appear (6 at once)
// when telegraphs appear, center puddle spawns (0x00010002) and connected puddles get 0x00800100
// before cast starts, destination puddles(?) play 0x00100020 and other puddles play 0x00010200
// fucking with animations is too much and explosion is already a 5s cast which is enough for ai mode, people can deal with it if they need to
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, 16, maxCasts: 4, highlightImminent: true);
// 58.45 -> 48.59
class WheelOfImpregnability(BossModule module) : Components.GenericAOEs(module, AID.WheelOfImpregnability)
{
    DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeCircle(13), Module.PrimaryActor.Position, default, _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.WheelOfImpregnability)
            _activation = WorldState.FutureTime(9.9f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}
class BastionOfTwilight(BossModule module) : Components.GenericAOEs(module, AID.BastionOfTwilight)
{
    DateTime _activation;
    public bool Risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeDonut(13, 50), Module.PrimaryActor.Position, default, _activation, Risky: Risky);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Explosion)
            Risky = false;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.BastionOfTwilight)
            _activation = WorldState.FutureTime(9.9f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }

        if ((AID)spell.Action.ID == AID.Explosion)
            Risky = true;
    }
}
class PestilentPenance(BossModule module) : Components.StandardAOEs(module, AID.PestilentPenanceCast, new AOEShapeRect(50, 25));
class Comet(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, AID.Comet, centerAtTarget: true);

class FalseGenesis(BossModule module) : BossComponent(module)
{
    DateTime _activation;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_activation == default)
            return;

        Arena.ZoneRect(Arena.Center + new WDir(0, 13), default(Angle), 6.5f, 6.5f, 6.5f, ArenaColor.SafeFromAOE);
        Arena.ZoneRect(Arena.Center + new WDir(0, 13).Rotate(120.Degrees()), 120.Degrees(), 6.5f, 6.5f, 6.5f, ArenaColor.SafeFromAOE);
        Arena.ZoneRect(Arena.Center + new WDir(0, 13).Rotate(-120.Degrees()), -120.Degrees(), 6.5f, 6.5f, 6.5f, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;

        var pos = actor.Position;
        if (!(pos.InRect(Arena.Center + new WDir(0, 13), default(Angle), 6.5f, 6.5f, 6.5f) || pos.InRect(Arena.Center + new WDir(0, 13).Rotate(120.Degrees()), 120.Degrees(), 6.5f, 6.5f, 6.5f) || pos.InRect(Arena.Center + new WDir(0, 13).Rotate(-120.Degrees()), -120.Degrees(), 6.5f, 6.5f, 6.5f)))
            hints.Add("Go to platform!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default)
            return;

        var center = Arena.Center;
        hints.AddForbiddenZone(p => !(p.InRect(center + new WDir(0, 13), default(Angle), 6.5f, 6.5f, 6.5f) || p.InRect(center + new WDir(0, 13).Rotate(120.Degrees()), 120.Degrees(), 6.5f, 6.5f, 6.5f) || p.InRect(center + new WDir(0, 13).Rotate(-120.Degrees()), -120.Degrees(), 6.5f, 6.5f, 6.5f)), _activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0C && state == 0x00200010)
            _activation = WorldState.FutureTime(10.7f);

        if (index == 0x0C && state == 0x00020001)
        {
            _activation = default;
            WDir[] corner = [
                new(3.5f, -6.5f),
                new(3.5f, -6.05f),
                new(6.05f, -6.05f),
                new(6.05f, -3.5f),
                new(6.5f, -3.5f),
            ];
            List<WDir> plat = [.. corner, .. corner.Select(c => c.OrthoR()), .. corner.Select(c => -c), .. corner.Select(c => c.OrthoL())];
            var platform = plat.Select(p => p + new WDir(0, 13));
            var combined = Arena.Bounds.Clipper.UnionAll(new(platform), [new(platform.Select(p => p.Rotate(120.Degrees()))), new(platform.Select(p => p.Rotate(-120.Degrees())))]);
            Arena.Bounds = new ArenaBoundsCustom(20, combined);
        }

        if (index == 0x0C && state == 0x00080004)
            Arena.Bounds = new ArenaBoundsCircle(25);
    }
}
class FalseGenesisRaidwide(BossModule module) : Components.RaidwideCastDelay(module, AID.FalseGenesisCast, AID.FalseGenesis, 0.6f);
class DeadlyRebirthRaidwide(BossModule module) : Components.RaidwideCastDelay(module, AID.DeadlyRebirthCast, AID.DeadlyRebirthRaidwide, 2);
class DeadlyRebirthKB(BossModule module) : Components.Knockback(module, AID.DeadlyRebirthRaidwide, true)
{
    DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(default, 20, _activation, Direction: 0.Degrees(), Kind: Kind.DirForward);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(Arena.Center - new WDir(0, 20), 25), src.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeadlyRebirthCast)
            _activation = Module.CastFinishAt(spell, 2);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class MemoryReceptacle(BossModule module) : Components.Adds(module, (uint)OID.MemoryReceptacle);
class AuroralDrape(BossModule module) : Components.StandardAOEs(module, AID.AuroralDrape, new AOEShapeRect(7, 3.5f));
class EmptyBeleaguer(BossModule module) : Components.StandardAOEs(module, AID.EmptyBeleaguer, 6, 2);
// winds: 4 casts, rotates 30 degrees between each
class WindsOfPromyvion(BossModule module) : Components.GenericRotatingAOE(module)
{
    Actor? _add;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.TurningRight:
                _add = Module.Enemies(OID.MemoryReceptacle).Closest(actor.Position);
                Sequences.Add(new(new AOEShapeRect(16, 3), actor.Position, actor.Rotation, -30.Degrees(), WorldState.FutureTime(4.5f), 1.4f, 4));
                break;
            case IconID.TurningLeft:
                _add = Module.Enemies(OID.MemoryReceptacle).Closest(actor.Position);
                Sequences.Add(new(new AOEShapeRect(16, 3), actor.Position, actor.Rotation, 30.Degrees(), WorldState.FutureTime(4.5f), 1.4f, 4));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WindsOfPromyvionFirst or AID.WindsOfPromyvionRest)
        {
            NumCasts++;
            if (Sequences.Count > 0)
            {
                AdvanceSequence(0, WorldState.CurrentTime);
                if (Sequences.Count == 0)
                    _add = null;
            }
        }
    }

    public override void Update()
    {
        if (_add?.IsDeadOrDestroyed == true)
            Sequences.Clear();
    }
}

class EmptySeed(BossModule module) : Components.KnockbackFromCastTarget(module, AID.EmptySeed, 10, shape: new AOEShapeCircle(10))
{
    // 29-ish degrees
    static readonly float SafeConeHalfWidth = MathF.Atan2(6, 3.5f) - MathF.Atan2(1, 1);

    Angle PlatformOrientation(WPos p)
    {
        if (p.Z > Arena.Center.Z)
            return default;
        if (p.X < Arena.Center.X)
            return -120.Degrees();
        return 120.Degrees();
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var src in base.Sources(slot, actor))
        {
            if (IsImmune(slot, src.Activation))
                continue;

            if (!actor.Position.AlmostEqual(src.Origin, src.Distance))
                continue;

            var safeCorner = 45.Degrees() + PlatformOrientation(src.Origin);
            var ad = actor.Position - src.Origin;

            var safe = false;
            for (var i = 0; i < 4; i++)
            {
                if (ad.ToAngle().AlmostEqual(safeCorner, SafeConeHalfWidth))
                {
                    safe = true;
                    break;
                }
                ad = ad.OrthoR();
            }

            if (safe)
            {
                var intersect = Arena.Bounds.IntersectRay(actor.Position - Arena.Center, (actor.Position - src.Origin).Normalized());
                yield return src with { Distance = Math.Clamp(intersect - 0.1f, 0, 10) };
            }
            else
                yield return src;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // already ensured that the only sources returned are the ones that will actually hit us
        foreach (var src in Sources(slot, actor))
        {
            var center = src.Origin;
            var safeCorner = 45.Degrees() + PlatformOrientation(center);
            hints.AddForbiddenZone(p =>
            {
                if (!p.AlmostEqual(center, 10))
                    return false;
                var ad = p - center;
                for (var i = 0; i < 4; i++)
                {
                    if (ad.ToAngle().AlmostEqual(safeCorner, SafeConeHalfWidth))
                        return false;
                    ad = ad.OrthoR();
                }
                return true;
            }, src.Activation);
        }
    }
}

class MalevolentBlessingCone(BossModule module) : Components.StandardAOEs(module, AID.MalevolentBlessingCone, new AOEShapeCone(40, 11.5f.Degrees()));
class MalevolentBlessingSide(BossModule module) : Components.StandardAOEs(module, AID.MalevolentBlessingSide, new AOEShapeRect(50, 25));
class PestilentPenanceSkinny(BossModule module) : Components.StandardAOEs(module, AID.PestilentPenanceSmall, new AOEShapeRect(50, 2.5f));
class InfernalDeliveranceTower(BossModule module) : Components.CastTowers(module, AID.InfernalDeliveranceTower, 4, 6, 8);
class InfernalDeliverancePuddle(BossModule module) : Components.StandardAOEs(module, AID.InfernalDeliverancePuddle, 8);
class MeteorTank(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, AID.MeteorTank, centerAtTarget: true);
class MeteorSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, AID.MeteorSpread, 6, 5.1f);

class EnrageTimer(BossModule module) : BossComponent(module)
{
    public enum State
    {
        InProgress,
        Success,
        Fail
    }

    public State CurrentState { get; private set; } = State.InProgress;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000027)
        {
            if ((param1, param2, param3) == (0x73, 0x2, 0x3705))
                CurrentState = State.Success;

            if ((param1, param2, param3) == (0x77, 0x2, 0x39BB))
                CurrentState = State.Fail;
        }
    }

    // fallback in case i totally misunderstood what any of these director updates do: if boss reappears, assume we passed check (if it enrages instead, the module gets reloaded)
    public override void Update()
    {
        if (Module.PrimaryActor.IsTargetable)
            CurrentState = State.Success;
    }
}

class A35PromathiaStates : StateMachineBuilder
{
    public A35PromathiaStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1);
    }

    private void P1(uint id)
    {
        EmptySalvation(id, 8.7f);
        Eternity1(id + 0x1000, 3.2f);
        Eternity2(id + 0x2000, 3.7f);
        Wheel1(id + 0x3000, 3.8f);
        Bastion1(id + 0x4000, 0.9f);
        EmptySalvation(id + 0x5000, 8.2f);
        Eternity3(id + 0x6000, 3.1f);

        Intermission(id + 0x10000, 4.9f);
    }

    void EmptySalvation(uint id, float delay)
    {
        Cast(id, AID.EmptySalvation, delay, 5, "Raidwide")
            .ActivateOnEnter<EmptySalvation>()
            .DeactivateOnExit<EmptySalvation>();
    }

    void Eternity1(uint id, float delay)
    {
        CastMulti(id, [AID.FleetingEternity1, AID.FleetingEternity2], delay, 3.5f)
            .ActivateOnEnter<Explosion>();

        ComponentCondition<Explosion>(id + 0x10, 8.8f, e => e.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Explosion>();
    }

    void Eternity2(uint id, float delay)
    {
        CastMulti(id, [AID.FleetingEternity1, AID.FleetingEternity2], delay, 3.5f)
            .ActivateOnEnter<Explosion>();

        ComponentCondition<Explosion>(id + 0x10, 5.8f, e => e.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Explosion>();
    }

    void Eternity3(uint id, float delay)
    {
        CastMulti(id, [AID.FleetingEternity1, AID.FleetingEternity2], delay, 3.5f)
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Comet>();

        ComponentCondition<Explosion>(id + 0x10, 5.8f, e => e.NumCasts > 0, "Puddles 1");
        ComponentCondition<Explosion>(id + 0x20, 3.1f, e => e.NumCasts > 2, "Puddles 2");
        ComponentCondition<Explosion>(id + 0x30, 3.1f, e => e.NumCasts > 4, "Puddles 3")
            .DeactivateOnExit<Explosion>();

        ComponentCondition<Comet>(id + 0x100, 3.5f, c => c.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<Comet>();
    }

    void Wheel1(uint id, float delay)
    {
        Cast(id, AID.WheelOfImpregnabilityCast, delay, 2)
            .ActivateOnEnter<WheelOfImpregnability>();

        ComponentCondition<WheelOfImpregnability>(id + 0x10, 10.7f, w => w.NumCasts > 0, "Out")
            .DeactivateOnExit<WheelOfImpregnability>();
    }

    void Bastion1(uint id, float delay)
    {
        Cast(id, AID.BastionOfTwilightCast, delay, 2)
            .ActivateOnEnter<BastionOfTwilight>()
            .ExecOnEnter<BastionOfTwilight>(b => b.Risky = true);

        Cast(id + 0x10, AID.PestilentPenanceCast, 3.8f, 6.4f, "Big rect")
            .ActivateOnEnter<PestilentPenance>()
            .DeactivateOnExit<PestilentPenance>();

        ComponentCondition<BastionOfTwilight>(id + 0x20, 0.6f, b => b.NumCasts > 0, "Donut")
            .DeactivateOnExit<BastionOfTwilight>();
    }

    void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Boss disappears")
            .ActivateOnEnter<FalseGenesis>()
            .ActivateOnEnter<MemoryReceptacle>()
            .ActivateOnEnter<AuroralDrape>()
            .ActivateOnEnter<EmptyBeleaguer>()
            .ActivateOnEnter<WindsOfPromyvion>()
            .ActivateOnEnter<EmptySeed>();

        Cast(id + 0x10, AID.FalseGenesisCast, 2.2f, 9.5f)
            .ActivateOnEnter<FalseGenesisRaidwide>()
            .ActivateOnEnter<EnrageTimer>();

        ComponentCondition<FalseGenesisRaidwide>(id + 0x20, 0.6f, r => r.NumCasts > 0, "Raidwide + platforms")
            .DeactivateOnExit<FalseGenesisRaidwide>();

        ComponentCondition<MemoryReceptacle>(id + 0x30, 1.6f, r => r.ActiveActors.Any(), "Adds appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        Dictionary<EnrageTimer.State, (uint seqID, Action<uint> buildState)> fork = new()
        {
            [EnrageTimer.State.Success] = ((id >> 24) + 1, PostAdds),
            [EnrageTimer.State.Fail] = ((id >> 24) + 2, AddsEnrage)
        };

        ComponentConditionFork<EnrageTimer, EnrageTimer.State>(id + 0x100, 70, t => t.CurrentState != EnrageTimer.State.InProgress, t => t.CurrentState, fork, "Adds enrage")
            .DeactivateOnExit<MemoryReceptacle>()
            .DeactivateOnExit<AuroralDrape>()
            .DeactivateOnExit<EmptyBeleaguer>()
            .DeactivateOnExit<WindsOfPromyvion>()
            .DeactivateOnExit<EmptySeed>()
            .DeactivateOnExit<EnrageTimer>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    void AddsEnrage(uint id)
    {
        Cast(id, AID.DeadlyRebirthEnrageCast, 0.1f, 5.7f);
        Timeout(id + 0x10, 1.3f, "Enrage");
    }

    void PostAdds(uint id)
    {
        Targetable(id, true, 6.1f);
        CastStart(id + 1, AID.DeadlyRebirthCast, 2.8f)
            .ActivateOnEnter<DeadlyRebirthRaidwide>()
            .ActivateOnEnter<DeadlyRebirthKB>();
        CastEnd(id + 2, 8);
        ComponentCondition<DeadlyRebirthRaidwide>(id + 0x10, 2, r => r.NumCasts > 0, "Raidwide + stun")
            .DeactivateOnExit<DeadlyRebirthRaidwide>()
            .DeactivateOnExit<DeadlyRebirthKB>()
            .SetHint(StateMachine.StateHint.DowntimeStart);

        // duration of Down for the Count
        Timeout(id + 0x100, 8).SetHint(StateMachine.StateHint.DowntimeEnd);

        EmptySalvation(id + 0x1000, 1.7f);
        Earthbound1(id + 0x2000, 3.2f);
        MalevolentBlessing(id + 0x3000, 1.6f);
        EmptySalvation(id + 0x4000, 3.4f);
        Earthbound2(id + 0x5000, 3.2f);
        EmptySalvation(id + 0x10000, 7.2f);
        InfernalDeliverance(id + 0x11000, 3.1f);
        Eternity4(id + 0x12000, 0.7f);
        EmptySalvation(id + 0x13000, 10.3f);

        Timeout(id + 0xFF0000, 10000, "Repeat mechanics until death")
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<PestilentPenance>()
            .ActivateOnEnter<PestilentPenanceSkinny>()
            .ActivateOnEnter<InfernalDeliverancePuddle>()
            .ActivateOnEnter<InfernalDeliveranceTower>()
            .ActivateOnEnter<MeteorTank>()
            .ActivateOnEnter<MeteorSpread>()
            .ActivateOnEnter<MalevolentBlessingCone>()
            .ActivateOnEnter<MalevolentBlessingSide>()
            .ActivateOnEnter<WheelOfImpregnability>()
            .ActivateOnEnter<BastionOfTwilight>()
            .ActivateOnEnter<EmptySalvation>();
    }

    void Earthbound1(uint id, float delay)
    {
        Cast(id, AID.EarthboundHeaven, delay, 2)
            .ActivateOnEnter<Explosion>();

        ComponentCondition<Explosion>(id + 0x10, 8.9f, e => e.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Explosion>();
    }

    void MalevolentBlessing(uint id, float delay)
    {
        CastMulti(id, [AID.MalevolentBlessingCast2, AID.MalevolentBlessingCast1], delay, 5.7f)
            .ActivateOnEnter<MalevolentBlessingCone>()
            .ActivateOnEnter<MalevolentBlessingSide>();

        ComponentCondition<MalevolentBlessingSide>(id + 0x10, 0.8f, b => b.NumCasts > 0, "Cones + side")
            .DeactivateOnExit<MalevolentBlessingCone>()
            .DeactivateOnExit<MalevolentBlessingSide>();
    }

    void Earthbound2(uint id, float delay)
    {
        Cast(id, AID.EarthboundHeaven, delay, 2)
            .ActivateOnEnter<Explosion>();

        Cast(id + 0x10, AID.BastionOfTwilightCast, 2.1f, 2)
            .ActivateOnEnter<BastionOfTwilight>()
            .ActivateOnEnter<PestilentPenance>()
            .ActivateOnEnter<PestilentPenanceSkinny>();

        ComponentCondition<Explosion>(id + 0x100, 4.8f, e => e.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Explosion>();

        ComponentCondition<PestilentPenance>(id + 0x200, 5.4f, p => p.NumCasts > 0, "Big rect")
            .DeactivateOnExit<PestilentPenance>();
        ComponentCondition<BastionOfTwilight>(id + 0x210, 0.5f, b => b.NumCasts > 0, "Donut")
            .DeactivateOnExit<BastionOfTwilight>();
        ComponentCondition<PestilentPenanceSkinny>(id + 0x220, 2.3f, p => p.NumCasts > 0, "Lasers")
            .DeactivateOnExit<PestilentPenanceSkinny>();
    }

    void InfernalDeliverance(uint id, float delay)
    {
        Cast(id, AID.InfernalDeliverance, delay, 5.5f)
            .ActivateOnEnter<InfernalDeliverancePuddle>()
            .ActivateOnEnter<InfernalDeliveranceTower>();

        ComponentCondition<InfernalDeliveranceTower>(id + 0x10, 1.5f, t => t.NumCasts > 0, "Towers")
            .DeactivateOnExit<InfernalDeliveranceTower>();
        ComponentCondition<InfernalDeliverancePuddle>(id + 0x20, 5.2f, t => t.NumCasts > 0, "Puddles")
            .DeactivateOnExit<InfernalDeliverancePuddle>();
    }

    void Eternity4(uint id, float delay)
    {
        CastMulti(id, [AID.FleetingEternity1, AID.FleetingEternity2], delay, 3.5f)
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<MeteorSpread>()
            .ActivateOnEnter<MeteorTank>();

        ComponentCondition<Explosion>(id + 0x10, 5.8f, e => e.NumCasts > 0, "Puddles 1");
        ComponentCondition<Explosion>(id + 0x20, 3.1f, e => e.NumCasts > 2, "Puddles 2");
        ComponentCondition<Explosion>(id + 0x30, 3.1f, e => e.NumCasts > 4, "Puddles 3")
            .DeactivateOnExit<Explosion>();

        ComponentCondition<MeteorTank>(id + 0x100, 4.5f, t => t.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<MeteorTank>();
        ComponentCondition<MeteorSpread>(id + 0x110, 1.9f, c => c.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<MeteorSpread>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117, NameID = 14779)]
public class A35Promathia(WorldState ws, Actor primary) : BossModule(ws, primary, new(-820, -820), new ArenaBoundsCircle(25));

