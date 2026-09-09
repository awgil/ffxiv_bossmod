namespace BossMod.Dawntrail.Foray.ForkedTower.FTM1TwoHeadedAevis;

public enum OID : uint
{
    Boss = 0x4C11, // R18.000, x1
    GreenHead = 0x4C12, // R15.000, x1
    BlueHead = 0x4C13, // R15.000, x1
    GreenHeadAutos = 0x4C14, // R1.000, x1
    BlueHeadAutos = 0x4C15, // R1.000, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    TetherHelper = 0x4C24, // R1.000, x2
    BallLightning = 0x4C16, // R2.400, x0 (spawn during fight)
    SwirlingOrb = 0x4C17, // R2.800, x0 (spawn during fight)
}

public enum AID : uint
{
    GreenAutosVisual = 47753, // GreenHeadAutos->player, no cast, single-target
    BlueAutosVisual = 47754, // BlueHeadAutos->player, no cast, single-target
    GreenAutoAttack = 50709, // Helper->player, no cast, single-target
    BlueAutoAttack = 50710, // Helper->player, no cast, single-target
    BuffetCast = 49726, // BlueHead/GreenHead->self, 5.0s cast, single-target
    Aethersplit = 48642, // GreenHeadAutos->BlueHeadAutos, no cast, single-target
    PoisonBreathCast = 50715, // BlueHead->self, 8.0s cast, single-target
    PoisonBreathAOE = 47617, // Helper->location, 8.0s cast, range 18 circle
    StormsBreathCast = 47613, // GreenHead->self, 8.0s cast, single-target
    StormsBreathKB = 47616, // Helper->location, 8.0s cast, ???
    ThunderfrostTempestCast = 47735, // BlueHead/GreenHead->self, 5.0s cast, single-target
    ThunderfrostTempestLightning = 47737, // Helper->self, no cast, ???
    ThunderfrostTempestIce = 47738, // Helper->self, no cast, ???
    TwoTerrorsCast1 = 50655, // BlueHead/GreenHead->self, 6.0s cast, single-target
    TwoTerrors = 50658, // Helper->self, 6.0s cast, range 40 width 10 rect
    HissingReprise = 49722, // BlueHead/GreenHead->self, 3.0s cast, single-target
    BuffetRight = 49724, // Helper->self, no cast, ???
    BuffetLeft = 49725, // Helper->self, no cast, ???
    Summon = 47704, // BlueHead/GreenHead->self, 3.0s cast, single-target
    LightningClusterCast1 = 47642, // GreenHead->self, 8.0s cast, single-target
    LightningClusterCast2 = 47644, // GreenHeadAutos->location, 8.0s cast, single-target
    IceClusterCast1 = 48220, // BlueHead->self, 8.0s cast, single-target
    IceClusterCast2 = 47645, // BlueHeadAutos->location, 8.0s cast, single-target
    LightningClusterAOE = 50697, // Helper->location, 8.0s cast, range 15 circle
    IceClusterAOE = 50698, // Helper->location, 8.0s cast, range 15 circle
    Shock = 47706, // BallLightning->self, 2.0s cast, range 15 circle
    HypothermalCombustion = 47707, // SwirlingOrb->self, 2.0s cast, range 15 circle
    BlazeloopAOE = 47660, // Helper->self, 2.5s cast, range 5-60 donut
    BlazeAOE1 = 50703, // Helper->location, 6.0s cast, range 5 circle
    BlazeAOE2 = 50704, // Helper->location, 6.0s cast, range 5 circle
    BlazeAOE3 = 50705, // Helper->location, 6.0s cast, range 5 circle
    BlazeloopCast1 = 47654, // GreenHead->self, 6.0s cast, single-target
    BlazeCast1 = 47659, // GreenHeadAutos->location, 6.0s cast, single-target
    BlazeCast2 = 47663, // GreenHeadAutos->location, 6.0s cast, single-target
    BlazeCast3 = 47664, // BlueHeadAutos->location, 6.0s cast, single-target
    BlazeloopCast2 = 47661, // GreenHead->self, 6.0s cast, single-target
    BlazeloopCast3 = 47662, // BlueHead->self, 5.3+0.7s cast, single-target
    ArcaneRevelation = 49716, // BlueHead/GreenHead->self, 3.0s cast, single-target
    ArcaneBeacon = 49720, // 4B73->self, 4.0s cast, range 60 width 5 rect
    ArchaeofuryCast = 47745, // BlueHead/GreenHead->self, 5.0s cast, single-target
    Archaeofury1 = 47747, // Helper->player, 5.0s cast, range 6 circle
    Archaeofury2 = 47748, // Helper->player, 5.0s cast, range 6 circle
    Unk1 = 49727, // Boss->self, 5.0s cast, single-target
    Unk2 = 47615, // Boss->self, 7.2+0.8s cast, single-target
    Unk3 = 47614, // Boss->self, 7.2+0.8s cast, single-target
    Unk4 = 48243, // Helper->location, 8.0s cast, range 30 circle
    Unk5 = 47736, // Boss->self, 5.0s cast, single-target
    Unk6 = 50656, // Boss->self, 5.0s cast, single-target
    Unk7 = 50657, // Boss->self, 5.0s cast, single-target
    Unk8 = 49723, // Boss->self, 3.0s cast, single-target
    Unk9 = 47705, // Boss->self, 3.0s cast, single-target
    Unk10 = 47643, // Boss->self, 7.4s cast, single-target
    Unk11 = 47655, // Boss->self, 5.3s cast, single-target
    Unk12 = 47658, // Boss->self, no cast, single-target
    Unk13 = 49717, // Boss->self, 3.0s cast, single-target
    Unk14 = 47656, // Boss->self, 5.3s cast, single-target
    Unk15 = 47657, // Boss->self, no cast, single-target
    Unk16 = 47746, // Boss->self, 5.0s cast, single-target
}

