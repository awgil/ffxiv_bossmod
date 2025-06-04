namespace BossMod.Dawntrail.Foray.CriticalEngagement.Hinkypunk;

public enum OID : uint
{
    Boss = 0x46D1, // R4.000, x1
    DeathWallHelper = 0x4863, // R0.500, x1
    AvianHusk = 0x46D2, // R4.000, x5
    Hinkypunk = 0x46D3, // R4.000, x4
    Helper = 0x233C, // R0.500, x16, Helper type
}

public enum AID : uint
{
    DeathWall = 41382, // DeathWallHelper->self, no cast, range 20?-25 donut
    AutoAttack = 870, // Boss->player, no cast, single-target
    LamplightCast = 41381, // Boss->self, 5.0s cast, single-target
    Lamplight = 41744, // Helper->self, 5.8s cast, ???
    DreadDive = 41380, // Boss->player, 5.0s cast, single-target
    DonutOmen = 41374, // Helper->self, 2.5s cast, range 7.5?-50 donut
    CrossOmen = 41377, // Helper->self, 2.5s cast, range 50 width 15 cross
    KnockbackOmen = 41379, // Helper->self, 2.5s cast, range 50 circle
    Molt = 41368, // Boss->self, 13.0s cast, single-target
    MoltHelper = 41369, // Hinkypunk->self, 13.0s cast, single-target
    HuskActivate = 41371, // Helper->AvianHusk/Boss, no cast, single-target
    BlowoutCast = 41378, // Boss/Hinkypunk->self, 2.0s cast, single-target
    Blowout = 41397, // Helper->self, 2.8s cast, distance 20 knockback
    ShadesNestHuskCast = 41373, // Boss/Hinkypunk->self, 2.0s cast, single-target
    ShadesNestHusk = 42033, // Helper->self, 2.8s cast, range 7.5?-50 donut
    ShadesCrossingHuskCast = 41376, // Boss/Hinkypunk->self, 2.0s cast, single-target
    ShadesCrossingHusk = 42035, // Helper->self, 2.8s cast, range 50 width 15 cross
    FlockOfSouls = 41370, // Boss->self, 5.0s cast, single-target
    ShadesNestCast = 41372, // Boss->self, 5.0+0.8s cast, single-target
    ShadesNest = 42032, // Helper->self, 5.8s cast, range 7.5?-50 donut
    ShadesCrossingCast = 41375, // Boss->self, 5.0+0.8s cast, single-target
    ShadesCrossing = 42034, // Helper->self, 5.8s cast, range 50 width 15 cross
    Unk1 = 41396, // Boss->self, no cast, single-target
}

class Lamplight(BossModule module) : Components.RaidwideCast(module, AID.LamplightCast);
class DreadDive(BossModule module) : Components.SingleTargetCast(module, AID.DreadDive);

class Molt(BossModule module) : BossComponent(module)
{
    enum Cast
    {
        None,
        Boss,
        Adds
    }

    private Cast _cast;

    public record class Bird(WPos Source, M Mechanic, DateTime Activation, bool Ready = false)
    {
        public WPos Source = Source;
        public bool Ready = Ready;
    }

    public enum M
    {
        None,
        Donut,
        Cross,
        KB
    }

    private readonly List<Bird> _casters = [];

    public Bird? NextBird => _casters.FirstOrDefault(c => c.Ready);

