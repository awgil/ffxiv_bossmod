namespace BossMod.Endwalker.Raid.P6NHegemone;

public enum OID : uint
{
    Boss = 0x3A35, // R10.010, x1
    Helper = 0x233C, // R0.500, x25, Helper type
    Parasitos = 0x3A36, // R1.000, x8
    PolyQuadrant = 0x3AD9, // R1.000, x4, sits on a 2x2 quadrant centre, tethered for aetherial exchange
    ParasitosCaster = 0x3AEA, // R0.500, x8, casts reek havoc from an infected player's position
    ActorFEC1D = 0xFEC1D, // R0.500, x1, EventNpc type
    ActorFEC1E = 0xFEC1E, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1E8536 = 0x1E8536, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 30795, // Boss->player, no cast, single-target
    Teleport = 30796, // Boss->location, no cast, single-target
    AetherialExchange = 30797, // Boss->self, 3.0s cast, single-target, visual (next polyominoid has swap tethers)
    HemitheossDarkIV = 30798, // Boss->self, 5.0s cast, range 60 circle, raidwide
    Transmission = 30799, // Boss->self, 5.0s cast, single-target, visual (tethered players get glossomorph)
    AethericPolyominoid = 30803, // Boss->self, 4.0s cast, single-target, visual (quadrant markers appear)
    PolyominoidVisual = 30804, // Helper->self, no cast, single-target
    PolyominousDarkIV = 30805, // Helper->self, 1.0s cast, range 10 width 10 rect, cell explosion
    PolyominoidSigma = 30806, // Boss->self, 4.0s cast, single-target, visual (quadrant markers appear)
    ChorosIxouFrontBack = 30807, // Boss->self, 4.5+0.5s cast, single-target, visual
    ChorosIxouSides = 30808, // Boss->self, 4.5+0.5s cast, single-target, visual
    ChorosIxouFrontBackAOE = 30809, // Helper->self, 5.0s cast, range 40 90-degree cone
    ChorosIxouSidesAOE = 30810, // Helper->self, 5.0s cast, range 40 90-degree cone
    Synergy = 30811, // Boss->self, 4.3+0.7s cast, single-target, visual (double tankbuster)
    SynergyAOE1 = 30812, // Helper->player, 5.0s cast, range 5 circle, tankbuster
    SynergyAOE2 = 30813, // Helper->player, 5.0s cast, range 5 circle, tankbuster
    DarkAshes = 30814, // Boss->self, 4.0s cast, single-target, visual (spreads)
    DarkAshesAOE = 30815, // Helper->player, 8.0s cast, range 6 circle spread
    ReekHavoc = 31213, // ParasitosCaster->self, 3.4s cast, range 60 30-degree cone, from an infected player's facing
    StropheIxou = 31249, // Boss->self, 5.0s cast, single-target, visual (rotating front/back cones)
    StropheIxouRest = 31251, // Boss->self, no cast, single-target
    PathogenicCellsFirst = 31252, // Helper->self, 5.0s cast, range 40 45-degree cone, boss front
    ChelicVectorFirst = 31253, // Helper->self, 5.0s cast, range 40 45-degree cone, boss back
    PathogenicCellsRest = 31254, // Helper->self, no cast, range 40 45-degree cone
    ChelicVectorRest = 31255, // Helper->self, no cast, range 40 45-degree cone
}

public enum SID : uint
{
    Glossomorph = 3314, // none->player, extra=0x0, snake infection, becomes a stun and a forward cone when it runs out
    OutOfControl = 3316, // none->player, extra=0x0, stun
}

public enum IconID : uint
{
    DarkAshes = 101, // player
    StropheRotateCCW = 167, // Boss, cones turn -45 degrees per hit
    StropheRotateCW = 168, // Boss, cones turn +45 degrees per hit
    Synergy = 343, // player
    Transmission = 354, // player
}

public enum TetherID : uint
{
    PolyExchange = 207, // PolyQuadrant->PolyQuadrant
    Transmission = 208, // player->Boss
}

class HemitheossDarkIV(BossModule module) : Components.RaidwideCast(module, AID.HemitheossDarkIV);
class ChorosIxouFrontBack(BossModule module) : Components.StandardAOEs(module, AID.ChorosIxouFrontBackAOE, new AOEShapeCone(40, 45.Degrees()));
class ChorosIxouSides(BossModule module) : Components.StandardAOEs(module, AID.ChorosIxouSidesAOE, new AOEShapeCone(40, 45.Degrees()));
class ReekHavoc(BossModule module) : Components.StandardAOEs(module, AID.ReekHavoc, new AOEShapeCone(60, 15.Degrees()));
class SynergyAOE1(BossModule module) : Components.BaitAwayCast(module, AID.SynergyAOE1, new AOEShapeCircle(5), true);
class SynergyAOE2(BossModule module) : Components.BaitAwayCast(module, AID.SynergyAOE2, new AOEShapeCircle(5), true);
class DarkAshes(BossModule module) : Components.SpreadFromCastTargets(module, AID.DarkAshesAOE, 6);

class Polyominoid(BossModule module) : Components.GenericAOEs(module, AID.PolyominousDarkIV)
{
    private enum Marker { None, Plus, Cross, Full }

    private readonly Marker[] _markers = new Marker[4];
    private readonly List<(Actor from, Actor to)> _tethers = [];
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var markers = (Marker[])_markers.Clone();
        foreach (var (from, to) in _tethers)
        {
            var q1 = QuadrantIndex(from.Position);
            var q2 = QuadrantIndex(to.Position);
            (markers[q1], markers[q2]) = (markers[q2], markers[q1]);
        }