public enum SID : uint
{
    EpicHero = 4192, // none->player, extra=0x0
    FatedHero = 4194, // none->player, extra=0x0
    EpicVillain = 5400, // none->4C12, extra=0x0
    FatedVillain = 5401, // none->Boss, extra=0x0
    Unk2552 = 2552, // none->4C11, extra=0x471/0x470
    EasterlyReprise = 5403, // none->player, extra=0x0
    WesterlyReprise = 5404, // none->player, extra=0x0
}

public enum IconID : uint
{
    WindCountdown = 585, // player->self
}

public enum TetherID : uint
{
    DecisiveBattleTether = 429, // player->4C15/4C14
    AOE = 411, // 4C15/4C14->4C24
    Lightning = 412, // 4C14->4C24
    Ice = 413, // 4C15->4C24
}

class Heads(BossModule module) : Components.AddsMulti(module, [OID.GreenHead, OID.BlueHead]);
class DecisiveBattle(BossModule module) : Components.GenericInvincible(module)
{
    readonly int[] PlayerStates = Utils.MakeArray(8, -1);
    readonly Actor?[] Bosses = [null, null];

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var ix = PlayerStates.BoundSafeAt(slot, -1);
        if (ix == 0 && Bosses[1] != null)
            yield return Bosses[1]!;
        if (ix == 1 && Bosses[0] != null)
            yield return Bosses[0]!;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.EpicHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    PlayerStates[slot] = 0;
                break;
            case SID.FatedHero:
                if (Raid.TryFindSlot(actor, out slot))
                    PlayerStates[slot] = 1;
                break;
            case SID.EpicVillain:
                Bosses[0] = actor;
                break;
            case SID.FatedVillain:
                Bosses[1] = actor;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.EpicHero:
            case SID.FatedHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    PlayerStates[slot] = -1;
                break;
            case SID.EpicVillain:
            case SID.FatedVillain:
                Array.Fill(Bosses, null);
                break;
        }
    }
}

class PoisonBreath(BossModule module) : Components.StandardAOEs(module, AID.PoisonBreathAOE, 18);

class ThunderfrostTempest(BossModule module) : Components.RaidwideCastDelay(module, AID.ThunderfrostTempestCast, AID.ThunderfrostTempestIce, 0.8f);

class TwoTerrors(BossModule module) : Components.StandardAOEs(module, AID.TwoTerrors, new AOEShapeRect(40, 5));

// in one component because it works better this way (i hate everything)
class Knockbacks(BossModule module) : Components.Knockback(module)
{
    record struct PlayerState(WDir Direction, DateTime Activation);

    readonly PlayerState[] _playerStates = new PlayerState[8];

    Actor? _stormsBreathCaster;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var st = _playerStates[slot];
        if (st.Direction != default && !IsImmune(slot, st.Activation))
            yield return new(Arena.Center - st.Direction * 20, 20, st.Activation, null, st.Direction.ToAngle(), Kind.DirForward);

        if (_stormsBreathCaster is { } sb && !IsImmune(slot, Module.CastFinishAt(sb.CastInfo)))
            yield return new(sb.CastInfo!.LocXZ, 14, Module.CastFinishAt(sb.CastInfo), Kind: Kind.AwayFromOrigin);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        base.OnStatusGain(actor, status);

        WDir dir = (SID)status.ID switch
        {
            SID.EasterlyReprise => new(-1, 0),
            SID.WesterlyReprise => new(1, 0),
            _ => default
        };

