namespace BossMod.Dawntrail.Raid.RM11TheTyrant;

public enum OID : uint
{
    Boss = 0x4AE8, // R5.000-10.000, x?
    TheTyrant2 = 0x4AEE, // R3.000-30.000, x?
    Comet = 0x4AE9, // R2.160, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    Maelstrom = 0x4AEB, // R1.000, x?
    Axe = 0x4AF0, // R1.000, x2
    Scythe = 0x4AF1, // R1.000, x2
    Sword = 0x4AF2, // R1.000, x2
}
public enum AID : uint
{
    AutoAttack = 46005, // 4AE8->player, no cast, single-target
    CrownOfArcadia = 46006, // 4AE8->self, 5.0s cast, range 60 circle
    DrawSteelAxe = 46007, // 4AE8->self, 2.0+4.0s cast, single-target
    DrawSteelScythe = 46008, // 4AE8->self, 2.0+4.0s cast, single-target
    DrawSteelSword = 46009, // 4AE8->self, 2.0+4.0s cast, single-target

    SmashdownScytheVisual = 46012, // 4AE8->self, 2.0+1.0s cast, single-target
    SmashdownScythe = 46013, // 233C->self, 3.0s cast, range 5-60 donut
    SmashdownAxeVisual = 46010, // 4AE8->self, 2.0+1.0s cast, single-target
    SmashdownAxe = 46011, // 233C->self, 3.0s cast, range 8 circle
    SmashdownSwordVisual = 46014, // 4AE8->self, 2.0+1.0s cast, single-target
    SmashdownSword = 46015, // 233C->self, 3.0s cast, range 40 width 10 cross

    VoidStardust = 46024, // 4AE8->self, 5.0s cast, single-target
    Comet = 46025, // 233C->player, 5.0s cast, range 4 circle
    Comet1 = 46027, // 233C->players, no cast, range 4 circle
    Cometite = 46026, // 233C->location, 2.5s cast, range 4 circle

    TrophyWeapons = 46028, // 4AE8->self, 3.0s cast, single-target
    AssaultEvolvedCast = 46029, // 4AE8->self, 5.0s cast, single-target
    AssaultEvolvedSwordVisual = 46402, // 4AE8->location, no cast, single-target
    AssaultEvolvedSword = 46032, // 233C->self, 2.0s cast, range 40 width 10 cross
    AssaultEvolvedScytheVisual = 46401, // 4AE8->location, no cast, single-target
    AssaultEvolvedScythe = 46031, // 233C->self, 2.0s cast, range 5-60 donut
    AssaultEvolvedAxeVisual = 46400, // 4AE8->location, no cast, single-target
    AssaultEvolvedAxe = 46030, // 233C->self, 2.0s cast, range 8 circle

    DanceOfDominationTrophy = 47034, // 4AE8->self, 2.0+4.0s cast, single-target
    DanceOfDomination = 46033, // 4AE8->self, 4.5+0.7s cast, single-target
    DanceOfDomination1 = 46034, // 233C->self, 5.0s cast, range 60 circle
    DanceOfDomination2 = 46036, // 233C->self, no cast, range 60 circle
    DanceOfDomination3 = 47081, // 233C->self, no cast, range 60 circle
    Explosion1 = 46035, // 233C->self, 6.0s cast, range 60 width 10 rect
    Explosion2 = 47033, // 233C->self, 6.5s cast, range 60 width 10 rect
    Explosion3 = 46077, // 233C->self, 3.0s cast, range 60 width 6 rect

    RawSteelTrophy = 46037, // 4AE8->self, 2.0+4.0s cast, single-target
    RawSteel = 46016, // 4AE8->self, 5.0+1.0s cast, single-target
    RawSteel1 = 46017, // 4AE8->players, no cast, range 6 circle
    Impact = 46018, // 233C->player, 6.5s cast, range 6 circle

    Charybdistopia = 46039, // 4AE8->self, 5.0s cast, range 60 circle
    UltimateTrophyWeapons = 47083, // 4AE8->self, 3.0s cast, single-target
    AssaultApex = 47084, // 4AE8->self, 5.0s cast, single-target

