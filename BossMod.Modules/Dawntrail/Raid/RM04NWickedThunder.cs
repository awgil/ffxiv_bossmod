namespace BossMod.Dawntrail.Raid.RM04NWickedThunder;

public enum OID : uint
{
    Boss = 0x4263, // R4.900, x1
    Helper = 0x233C, // R0.500, x25, Helper type
    WickedReplica = 0x4264, // R3.675-4.900, x16
    WickedThunder = 0x4569, // R1.000, x1
}

public enum AID : uint
{
    AutoAttack1 = 39146, // Boss->player, no cast, single-target
    AutoAttack2 = 36759, // Boss->player, no cast, single-target
    Teleport = 37577, // Boss->location, no cast, single-target

    WrathOfZeus = 37575, // Boss->self, 5.0s cast, range 60 circle, raidwide
    WickedJolt = 37576, // Boss->player, 5.0s cast, range 60 width 5 rect, tankbuster
    SidewiseSpark1 = 37564, // Boss->self, 7.0s cast, range 60 180-degree cone
    SidewiseSpark2 = 37565, // Boss->self, 7.0s cast, range 60 180-degree cone
    SidewiseSpark3 = 37566, // Boss->self, 7.0s cast, range 60 180-degree cone
    SidewiseSpark4 = 37567, // Boss->self, 7.0s cast, range 60 180-degree cone
    ShadowsSabbath1 = 38044, // Boss->self, 3.0s cast, single-target, visual (replicas)
    ShadowsSabbath2 = 39871, // Boss->self, 3.0s cast, single-target, visual (replicas)
    ReplicaSidewiseSpark1 = 39429, // WickedReplica->self, 1.0s cast, range 60 180-degree cone
    ReplicaSidewiseSpark2 = 39439, // WickedReplica->self, 1.0s cast, range 60 180-degree cone

    StampedingThunderStart = 37543, // Boss->self, no cast, single-target, visual
    StampedingThunderWest = 37547, // Boss->self, no cast, single-target, visual (west side hit)
    StampedingThunderEast = 37548, // Boss->self, no cast, single-target, visual (east side hit)
    StampedingThunderVisual = 35334, // Boss->self, no cast, single-target
    StampedingThunderLast = 36150, // Helper->self, 7.3s cast, range 40 width 30 rect
    StampedingThunderAOE = 35335, // Helper->self, no cast, range 40 width 30 rect

    ThreefoldBlast1 = 37549, // Boss->self, 11.0s cast, single-target, visual (cannons)
    ThreefoldBlast2 = 37552, // Boss->self, 11.0s cast, single-target, visual (cannons)
    FourfoldBlast1 = 39759, // Boss->self, 13.5s cast, single-target, visual (cannons)
    FourfoldBlast2 = 39765, // Boss->self, 13.5s cast, single-target, visual (cannons)
    FivefoldBlast1 = 39766, // Boss->self, 16.0s cast, single-target, visual (cannons)
    FivefoldBlast2 = 39767, // Boss->self, 16.0s cast, single-target, visual (cannons)
    WickedCannon1 = 20032, // Helper->self, no cast, range 40 width 10 rect
    WickedCannon2 = 37550, // Boss->self, no cast, range 40 width 10 rect
    WickedCannon3 = 37551, // Boss->self, no cast, range 40 width 10 rect
    WickedCannon4 = 39852, // Boss->self, no cast, range 40 width 10 rect
    WickedCannon5 = 39870, // Boss->self, no cast, range 40 width 10 rect

    WickedHypercannon1 = 37553, // Boss->self, 5.0s cast, single-target, visual (line stack)
    WickedHypercannon2 = 37554, // Boss->self, 5.0s cast, single-target, visual (line stack)
    WickedHypercannonVisual = 37102, // WickedThunder->self, no cast, single-target
    WickedHypercannonAOE = 37555, // Helper->self, no cast, range 40 width 10 rect, 10 hits

    Thunderslam = 37574, // Helper->location, 3.0s cast, range 5 circle
    ThunderstormVisual = 37544, // Boss->self, no cast, single-target
    Thunderstorm = 37573, // Helper->player, 8.0s cast, range 5 circle spread
    WickedBoltVisual = 37570, // Boss->self, 4.0+1.0s cast, single-target, visual (stack)
    WickedBolt = 37571, // Helper->players, no cast, range 5 circle, 5 hits
    SoaringSoulpressVisual1 = 37568, // Boss->self, 5.0s cast, single-target
    SoaringSoulpressVisual2 = 37546, // Boss->self, no cast, single-target
    SoaringSoulpress = 37569, // Boss->players, no cast, range 6 circle stack

