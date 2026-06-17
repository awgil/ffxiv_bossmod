namespace BossMod.Dawntrail.Alliance.A36ShinryuParadox;

public enum OID : uint
{
    Boss = 0x4D92, // R25.000, x1
    Unk4EB3 = 0x4EB3, // R2.000, x5, no clue what these are, they stay in arena center and do nothing
    ArcaneSphere1 = 0x4D97, // R1.000, x1
    ArcaneSphere2 = 0x4DCD, // R1.000, x1
    ShinryuAutos = 0x4D9A, // R0.000, x3, Part type
    ShinryusGroin = 0x4D93, // R25.000, x1, Part type
    Helper = 0x233C, // R0.500, x24, Helper type
    GuloolJaJa = 0x4E53, // R5.000, x0 (spawn during fight)
    HollowKing = 0x4D96, // R25.000, x0 (spawn during fight)
    HollowKingAutos = 0x4D9B, // R0.000, x0 (spawn during fight), Part type
}

public enum AID : uint
{
    ShinryuAutoVisual = 49137, // Boss->self, no cast, single-target
    ShinryuAuto = 49138, // ShinryuAutos->player, no cast, single-target

    CosmicBreathVisual1 = 49105, // Boss->self, 6.0+1.0s cast, single-target
    CosmicBreathVisual2 = 49106, // ShinryusGroin->self, 6.0+1.0s cast, single-target
    CosmicBreath = 49107, // Helper->self, 7.0s cast, range 50 width 70 rect
    CosmicTailVisual1 = 49108, // Boss->self, 6.0+1.0s cast, single-target
    CosmicTailVisual2 = 49109, // ShinryusGroin->self, 6.0+1.0s cast, single-target
    CosmicTail = 49110, // Helper->self, 7.0s cast, range 50 width 70 rect
    CloakOfTwilight1 = 49111, // Boss->self, 3.0s cast, single-target
    CloakOfTwilight2 = 49112, // ShinryusGroin->self, 3.0s cast, single-target
    TwilightNebula1 = 49113, // Boss->self, 6.0s cast, single-target
    TwilightNebula2 = 49114, // ShinryusGroin->self, 6.0s cast, single-target
    TwilightRadiance = 49115, // Helper->self, no cast, range 60 circle
    TwilightShadow = 49116, // Helper->self, no cast, range 60 circle
    StarflareVisual1 = 49124, // Boss->self, 3.0s cast, single-target
    StarflareVisual2 = 49125, // ShinryusGroin->self, 3.0s cast, single-target
    StarflareP1Fast = 49126, // Helper->self, 5.0s cast, range 60 width 10 rect
    StarflareP1Slow = 49127, // Helper->self, 7.0s cast, range 60 width 10 rect
    CataclysmicVortexVisual1 = 49121, // Boss->self, 7.0s cast, single-target
    CataclysmicVortexVisual2 = 49122, // ShinryusGroin->self, 7.0s cast, single-target
    CataclysmicVortexFail = 49123, // Helper->player, no cast, single-target
    DarkNovaVisual1 = 49134, // Boss->self, 5.0s cast, single-target
    DarkNovaVisual2 = 49135, // ShinryusGroin->self, 5.0s cast, single-target
    DarkNova = 49136, // Helper->player, no cast, range 6 circle
    AtomicTailVisual1 = 49128, // Boss->self, 6.0+1.0s cast, single-target
    AtomicTailVisual2 = 49129, // ShinryusGroin->self, 6.0+1.0s cast, single-target
    AtomicTail = 49130, // Helper->self, 7.0s cast, range 50 width 70 rect
    GyreChargeVisual1 = 49131, // Boss->self, no cast, single-target
    GyreChargeVisual2 = 49132, // ShinryusGroin->self, no cast, single-target
    GyreCharge = 49133, // Helper->self, 0.5s cast, range 60 circle