        for (var q = 0; q < 4; ++q)
        {
            var x0 = (q & 1) * 2;
            var z0 = (q >> 1) * 2;
            foreach (var (dx, dz) in Cells(markers[q]))
                yield return new(_shape, Arena.Center + new WDir(-15 + 10 * (x0 + dx), -15 + 10 * (z0 + dz)), default, _activation);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is < 1 or > 4)
            return;
        var marker = state switch
        {
            0x00020001 or 0x00100001 => Marker.Plus,
            0x00400020 or 0x00800020 => Marker.Cross,
            0x02000100 or 0x04000100 => Marker.Full,
            _ => Marker.None
        };
        if (marker == Marker.None)
            return;
        _markers[index - 1] = marker;
        if (state is 0x00020001 or 0x00400020 or 0x02000100)
            _activation = WorldState.FutureTime(8.1f);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PolyExchange && WorldState.Actors.Find(tether.Target) is var target && target != null)
            _tethers.Add((source, target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PolyExchange)
            _tethers.RemoveAll(t => t.from == source);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Array.Fill(_markers, Marker.None);
            _tethers.Clear();
        }
    }

    private int QuadrantIndex(WPos pos) => (pos.Z > Arena.Center.Z ? 2 : 0) + (pos.X > Arena.Center.X ? 1 : 0);

    private static (int, int)[] Cells(Marker marker) => marker switch
    {
        Marker.Plus => [(0, 0), (1, 1)],
        Marker.Cross => [(1, 0), (0, 1)],
        Marker.Full => [(0, 0), (1, 0), (0, 1), (1, 1)],
        _ => []
    };
}

class Transmission(BossModule module) : Components.GenericAOEs(module)
{
    private readonly DateTime[] _expire = new DateTime[PartyState.MaxPartySize];
    private BitMask _infected;
    private BitMask _stunned;

    private static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (i, p) in Raid.WithSlot(true).IncludedInMask(_infected | _stunned))
            if (i != slot && (_stunned[i] || (_expire[i] - WorldState.CurrentTime).TotalSeconds < 3))
                yield return new(_shape, p.Position, p.Rotation, _expire[i].AddSeconds(4.9f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_infected[slot])
            hints.Add("Face away from raid!", Raid.WithoutSlot().Exclude(actor).InShape(_shape, actor.Position, actor.Rotation).Any());
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_infected[slot])
            foreach (var p in Raid.WithoutSlot().Exclude(actor))
                hints.ForbiddenDirections.Add((Angle.FromDirection(p.Position - actor.Position), 15.Degrees(), _expire[slot]));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_infected[pcSlot] || _stunned[pcSlot])
            _shape.Outline(Arena, pc.Position, pc.Rotation);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (!Raid.TryFindSlot(actor, out var slot))
            return;
        switch ((SID)status.ID)
        {
            case SID.Glossomorph:
                _infected.Set(slot);
                _expire[slot] = status.ExpireAt;
                break;
            case SID.OutOfControl:
                _infected.Clear(slot);
                _stunned.Set(slot);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID is SID.Glossomorph or SID.OutOfControl && Raid.TryFindSlot(actor, out var slot))
        {
            if ((SID)status.ID == SID.Glossomorph)
                _infected.Clear(slot);
            else
                _stunned.Clear(slot);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ReekHavoc)
            _stunned.Reset();
    }
}

class StropheIxou(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;

    private static readonly AOEShapeCone _shape = new(40, 22.5f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = (IconID)iconID switch
        {
            IconID.StropheRotateCCW => -45.Degrees(),
            IconID.StropheRotateCW => 45.Degrees(),
            _ => _increment
        };
        foreach (ref var s in Sequences.AsSpan())
            if (s.Increment == default)
                s.Increment = _increment;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PathogenicCellsFirst or AID.ChelicVectorFirst)
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation, _increment, Module.CastFinishAt(spell), 1.6f, 8));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var rot = (AID)spell.Action.ID switch
        {
            AID.PathogenicCellsFirst or AID.PathogenicCellsRest => spell.Rotation,
            AID.ChelicVectorFirst or AID.ChelicVectorRest => spell.Rotation + 180.Degrees(),
            _ => (Angle?)null
        };
        if (rot == null || Sequences.Count == 0)
            return;

        var index = 0;
        var best = float.MaxValue;
        for (var i = 0; i < Sequences.Count; ++i)
        {
            var d = MathF.Abs((Sequences[i].Rotation - rot.Value).Normalized().Rad);
            if (d < best)
            {
                best = d;
                index = i;
            }
        }
        ref var s = ref Sequences.AsSpan()[index];
        if (s.Increment == default && s.NumRemainingCasts < 8)
            s.Increment = (rot.Value - s.Rotation + s.Increment).Normalized();
        s.Rotation = rot.Value;
        AdvanceSequence(index, WorldState.CurrentTime);
        _increment = default;
    }
}

class P6NHegemoneStates : StateMachineBuilder
{
    public P6NHegemoneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HemitheossDarkIV>()
            .ActivateOnEnter<Polyominoid>()
            .ActivateOnEnter<ChorosIxouFrontBack>()
            .ActivateOnEnter<ChorosIxouSides>()
            .ActivateOnEnter<Transmission>()
            .ActivateOnEnter<ReekHavoc>()
            .ActivateOnEnter<StropheIxou>()
            .ActivateOnEnter<SynergyAOE1>()
            .ActivateOnEnter<SynergyAOE2>()
            .ActivateOnEnter<DarkAshes>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 880, NameID = 11381)]
public class P6NHegemone(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