    Charybdis = 46040, // 233C->player, no cast, single-target
    PowerfulGust = 46041, // 233C->self, 6.0s cast, range 60 45-degree cone
    ImmortalReign = 46042, // 4AE8->self, 3.0+1.0s cast, single-target
    OneAndOnly = 46043, // 4AE8->self, 6.0+2.0s cast, single-target
    OneAndOnly1 = 46044, // 233C->self, 9.0s cast, range 60 circle

    Meteorain = 46045, // 4AE8->self, 5.0s cast, single-target
    CosmicKiss = 46046, // 4AE9->self, 8.0s cast, range 4 circle
    ForegoneFatality = 46048, // 4AEE->player, no cast, single-target
    MassiveMeteor = 46051, // 233C->players, 6.0s cast, range 6 circle
    DoubleTyrannhilation = 46052, // 4AE8->self, no cast, single-target
    DoubleTyrannhilation1 = 46053, // 4AE8->self, 8.0+1.0s cast, range 30 circle
    HiddenTyrannhilation = 46215,

    Flatliner = 46056, // 4AE8->self, 4.0+2.0s cast, single-target
    FlatlinerKnockUp = 47759, // 233C->self, 6.0s cast, range 60 circle
    MajesticMeteor = 46057, // 4AE8->self, 4.0s cast, single-target
    MajesticMeteor1 = 46058, // 233C->location, 4.0s cast, range 6 circle
    MajesticMeteorain = 46059, // 233C->self, 6.0s cast, range 60 width 10 rect

    MammothMeteor = 46060, // 233C->location, 6.5s cast, range 60 circle
    FireAndFuryVisual = 46071, // 4AE8->self, 5.0+1.0s cast, single-target
    FireAndFuryCone1 = 46073, // 233C->self, 6.0s cast, range 60 90-degree cone
    FireAndFuryCone2 = 46072, // 233C->self, 6.0s cast, range 60 90-degree cone
    ExplosionKnockUp = 46061, // 233C->self, 10.0s cast, range 4 circle
    ArcadionAvalancheBoss1 = 46063, // 4AE8->self, 8.0+9.5s cast, single-target
    ArcadionAvalancheWest1 = 46064, // 233C->self, 17.5s cast, range 40 width 40 rect, west platform goes
    ArcadionAvalancheBoss2 = 46065, // 4AE8->self, 8.0+9.5s cast, single-target
    ArcadionAvalancheWest2 = 46066, // 233C->self, 17.5s cast, range 40 width 40 rect, west platform goes
    ArcadionAvalancheBoss3 = 46067, // 4AE8->self, 8.0+9.5s cast, single-target
    ArcadionAvalancheEast1 = 46068, // 233C->self, 17.5s cast, range 40 width 40 rect, east platform goes
    ArcadionAvalancheBoss4 = 46069, // 4AE8->self, 8.0+9.5s cast, single-target
    ArcadionAvalancheEast2 = 46070, // 233C->self, 17.5s cast, range 40 width 40 rect, east platform goes

    HeartbreakKick = 46079, // 4AE8->self, 5.0+1.0s cast, single-target
    HeartbreakKick1 = 46080, // 233C->self, no cast, range 4 circle
    GreatWallOfFire = 46074, // 4AE8->self, 5.0s cast, single-target
    GreatWallOfFire1 = 46075, // 4AE8->self, no cast, range 60 width 6 rect
    GreatWallOfFire2 = 46076, // 233C->self, no cast, single-target
}
public enum IconID : uint
{
    VoidStardustSpread = 630, // player->self
    RawSteelSpread = 311, // player->self
    RawSteelSharedTankbuster = 600, // player->self
    MassiveMeteorStack = 318, // player->self
    WallOfFireTankbuster = 598, // player->self
}
public enum TetherID : uint
{
    TankInterceptTether = 356, // TheTyrant2->Comet/player
}
class CrownOfArcadia(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 6, 20);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var e in _aoes)
            yield return e with { Color = ArenaColor.AOE };
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CrownOfArcadia)
        {
            _aoes.Add(new(rect, new WPos(74, 100)));
            _aoes.Add(new(rect, new WPos(126, 100)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CrownOfArcadia)
        {
            _aoes.Clear();
        }
    }
}
abstract class Weapons
{
    public static readonly AOEShape Axe = new AOEShapeCircle(8);
    public static readonly AOEShape Scythe = new AOEShapeDonut(5, 60);
    public static readonly AOEShape Sword = new AOEShapeCross(40, 5);

