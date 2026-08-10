namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D071Barreltender;

public enum OID : uint
{
    Boss = 0x4234, // R5.0
    CactusBig = 0x1EBBF1, // R0.5
    CactusSmall = 0x1EBBF0, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 37393, // Boss->location, no cast, single-target

    BarbedBellow = 37392, // Boss->self, 5.0s cast, range 50 circle, raidwide
    HeavyweightNeedlesVisual = 37384, // Boss->self, 6.0s cast, single-target
    HeavyweightNeedles = 37386, // Helper->self, 6.5s cast, range 36 50-degree cone

    TenderDrop = 37387, // Boss->self, 3.0s cast, single-target, spawns cacti
    BarrelBreaker = 37390, // Boss->location, 6.0s cast, range 50 circle, knockback 20, away from source

    NeedleSuperstorm = 37389, // Helper->self, 5.0s cast, range 11 circle
    NeedleStorm = 37388, // Helper->self, 5.0s cast, range 6 circle

    SucculentStomp = 37391, // Boss->players, 5.0s cast, range 6 circle, stack
    PricklyRight = 39154, // Boss->self, 7.0s cast, range 36 270-degree cone
    PricklyLeft = 39155, // Boss->self, 7.0s cast, range 36 270-degree cone

    TenderFury = 39242 // Boss->player, 5.0s cast, single-target, tankbuster
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyweightNeedlesVisual && Arena.Bounds.Radius > 21f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 24.5f)], [new Square(center, 20f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.7d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x03 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}

sealed class HeavyweightNeedles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavyweightNeedles, new AOEShapeCone(36f, 25f.Degrees()))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
        if (NumCasts < 16)
        {
            return;
        }
        ref var aoe = ref Casters.Ref(0);
        // stay close to the middle to switch safespots
        hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 3f), aoe.Activation);
    }
}

sealed class NeedleStormSuperstorm(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(16)];
    private readonly AOEShapeCircle circleBig = new(11f), circleSmall = new(6f);

    private readonly BarrelBreaker _kb = module.FindComponent<BarrelBreaker>()!;
    private readonly HeavyweightNeedles _aoe = module.FindComponent<HeavyweightNeedles>()!;
    private bool cactiActive;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Casters.Count != 0)
        {
            return [];
        }
        var count = AOEs.Count;
        var max = cactiActive ? (count > 8 ? 8 : count) : 0; // next wave of cactus helpers spawns immediately after first wave starts casting, so we need to limit to 8
        if (max == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(AOEs);

        var kb = _kb;
        var isKnockback = kb.Casters.Count != 0;
        var isKnockbackImmune = isKnockback && _kb.IsImmune(slot, _kb.Casters.Ref(0).Activation);
        var isKnockbackButImmune = isKnockback && isKnockbackImmune;
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            aoe.Risky = !isKnockback || isKnockbackButImmune;
        }
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.CactusSmall => circleSmall,
            (uint)OID.CactusBig => circleBig,
            _ => null
        };
        if (shape != null)
        {
            AOEs.Add(new(shape, actor.Position.Quantized(), actorID: shape == circleBig ? 1ul : default));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TenderDrop)
        {
            cactiActive = true;
            var count = AOEs.Count;
            var max = count > 8 ? 8 : count;
            var aoes = CollectionsMarshal.AsSpan(AOEs)[..max];

            for (var i = 0; i < max; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Activation = Module.CastFinishAt(spell, 13.7d);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.NeedleStorm or (uint)AID.NeedleSuperstorm)
        {
            AOEs.RemoveAt(0);
            cactiActive = false;
        }
    }
}

sealed class Prickly(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PricklyRight, (uint)AID.PricklyLeft], new AOEShapeCone(36f, 165f.Degrees()));
sealed class SucculentStomp(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.SucculentStomp, 6f, 4, 4);
sealed class BarrelBreaker(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BarrelBreaker, 20f)
{
    private NeedleStormSuperstorm? _aoe;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }
        ref readonly var c = ref Casters.Ref(0);
        _aoe ??= Module.FindComponent<NeedleStormSuperstorm>();

        var act = c.Activation;
        if (!IsImmune(slot, act))
        {
            var aoes = CollectionsMarshal.AsSpan(_aoe!.AOEs);
            var len = aoes.Length;
            var max = len > 8 ? 8 : len;
            var circles = new (WPos origin, float Radius)[max];
            for (var i = 0; i < max; ++i)
            {
                ref var aoe = ref aoes[i];
                circles[i] = (aoe.Origin, aoe.ActorID == default ? 7f : 12f);
            }
            // square intentionally slightly smaller to prevent sus knockback
            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOriginPlusAOECirclesMixedRadii(Arena.Center, c.Origin, 20f, 19f, circles, max), act);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _aoe ??= Module.FindComponent<NeedleStormSuperstorm>();
        var aoes = CollectionsMarshal.AsSpan(_aoe!.AOEs);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class TenderFury(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TenderFury);
sealed class BarbedBellow(BossModule module) : Components.RaidwideCast(module, (uint)AID.BarbedBellow);

sealed class D071BarreltenderStates : StateMachineBuilder
{
    public D071BarreltenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<BarrelBreaker>()
            .ActivateOnEnter<HeavyweightNeedles>()
            .ActivateOnEnter<NeedleStormSuperstorm>()
            .ActivateOnEnter<Prickly>()
            .ActivateOnEnter<TenderFury>()
            .ActivateOnEnter<SucculentStomp>()
            .ActivateOnEnter<BarbedBellow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834u, NameID = 12889u)]
public sealed class D071Barreltender(WorldState ws, Actor primary) : BossModule(ws, primary, new(-65f, 470f), new ArenaBoundsSquare(24.5f));
