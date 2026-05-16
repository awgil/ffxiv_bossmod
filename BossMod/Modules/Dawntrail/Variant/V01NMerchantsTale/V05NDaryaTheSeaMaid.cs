namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V05NDaryaTheSeaMaid;

public enum OID : uint
{
    Boss = 0x4A8F, // R5.000, x?
    SeabornSteed = 0x4A90, // R2.200, x?
    SeabornStalwart = 0x4A91, // R2.000, x?
    BubbleMarker = 0x1EBF1B,
    TreasureOrb = 0x1EBF1C,
    SirenSphere = 0x4A93,
    TileVoidzone = 0x1EBF1E,
    SeabornSpeaker = 0x4CB5,
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{
    PiercingPlunge = 45803, // 4A8F->self, 5.0s cast, range 70 circle
    FamiliarCall = 45771, // 4A8F->self, 3.0+1.0s cast, single-target
    EchoedSerenade = 45772, // 4A8F->self, 6.5+0.5s cast, range 60 circle
    EchoedSerenade1 = 45773, // 4A8F->self, 8.5+0.5s cast, range 60 circle
    WatersongSteed = 45774, // 4A90->self, 1.0s cast, range 40 width 8 rect
    WatersongStalwart = 45775, // 4A91->self, 1.0s cast, range 40 width 8 rect

    SurgingCurrent = 45791, // 4A8F->self, 4.0+1.0s cast, single-target
    SurgingCurrent1 = 47052, // 233C->self, 5.0s cast, range 60 ?-degree cone
    SwimmingInTheAir = 45776, // 4A8F->self, 4.0s cast, single-target
    Hydrofall = 45777, // 233C->location, 2.0s cast, range 12 circle
    SunkenTreasure = 45778, // 4A8F->self, 3.0+1.0s cast, single-target
    SphereShatter = 45779, // 233C->self, 1.5s cast, range 18 circle
    AquaBall = 45799, // 4A8F->self, 2.0+1.0s cast, single-target
    AquaBallTarget = 45800, // 233C->location, 3.0s cast, range 5 circle
    RecedingTwinTides = 45793, // 4A8F->self, 3.0+1.0s cast, single-target
    NearTide = 45794, // 233C->location, 4.0s cast, range 10 circle
    FarTide = 45798, // 233C->location, no cast, range ?-40 donut

    HydrocannonCast = 45801, // 4A8F->self, 4.0+1.0s cast, single-target
    Hydrocannon = 45802, // 233C->self/player, 5.0s cast, range 70 width 6 rect
    EncroachingTwinTides = 45796, // 4A8F->self, 3.0+1.0s cast, single-target
    FarTide1 = 45797, // 233C->location, 4.0s cast, range 10-40 donut
    NearTide1 = 45795, // 233C->location, no cast, range 10 circle
    CeaselessCurrent = 45788, // 4A8F->self, 4.0+1.0s cast, single-target
    CeaselessCurrent1 = 45789, // 233C->self, 5.0s cast, range 8 width 40 rect
    CeaselessCurrent2 = 45790, // 233C->self, no cast, range 8 width 40 rect

    AquaSpear = 45783, // Boss->self, 4.0s cast, single-target
    AquaSpear1 = 45784, // Helper->self, 3.0s cast, range 8 width 8 rect
    AlluringOrder = 45804, // Boss->self, 4.0s cast, range 70 circle

    BigWaveVisual = 45785, // Boss->self, 4.0+1.0s cast, single-target
    BigWaveKnockback = 45786, // Helper->self, 5.5s cast, range 60 width 60 rect
    AquaBallDelayed = 45787, // Helper->location, 7.5s cast, range 5 circle

}
public enum VFXID : uint
{
    StalwartVFX = 2742,
    SteedVFX = 2741,
}
public enum IconID : uint
{
    Hydrocannon = 471,
}
public enum SID : uint
{
    RightFace = 2164,
    LeftFace = 2163,
    AboutFace = 2162,
    ForwardMarch = 2161,
}
class PiercingPlunge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5f, 20f);
    private static readonly WPos center = new(375f, 530f);
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PiercingPlunge && !_active)
        {
            _aoes.Add(new(rect, new WPos(center.X + 19.9f, center.Z), 90f.Degrees()));
            _aoes.Add(new(rect, new WPos(center.X - 19.9f, center.Z), -90f.Degrees()));
            _aoes.Add(new(rect, new WPos(center.X, center.Z + 19.9f), 0f.Degrees()));
            _aoes.Add(new(rect, new WPos(center.X, center.Z - 19.9f), 180f.Degrees()));
            _active = true;
        }
    }
}
class EchoedSerenade(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _stalwarts = [];
    private readonly List<Actor> _steeds = [];
    private readonly Queue<OrderType> _order = new();
    private OrderType? _currentWave;
    private int _activeWaveCasts;

    private static readonly AOEShapeRect rect = new(40f, 4f);

    private enum OrderType
    {
        Stalwart,
        Steed
    }
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_order.Count > 0)
        {
            var next = _order.Peek();
            var actors = next == OrderType.Stalwart ? _stalwarts : _steeds;
            foreach (var e in actors)
                yield return new AOEInstance(rect, e.Position, e.Rotation) with { Color = ArenaColor.AOE };
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SeabornStalwart)
            _stalwarts.Add(actor);
        if ((OID)actor.OID == OID.SeabornSteed)
            _steeds.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.SeabornStalwart)
            _stalwarts.Remove(actor);
        if ((OID)actor.OID == OID.SeabornSteed)
            _steeds.Remove(actor);
    }
    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if (vfxID == (uint)VFXID.StalwartVFX)
            _order.Enqueue(OrderType.Stalwart);
        if (vfxID == (uint)VFXID.SteedVFX)
            _order.Enqueue(OrderType.Steed);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        OrderType? started = (AID)spell.Action.ID switch
        {
            AID.WatersongStalwart => OrderType.Stalwart,
            AID.WatersongSteed => OrderType.Steed,
            _ => null
        };
        if (started == null)
            return;
        if (_currentWave == null)
        {
            _currentWave = started;
            _activeWaveCasts = 0;
        }
        if (_currentWave == started)
            ++_activeWaveCasts;
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        OrderType? finished = (AID)spell.Action.ID switch
        {
            AID.WatersongStalwart => OrderType.Stalwart,
            AID.WatersongSteed => OrderType.Steed,
            _ => null
        };
        if (finished == null || _currentWave != finished)
            return;
        --_activeWaveCasts;
        if (_activeWaveCasts <= 0)
        {
            if (_order.Count > 0 && _order.Peek() == _currentWave)
                _order.Dequeue();

            _currentWave = null;
            _activeWaveCasts = 0;
        }
    }
}
class SwimmingInTheAir(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _bubbles = [];
    private static readonly AOEShapeCircle circ = new(12f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var e in _bubbles)
        {
            yield return new AOEInstance(circ, e.Position) with { Color = ArenaColor.AOE };
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.BubbleMarker)
        {
            _bubbles.Add(actor);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.BubbleMarker)
        {
            _bubbles.Remove(actor);
        }
    }
}
class SunkenTreasure(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _orbs = [];
    private readonly List<(Actor orb, DateTime activation)> _brokenOrbs = [];
    private static readonly AOEShapeCircle circ = new(18f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_brokenOrbs.Count > 0)
        {
            var active = _brokenOrbs.OrderBy(x => x.activation).Take(3);

            foreach (var e in active)
                yield return new AOEInstance(circ, e.orb.Position, default, e.activation) with { Color = ArenaColor.AOE };
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.TreasureOrb)
            _orbs.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.TreasureOrb)
        {
            _orbs.Remove(actor);
            _brokenOrbs.RemoveAll(x => x.orb == actor);
        }
    }
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID is OID.TreasureOrb)
        {
            if (state is 0x00100020)
            {
                _orbs.Remove(actor);
                if (!_brokenOrbs.Any(x => x.orb == actor))
                    _brokenOrbs.Add((actor, WorldState.CurrentTime.AddSeconds(7.5f)));
            }
            if (state is 0x00040008)
            {
                _brokenOrbs.RemoveAll(x => x.orb == actor);
            }
        }
    }
}
class SurgingCurrent(BossModule module) : Components.StandardAOEs(module, AID.SurgingCurrent, new AOEShapeCone(60f, 90.Degrees()));
class AquaBall(BossModule module) : Components.GroupedAOEs(module, [AID.AquaBallTarget, AID.AquaBallDelayed], new AOEShapeCircle(5f));
class NearFarTide(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle circ = new(10f);
    private readonly AOEShapeDonut donut = new(10f, 40f);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes.Take(1))
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RecedingTwinTides)
        {
            _aoes.Add(new(circ, Module.PrimaryActor.Position, Activation: WorldState.CurrentTime.AddSeconds(4)));
            _aoes.Add(new(donut, Module.PrimaryActor.Position));
        }
        if ((AID)spell.Action.ID is AID.EncroachingTwinTides)
        {
            _aoes.Add(new(donut, Module.PrimaryActor.Position, Activation: WorldState.CurrentTime.AddSeconds(4)));
            _aoes.Add(new(circ, Module.PrimaryActor.Position));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NearTide or AID.NearTide1 or AID.FarTide or AID.FarTide1)
        {
            _aoes.RemoveAt(0);
        }
    }
}
class SirenSphere : Components.PersistentVoidzone
{
    private readonly List<Actor> _spheres = [];