    public static AOEShape? Shape(Actor weapon) => (OID)weapon.OID switch
    {
        OID.Axe => Axe,
        OID.Scythe => Scythe,
        OID.Sword => Sword,
        _ => null
    };
}

class DrawSteel(BossModule module) : Components.GenericAOEs(module)
{
    private AOEShape? _shape;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_shape != null)
            yield return new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = (AID)spell.Action.ID switch
        {
            AID.DrawSteelAxe => Weapons.Axe,
            AID.DrawSteelScythe => Weapons.Scythe,
            AID.DrawSteelSword => Weapons.Sword,
            _ => null
        };
        if (shape != null)
        {
            _shape = shape;
            _activation = Module.CastFinishAt(spell, 9.4f);
        }
        else if ((AID)spell.Action.ID is AID.SmashdownAxe or AID.SmashdownScythe or AID.SmashdownSword)
        {
            _shape = null;
        }
    }
}

class TrophyWeapons(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _weapons = [];
    private readonly List<Actor> _unordered = [];
    private readonly Dictionary<ulong, ulong> _chain = [];
    private ulong _first;
    private bool _apex;
    private float _interval;
    private DateTime _nextHit;

    public bool Pending => _weapons.Count > 0 || _unordered.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < _weapons.Count && i < 2; ++i)
        {
            var w = _weapons[i];
            if (Weapons.Shape(w) is { } shape)
            {
                var activation = _nextHit.AddSeconds(i * _interval);
                yield return i == 0 ? new(shape, w.Position, w.Rotation, activation, ArenaColor.Danger) : new(shape, w.Position, w.Rotation, activation, Risky: false);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TrophyWeapons:
                Reset(false, 4.2f);
                break;
            case AID.UltimateTrophyWeapons:
                Reset(true, 3.65f);
                break;
            case AID.AssaultEvolvedCast:
            case AID.AssaultApex:
                _nextHit = Module.CastFinishAt(spell, 2.2f);
                break;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id is not (0x11D1 or 0x11D2 or 0x11D3) || Weapons.Shape(actor) == null)
            return;
        if (_apex)
        {
            _weapons.Add(actor);
            if (_weapons.Count == 1 && _nextHit == default)
                _nextHit = WorldState.FutureTime(8.9f);
        }
        else
        {
            _unordered.Add(actor);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != 249 || _apex)
            return;
        if (source == Module.PrimaryActor)
            _first = tether.Target;
        else
            _chain[source.InstanceID] = tether.Target;

        List<Actor> order = [];
        for (var id = _first; id != 0 && order.Count < _unordered.Count; id = _chain.GetValueOrDefault(id))
        {
            if (_unordered.FirstOrDefault(w => w.InstanceID == id) is not { } w)
                break;
            order.Add(w);
        }
        if (order.Count == _unordered.Count && order.Count > 0)
        {
            _weapons.AddRange(order);
            _unordered.Clear();
            if (_nextHit == default)
                _nextHit = WorldState.FutureTime(7.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AssaultEvolvedSword or AID.AssaultEvolvedScythe or AID.AssaultEvolvedAxe)
        {
            ++NumCasts;
            if (_weapons.Count > 0)
                _weapons.RemoveAt(0);
            _nextHit = WorldState.FutureTime(_interval);
        }
    }

    private void Reset(bool apex, float interval)
    {
        _weapons.Clear();
        _unordered.Clear();
        _chain.Clear();
        _first = 0;
        _apex = apex;
        _interval = interval;
        _nextHit = default;
    }
}