    CelestialTrailVisual1 = 49139, // HollowKing->self, no cast, single-target
    CelestialTrailTower = 49140, // Helper->self, 8.0s cast, range 2 circle
    CelestialTrailVisual2 = 49141, // HollowKing->self, no cast, single-target
    CelestialTrailHPDown = 49142, // Helper->player/4D98, no cast, single-target
    CelestialTrailKnockback = 49143, // Helper->player/4D98, no cast, single-target
    CelestialTrailVisual3 = 49144, // HollowKing->self, no cast, single-target
    CelestialTrailExplosion = 49147, // Helper->self, 5.5s cast, range 60 circle
    HollowKingAutoVisual = 49180, // HollowKing->self, no cast, single-target
    HollowKingAuto = 49181, // HollowKingAutos->player, no cast, single-target
    EmptyProclamation = 49179, // HollowKing->self, 4.0s cast, range 60 circle
    RightSwordscrossVisual = 49151, // HollowKing->self, 8.0+1.0s cast, single-target
    LeftSwordscrossVisual = 49152, // HollowKing->self, 8.0+1.0s cast, single-target
    RightSwordscross1 = 49153, // Helper->self, 9.0s cast, range 60 width 30 rect
    LeftSwordscross1 = 49154, // Helper->self, 9.0s cast, range 60 width 30 rect
    RightSwordscross2 = 49155, // Helper->self, 9.0s cast, range 70 width 36 rect
    LeftSwordscross2 = 49156, // Helper->self, 9.0s cast, range 70 width 36 rect
    TwinBlazeVisual1 = 49157, // HollowKing->self, 5.0+1.0s cast, single-target
    TwinBlazeVisual2 = 49158, // HollowKing->self, 5.0+1.0s cast, single-target
    TwinBlazeIn = 49159, // Helper->self, 6.0s cast, range 20-60 donut
    TwinBlazeOut = 49160, // Helper->self, 6.0s cast, range 35 90-degree cone
    CataclysmicBladeVisual = 49161, // HollowKing->self, 7.0s cast, single-target
    CataclysmicBladeCone = 49162, // Helper->self, 7.0s cast, range 60 45-degree cone
    CataclysmicBladeFail = 49163, // Helper->player, no cast, single-target
    AtomicRayVisual = 49164, // HollowKing->self, 3.0s cast, single-target
    AtomicRay = 49165, // ArcaneSphere1/ArcaneSphere2->self, 1.5s cast, range 60 width 15 rect
    CosmicFlameVisual = 49166, // HollowKing->self, 5.0s cast, single-target
    CosmicFlameFirst = 49168, // Helper->self, 5.0s cast, range 6 circle
    CosmicFlameRest = 49169, // Helper->self, no cast, range 6 circle
    BurstVisual = 49170, // HollowKing->self, 3.0s cast, single-target
    Burst1 = 49171, // Helper->self, 5.0s cast, range 10 circle
    Burst2 = 49172, // Helper->self, 7.0s cast, range 10-20 donut
    Burst3 = 49173, // Helper->self, 9.0s cast, range 20-30 donut
    StarflareP2Cast = 49174, // HollowKing->self, 3.0s cast, single-target
    StarflareP2Fast = 49175, // Helper->self, 5.0s cast, range 60 width 10 rect
    StarflareP2Slow = 49176, // Helper->self, 7.0s cast, range 60 width 10 rect
    DarkNovaP2Visual = 49177, // HollowKing->self, 5.0s cast, single-target
    DarkNovaP2 = 49178, // Helper->players, no cast, range 6 circle
    SuperNovaVisual = 49182, // HollowKing->self, 5.0s cast, single-target
    SuperNova = 49183, // Helper->players, no cast, range 6 circle
}

