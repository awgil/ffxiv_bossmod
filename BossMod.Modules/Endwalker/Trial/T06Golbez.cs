namespace BossMod.Endwalker.Trial.T06Golbez;

public enum OID : uint
{
    Boss = 0x3F54, // x1
    ShadowDragon = 0x3F55, // x1, Dragon's Descent and Cauterize source
    GaleSphere = 0x3F56, // spawn during fight
    GolbezShadow = 0x3F5A, // x4
    Helper = 0x233C, // Helper type
}

public enum AID : uint
{
    AutoAttack = 33867, // Boss->player, no cast, single-target
    Terrastorm = 33893, // Helper->self, 8.0s cast, range 16 circle
    CrescentBlade = 33899, // Boss->self, 5.0s cast, range 22 180-degree cone
    ArcticAssault = 33886, // Boss->self, 3.0s cast, single-target, visual
    ArcticAssaultAOE = 33888, // Helper->self, 4.0s cast, range 15 width 15 rect
    VoidMeteor = 33964, // Boss->self, 5.0s cast, single-target, visual
    VoidComet = 33966, // Helper->player, no cast, range 4 circle, x4 before Void Meteor
    VoidMeteorAOE = 33967, // Helper->player, no cast, range 6 circle, tankbuster
    LingeringSpark = 33895, // Boss->self, 3.0s cast, single-target, visual
    LingeringSparkAOE = 33897, // Helper->location, 3.0s cast, range 5 circle
    BindingCold = 33970, // Boss->self, 5.0s cast, range 40 circle, raidwide
    AzdajasShadow1 = 34558, // Boss->self, 5.0s cast, single-target, visual
    AzdajasShadow2 = 33910, // Boss->self, 4.0s cast, single-target, visual
    BlackFang = 33905, // Boss->self, 4.0s cast, single-target, visual
    BlackFangAOE = 33907, // Helper->self, 10.7s cast, range 40 circle, raidwide, after multiple small hits
    ShadowCrescent = 33927, // Boss->self, 5.0s cast, range 22 180-degree cone
    RisingBeaconVisual = 33931, // ShadowDragon->self, 5.0s cast, single-target, visual
    RisingBeacon = 33935, // Helper->self, 7.2s cast, range 10 circle
    RisingRing = 33936, // Helper->self, 7.2s cast, range 6-22 donut, not seen
    BurningShade = 33939, // Helper->player, 5.0s cast, range 5 circle, spread
    ImmolatingShade = 33941, // Helper->player, 6.0s cast, range 6 circle, stack
    Explosion1 = 33945, // Helper->self, 11.5s cast, range 4 circle, tower for 1
    Explosion2 = 33946, // Helper->self, 11.5s cast, range 4 circle, tower for 2
    Explosion3 = 33947, // Helper->self, 11.5s cast, range 4 circle, tower for 3
    MassiveExplosion = 33951, // Helper->self, no cast, range 40 circle, raidwide after a tower without enough soakers
    DragonsDescent = 33943, // ShadowDragon->self, no cast, range 45 circle, knockback from the marked player
    Cauterize = 33953, // ShadowDragon->self, 3.0s cast, range 50 width 12 rect
    DoubleMeteor = 33972, // Boss->self, 5.0s cast, single-target, visual
    DoubleMeteorAOE1 = 33974, // Helper->location, 6.0s cast, range 40 circle with falloff
    DoubleMeteorAOE2 = 34699, // Helper->location, 6.0s cast, range 40 circle with falloff
    GaleSphere = 33869, // Boss->self, 3.0s cast, single-target, visual
    GaleSpherePrepareN = 33871, // GolbezShadow->self, no cast, single-target, visual
    GaleSpherePrepareE = 33872, // GolbezShadow->self, no cast, single-target, visual
    GaleSpherePrepareW = 33873, // GolbezShadow->self, no cast, single-target, visual
    GaleSpherePrepareS = 33874, // GolbezShadow->self, no cast, single-target, visual
    GaleSphereAOE1 = 33876, // GaleSphere->self, 4.0s cast, range 30 width 5 rect
    GaleSphereAOE2 = 33877, // GaleSphere->self, 7.5s cast, range 30 width 5 rect
    GaleSphereAOE3 = 33878, // GaleSphere->self, 11.0s cast, range 30 width 5 rect
    GaleSphereAOE4 = 33879, // GaleSphere->self, 14.5s cast, range 30 width 5 rect
    VoidStardust1 = 33955, // Boss->self, 3.0s cast, single-target, visual
    VoidStardust2 = 33957, // Helper->self, 6.8s cast, range 6 circle, first pair
    VoidStardustRestVisual = 33959, // Helper->self, 1.0s cast, range 6 circle, resolves ~2.9s after it ends
    VoidStardustRestAOE = 33961, // Helper->self, no cast, range 6 circle
    EventideFall = 33922, // Boss->self, 7.0s cast, single-target, visual
    EventideFallMark = 33924, // Helper->player, no cast, marks both healers
    EventideFallVisual = 33923, // Boss->self, no cast, single-target, visual
    EventideFallAOE = 34483, // Helper->self, no cast, line stack for 4 aimed at a marked player
}