class Smashdown1(BossModule module) : Components.StandardAOEs(module, AID.SmashdownScythe, Weapons.Scythe);
class Smashdown2(BossModule module) : Components.StandardAOEs(module, AID.SmashdownAxe, Weapons.Axe);
class Smashdown3(BossModule module) : Components.StandardAOEs(module, AID.SmashdownSword, Weapons.Sword);
class VoidStardust(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VoidStardustSpread, AID.Comet, 4f, 4.7f, true)
{
    private readonly Dictionary<ulong, int> _hits = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.Comet or AID.Comet1))
            return;
        var target = spell.MainTargetID;
        var hits = _hits[target] = _hits.GetValueOrDefault(target) + 1;
        for (var i = Spreads.Count - 1; i >= 0; --i)
        {
            if (Spreads[i].Target.InstanceID != target)
                continue;
            if (hits >= 3)
            {
                Spreads.RemoveAt(i);
                ++NumFinishedSpreads;
                _hits.Remove(target);
            }
            else
            {
                Spreads[i] = Spreads[i] with { Activation = WorldState.FutureTime(2) };
            }
        }
    }
}
class Cometite(BossModule module) : Components.StandardAOEs(module, AID.Cometite, 4f);
class DanceOfDomination(BossModule module) : Components.RaidwideCast(module, AID.DanceOfDomination);
class Explosion1(BossModule module) : Components.StandardAOEs(module, AID.Explosion1, new AOEShapeRect(60f, 5f));
class Explosion2(BossModule module) : Components.StandardAOEs(module, AID.Explosion2, new AOEShapeRect(60f, 5f));
class Explosion3(BossModule module) : Components.StandardAOEs(module, AID.Explosion3, new AOEShapeRect(60f, 3f));
class RawSteelTankBuster(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.RawSteelSharedTankbuster, AID.RawSteel1, 6f, 5.4f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Target == actor && Raid.WithoutSlot().Exclude(actor).Where(p => p.Role == Role.Tank).MinBy(p => (p.Position - actor.Position).LengthSq()) is { } partner)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(partner.Position, 3), Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Target != null)
            Shape.Outline(Arena, Target, pc.Role == Role.Tank ? ArenaColor.Safe : 0);
    }
}
class RawSteelSpreads(BossModule module) : Components.SpreadFromCastTargets(module, AID.Impact, 6);
class Charybdistopia(BossModule module) : Components.RaidwideCast(module, AID.Charybdistopia);
class Maelstrom(BossModule module) : Components.Voidzone(module, 5, OID.Maelstrom);
class PowerfulGust(BossModule module) : Components.GenericAOEs(module, AID.PowerfulGust)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;
        var risky = Module.FindComponent<TrophyWeapons>()?.Pending != true;
        var deadline = _aoes[0].Activation.AddSeconds(1);
        foreach (var aoe in _aoes)
            if (aoe.Activation < deadline)
                yield return aoe with { Risky = risky };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(a => a.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            var index = _aoes.FindIndex(a => a.Origin.AlmostEqual(spell.LocXZ, 0.5f) && a.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }
}
class OneAndOnly(BossModule module) : Components.RaidwideCast(module, AID.OneAndOnly);
class CosmicKiss(BossModule module) : Components.CastTowers(module, AID.CosmicKiss, 4f);
class MassiveMeteor(BossModule module) : Components.StackWithCastTargets(module, AID.MassiveMeteor, 6, 4)
{
    private BitMask _tethered;

    public override void Update()
    {
        var forbidden = _tethered;
        if (Module.FindComponent<ForegoneFatality>()?.Pending == true)
            forbidden |= Raid.WithSlot().WhereActor(p => p.Role == Role.Tank).Mask();
        for (var i = 0; i < Stacks.Count; ++i)
            Stacks.Ref(i).ForbiddenPlayers = forbidden;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.TankInterceptTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            _tethered.Set(slot);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.TankInterceptTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            _tethered.Clear(slot);
    }
}
class ForegoneFatality(BossModule module) : Components.TankbusterTether(module, AID.ForegoneFatality, (uint)TetherID.TankInterceptTether, 6f)
{
    private readonly List<(Actor Source, Actor Target)> _cometTethers = [];
    private DateTime _resolve;

