namespace BossMod.Dawntrail.Foray.CriticalEngagement.Abductor;

public enum OID : uint
{
    Boss = 0x4BE1, // R5.004, x1
    Helper = 0x233C, // R0.500, x16 (spawn during fight), Helper type
    BitingWind = 0x4BE2, // R1.000, x0 (spawn during fight)
    AbductorsPlume = 0x4BE3, // R1.000, x0 (spawn during fight)
    Abductor = 0x4BE4, // R1.000, x1
}

public enum AID : uint
{
    DeathWall = 47435, // 4BE4->self, no cast, range ?-30 donut
    AutoAttack = 47434, // Boss->player, no cast, single-target
    WindBlade = 47441, // Boss->self, 5.0s cast, range 60 180-degree cone
    SkydiveVisual = 47446, // Boss->location, no cast, single-target
    CyclonicRingVisual = 47447, // Boss->location, no cast, single-target
    Skydive = 47448, // Helper->self, 5.5s cast, range 15 circle
    CyclonicRing = 47449, // Helper->self, 5.5s cast, range 5-60 donut
    PlumefallTrap = 47442, // Boss->self, 3.0s cast, single-target
    Splinter = 47443, // 4BE3->self, 4.5s cast, range 13 circle
    BuffetVisual = 48250, // Helper->self, 4.0s cast, range 60 width 60 rect
    Buffet = 47440, // Helper->self, no cast, ???
    Teleport = 47433, // Boss->location, no cast, single-target
    HurricaneCast = 47436, // Boss->self, 5.0s cast, single-target
    Hurricane = 48120, // Helper->self, no cast, ???
    StrongWind = 47437, // Helper->self, no cast, range 4 circle, Biting Wind puddle
    TendonRipperVisual = 47438, // 4BE2->self, 1.0s cast, single-target
    TendonRipperCast = 47439, // Helper->self, 1.0s cast, range 60 width 8 cross
    AerosnareCast = 47444, // Boss->self, 3.5+0.5s cast, single-target
    Aerosnare = 47445, // Helper->self, 4.0s cast, range 60 60-degree cone
}

public enum IconID : uint
{
    TendonRipper = 506, // 4BE2->self
}

class Skydive(BossModule module) : Components.StandardAOEs(module, AID.Skydive, 15);
class CyclonicRing(BossModule module) : Components.StandardAOEs(module, AID.CyclonicRing, new AOEShapeDonut(5, 60));
class Splinter(BossModule module) : Components.StandardAOEs(module, AID.Splinter, 13);
class WindBlade(BossModule module) : Components.StandardAOEs(module, AID.WindBlade, new AOEShapeCone(60, 90.Degrees()));
class Aerosnare(BossModule module) : Components.StandardAOEs(module, AID.Aerosnare, new AOEShapeCone(60, 30.Degrees()), 3);

class Buffet(BossModule module) : Components.Knockback(module)
{
    readonly List<(Angle Direction, DateTime Activation, bool Imminent)> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var src in _sources)
            if (!IsImmune(slot, src.Activation))
                yield return new(Arena.Center, 24, src.Activation, Direction: src.Direction, Kind: Kind.DirForward);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002 && actor.OID == 0x1EBFA9)
            // delay is usually 11.1 but for some reason there is >4s of variance 
            _sources.Add((actor.Rotation, WorldState.FutureTime(15.1f), false));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_sources.Any(s => s.Imminent))
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in _sources)
            if (!IsImmune(slot, src.Activation))
            {
                var safeCenter = Arena.Center - src.Direction.ToDirection() * 24;
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(safeCenter, Arena.Bounds.Radius), src.Activation);
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Buffet)
            _sources.Clear();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BuffetVisual && _sources.Count > 0)
        {
            _sources.Clear();
            _sources.Add((spell.Rotation, Module.CastFinishAt(spell, 0.6f), true));
        }
    }
}

class Hurricane(BossModule module) : Components.RaidwideCastDelay(module, AID.HurricaneCast, AID.Hurricane, 0.9f);

class BitingWind(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.BitingWind).Where(e => !e.IsDead), 4);

class TendonRipper(BossModule module) : Components.GenericAOEs(module, AID.TendonRipperCast)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var isKB = actor.PendingKnockbacks.Count > 0 || Module.FindComponent<Buffet>()?.Sources(slot, actor).Any() == true;

        foreach (var p in _predicted)
            yield return p with { Risky = !isKB };
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((OID)actor.OID == OID.BitingWind && (IconID)iconID == IconID.TendonRipper)
        {
            var toCenter = Arena.Center - actor.Position;
            var advance = actor.Position.InCircle(Arena.Center, 15) ? 35.Degrees() : 40.Degrees();
            if (actor.Rotation.ToDirection().OrthoL().Dot(toCenter) < 0)
                advance = -advance;

            var srcPredicted = Arena.Center + (-toCenter).Rotate(advance);
            _predicted.Add(new(new AOEShapeCross(60, 4), srcPredicted, default, WorldState.FutureTime(10)));
            _predicted.Add(new(new AOEShapeCross(60, 4), srcPredicted, 45.Degrees(), WorldState.FutureTime(10)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            for (var i = 0; i < _predicted.Count; i++)
                _predicted.Ref(i).Origin = spell.LocXZ;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class AbductorStates : StateMachineBuilder
{
    public AbductorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hurricane>()
            .ActivateOnEnter<Skydive>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<Splinter>()
            .ActivateOnEnter<WindBlade>()
            .ActivateOnEnter<Aerosnare>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<BitingWind>()
            .ActivateOnEnter<TendonRipper>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14505)]
public class Abductor(WorldState ws, Actor primary) : CEModule(ws, primary, new(-150, -860), new ArenaBoundsCircle(24));