public enum SID : uint
{
    Bleeding1 = 3077, // none->player, extra=0x0
    Bleeding2 = 3078, // none->player, extra=0x0
    CloakOfWaningLight = 5352, // none->player, extra=0x0
    CloakOfWaxingDark = 5353, // none->player, extra=0x0
    DownForTheCount = 1963, // Helper->player, extra=0xEC7
    Unk1 = 2202, // none->player, extra=0x0
    Unk3 = 2056, // none->_Gen_HollowKing/_Gen_ArcaneSphere1/_Gen_ArcaneSphere, extra=0x474/0x48E/0x497/0x496
    Unk4 = 2552, // none->player, extra=0x48F
    Clashing = 1271, // none->player, extra=0x317A/0x1836
    HPRecoveryDown = 2852, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    NoLook = 680, // player->self
    Look = 681, // player->self
    NoMove = 682, // player->self
    Move = 683, // player->self
    Checkmark = 136, // player->self
    X = 137, // player->self
    Tankbuster = 344, // player->self
    Countdown = 720, // _Gen_ArcaneSphere1/_Gen_ArcaneSphere->self
    Stack = 305, // player->self
}

abstract class FloorAOE(BossModule module, Enum? action = null) : Components.GenericAOEs(module, action)
{
    protected readonly List<Actor> Casters = [];

    protected abstract int GetDangerFloor(int slot, Actor actor);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            var activation = Module.CastFinishAt(c.CastInfo);
            var danger = GetDangerFloor(slot, actor);
            var floor = Helpers.Level(actor);

            if (danger == 1 && floor == 0)
                // stay away from launcher
                yield return new(new AOEShapeCircle(2), Arena.Center - new WDir(0, 6), default, activation);
            else if (danger == 1 && floor == 1)
                // go to hole
                yield return new(new AOEShapeDonut(6, 100), Arena.Center - new WDir(0, 6), default, activation);
            else if (danger == 0 && floor == 0)
                yield return new(new AOEShapeDonut(2, 100), Arena.Center - new WDir(0, 6), default, activation);
            else if (danger == 0 && floor == 1)
                yield return new(new AOEShapeCircle(6), Arena.Center - new WDir(0, 6), default, activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }
}

class CosmicBreath(BossModule module) : FloorAOE(module, AID.CosmicBreath)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 1;
}

class TwilightNebula(BossModule module) : FloorAOE(module, AID.TwilightNebula1)
{
    readonly int[] colors = Utils.MakeArray(PartyState.MaxAllies, -1);

    protected override int GetDangerFloor(int slot, Actor actor)
    {
        var x = 1 - colors[slot];
        return x > 1 ? -1 : x;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.CloakOfWaningLight:
                if (Raid.TryFindSlot(actor, out var s1))
                    colors[s1] = 1;
                break;
            case SID.CloakOfWaxingDark:
                if (Raid.TryFindSlot(actor, out var s2))
                    colors[s2] = 0;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TwilightRadiance)
        {
            NumCasts++;
            Casters.Clear();
        }
    }
}

class Starflare(BossModule module) : Components.GroupedAOEs(module, [AID.StarflareP1Fast, AID.StarflareP1Slow], new AOEShapeRect(60, 5))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Where(c => Helpers.Level(c) == Helpers.Level(actor)).Take(5).Select(c => new AOEInstance(Shape, c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
}

class VortexLook(BossModule module) : Components.GenericGaze(module, null, true)
{
    readonly DateTime[] activations = new DateTime[PartyState.MaxAllies];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Look && Raid.TryFindSlot(actor, out var slot))
            activations[slot] = WorldState.FutureTime(7);
    }

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        var a = activations.BoundSafeAt(slot);
        if (a != default)
            yield return new(Module.PrimaryActor.Position, a);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CataclysmicVortexVisual1 or AID.CataclysmicBladeVisual)
            Array.Fill(activations, default);
    }
}