    public bool Pending => _resolve != default;

    private const float GrabHalfWidth = 1;
    private static WPos GrabEnd(Actor source, Actor holder)
    {
        var dir = holder.Position - source.Position;
        var len = dir.Length();
        var back = holder.Type == ActorType.Player ? 3 : 5;
        return source.Position + dir * (MathF.Max(len - back, 1) / MathF.Max(len, 0.001f));
    }
    private const float AvoidHalfWidth = 2.5f;

    private IEnumerable<(Actor Source, Actor Holder)> AllTethers => _cometTethers.Concat(Tethers.Select(t => (t.Enemy, t.Player)));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_resolve == default)
            return;
        var tethers = AllTethers.ToList();
        if (tethers.Count == 0)
            return;

        var holding = TetheredPlayers[slot];
        var grab = actor.Role == Role.Tank && !holding ? PickTether(actor, tethers) : null;

        foreach (var (source, holder) in tethers)
            if (holder != actor && (grab == null || grab.Value.Source != source))
                hints.AddForbiddenZone(ShapeDistance.Rect(source.Position, holder.Position, AvoidHalfWidth), _resolve);

        if (grab is var (grabSource, grabHolder))
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(grabSource.Position, GrabEnd(grabSource, grabHolder), GrabHalfWidth), WorldState.FutureTime(1));

        foreach (var (_, holder) in tethers)
            if (holder.Type == ActorType.Player && holder != actor)
                hints.AddForbiddenZone(ShapeDistance.Circle(holder.Position, Radius), _resolve);
        if (holding)
            foreach (var p in Raid.WithoutSlot().Exclude(actor))
                hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, Radius), _resolve);
    }

    private (Actor Source, Actor Holder)? PickTether(Actor tank, List<(Actor Source, Actor Holder)> tethers)
    {
        var needing = tethers.Where(t => t.Holder.Role != Role.Tank).OrderBy(t => t.Holder.Type == ActorType.Player).ToList();
        if (needing.Count == 0)
            return null;
        if (needing.Any(t => t.Holder.Type != ActorType.Player))
            needing.RemoveAll(t => t.Holder.Type == ActorType.Player);
        var otherTank = Raid.WithSlot().Exclude(tank).Where(p => p.Item2.Role == Role.Tank && !p.Item2.IsDead && !TetheredPlayers[p.Item1]).Select(p => p.Item2).FirstOrDefault();
        if (otherTank != null && needing.Count >= 2)
        {
            static float dist(Actor p, (Actor Source, Actor Holder) t) => SegmentDistance(p.Position, t.Source.Position, GrabEnd(t.Source, t.Holder));
            var keep = dist(tank, needing[0]) + dist(otherTank, needing[1]);
            var swap = dist(tank, needing[1]) + dist(otherTank, needing[0]);
            return keep <= swap ? needing[0] : needing[1];
        }
        return needing.MinBy(t => SegmentDistance(tank.Position, t.Source.Position, GrabEnd(t.Source, t.Holder)));
    }

    private static float SegmentDistance(WPos p, WPos a, WPos b)
    {
        var ab = b - a;
        var t = Math.Clamp((p - a).Dot(ab) / MathF.Max(ab.LengthSq(), 0.001f), 0, 1);
        return (p - (a + ab * t)).Length();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            _resolve = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (!Active && _cometTethers.Count > 0 && actor.Role == Role.Tank)
            hints.Add("Grab the tether!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var (source, target) in _cometTethers)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(source.Position, target.Position, 0xFF000000, 2);
            Arena.AddLine(source.Position, target.Position, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == TID && (OID)source.OID == OID.TheTyrant2 && WorldState.Actors.Find(tether.Target) is { OID: (uint)OID.Comet } target)
        {
            _cometTethers.Add((source, target));
            if (_resolve == default)
                _resolve = WorldState.FutureTime(8.2f);
        }
        else
            base.OnTethered(source, tether);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var targetID = tether.Target;
        if (tether.ID != TID || _cometTethers.RemoveAll(t => t.Source == source && t.Target.InstanceID == targetID) == 0)
            base.OnUntethered(source, tether);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _cometTethers.RemoveAll(t => t.Source == actor || t.Target == actor);
    }
}
abstract class Tyrannhilation(BossModule module, Enum aid) : Components.CastLineOfSightAOE(module, aid, 60f, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Comet).Where(a => !a.Position.InCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + a.HitboxRadius + 9));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Comet)
            Refresh();
    }
}
class DoubleTyrannhilation(BossModule module) : Tyrannhilation(module, AID.DoubleTyrannhilation1);
class HiddenTyrannhilation(BossModule module) : Tyrannhilation(module, AID.HiddenTyrannhilation);
class Platforms(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _split;

    public static readonly AOEShapeRect Strip = new(20, 6, 20);
    public static readonly AOEShapeRect Platform = new(20, 10, 20);
    public static readonly WPos West = new(84, 100);
    public static readonly WPos East = new(116, 100);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_split != default)
            yield return new(Strip, Arena.Center, default, _split, Risky: false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlatlinerKnockUp)
        {
            _split = Module.CastFinishAt(spell);
            Arena.Bounds = new ArenaBoundsRect(26, 20);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlatlinerKnockUp:
                _split = default;
                var plat = CurveApprox.Rect(new WDir(0, 20), new WDir(10, 0));
                var clipper = Arena.Bounds.Clipper;
                var west = new RelSimplifiedComplexPolygon(plat.Select(p => p + (West - Arena.Center)));
                var east = new RelSimplifiedComplexPolygon(plat.Select(p => p + (East - Arena.Center)));
                Arena.Bounds = new ArenaBoundsCustom(26, clipper.Union(new(west), new(east)));
                break;
            case AID.CrownOfArcadia:
                Arena.Bounds = new ArenaBoundsSquare(20);
                Arena.Center = new(100, 100);
                break;
        }
    }
}