    private float CastDelay(int order) => _cast switch
    {
        Cast.Boss => order switch
        {
            0 => 16.1f,
            1 => 18.4f,
            2 => 20.8f,
            3 => 23.1f,
            _ => 0
        },
        Cast.Adds => order switch
        {
            0 => 15,
            1 => 15.3f,
            2 => 22.2f,
            3 => 22.4f,
            _ => 0
        },
        _ => 0,
    };

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Molt:
                _cast = Cast.Boss;
                break;
            case AID.MoltHelper:
                _cast = Cast.Adds;
                break;
        }

        if (_cast != default)
        {
            var order = Module.CastFinishAt(spell, CastDelay(_casters.Count));
            switch ((AID)spell.Action.ID)
            {
                case AID.DonutOmen:
                    _casters.Add(new(caster.Position, M.Donut, order));
                    break;
                case AID.CrossOmen:
                    _casters.Add(new(caster.Position, M.Cross, order));
                    break;
                case AID.KnockbackOmen:
                    _casters.Add(new(caster.Position, M.KB, order));
                    break;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Molt or AID.MoltHelper)
            _cast = Cast.None;
    }

    private bool _pendingKnockbackCast;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ShadesNestHusk or AID.ShadesCrossingHusk && _casters.Count > 0)
            _casters.RemoveAt(0);

        if ((AID)spell.Action.ID is AID.BlowoutCast)
            _pendingKnockbackCast = true;

        if ((AID)spell.Action.ID is AID.Blowout && _pendingKnockbackCast)
        {
            _pendingKnockbackCast = false;
            _casters.RemoveAt(0);
        }

        if ((AID)spell.Action.ID == AID.HuskActivate)
        {
            var mark = true;
            var dest = WorldState.Actors.Find(spell.MainTargetID)!.Position;
            var src = caster.Position;
            for (var i = 0; i < _casters.Count; i++)
            {
                if (_casters[i].Source.AlmostEqual(src, 0.5f))
                {
                    _casters[i].Source = dest;
                    _casters[i].Ready = mark;
                    mark = false;
                }
            }
        }
    }
}

class Blowout(BossModule module) : Components.Knockback(module, null)
{
    private readonly Molt _molt = module.FindComponent<Molt>()!;

    // if we edge the arena to the maximum extent possible, it can make getting back inside (for boss Shades' Nest cast) very sketchy
    public const float ExtraKnockbackCushion = 2;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_molt.NextBird is { } bird && bird.Mechanic == Molt.M.KB)
            yield return new(bird.Source, 20, bird.Activation);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(p =>
                {
                    var dir = (p - src.Origin).Normalized();
                    return !(p + dir * 20).InCircle(Arena.Center, 20 - ExtraKnockbackCushion);
                }, src.Activation);
        }
    }
}

class ShadesNest(BossModule module) : Components.GenericAOEs(module, AID.ShadesNestHusk)
{
    private readonly Molt _molt = module.FindComponent<Molt>()!;

    // TODO fix donut radius
    public static readonly AOEShape DonutShape = new AOEShapeDonut(7, 50);
    public static readonly AOEShape CrossShape = new AOEShapeCross(50, 7.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_molt.NextBird is { } bird)
        {
            if (bird.Mechanic == Molt.M.Donut)
                yield return new(DonutShape, bird.Source, default, bird.Activation);
            if (bird.Mechanic == Molt.M.Cross)
                yield return new(CrossShape, bird.Source, 45.Degrees(), bird.Activation);
        }
    }
}

class ShadesNestBoss(BossModule module) : Components.StandardAOEs(module, AID.ShadesNest, ShadesNest.DonutShape)
{
    private readonly Molt _molt = module.FindComponent<Molt>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _molt.NextBird switch
    {
        { Mechanic: Molt.M.KB } => base.ActiveAOEs(slot, actor).Select(a => a with { Risky = false }), // draw anticipated aoe, but don't highlight
        { } => [], // drawing other birds (donut/cross) clutters the radar
        _ => base.ActiveAOEs(slot, actor), // draw normally
    };
}
class ShadesCrossingBoss(BossModule module) : Components.StandardAOEs(module, AID.ShadesCrossing, ShadesNest.CrossShape)
{
    private readonly Molt _molt = module.FindComponent<Molt>()!;

    private bool IsRisky => _molt.NextBird is not { Mechanic: Molt.M.KB };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(s => s with { Risky = IsRisky });
}

class HinkypunkStates : StateMachineBuilder
{
    public HinkypunkStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Lamplight>()
            .ActivateOnEnter<DreadDive>()
            .ActivateOnEnter<Molt>()
            .ActivateOnEnter<Blowout>()
            .ActivateOnEnter<ShadesNest>()
            .ActivateOnEnter<ShadesNestBoss>()
            .ActivateOnEnter<ShadesCrossingBoss>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13659)]
public class Hinkypunk(WorldState ws, Actor primary) : BossModule(ws, primary, new(-570, -160), new ArenaBoundsCircle(20))
{
    public override bool DrawAllPlayers => true;
}

