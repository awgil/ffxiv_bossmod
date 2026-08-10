namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D113SpectralBerserker;

public enum OID : uint
{
    Boss = 0x2EFD, // R3.0
    Rubble = 0x2EFE, // R2.5
    Crater = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BeastlyFury = 21004, // Boss->self, 4.0s cast, range 50 circle

    WildAnguish1 = 21000, // Boss->players, 5.0s cast, range 6 circle
    WildAnguish2 = 21001, // Boss->players, no cast, range 6 circle
    FallingRock = 20997, // Rubble->self, no cast, range 8 circle

    WildRageVisual = 20994, // Boss->location, 5.0s cast, range 8 circle
    WildRage = 20995, // Helper->location, 5.7s cast, range 8 circle
    WildRageKnockback = 20996, // Helper->self, 5.7s cast, range 8-50 donut, raidwide, knockback 15, away fromsource

    WildRampageVisual = 20998, // Boss->self, 5.0s cast, single-target
    WildRampage = 20999, // Helper->self, 5.5s cast, range 50 width 50 rect

    RagingSliceFirst = 21002, // Boss->self, 3.7s cast, range 50 width 6 rect
    RagingSliceRest = 21003 // Boss->self, 2.5s cast, range 50 width 6 rect
}

public enum IconID : uint
{
    Stackmarker = 93, // player
    Spreadmarker = 229 // player
}

sealed class BeastlyFuryArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BeastlyFury && Arena.Bounds.Radius > 20f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 22.5f)], [new Cross(center, 20f, 10f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 1.1d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0B && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsCustom([new Cross(Arena.Center, 20f, 10f)]);
            _aoe = [];
        }
    }
}

sealed class FallingRock(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.WildAnguish2, 8.5f, 6d) // 8.5 instead of 6 to prevent aoe from intersecting additional rubble hitboxes
{
    public override void Update()
    {
        if (Spreads.Count != 0)
        {
            var count = Spreads.Count;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                ref var spread = ref spreads[i];
                if (spread.Target.IsDead)
                {
                    Spreads.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsSpreadTarget(actor))
        {
            var rubble = Module.Enemies((uint)OID.Rubble);
            var count = rubble.Count;
            for (var i = 0; i < count; ++i)
            {
                if (!rubble[i].IsDead)
                {
                    hints.Add("Stack alone with rubble!");
                    return;
                }
            }
        }
    }
}

sealed class WildAnguish1(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.WildAnguish1, 6f, 4, 4)
{
    public static bool IsQuadrupleStack(BossModule module)
    {
        var rubble = module.Enemies((uint)OID.Rubble);
        var count = rubble.Count;
        for (var i = 0; i < count; ++i)
        {
            if (!rubble[i].IsDead)
                return true;
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.DrawArenaForeground(pcSlot, pc);
    }
}

sealed class WildAnguish2(BossModule module) : Components.GenericTowers(module)
{
    private readonly FallingRock _sp = module.FindComponent<FallingRock>()!;

    public override void Update()
    {
        if (Towers.Count != 0 && _sp.Spreads.Count == 0)
            Towers.Clear();
    }

    public override void OnActorCreated(Actor actor)
    {
        // theoretically it would be 8.5 (rubble hitboxradius + aoe hitbox radius), but that makes it harder to spread out correctly, because then we would need to spread rubbles twice as far apart
        if (actor.OID == (uint)OID.Rubble)
            Towers.Add(new(actor.Position, actor.HitboxRadius, activation: WorldState.FutureTime(6.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WildAnguish1 or (uint)AID.WildAnguish2)
        {
            var t = WorldState.Actors.Find(spell.MainTargetID);
            var count = Towers.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                if (Towers[i].Position.InCircle(t!.Position, 8.5f))
                {
                    Towers.RemoveAt(i);
                }
            }
            var count2 = _sp.Spreads.Count;
            if (count2 > 0 && spell.Action.ID == (uint)AID.WildAnguish1)
            {
                for (var i = count2 - 1; i >= 0; --i)
                {
                    if (_sp.Spreads[i].Target == t)
                    {
                        _sp.Spreads.RemoveAt(i);
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

sealed class WildRageKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WildRageKnockback, 15f)
{
    private RelSimplifiedComplexPolygon polygon;
    private bool polygonInit;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                if (!polygonInit)
                {
                    polygon = Arena.Bounds.Shape.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
                    polygonInit = true;
                }
                hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOriginPlusAOECircles(Arena.Center, c.Origin, 15f, polygon, [new(738f, 482f), new(762f, 482f)], 7.5f, 2), c.Activation);
            }
        }
    }
}

sealed class WildRageRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.WildRageKnockback);
sealed class WildRage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WildRage, 8f);
sealed class BeastlyFury(BossModule module) : Components.RaidwideCast(module, (uint)AID.BeastlyFury);

sealed class CratersWildRampage(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Circle> circles = [with(2)];
    private bool invert;
    private DateTime activation;
    private AOEShapeCustom? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is AOEShapeCustom aoe)
        {
            aoe.InvertForbiddenZone = invert;
            return new AOEInstance[1] { new(aoe, Arena.Center, default, activation, invert ? Colors.SafeFromAOE : default) };
        }
        return [];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u && actor.OID == (uint)OID.Crater)
        {
            var count = circles.Count; // prevent duplicates because eanim happens twice
            var pos = actor.Position;
            for (var i = 0; i < count; ++i)
            {
                if (circles[i].Center == pos)
                {
                    return;
                }
            }
            circles.Add(new Circle(pos, 7f));

            _aoe = new AOEShapeCustom(Arena.Center, [.. circles]);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WildRampage)
        {
            invert = true;
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WildRampage)
        {
            invert = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (invert)
        {
            var aoes = ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var isRisky = true;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(actor.Position))
                {
                    isRisky = false;
                    break;
                }
            }
            hints.Add("Go inside crater!", isRisky);
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

sealed class RagingSlice(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RagingSliceFirst, (uint)AID.RagingSliceRest], new AOEShapeRect(50f, 3f));

sealed class D113SpectralBerserkerStates : StateMachineBuilder
{
    public D113SpectralBerserkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeastlyFuryArenaChange>()
            .ActivateOnEnter<BeastlyFury>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<CratersWildRampage>()
            .ActivateOnEnter<WildAnguish1>()
            .ActivateOnEnter<WildAnguish2>()
            .ActivateOnEnter<WildRageKnockback>()
            .ActivateOnEnter<WildRageRaidwide>()
            .ActivateOnEnter<WildRage>()
            .ActivateOnEnter<RagingSlice>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737u, NameID = 9511u)]
public sealed class D113SpectralBerserker(WorldState ws, Actor primary) : BossModule(ws, primary, new(750f, 482f), new ArenaBoundsSquare(22.5f));