    BewitchingFlightVisual = 37545, // Boss->self, no cast, single-target
    BewitchingFlight1 = 36324, // Boss/WickedReplica->self, 6.0+1.0s cast, single-target, visual (lines)
    BewitchingFlight2 = 36326, // Boss/WickedReplica->self, 6.0+1.0s cast, single-target, visual (lines)
    BewitchingFlightAOE = 37560, // Helper->self, 7.0s cast, range 40 width 5 rect
    Burst = 37561, // Helper->self, 7.0s cast, range 40 width 16 rect

    WitchHunt = 37556, // Boss->self, 3.0s cast, single-target
    WitchHuntTelegraph = 37557, // Helper->location, 1.5s cast, range 6 circle
    WitchHuntAOE = 37558, // WickedReplica->location, no cast, range 6 circle
}

public enum SID : uint
{
    WickedReplica = 2056, // none->WickedReplica, extra=0x319/0x31A
    WickedCannon = 2970, // none->Boss, extra=0x2D3 (north) / 0x2D4 (south)
}

public enum IconID : uint
{
    SoaringSoulpress = 161, // player
    WickedBolt = 316, // player
    Thunderstorm = 345, // player
    WickedJolt = 471, // player
}

class WrathOfZeus(BossModule module) : Components.RaidwideCast(module, AID.WrathOfZeus);
class WickedJolt(BossModule module) : Components.BaitAwayCast(module, AID.WickedJolt, new AOEShapeRect(60, 2.5f), false, true);
class SidewiseSpark(BossModule module) : Components.GroupedAOEs(module, [AID.SidewiseSpark1, AID.SidewiseSpark2, AID.SidewiseSpark3, AID.SidewiseSpark4], new AOEShapeCone(60, 90.Degrees()));
class Thunderslam(BossModule module) : Components.StandardAOEs(module, AID.Thunderslam, new AOEShapeCircle(5));
class Thunderstorm(BossModule module) : Components.SpreadFromCastTargets(module, AID.Thunderstorm, 6);
class WickedBolt(BossModule module) : Components.StackWithIcon(module, (uint)IconID.WickedBolt, AID.WickedBolt, 5, 5.1f, 4, 8, 5)
{
    private int _hits;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != StackAction)
            return;
        if (++_hits < 5)
        {
            for (var i = 0; i < Stacks.Count; ++i)
                Stacks[i] = Stacks[i] with { Activation = WorldState.FutureTime(1), Hits = 5 - _hits };
            return;
        }
        _hits = 0;
        base.OnEventCast(caster, spell);
    }
}
class SoaringSoulpress(BossModule module) : Components.StackWithIcon(module, (uint)IconID.SoaringSoulpress, AID.SoaringSoulpress, 6, 5.4f, 4, 8);
class BewitchingFlight(BossModule module) : Components.StandardAOEs(module, AID.BewitchingFlightAOE, new AOEShapeRect(40, 2.5f));
class Burst(BossModule module) : Components.StandardAOEs(module, AID.Burst, new AOEShapeRect(40, 8), 2, highlightImminent: true);
class WickedHypercannon1(BossModule module) : Components.RaidwideMultiHit(module, AID.WickedHypercannon1, [AID.WickedHypercannonAOE], 0.6f, 1.06f, 10, "Line stack");
class WickedHypercannon2(BossModule module) : Components.RaidwideMultiHit(module, AID.WickedHypercannon2, [AID.WickedHypercannonAOE], 0.6f, 1.06f, 10, "Line stack");

// TODO: never cast in a replay
class WitchHunt(BossModule module) : Components.GenericAOEs(module, AID.WitchHuntAOE)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;
        var deadline = _aoes[0].Activation.AddSeconds(1);
        foreach (var aoe in _aoes)
            yield return aoe.Activation < deadline ? aoe with { Color = ArenaColor.Danger } : aoe with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WitchHuntTelegraph)
            _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell, 6.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && _aoes.Count > 0)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public int NumChanges;

    private static readonly ArenaBoundsRect _strip = new(5, 20);
    private static readonly ArenaBoundsSquare _full = new(20);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x00)
            return;
        switch (state)
        {
            case 0x00020001:
                SetArena(_strip, new(85, 100));
                break;
            case 0x00200010:
                SetArena(_strip, new(115, 100));
                break;
            case 0x00400004:
            case 0x00800004:
                SetArena(_full, new(100, 100));
                break;
        }
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        ++NumChanges;
        Arena.Bounds = bounds;
        Arena.Center = center;
    }
}