public enum IconID : uint
{
    VoidMeteor = 344, // player
    DragonsDescent = 476, // player
}

public enum TetherID : uint
{
    Cauterize = 17, // ShadowDragon->player
}

class Terrastorm(BossModule module) : Components.StandardAOEs(module, AID.Terrastorm, 16);
class CrescentBlade(BossModule module) : Components.StandardAOEs(module, AID.CrescentBlade, new AOEShapeCone(22, 90.Degrees()));
class ShadowCrescent(BossModule module) : Components.StandardAOEs(module, AID.ShadowCrescent, new AOEShapeCone(22, 90.Degrees()));
class ArcticAssault(BossModule module) : Components.StandardAOEs(module, AID.ArcticAssaultAOE, new AOEShapeRect(15, 7.5f));
class LingeringSpark(BossModule module) : Components.StandardAOEs(module, AID.LingeringSparkAOE, 5);
class BindingCold(BossModule module) : Components.RaidwideCast(module, AID.BindingCold);
class BlackFang(BossModule module) : Components.RaidwideCast(module, AID.BlackFangAOE, "Raidwide x3");
class RisingBeacon(BossModule module) : Components.StandardAOEs(module, AID.RisingBeacon, 10);
class RisingRing(BossModule module) : Components.StandardAOEs(module, AID.RisingRing, new AOEShapeDonut(6, 22));
class BurningShade(BossModule module) : Components.SpreadFromCastTargets(module, AID.BurningShade, 5);
class ImmolatingShade(BossModule module) : Components.StackWithCastTargets(module, AID.ImmolatingShade, 6, 4);
class DoubleMeteor1(BossModule module) : Components.StandardAOEs(module, AID.DoubleMeteorAOE1, 15);
class DoubleMeteor2(BossModule module) : Components.StandardAOEs(module, AID.DoubleMeteorAOE2, 15);

class VoidMeteor(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.VoidMeteor, AID.VoidMeteorAOE, 9.1f, true, AIHints.PredictedDamageType.None);

class DragonsDescent(BossModule module) : Components.Knockback(module, AID.DragonsDescent)
{
    private Actor? _source;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_source != null && _source != actor)
            yield return new(_source.Position, 10, _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DragonsDescent)
        {
            _source = actor;
            _activation = WorldState.FutureTime(8.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _source = null;
        }
    }
}

class Explosion(BossModule module, AID aid, int minSoakers) : Components.CastTowers(module, aid, 4, minSoakers, PartyState.MaxPartySize)
{
    private BitMask _forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DragonsDescent)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        for (var i = 0; i < Towers.Count; ++i)
        {
            var tower = Towers[i];
            tower.ForbiddenSoakers = _forbidden;
            Towers[i] = tower;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (Towers.Count == 0)
            _forbidden.Reset();
    }
}

class Explosion1(BossModule module) : Explosion(module, AID.Explosion1, 1);
class Explosion2(BossModule module) : Explosion(module, AID.Explosion2, 2);
class Explosion3(BossModule module) : Explosion(module, AID.Explosion3, 3);

class CauterizeBait(BossModule module) : Components.GenericBaitAway(module, AID.Cauterize)
{
    private static readonly AOEShapeRect _shape = new(50, 6);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Cauterize && WorldState.Actors.Find(tether.Target) is { } target)
            CurrentBaits.Add(new(source, target, _shape, WorldState.FutureTime(8.1f)));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Cauterize)
            CurrentBaits.Clear();
    }
}