        if (dir != default && Raid.TryFindSlot(actor, out var slot))
            _playerStates[slot] = new(dir, status.ExpireAt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StormsBreathKB)
            _stormsBreathCaster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BuffetRight or AID.BuffetLeft)
            Array.Fill(_playerStates, default);

        if ((AID)spell.Action.ID == AID.StormsBreathKB)
            _stormsBreathCaster = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor).Where(s => !IsImmune(slot, s.Activation)).Take(1))
        {
            if (src.Kind == Kind.DirForward)
            {
                hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center, src.Direction, 20, 5, 20), src.Activation);
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, default(Angle), 15, 15, 15), src.Activation);
            }

            if (src.Kind == Kind.AwayFromOrigin)
            {
                var orig = src.Origin;
                hints.AddForbiddenZone(Sdf.Discrete(p =>
                {
                    var dir = (p - orig).Normalized() * 14;
                    return !(p + dir).AlmostEqual(Arena.Center, 20);
                }), src.Activation);
            }
        }
    }
}

class Cluster(BossModule module) : Components.GroupedAOEs(module, [AID.IceClusterAOE, AID.LightningClusterAOE], new AOEShapeCircle(15))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var ignore = Module.FindComponent<Knockbacks>()?.Sources(slot, actor).Any() == true;

        foreach (var b in base.ActiveAOEs(slot, actor))
            yield return b with { Risky = !ignore };
    }
}

class HypothermalShock(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];
    readonly List<Actor> _possibleCasters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var ignore = Module.FindComponent<Knockbacks>()?.Sources(slot, actor).Any() == true;

        return _predicted.Select(p => p with { Risky = !ignore });
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.BallLightning or OID.SwirlingOrb)
            _possibleCasters.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IceClusterAOE:
                Predict(spell.LocXZ, 15, Module.CastFinishAt(spell), OID.SwirlingOrb);
                break;
            case AID.LightningClusterAOE:
                Predict(spell.LocXZ, 15, Module.CastFinishAt(spell), OID.BallLightning);
                break;
            case AID.ThunderfrostTempestCast:
                Predict(Arena.Center, 100, Module.CastFinishAt(spell, 0.8f), OID.SwirlingOrb, OID.BallLightning);
                break;
            case AID.HypothermalCombustion:
            case AID.Shock:
                var pred = _predicted.FindIndex(p => p.Origin.AlmostEqual(caster.Position, 1));
                if (pred >= 0)
                    _predicted.Ref(pred).Activation = Module.CastFinishAt(spell);
                _possibleCasters.Remove(caster);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HypothermalCombustion or AID.Shock && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }

    void Predict(WPos center, float radius, DateTime activation, params OID[] ids)
    {
        foreach (var a in _possibleCasters.Drain(a => a.Position.InCircle(center, radius) && ids.Contains((OID)a.OID)))
            _predicted.Add(new(new AOEShapeCircle(15), a.Position, a.Rotation, activation.AddSeconds(2)));
    }
}

class Blaze(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BlazeAOE1 or AID.BlazeAOE2 or AID.BlazeAOE3)
        {
            _predicted.Add(new(new AOEShapeCircle(5), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _predicted.Add(new(new AOEShapeDonut(5, 60), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 2.6f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BlazeAOE1 or AID.BlazeAOE2 or AID.BlazeAOE3 or AID.BlazeloopAOE && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_predicted.Count > 0 && _predicted[0].Shape is AOEShapeCircle)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_predicted[0].Origin, 7), _predicted[0].Activation.AddSeconds(2.6f));
    }
}

class ArcaneBeacon(BossModule module) : Components.StandardAOEs(module, AID.ArcaneBeacon, new AOEShapeRect(60, 2.5f), maxCasts: 8);

class Archaeofury(BossModule module) : Components.BaitAwayCast(module, null, new AOEShapeCircle(6), true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Archaeofury1 or AID.Archaeofury2 && WorldState.Actors.Find(spell.TargetID) is { } target)
            CurrentBaits.Add(new(caster, target, Shape, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Archaeofury1 or AID.Archaeofury2)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}

class FTM1TwoHeadedAevisStates : StateMachineBuilder
{
    public FTM1TwoHeadedAevisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Heads>()
            .ActivateOnEnter<DecisiveBattle>()
            .ActivateOnEnter<PoisonBreath>()
            .ActivateOnEnter<ThunderfrostTempest>()
            .ActivateOnEnter<TwoTerrors>()
            .ActivateOnEnter<Knockbacks>()
            .ActivateOnEnter<Cluster>()
            .ActivateOnEnter<HypothermalShock>()
            .ActivateOnEnter<Blaze>()
            .ActivateOnEnter<ArcaneBeacon>()
            .ActivateOnEnter<Archaeofury>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14489)]
public class FTM1TwoHeadedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-900, 700), new ArenaBoundsSquare(20))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;

    public override bool DrawAllPlayers => true;
}
