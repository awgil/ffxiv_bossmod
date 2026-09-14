namespace BossMod.Endwalker.Variant.V03Aloalo.V012Ketuduke;

public enum OID : uint
{
    Boss = 0x4091, // R8.000, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    KnockbackHelper = 0x40B2, // R0.500, x?
    SpringCrystalSphere = 0x4092, // R4.200, x?, polyhedron crystals (point-blank Saturate)
    SpringCrystalRect = 0x4093, // R4.200, x?, flat crystals (line Saturate)
    AiryBubble = 0x4095, // R1.300, x?, Blowing Bubbles
    ZealBlindZozone = 0x4096, // R0.500, x?, route 1/2 NPC
    SummonedApa = 0x4113, // R2.880, x?, Water III / Wavefoam
    SummonedAnila = 0x4097, // R2.400, x?, route 2 Fluke Typhoon
    WavefoamBubble = 0x4094, // R1.200, x?, route 2 stand-in bubbles
    BubbleStrewer = 0x1EB936, // R0.500, EventObj type, Strewn Bubbles lines
}

public enum AID : uint
{
    AutoAttack = 35448, // Boss->player, no cast, single-target
    Teleport = 35447, // Boss->location, no cast, single-target

    TidalRoar = 35493, // Boss->self, 5.0s cast, single-target, visual
    TidalRoarAOE = 35494, // Helper->self, no cast, range 100 circle, raidwide

    SpringCrystals = 35449, // Boss->self, 2.2+0.8s cast, single-target, visual
    SpringCrystalSphereAppear = 35450, // SpringCrystalSphere->self, no cast, single-target, visual
    SaturateSphere = 35452, // SpringCrystalSphere->self, 3.0s cast, range 8 circle
    SaturateSphereFast = 35453, // SpringCrystalSphere->self, 1.0s cast, range 8 circle
    SpringCrystalRectAppear = 35451, // SpringCrystalRect->self, no cast, single-target, visual
    SaturateRect = 35454, // SpringCrystalRect->self, 3.0s cast, range 76 width 10 rect
    SaturateRectFast = 35455, // SpringCrystalRect->self, 1.0s cast, range 76 width 10 rect

    BubbleNet = 35456, // Boss->self, 4.1+0.9s cast, single-target, visual
    BubbleNetAOE = 35457, // Helper->self, 5.0s cast, range 65 circle, raidwide

    FlukeTyphoon = 35460, // Boss->self, 3.0s cast, single-target, visual (move bubbled crystal)
    FlukeTyphoonAOE = 35461, // Helper->self, 5.0s cast, range 40 width 40 rect

    StrewnBubbles = 35462, // Boss->self, 2.2+0.8s cast, single-target, visual
    SphereShatter = 35463, // Helper->self, no cast, range 20 width 10 rect

    RecedingTwintides = 35485, // Boss->self, 5.0s cast, range 14 circle
    FarTide = 35488, // Boss->self, 1.5s cast, range 8-60 donut
    EncroachingTwintides = 35487, // Boss->self, 5.0s cast, range 8-60 donut
    NearTide = 35486, // Boss->self, 1.5s cast, range 14 circle

    Hydrobomb = 35489, // Boss->self, 2.2+0.8s cast, single-target, visual
    HydrobombAOE = 35490, // Helper->location, 3.0s cast, range 5 circle

    BlowingBubbles = 35464, // Boss->self, 2.2+0.8s cast, single-target, visual

    Hydroblast = 35491, // Boss->player, 5.0s cast, single-target, tankbuster
    HydroblastAOE = 35492, // Helper->player, no cast, range 5 circle

    Summon = 35470, // ZealBlindZozone->self, 3.0s cast, single-target
    WaterIII = 36116, // SummonedApa->self, 10.0s cast, single-target, proximity
    WaterIIIApply = 36125, // SummonedApa->player, no cast, single-target

    TidalWave = 36113, // Boss->self, 2.2+0.8s cast, single-target, visual
    TidalWaveAOE = 36114, // Helper->self, 6.0s cast, range 46 width 46 rect, knockback
    Hydrosurge = 35467, // Boss->self, 5.2+0.8s cast, single-target, visual
    HydrosurgeAOE = 35468, // Helper->self, 6.0s cast, single-target, starts persistent mid-line
    Wavefoam = 36115, // SummonedApa->self, 3.0s cast, single-target, spawn WavefoamBubble
    FlukeTyphoonAnila = 35471, // SummonedAnila->self, 8.0s cast, range 40 width 40 rect, knock bubbled players across
}