class FlatlinerKnockUp(BossModule module) : Components.KnockbackFromCastTarget(module, AID.FlatlinerKnockUp, 15, true)
{
    public override PointTest DestinationUnsafe(int slot, Actor actor) => p => p.InRect(Arena.Center, new WDir(0, 1), 20, 20, 6);
}

class MajesticMeteor(BossModule module) : Components.StandardAOEs(module, AID.MajesticMeteor1, 6f);
class MajesticMeteorain(BossModule module) : Components.StandardAOEs(module, AID.MajesticMeteorain, new AOEShapeRect(60f, 5f));
class MammothMeteor(BossModule module) : Components.StandardAOEs(module, AID.MammothMeteor, new AOEShapeCircle(22));
class FireAndFury1(BossModule module) : Components.StandardAOEs(module, AID.FireAndFuryCone1, new AOEShapeCone(60f, 45.Degrees()));
class FireAndFury2(BossModule module) : Components.StandardAOEs(module, AID.FireAndFuryCone2, new AOEShapeCone(60f, 45.Degrees()));
class ExplosionTower(BossModule module) : Components.CastTowers(module, AID.ExplosionKnockUp, 4f, 1, 8)
{
    public override void Update()
    {
        for (var i = 0; i < Towers.Count; ++i)
        {
            ref var t = ref Towers.Ref(i);
            var west = t.Position.X < 100;
            t.ForbiddenSoakers = Raid.WithSlot().WhereActor(p => p.Position.X < 100 != west).Mask();
        }
    }
}
class ExplosionKnockUp(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ExplosionKnockUp, 20, true, shape: new AOEShapeCircle(4));

class ArcadionAvalancheSlab(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(40, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Where(a => a.Activation <= WorldState.FutureTime(6));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcadionAvalancheWest1 or AID.ArcadionAvalancheWest2 or AID.ArcadionAvalancheEast1 or AID.ArcadionAvalancheEast2)
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcadionAvalancheWest1 or AID.ArcadionAvalancheWest2 or AID.ArcadionAvalancheEast1 or AID.ArcadionAvalancheEast2)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}