class VortexNoLook(BossModule module) : Components.GenericGaze(module)
{
    readonly DateTime[] activations = new DateTime[PartyState.MaxAllies];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.NoLook && Raid.TryFindSlot(actor, out var slot))
            activations[slot] = WorldState.FutureTime(7);
    }

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        var a = activations.BoundSafeAt(slot);
        if (a != default)
            yield return new(Module.PrimaryActor.Position, a);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CataclysmicVortexVisual1 or AID.CataclysmicBladeVisual)
            Array.Fill(activations, default);
    }
}

class VortexStayMove(BossModule module) : Components.StayMove(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Requirement r;
        switch ((IconID)iconID)
        {
            case IconID.NoMove:
                r = Requirement.Stay;
                break;
            case IconID.Move:
                r = Requirement.Move;
                break;
            default:
                return;
        }

        if (Raid.TryFindSlot(actor, out var slot))
            SetState(slot, new(r, WorldState.FutureTime(7)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CataclysmicVortexVisual1 or AID.CataclysmicBladeVisual)
            Array.Fill(PlayerStates, default);
    }
}

class CosmicTail(BossModule module) : FloorAOE(module, AID.CosmicTail)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 0;
}

class UpDownCounter(BossModule module) : Components.CastCounterMulti(module, [AID.CosmicBreath, AID.CosmicTail]);

class DarkNova(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, AID.DarkNova, 5.1f, true);
class DarkNovaP2(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, AID.DarkNovaP2, 5.1f, true);
class AtomicTail(BossModule module) : FloorAOE(module, AID.AtomicTail)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 0;
}
class AtomicTailArena(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00200010)
        {
            var rect = CurveApprox.Rect(new WDir(30, 0), new WDir(0, 20));
            var hole = CurveApprox.Circle(6, 1 / 90f).Select(c => c - new WDir(0, 6));
            var shape = Arena.Bounds.Clipper.Difference(new(rect), new(hole));
            Arena.Bounds = new ArenaBoundsCustom(30, shape);
        }

        if (index == 0x00 && state == 0x02000100)
            Arena.Bounds = new ArenaBoundsRect(30, 20);
    }
}

class CelestialTrail(BossModule module) : Components.CastTowers(module, AID.CelestialTrailTower, 2, 1, 10)
{
    BitMask _forbidden;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HPRecoveryDown)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
        {
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers = _forbidden;
        }
    }
}

class EmptyProclamation(BossModule module) : Components.RaidwideCast(module, AID.EmptyProclamation);
class Swordscross1(BossModule module) : Components.GroupedAOEs(module, [AID.RightSwordscross1, AID.LeftSwordscross1], new AOEShapeRect(60, 15));
class Swordscross2(BossModule module) : Components.GroupedAOEs(module, [AID.RightSwordscross2, AID.LeftSwordscross2], new AOEShapeRect(70, 18));
class TwinBlaze1(BossModule module) : Components.StandardAOEs(module, AID.TwinBlazeIn, new AOEShapeDonutSector(20, 60, 45.Degrees()));
class TwinBlaze2(BossModule module) : Components.StandardAOEs(module, AID.TwinBlazeOut, new AOEShapeCone(35, 45.Degrees()));
class CataclysmicBlade(BossModule module) : Components.StandardAOEs(module, AID.CataclysmicBladeCone, new AOEShapeCone(60, 22.5f.Degrees()));

class Burst(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Burst1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.Burst1 => 0,
            AID.Burst2 => 1,
            AID.Burst3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class CosmicFlame(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CosmicFlameFirst)
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 8,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 8,
                MaxShownExplosions = 3
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CosmicFlameFirst or AID.CosmicFlameRest)
        {
            NumCasts++;
            var l = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (l >= 0)
                AdvanceLine(Lines[l], caster.Position);
        }
    }
}

// 379.80 appear -> 385.099 cast start (5.3s) orb moves about 40 units
// horizontal min=799.041 max=841.184 (+/-21 units)
// vertical min=-832.853 max=-806.823 (+/-12 units?)