class StampedingThunder(BossModule module) : Components.GenericAOEs(module, AID.StampedingThunderAOE)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeRect _shape = new(40, 15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StampedingThunderWest:
                _aoe = new(_shape, new(95, 80), default, WorldState.FutureTime(2.3f));
                break;
            case AID.StampedingThunderEast:
                _aoe = new(_shape, new(105, 80), default, WorldState.FutureTime(2.3f));
                break;
            case AID.StampedingThunderAOE:
                ++NumCasts;
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state is 0x00200010 or 0x00020001)
            _aoe = null;
    }
}

class WickedCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private float[] _delays = [];

    private static readonly AOEShapeRect _shape = new(40, 5);
    private static readonly float[] _delays3 = [9.5f, 8.3f, 7.7f];
    private static readonly float[] _delays4 = [12, 10.9f, 10.2f, 9.5f];
    private static readonly float[] _delays5 = [14.5f, 13.4f, 12.8f, 12.2f, 11.6f];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes.Count > 1 ? _aoes[0] with { Color = ArenaColor.Danger } : _aoes[0];
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _delays = (AID)spell.Action.ID switch
        {
            AID.ThreefoldBlast1 or AID.ThreefoldBlast2 => _delays3,
            AID.FourfoldBlast1 or AID.FourfoldBlast2 => _delays4,
            AID.FivefoldBlast1 or AID.FivefoldBlast2 => _delays5,
            _ => _delays
        };
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.WickedCannon && _aoes.Count + NumCasts < _delays.Length)
            _aoes.Add(new(_shape, actor.Position, status.Extra == 0x2D3 ? 180.Degrees() : default, WorldState.FutureTime(_delays[_aoes.Count + NumCasts])));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WickedCannon1 or AID.WickedCannon2 or AID.WickedCannon3 or AID.WickedCannon4 or AID.WickedCannon5)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class ReplicaSidewiseSpark(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, AOEInstance aoe)> _aoes = [];

    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;
        var deadline = _aoes[0].aoe.Activation.AddSeconds(1);
        foreach (var (_, aoe) in _aoes)
            yield return aoe.Activation < deadline ? aoe : aoe with { Risky = false };
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID != OID.WickedReplica)
            return;
        var offset = id switch
        {
            0x11D8 => 90.Degrees(),
            0x11D6 => -90.Degrees(),
            _ => default
        };
        if (offset != default)
        {
            _aoes.Add((actor, new(_shape, actor.Position, actor.Rotation + offset, WorldState.FutureTime(8))));
            _aoes.SortBy(a => a.aoe.Activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ReplicaSidewiseSpark1 or AID.ReplicaSidewiseSpark2)
        {
            var index = _aoes.FindIndex(a => a.caster == caster);
            var aoe = new AOEInstance(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
            if (index >= 0)
                _aoes[index] = (caster, aoe);
            else
                _aoes.Add((caster, aoe));
            _aoes.SortBy(a => a.aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ReplicaSidewiseSpark1 or AID.ReplicaSidewiseSpark2)
        {
            ++NumCasts;
            var index = _aoes.FindIndex(a => a.caster == caster);
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }
}

class RM04NWickedThunderStates : StateMachineBuilder
{
    public RM04NWickedThunderStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Thunderslam>();
    }

    private void SinglePhase(uint id)
    {
        WrathOfZeus(id, 6.1f);
        SidewiseSpark(id + 0x10000, 6.2f);
        ShadowsSabbath1(id + 0x20000, 7.6f);
        WickedJolt(id + 0x30000, 1.9f);
        StampedingThunder(id + 0x40000, 17.2f, 1);
        Cannons(id + 0x50000, [AID.ThreefoldBlast1, AID.ThreefoldBlast2], 4.1f, 11, 3);
        Cannons(id + 0x60000, [AID.FourfoldBlast1, AID.FourfoldBlast2], 3.2f, 13.5f, 4);
        WickedBolt(id + 0x70000, 8.2f);
        BewitchingFlight(id + 0x80000, 13.8f);
        WrathOfZeus(id + 0x90000, 8.3f);
        ShadowsSabbath2(id + 0xA0000, 8.5f);
        WrathOfZeus(id + 0xB0000, 4.1f);
        WickedJolt(id + 0xC0000, 3.4f);
        StampedingThunder(id + 0xD0000, 18.2f, 3);

        Dictionary<AID, (uint seqID, Action<uint> buildState)> fork = new()
        {
            [AID.FivefoldBlast1] = ((id >> 24) + 1, ForkCannons),
            [AID.FivefoldBlast2] = ((id >> 24) + 2, ForkCannons),
            [AID.WickedHypercannon1] = ((id >> 24) + 3, ForkHypercannon)
        };
        CastStartFork(id + 0xE0000, fork, 4.1f, "Cannons -or- Hypercannon");
    }

    private void ForkCannons(uint id)
    {
        Cannons(id, [AID.FivefoldBlast1, AID.FivefoldBlast2], 0, 16, 5);
        Thunderstorm(id + 0x10000, 8.9f);
        WickedBolt(id + 0x20000, 2.3f);
        StampedingThunder(id + 0x30000, 12.1f, 5);
        Hypercannon1(id + 0x40000, 4.1f);
        Hypercannon2(id + 0x50000, 10);
        Enrage(id + 0x60000);
    }

    private void ForkHypercannon(uint id)
    {
        Hypercannon1(id, 0);
        Hypercannon2(id + 0x10000, 10);
        Enrage(id + 0x20000);
    }

    private void Thunderstorm(uint id, float delay)
    {
        ComponentCondition<Thunderstorm>(id, delay, c => c.Spreads.Count == 0 && c.NumFinishedSpreads > 0, "Spreads")
            .ActivateOnEnter<Thunderstorm>()
            .DeactivateOnExit<Thunderstorm>();
    }

    // TODO: enrage unseen (kills end during second hypercannon)
    private void Enrage(uint id)
    {
        SimpleState(id, 120, "Until enrage")
            .ActivateOnEnter<WrathOfZeus>()
            .ActivateOnEnter<WickedJolt>()
            .ActivateOnEnter<SidewiseSpark>()
            .ActivateOnEnter<ReplicaSidewiseSpark>()
            .ActivateOnEnter<StampedingThunder>()
            .ActivateOnEnter<WickedCannon>()
            .ActivateOnEnter<WickedBolt>()
            .ActivateOnEnter<SoaringSoulpress>()
            .ActivateOnEnter<Thunderstorm>()
            .ActivateOnEnter<BewitchingFlight>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<WickedHypercannon1>()
            .ActivateOnEnter<WickedHypercannon2>()
            .ActivateOnEnter<WitchHunt>();
    }

    private void WrathOfZeus(uint id, float delay)
    {
        Cast(id, AID.WrathOfZeus, delay, 5, "Raidwide")
            .ActivateOnEnter<WrathOfZeus>()
            .DeactivateOnExit<WrathOfZeus>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SidewiseSpark(uint id, float delay)
    {
        CastMulti(id, [AID.SidewiseSpark1, AID.SidewiseSpark2, AID.SidewiseSpark3, AID.SidewiseSpark4], delay, 7, "Cleave")
            .ActivateOnEnter<SidewiseSpark>()
            .DeactivateOnExit<SidewiseSpark>();
    }

    private void WickedJolt(uint id, float delay)
    {
        Cast(id, AID.WickedJolt, delay, 5, "Tankbuster")
            .ActivateOnEnter<WickedJolt>()
            .DeactivateOnExit<WickedJolt>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ShadowsSabbath1(uint id, float delay)
    {
        CastMulti(id, [AID.ShadowsSabbath1, AID.ShadowsSabbath2], delay, 3)
            .ActivateOnEnter<ReplicaSidewiseSpark>();
        CastStartMulti(id + 0x10, [AID.ShadowsSabbath1, AID.ShadowsSabbath2], 8.1f);
        ComponentCondition<ReplicaSidewiseSpark>(id + 0x20, 1.1f, c => c.NumCasts >= 1, "Replica cleave 1");
        CastEnd(id + 0x30, 1.9f);
        ComponentCondition<ReplicaSidewiseSpark>(id + 0x40, 9.3f, c => c.NumCasts >= 2, "Replica cleave 2")
            .DeactivateOnExit<ReplicaSidewiseSpark>();
    }

    private void ShadowsSabbath2(uint id, float delay)
    {
        CastMulti(id, [AID.ShadowsSabbath1, AID.ShadowsSabbath2], delay, 3)
            .ActivateOnEnter<ReplicaSidewiseSpark>()
            .ActivateOnEnter<SidewiseSpark>();
        ComponentCondition<ReplicaSidewiseSpark>(id + 0x10, 9.2f, c => c.NumCasts >= 1, "Replica cleave 1");
        CastStartMulti(id + 0x20, [AID.SidewiseSpark1, AID.SidewiseSpark2, AID.SidewiseSpark3, AID.SidewiseSpark4], 1.2f);
        ComponentCondition<ReplicaSidewiseSpark>(id + 0x30, 4.8f, c => c.NumCasts >= 4, "Replica cleave 4")
            .DeactivateOnExit<ReplicaSidewiseSpark>();
        CastEnd(id + 0x40, 2.2f, "Cleave")
            .DeactivateOnExit<SidewiseSpark>();
    }

    private void StampedingThunder(uint id, float delay, int numChanges)
    {
        ComponentCondition<StampedingThunder>(id, delay, c => c.NumCasts > 0, "Stampede")
            .ActivateOnEnter<StampedingThunder>();
        ComponentCondition<ArenaChanges>(id + 0x10, 7.1f, c => c.NumChanges >= numChanges, "Platform breaks")
            .DeactivateOnExit<StampedingThunder>();
    }

    private void Cannons(uint id, AID[] aids, float delay, float castTime, int count)
    {
        CastMulti(id, aids, delay, castTime)
            .ActivateOnEnter<WickedCannon>();
        ComponentCondition<WickedCannon>(id + 0x10, 0.5f, c => c.NumCasts >= 1, "Cannon 1");
        ComponentCondition<WickedCannon>(id + 0x20, count switch { 3 => 3.2f, 4 => 5.1f, _ => 7 }, c => c.NumCasts >= count, $"Cannon {count}")
            .DeactivateOnExit<WickedCannon>();
    }

    private void WickedBolt(uint id, float delay)
    {
        CastStart(id, AID.WickedBoltVisual, delay)
            .ActivateOnEnter<WickedBolt>();
        CastEnd(id + 1, 4);
        ComponentCondition<WickedBolt>(id + 0x10, 5.1f, c => c.NumFinishedStacks > 0, "Multi-hit stack")
            .DeactivateOnExit<WickedBolt>();
    }

    private void BewitchingFlight(uint id, float delay)
    {
        CastMulti(id, [AID.BewitchingFlight1, AID.BewitchingFlight2], delay, 6)
            .ActivateOnEnter<BewitchingFlight>()
            .ActivateOnEnter<Burst>();
        ComponentCondition<BewitchingFlight>(id + 0x10, 1, c => c.NumCasts > 0, "Lines")
            .DeactivateOnExit<BewitchingFlight>();
        ComponentCondition<Burst>(id + 0x20, 10.1f, c => c.NumCasts >= 1, "Burst 1");
        CastStartMulti(id + 0x30, [AID.SidewiseSpark1, AID.SidewiseSpark2, AID.SidewiseSpark3, AID.SidewiseSpark4], 11.4f)
            .ActivateOnEnter<SidewiseSpark>();
        CastEnd(id + 0x40, 7, "Cleave")
            .DeactivateOnExit<SidewiseSpark>()
            .DeactivateOnExit<Burst>();
    }

    private void Hypercannon1(uint id, float delay)
    {
        Cast(id, AID.WickedHypercannon1, delay, 5)
            .ActivateOnEnter<WickedHypercannon1>();
        ComponentCondition<WickedHypercannon1>(id + 0x10, 0.6f, c => c.NumCasts > 0, "Line stack start")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<WickedHypercannon1>(id + 0x20, 9.6f, c => c.NumCasts >= 10, "Line stack end")
            .DeactivateOnExit<WickedHypercannon1>();
    }

    private void Hypercannon2(uint id, float delay)
    {
        Cast(id, AID.WickedHypercannon2, delay, 5)
            .ActivateOnEnter<WickedHypercannon2>();
        ComponentCondition<WickedHypercannon2>(id + 0x10, 0.6f, c => c.NumCasts > 0, "Line stack start")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<WickedHypercannon2>(id + 0x20, 9.6f, c => c.NumCasts >= 10, "Line stack end")
            .DeactivateOnExit<WickedHypercannon2>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 991, NameID = 13057)]
public class RM04NWickedThunder(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