public enum SID : uint
{
    Bubble = 3745, // on bubbled spring crystals (extra=0xC8)
}

public enum TetherID : uint
{
    WaterIIIGood = 1, // Apa->player, stretched
    WaterIIIStretch = 57, // Apa->player, too close
}

class TidalRoar(BossModule module) : Components.RaidwideCastDelay(module, AID.TidalRoar, AID.TidalRoarAOE, 1);
class BubbleNet(BossModule module) : Components.RaidwideCast(module, AID.BubbleNetAOE);
class Hydrobomb(BossModule module) : Components.StandardAOEs(module, AID.HydrobombAOE, 5);
class Hydroblast(BossModule module) : Components.SingleTargetCast(module, AID.Hydroblast);

class WaterIII(BossModule module) : Components.GenericAOEs(module, AID.WaterIII)
{
    private readonly List<Actor> _casters = [];
    private readonly List<(Actor Apa, Actor Target, bool Stretched)> _tethers = [];

    private const float MinStretch = 20;
    private static readonly AOEShapeCircle _tooClose = new(MinStretch);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(_tooClose, c.Position, default, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.RemoveAll(c => c == caster);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is not (TetherID.WaterIIIGood or TetherID.WaterIIIStretch))
            return;
        if ((OID)source.OID != OID.SummonedApa || WorldState.Actors.Find(tether.Target) is not { } target)
            return;

        _tethers.RemoveAll(t => t.Apa == source || t.Target == target);
        _tethers.Add((source, target, (TetherID)tether.ID == TetherID.WaterIIIGood));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.SummonedApa)
            _tethers.RemoveAll(t => t.Apa == source);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var t in _tethers.Where(t => t.Target == actor))
        {
            if (!t.Stretched)
                hints.Add("Stretch tether!");
            else
                hints.Add("Tether is stretched!", false);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var t in _tethers.Where(t => t.Target == actor && !t.Stretched))
        {
            var activation = t.Apa.CastInfo != null ? Module.CastFinishAt(t.Apa.CastInfo) : default;
            hints.AddForbiddenZone(_tooClose.Distance(t.Apa.Position, default), activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
        {
            var color = t.Stretched ? ArenaColor.Safe : ArenaColor.Danger;
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(t.Apa.Position, t.Target.Position, 0xFF000000, 2);
            Arena.AddLine(t.Apa.Position, t.Target.Position, color);
        }
    }
}

class SpringCrystals(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(ulong InstanceID, AOEInstance AOE)> _aoes = [];

    private static readonly AOEShapeCircle _sphere = new(8);
    private static readonly AOEShapeRect _rect = new(38, 5, 38);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // make not risky for hints if Hydrosurge is active so AI doesn't try to cross the midline
        var soften = Module.FindComponent<Hydrosurge>()?.Active == true;
        foreach (var a in _aoes)
            yield return soften ? a.AOE with { Color = ArenaColor.AOE, Risky = false } : a.AOE;
    }

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = (OID)actor.OID switch
        {
            OID.SpringCrystalSphere => _sphere,
            OID.SpringCrystalRect => _rect,
            _ => null
        };
        if (shape == null)
            return;

        _aoes.Add((actor.InstanceID, new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(15))));
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID != SID.Bubble)
            return;
        if ((OID)actor.OID is not (OID.SpringCrystalSphere or OID.SpringCrystalRect))
            return;

        var index = _aoes.FindIndex(a => a.InstanceID == actor.InstanceID);
        if (index < 0)
        {
            ReportError($"Failed to find crystal AOE for bubbled {actor}");
            return;
        }

        ref var entry = ref _aoes.Ref(index);
        // bubble crystals move +20 towards center
        var origin = entry.AOE.Origin + new WDir(entry.AOE.Origin.X < Module.Center.X ? 20 : -20, 0);
        entry = (entry.InstanceID, entry.AOE with { Origin = origin });
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is not (AID.SaturateSphere or AID.SaturateSphereFast or AID.SaturateRect or AID.SaturateRectFast))
            return;

        var index = _aoes.FindIndex(a => a.InstanceID == caster.InstanceID);
        if (index < 0)
            return;

        ref var entry = ref _aoes.Ref(index);
        // update from actor position
        entry = (entry.InstanceID, entry.AOE with { Origin = caster.Position, Rotation = spell.Rotation, Activation = Module.CastFinishAt(spell) });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.SaturateSphere or AID.SaturateSphereFast or AID.SaturateRect or AID.SaturateRectFast))
            return;

        _aoes.RemoveAll(a => a.InstanceID == caster.InstanceID);
        ++NumCasts;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.SpringCrystalSphere or OID.SpringCrystalRect)
            _aoes.RemoveAll(a => a.InstanceID == actor.InstanceID);
    }
}