// [800, -840]   dir [0.1, 0]  => [842.5, -840]

// [814, -840]   dir [0.1, 0]  => [827.6, -840]

// [826, -840]   dir [-0.1, 0] => [812.5, -840]
// [826, -840]   dir [-0.3, 0] => [812.5, -840]
// [826, -840]   dir [-0.1, 0] => [812.5, -840]
// [827, -840]   dir [-0.1, 0] => [812.5, -840]

// [850, -820]   dir [0, -0.1] => [850, -806]
// [850, -820]   dir [0, -0.1] => [850, -806]
// [850, -820]   dir [0, -0.1] => [850, -806]
// [850, -820]   dir [0, -0.1] => [850, -806]

// [850, -820]   dir [0, 0.1]  => [850, -834]

// [850, -832.5] dir [0, 0.1]  => [850, -820]
class AtomicRay(BossModule module) : Components.GenericAOEs(module, AID.AtomicRay)
{
    readonly List<(Actor Caster, WPos StartPos, WDir StartMove)> _recorded = [];
    readonly List<(Actor Caster, WPos Predicted, DateTime Activation)> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, p, d) in _predicted)
            yield return new(new AOEShapeRect(60, 7.5f), p, c.Rotation, d);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _predicted.RemoveAll(p => p.Caster == caster);
            var ix = _recorded.FindIndex(p => p.Caster == caster);
            if (ix >= 0)
            {
                var (p, s, m) = _recorded[ix];
                _recorded.RemoveAt(ix);
                ReportError($"{p} casting at {caster.Position}, starting was {s} going {m}");
            }
        }
    }
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Countdown)
        {
            _recorded.Add((actor, actor.Position, actor.LastFrameMovement));
            var dt = WorldState.FutureTime(5.3f);

            // pepeW
            if (actor.Position.AlmostEqual(new(800, -840), 2) && actor.LastFrameMovement.X > 0)
                _predicted.Add((actor, new(842.5f, -840), dt));
            else if (actor.Position.AlmostEqual(new(826, -840), 2) && actor.LastFrameMovement.X < 0)
                _predicted.Add((actor, new(812.5f, -840), dt));
            else if (actor.Position.AlmostEqual(new(850, -820), 2))
                _predicted.Add((actor, new(850, actor.LastFrameMovement.Z < 0 ? -806 : -834), dt));
            else if (actor.Position.AlmostEqual(new(850, -832), 2))
                _predicted.Add((actor, new(850, -820), dt));
            else if (actor.Position.AlmostEqual(new(814, -840), 2) && actor.LastFrameMovement.X > 0)
                _predicted.Add((actor, new(827.5f, -840), dt));
            else
                ReportError($"not sure what to predict for orb at {actor.Position} with movement {actor.LastFrameMovement}");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(Module.Enemies(OID.ArcaneSphere2), ArenaColor.Object, true);
        Arena.Actors(Module.Enemies(OID.ArcaneSphere1), ArenaColor.Object, true);
    }
}

class AtomicRayCast(BossModule module) : Components.StandardAOEs(module, AID.AtomicRay, new AOEShapeRect(60, 7.5f));

class GyreCharge(BossModule module) : Components.RaidwideCastDelay(module, AID.AtomicTailVisual1, AID.GyreCharge, 6.3f);

class SuperNova(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.SuperNova, 6, 6.1f, minStackSize: 8)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {

        if (spell.Action == StackAction)
        {
            NumCasts++;
            if (NumCasts >= 3)
            {
                Stacks.Clear();
                NumFinishedStacks++;
            }
        }
    }
}

class StarflareP2(BossModule module) : Components.GroupedAOEs(module, [AID.StarflareP2Fast, AID.StarflareP2Slow], new AOEShapeRect(60, 5), maxCasts: 5);

class A36ShinryuParadoxStates : StateMachineBuilder
{
    readonly A36ShinryuParadox _module;