class ArcadionAvalanche(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private WPos _survivor;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe is { } doom && doom.Check(actor.Position) && Module.FindComponent<ExplosionTower>() is { } towers)
        {
            foreach (var t in towers.Towers)
            {
                if (t.Activation < doom.Activation && doom.Check(t.Position))
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(t.Position, t.Radius - 0.5f), t.Activation);
                    return;
                }
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcadionAvalancheWest1 or AID.ArcadionAvalancheWest2 or AID.ArcadionAvalancheEast1 or AID.ArcadionAvalancheEast2)
        {
            var westGoes = spell.LocXZ.X < 100;
            _survivor = westGoes ? Platforms.East : Platforms.West;
            _aoe = new(Platforms.Platform, westGoes ? Platforms.West : Platforms.East, default, Module.CastFinishAt(spell, -6.3f));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcadionAvalancheWest1 or AID.ArcadionAvalancheWest2 or AID.ArcadionAvalancheEast1 or AID.ArcadionAvalancheEast2)
        {
            ++NumCasts;
            _aoe = null;
            Arena.Bounds = new ArenaBoundsRect(10, 20);
            Arena.Center = _survivor;
        }
    }
}

class HeartbreakKick(BossModule module) : Components.GenericTowers(module, AID.HeartbreakKick1, AIHints.PredictedDamageType.Shared)
{
    private int _hits;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeartbreakKick)
        {
            _hits = 0;
            Towers.Clear();
            Towers.Add(new(caster.Position, 4, 8, 8, activation: Module.CastFinishAt(spell, 1.4f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && Towers.Count > 0)
        {
            ++NumCasts;
            if (++_hits >= 5)
                Towers.Clear();
            else
                Towers.Ref(0).Activation = WorldState.FutureTime(2);
        }
    }
}
class GreatWallOfFire(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.WallOfFireTankbuster, AID.GreatWallOfFire, new AOEShapeRect(60, 3f));
class RM11TheTyrantStates : StateMachineBuilder
{
    public RM11TheTyrantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrownOfArcadia>()
            .ActivateOnEnter<Smashdown1>()
            .ActivateOnEnter<Smashdown2>()
            .ActivateOnEnter<Smashdown3>()
            .ActivateOnEnter<VoidStardust>()
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<DrawSteel>()
            .ActivateOnEnter<TrophyWeapons>()
            .ActivateOnEnter<DanceOfDomination>()
            .ActivateOnEnter<Explosion1>()
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<Explosion3>()
            .ActivateOnEnter<RawSteelTankBuster>()
            .ActivateOnEnter<RawSteelSpreads>()
            .ActivateOnEnter<Charybdistopia>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<PowerfulGust>()
            .ActivateOnEnter<OneAndOnly>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<MassiveMeteor>()
            .ActivateOnEnter<ForegoneFatality>()
            .ActivateOnEnter<DoubleTyrannhilation>()
            .ActivateOnEnter<HiddenTyrannhilation>()
            .ActivateOnEnter<Platforms>()
            .ActivateOnEnter<FlatlinerKnockUp>()
            .ActivateOnEnter<MajesticMeteor>()
            .ActivateOnEnter<MajesticMeteorain>()
            .ActivateOnEnter<FireAndFury1>()
            .ActivateOnEnter<FireAndFury2>()
            .ActivateOnEnter<MammothMeteor>()
            .ActivateOnEnter<ExplosionTower>()
            .ActivateOnEnter<ExplosionKnockUp>()
            .ActivateOnEnter<ArcadionAvalanche>()
            .ActivateOnEnter<ArcadionAvalancheSlab>()
            .ActivateOnEnter<HeartbreakKick>()
            .ActivateOnEnter<GreatWallOfFire>();
    }
}
[ModuleInfo(Contributors = "VeraNala, Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1072, NameID = 14305)]
public class RM11TheTyrant(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsRect(20, 20));