class StrewnBubbles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(20, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(4);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BubbleStrewer)
            _aoes.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(5.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.SphereShatter)
            return;

        var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        if (count != 1)
            ReportError($"{spell.Action} removed {count} aoes at {caster.Position}");
        ++NumCasts;
    }
}

class Twintides(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeOut = new(14);
    private static readonly AOEShapeDonut _shapeIn = new(8, 60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RecedingTwintides:
                _aoes.Add(new(_shapeOut, caster.Position, default, Module.CastFinishAt(spell)));
                _aoes.Add(new(_shapeIn, caster.Position, default, Module.CastFinishAt(spell, 3.6f)));
                break;
            case AID.EncroachingTwintides:
                _aoes.Add(new(_shapeIn, caster.Position, default, Module.CastFinishAt(spell)));
                _aoes.Add(new(_shapeOut, caster.Position, default, Module.CastFinishAt(spell, 3.6f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RecedingTwintides or AID.EncroachingTwintides or AID.FarTide or AID.NearTide)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class BlowingBubbles(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _bubbles = [];

    private static readonly AOEShapeCircle _shape = new(5);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.AiryBubble)
            _bubbles.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.AiryBubble)
            _bubbles.Remove(actor);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var a in _bubbles)
            _shape.Draw(Arena, a);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var a in _bubbles)
            hints.AddForbiddenZone(_shape.Distance(a.Position, a.Rotation));
    }
}

class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWaveAOE, 27, ignoreImmunes: true, shape: new AOEShapeRect(46, 23), kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            if (c.CastInfo == null || IsImmune(slot, Module.CastFinishAt(c.CastInfo)))
                continue;
            // stay near wave origin
            var origin = c.Position;
            var activation = Module.CastFinishAt(c.CastInfo);
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(origin, 8), activation);
            hints.GoalZones.Add(AIHints.GoalProximity(origin, 4, 60));
        }
    }
}

class Hydrosurge(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect _shape = new(20, 5, 20);

    public bool Active => _aoe != null;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is { } aoe)
            yield return aoe;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydrosurgeAOE)
            _aoe = new(_shape, caster.Position, spell.Rotation, WorldState.CurrentTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlukeTyphoonAnila)
            _aoe = null;
    }
}

class WavefoamBubbles(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.WavefoamBubble)
            Towers.Add(new(actor.Position, 2, activation: WorldState.FutureTime(12)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.WavefoamBubble)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlukeTyphoonAnila)
            Towers.Clear();
    }
}

class FlukeTyphoonAnila(BossModule module) : Components.KnockbackFromCastTarget(module, AID.FlukeTyphoonAnila, 20, ignoreImmunes: true, shape: new AOEShapeRect(40, 20), kind: Kind.DirForward);

class V012KetudukeStates : StateMachineBuilder
{
    public V012KetudukeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TidalRoar>()
            .ActivateOnEnter<BubbleNet>()
            .ActivateOnEnter<SpringCrystals>()
            .ActivateOnEnter<StrewnBubbles>()
            .ActivateOnEnter<Hydrobomb>()
            .ActivateOnEnter<Hydroblast>()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<Twintides>()
            .ActivateOnEnter<BlowingBubbles>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<Hydrosurge>()
            .ActivateOnEnter<WavefoamBubbles>()
            .ActivateOnEnter<FlukeTyphoonAnila>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 961, NameID = 12605)]
public class V012Ketuduke(WorldState ws, Actor primary) : BossModule(ws, primary, new(-790, -395), new ArenaBoundsSquare(20));