    public SirenSphere(BossModule module) : base(module, 2.5f, _ => [], 15)
    {
        Sources = _ => _spheres;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.SirenSphere && !actor.Position.AlmostEqual(Module.Center, 5))
            _spheres.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.SirenSphere)
            _spheres.Remove(actor);
    }
}
class AquaSpear(BossModule module) : Components.StandardAOEs(module, AID.AquaSpear1, new AOEShapeRect(8f, 4f));
class AquaSpearVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _tiles = [];
    private readonly AOEShapeRect rect = new(4f, 4f, 4f);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_tiles.Count > 0)
        {
            foreach (var e in _tiles)
            {
                yield return new AOEInstance(rect, e.Position) with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.TileVoidzone)
        {
            _tiles.Add(actor);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.TileVoidzone)
        {
            _tiles.Remove(actor);
        }
    }
}
class AlluringOrder(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public List<(Angle dir, float duration, DateTime activation)> PendingMoves = [];
    private static readonly Func<WPos, bool> arenaBounds = ShapeContains.InvertedRect(new(355f, 530f), new(395f, 530f), 20f);

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (Module.FindComponent<AquaSpearVoidzone>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false)
            return true;
        if (!Module.InBounds(pos))
            return true;
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        bool marchUnsafe(WPos playerPos)
        {
            var pos = playerPos;

            foreach (var s in PendingMoves)
            {
                pos += 24 * s.dir.ToDirection();

                if (!arenaBounds(pos))
                    return true;
            }
            return false;
        }
        hints.AddForbiddenZone(marchUnsafe);
        base.AddAIHints(slot, actor, assignment, hints);
    }
}
class BigWave(BossModule module) : Components.Knockback(module, AID.BigWaveKnockback)
{
    private readonly List<Actor> _sources = [];
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_sources.Count >= 1)
        {
            foreach (var s in _sources)
                yield return new Source(s.Position, 32, _activation, Direction: s.Rotation, Kind: Kind.DirForward);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sources.Count >= 1)
        {
            var arenaBounds = ShapeContains.InvertedRect(new(355f, 530f), new(395f, 530f), 20f);

            bool kbSafe(WPos playerPos)
            {
                foreach (var source in _sources)
                {
                    var expected = playerPos + 32 * source.Rotation.ToDirection();
                    if (!arenaBounds(expected))
                        return false;
                }

                return true;
            }

            hints.AddForbiddenZone(kbSafe, _activation);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SeabornSpeaker && !_sources.Contains(actor))
            _sources.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _activation = default;
            _sources.Clear();
        }
    }
}
class Hydrocannon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 3f), (uint)IconID.Hydrocannon, AID.Hydrocannon, damageType: AIHints.PredictedDamageType.Tankbuster);
class CeaselessCurrent(BossModule module) : Components.Exaflare(module, new AOEShapeRect(4f, 40f, 4f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CeaselessCurrent1)
        {
            Lines.Add(new() { Next = caster.Position, Rotation = caster.Rotation, Advance = 8f * (spell.Rotation).ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = 5, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CeaselessCurrent1 or AID.CeaselessCurrent2)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class V05NDaryaTheSeaMaidStates : StateMachineBuilder
{
    public V05NDaryaTheSeaMaidStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PiercingPlunge>()
            .ActivateOnEnter<EchoedSerenade>()
            .ActivateOnEnter<SwimmingInTheAir>()
            .ActivateOnEnter<SurgingCurrent>()
            .ActivateOnEnter<SunkenTreasure>()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<NearFarTide>()
            .ActivateOnEnter<SirenSphere>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<AquaSpear>()
            .ActivateOnEnter<AquaSpearVoidzone>()
            .ActivateOnEnter<AlluringOrder>()
            .ActivateOnEnter<BigWave>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1066, NameID = 14291)]
public class V05NDaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375f, 530f), new ArenaBoundsSquare(20f));