class Cauterize(BossModule module) : Components.StandardAOEs(module, AID.Cauterize, new AOEShapeRect(50, 6));

class GaleSphere(BossModule module) : Components.GenericAOEs(module)
{
    private enum Side { S, E, N, W }

    private static readonly AOEShapeRect _shape = new(30, 2.5f);
    private readonly List<Side> _sides = [];
    private readonly List<Actor>[] _spheres = [[], [], [], []];

    private int Current => Array.FindIndex(_spheres, s => s.Count > 0);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Current is var order && order >= 0)
            foreach (var s in _spheres[order])
                yield return new(_shape, s.Position, s.CastInfo?.Rotation ?? s.Rotation, Module.CastFinishAt(s.CastInfo));
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_sides.Count > 0)
            hints.Add($"Order: {string.Join(" -> ", _sides)}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var order = CastOrder(spell.Action);
        if (order >= 0)
            _spheres[order].Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = CastOrder(spell.Action);
        if (order < 0)
            return;
        _spheres[order].Remove(caster);
        if (_sides.Count > 0 && _spheres[order].Count == 0)
            _sides.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GaleSpherePrepareN:
                _sides.Add(Side.N);
                break;
            case AID.GaleSpherePrepareE:
                _sides.Add(Side.E);
                break;
            case AID.GaleSpherePrepareW:
                _sides.Add(Side.W);
                break;
            case AID.GaleSpherePrepareS:
                _sides.Add(Side.S);
                break;
        }
    }

    private static int CastOrder(ActionID aid) => (AID)aid.ID switch
    {
        AID.GaleSphereAOE1 => 0,
        AID.GaleSphereAOE2 => 1,
        AID.GaleSphereAOE3 => 2,
        AID.GaleSphereAOE4 => 3,
        _ => -1
    };
}

class VoidStardust(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(6);
    private readonly List<(WPos pos, DateTime activation)> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in _aoes.Skip(NumCasts + 2).Take(10))
            yield return new(_shape, aoe.pos, default, aoe.activation);
        foreach (var aoe in _aoes.Skip(NumCasts).Take(2))
            yield return new(_shape, aoe.pos, default, aoe.activation, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.VoidStardust2:
                _aoes.Add((caster.Position, Module.CastFinishAt(spell)));
                break;
            case AID.VoidStardustRestVisual:
                _aoes.Add((caster.Position, Module.CastFinishAt(spell, 2.9f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.VoidStardust2 or AID.VoidStardustRestAOE)
            ++NumCasts;
    }
}

class EventideFall(BossModule module) : Components.UniformStackSpread(module, 3, 0, 4, 4)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EventideFallMark:
                if (WorldState.Actors.Find(spell.MainTargetID) is { } target)
                    AddStack(target, WorldState.FutureTime(8.6f));
                break;
            case AID.EventideFallAOE:
                Stacks.RemoveAll(s => spell.Targets.Any(t => t.ID == s.Target.InstanceID));
                break;
        }
    }
}

class T06GolbezStates : StateMachineBuilder
{
    public T06GolbezStates(BossModule module) : base(module)
    {
        // TODO: Rising Ring and Eventide Triad were not seen on the 11-08 run; Dragon's Descent distance and the Double Meteor radius are replay estimates
        TrivialPhase()
            .ActivateOnEnter<Terrastorm>()
            .ActivateOnEnter<CrescentBlade>()
            .ActivateOnEnter<ShadowCrescent>()
            .ActivateOnEnter<ArcticAssault>()
            .ActivateOnEnter<LingeringSpark>()
            .ActivateOnEnter<BindingCold>()
            .ActivateOnEnter<BlackFang>()
            .ActivateOnEnter<RisingBeacon>()
            .ActivateOnEnter<RisingRing>()
            .ActivateOnEnter<BurningShade>()
            .ActivateOnEnter<ImmolatingShade>()
            .ActivateOnEnter<DoubleMeteor1>()
            .ActivateOnEnter<DoubleMeteor2>()
            .ActivateOnEnter<VoidMeteor>()
            .ActivateOnEnter<DragonsDescent>()
            .ActivateOnEnter<Explosion1>()
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<Explosion3>()
            .ActivateOnEnter<CauterizeBait>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<GaleSphere>()
            .ActivateOnEnter<VoidStardust>()
            .ActivateOnEnter<EventideFall>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 949, NameID = 12365)]
public class T06Golbez(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(15));