    public A36ShinryuParadoxStates(A36ShinryuParadox module) : base(module)
    {
        _module = module;
        DeathPhase(0, SinglePhase)
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.HollowKing).All(k => k.IsDeadOrDestroyed);
    }

    private void SinglePhase(uint id)
    {
        UpDown(id, 7.2f);
        UpDown(id + 0x1000, 8.7f);
        Twilight1(id + 0x2000, 3.6f);
        Starflare1(id + 0x3000, 4.5f);
        Vortex(id + 0x4000, 5.8f);
        Twilight2(id + 0x5000, 5.7f);
        Starflare2(id + 0x6000, 3.5f);
        DarkNova(id + 0x7000, 3.6f);

        Cast(id + 0x8000, AID.AtomicTailVisual1, 6.5f, 6)
            .ActivateOnEnter<AtomicTail>()
            .ActivateOnEnter<AtomicTailArena>()
            .ActivateOnEnter<GyreCharge>();
        Timeout(id + 0x8010, 1, "Ground floor disappears");
        ComponentCondition<GyreCharge>(id + 0x8020, 5.2f, g => g.NumCasts > 0, "Raidwide + stun")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        P2(id + 0x10000, 52.8f);
    }

    void UpDown(uint id, float delay)
    {
        CastMulti(id, [AID.CosmicBreathVisual1, AID.CosmicTailVisual1], delay, 6)
            .ActivateOnEnter<CosmicBreath>()
            .ActivateOnEnter<CosmicTail>()
            .ActivateOnEnter<UpDownCounter>();

        ComponentCondition<UpDownCounter>(id + 0x10, 1.1f, d => d.NumCasts > 0, "Up/down")
            .DeactivateOnExit<CosmicBreath>()
            .DeactivateOnExit<CosmicTail>()
            .DeactivateOnExit<UpDownCounter>();
    }

    void Twilight1(uint id, float delay)
    {
        Cast(id, AID.CloakOfTwilight1, delay, 3)
            .ActivateOnEnter<TwilightNebula>();

        ComponentCondition<TwilightNebula>(id + 0x10, 13.8f, n => n.NumCasts > 0, "Light/dark")
            .DeactivateOnExit<TwilightNebula>();
    }

    void Starflare1(uint id, float delay)
    {
        Cast(id, AID.StarflareVisual1, delay, 3)
            .ActivateOnEnter<Starflare>()
            .ActivateOnEnter<CosmicBreath>()
            .ActivateOnEnter<CosmicTail>()
            .ActivateOnEnter<UpDownCounter>();

        ComponentCondition<Starflare>(id + 0x10, 5.1f, s => s.NumCasts >= 10, "Lines 1");
        ComponentCondition<Starflare>(id + 0x20, 2, s => s.NumCasts >= 20, "Lines 2")
            .DeactivateOnExit<Starflare>();

        ComponentCondition<UpDownCounter>(id + 0x30, 4.5f, c => c.NumCasts > 0, "Up/down")
            .DeactivateOnExit<CosmicBreath>()
            .DeactivateOnExit<CosmicTail>()
            .DeactivateOnExit<UpDownCounter>();
    }

    void Vortex(uint id, float delay)
    {
        CastStart(id, AID.CataclysmicVortexVisual1, delay)
            .ActivateOnEnter<VortexLook>()
            .ActivateOnEnter<VortexNoLook>()
            .ActivateOnEnter<VortexStayMove>();
        CastEnd(id + 1, 7, "Stay/move/gaze")
            .DeactivateOnExit<VortexLook>()
            .DeactivateOnExit<VortexNoLook>()
            .DeactivateOnExit<VortexStayMove>();
    }

    void Twilight2(uint id, float delay)
    {
        Cast(id, AID.CloakOfTwilight1, delay, 3)
            .ActivateOnEnter<TwilightNebula>();

        UpDown(id + 0x100, 3.8f);

        ComponentCondition<TwilightNebula>(id + 0x200, 8.7f, n => n.NumCasts > 0, "Light/dark")
            .DeactivateOnExit<TwilightNebula>();
    }

    void Starflare2(uint id, float delay)
    {
        Cast(id, AID.StarflareVisual1, delay, 3)
            .ActivateOnEnter<Starflare>();

        CastStart(id + 0x10, AID.CataclysmicVortexVisual1, 3.6f)
            .ActivateOnEnter<VortexLook>()
            .ActivateOnEnter<VortexNoLook>()
            .ActivateOnEnter<VortexStayMove>();

        ComponentCondition<Starflare>(id + 0x20, 1.4f, s => s.NumCasts >= 10, "Lines 1");
        ComponentCondition<Starflare>(id + 0x21, 2, s => s.NumCasts >= 20, "Lines 2")
            .DeactivateOnExit<Starflare>();

        CastEnd(id + 0x30, 3.6f, "Stay/move/gaze")
            .DeactivateOnExit<VortexLook>()
            .DeactivateOnExit<VortexNoLook>()
            .DeactivateOnExit<VortexStayMove>();
    }

    void DarkNova(uint id, float delay)
    {
        CastStart(id, AID.DarkNovaVisual1, delay)
            .ActivateOnEnter<DarkNova>();
        ComponentCondition<DarkNova>(id + 0x10, 6.2f, d => d.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<DarkNova>();
    }

    void P2(uint id, float delay)
    {
        ActorTargetable(id, _module.BossP2, true, delay, "Boss reappears")
            .DeactivateOnExit<AtomicTail>()
            .DeactivateOnExit<AtomicTailArena>()
            .DeactivateOnExit<GyreCharge>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        CelestialTrail(id + 0x100, 28.9f);
        EmptyProclamation(id + 0x1000, 32.9f);
        Swordscross(id + 0x10000, 3.1f);
        TwinBlaze(id + 0x11000, 5.7f);
        CataclysmicBlade(id + 0x12000, 6.2f);
        Burst(id + 0x13000, 8.2f);
        CosmicFlame(id + 0x14000, 3.2f);
        SuperNova(id + 0x15000, 7.2f);
        EmptyProclamation(id + 0x16000, 0.1f);

        Timeout(id + 0x20000, 10000, "Repeat mechanics until death")
            .ActivateOnEnter<EmptyProclamation>()
            .ActivateOnEnter<Swordscross1>()
            .ActivateOnEnter<Swordscross2>()
            .ActivateOnEnter<TwinBlaze1>()
            .ActivateOnEnter<TwinBlaze2>()
            .ActivateOnEnter<CataclysmicBlade>()
            .ActivateOnEnter<VortexLook>()
            .ActivateOnEnter<VortexNoLook>()
            .ActivateOnEnter<VortexStayMove>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<CosmicFlame>()
            .ActivateOnEnter<SuperNova>()
            .ActivateOnEnter<StarflareP2>()
            .ActivateOnEnter<DarkNovaP2>();
    }

    void CelestialTrail(uint id, float delay)
    {
        ComponentCondition<CelestialTrail>(id, delay, c => c.NumCasts >= 8, "Towers 1")
            .ActivateOnEnter<CelestialTrail>();
        ComponentCondition<CelestialTrail>(id + 0x10, 19.3f, c => c.NumCasts >= 16, "Towers 2")
            .DeactivateOnExit<CelestialTrail>();
    }

    void EmptyProclamation(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.EmptyProclamation, delay, 4, true, "Raidwide")
            .ActivateOnEnter<EmptyProclamation>()
            .DeactivateOnExit<EmptyProclamation>();
    }

    void Swordscross(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.RightSwordscrossVisual, AID.LeftSwordscrossVisual], delay, 8, true)
            .ActivateOnEnter<Swordscross1>()
            .ActivateOnEnter<Swordscross2>();

        ComponentCondition<Swordscross1>(id + 0x10, 1, s => s.NumCasts > 0, "Swords")
            .DeactivateOnExit<Swordscross1>()
            .DeactivateOnExit<Swordscross2>();
    }

    State TwinBlaze(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.TwinBlazeVisual1, AID.TwinBlazeVisual2], delay, 5)
            .ActivateOnEnter<TwinBlaze1>()
            .ActivateOnEnter<TwinBlaze2>();

        return ComponentCondition<TwinBlaze1>(id + 0x10, 1, t => t.NumCasts > 0, "In/out")
            .DeactivateOnExit<TwinBlaze1>()
            .DeactivateOnExit<TwinBlaze2>();
    }

    State CataclysmicBlade(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.CataclysmicBladeVisual, delay, true)
            .ActivateOnEnter<CataclysmicBlade>()
            .ActivateOnEnter<VortexLook>()
            .ActivateOnEnter<VortexNoLook>()
            .ActivateOnEnter<VortexStayMove>();

        return ComponentCondition<CataclysmicBlade>(id + 0x10, 7, c => c.NumCasts > 0, "Cones + stay/move/gaze")
            .DeactivateOnExit<CataclysmicBlade>()
            .DeactivateOnExit<VortexLook>()
            .DeactivateOnExit<VortexNoLook>()
            .DeactivateOnExit<VortexStayMove>();
    }

    void Burst(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.BurstVisual, delay, 3)
            .ActivateOnEnter<Burst>();

        TwinBlaze(id + 0x100, 7.2f)
            .DeactivateOnExit<Burst>();
    }

    void CosmicFlame(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.CosmicFlameVisual, delay)
            .ActivateOnEnter<CosmicFlame>();

        ComponentCondition<CosmicFlame>(id + 0x10, 5, f => f.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<AtomicRayCast>();

        ActorCast(id + 0x100, _module.BossP2, AID.AtomicRayVisual, 7.2f, 3, true);

        ComponentCondition<CosmicFlame>(id + 0x110, 4.2f, f => f.NumCasts >= 40, "Exaflares finish")
            .DeactivateOnExit<CosmicFlame>();

        CataclysmicBlade(id + 0x200, 8)
            .DeactivateOnExit<AtomicRay>()
            .DeactivateOnExit<AtomicRayCast>();
    }

    void SuperNova(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.SuperNovaVisual, delay, true)
            .ActivateOnEnter<SuperNova>();

        ComponentCondition<SuperNova>(id + 0x10, 6.2f, s => s.NumCasts > 0, "Stack 1");
        ComponentCondition<SuperNova>(id + 0x20, 1.9f, s => s.NumCasts >= 3, "Stack 3")
            .DeactivateOnExit<SuperNova>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117, NameID = 14729)]
public class A36ShinryuParadox(WorldState ws, Actor primary) : BossModule(ws, primary, new(820, -820), new ArenaBoundsRect(30, 20))
{
    Actor? Groin;
    Actor? _bossP2;

    public Actor? BossP2() => _bossP2;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.HollowKing), ArenaColor.Enemy);
    }

    protected override void UpdateModule()
    {
        Groin ??= Enemies(OID.ShinryusGroin).FirstOrDefault();
        _bossP2 ??= Enemies(OID.HollowKing).FirstOrDefault();
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var pBoss = 0;
        var pTail = AIHints.Enemy.PriorityInvincible;
        if (Helpers.Level(actor) == 0)
            (pTail, pBoss) = (pBoss, pTail);

        hints.SetPriority(PrimaryActor, pBoss);
        hints.SetPriority(Groin, pTail);
    }
}

static class Helpers
{
    public static int Level(Actor pc) => Level(pc.PosRot);
    public static int Level(Vector4 p) => p.Y < -890 ? 0 : 1;
}
