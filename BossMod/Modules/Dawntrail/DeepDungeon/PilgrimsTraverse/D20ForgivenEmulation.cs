namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D20ForgivenEmulation;

public enum OID : uint
{
    Boss = 0x485C,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 45130, // Boss->player, no cast, single-target
    TouchdownCast = 43454, // Boss->self, 14.5s cast, single-target
    Touchdown = 43455, // Helper->self, 14.0s cast, range 30 circle, distance 10 knockback
    Burst1 = 43456, // Helper->self, 2.0s cast, range 11 circle
    Burst2 = 43457, // Helper->self, 3.3s cast, range 11 circle
    Burst3 = 43458, // Helper->self, 4.6s cast, range 11 circle
    Burst4 = 43459, // Helper->self, 5.9s cast, range 11 circle
    BareRootPlantingVisual = 43460, // Boss->self, 5.0s cast, single-target
    BareRootPlanting = 43461, // Helper->location, 3.0s cast, range 6 circle
    WoodsEmbraceVoidzone = 43462, // Helper->self, no cast, range 3 width 6 cross
    DensePlantingVisual = 43463, // Boss->self, 5.0s cast, single-target
    DensePlanting = 43464, // Helper->location, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    Glow = 2056, // none->Boss, extra=0x395/0x396/0x397/0x398
    AreaOfInfluenceUp = 1909, // none->Helper, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9
}

class Planting(BossModule module) : Components.GroupedAOEs(module, [AID.BareRootPlanting, AID.DensePlanting], new AOEShapeCircle(6));

class PlantingBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == 23)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCircle(6), WorldState.FutureTime(6.1f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BareRootPlanting)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (ActiveBaitsOn(actor).FirstOrNull() is { } b)
            hints.AddForbiddenZone(ShapeContains.Circle(Arena.Center, 12), b.Activation);
    }
}

class WoodsEmbrace(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _source;
    private float _range;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source is { } src)
        {
            yield return new(new AOEShapeCross(3 + _range * 3, 3), src.Position);
            yield return new(new AOEShapeCross(3 + _range * 3, 3), src.Position, 45.Degrees());
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in ActiveAOEs(slot, actor))
        {
            hints.AddForbiddenZone(c.Shape, c.Origin, c.Rotation, c.Activation);
            if (_range < 8)
                // encourage ai to stay out of growing aoe
                hints.AddForbiddenZone(new AOEShapeCross(30, 3), c.Origin, c.Rotation, WorldState.FutureTime(2));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBE4B)
        {
            if (state == 0x00100020)
                _source = actor;
            else if (state == 0x00040008)
            {
                _source = null;
                _range = 0;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AreaOfInfluenceUp)
            _range = status.Extra;
    }
}

class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];
    private bool _risky;

    public Angle SafeDir { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(3).Select(p => p with { Risky = _risky });

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == 2056)
        {
            var angle = status.Extra switch
            {
                0x395 => -45.Degrees(),
                0x396 => 45.Degrees(),
                0x397 => -135.Degrees(),
                0x398 => 135.Degrees(),
                _ => default
            };
            if (angle != default)
            {
                var advanceTime = _predicted.Count switch
                {
                    0 => 14.7f,
                    1 => 13.2f,
                    2 => 11.4f,
                    3 => 9.6f,
                    _ => 0
                };
                if (advanceTime != 0)
                {
                    if (_predicted.Count == 3)
                        SafeDir = (actor.Rotation + angle).Normalized();

                    _predicted.Add(new(new AOEShapeCircle(11), actor.Position + (actor.Rotation + angle).ToDirection() * 9.25f, default, WorldState.FutureTime(advanceTime)));
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Touchdown)
            _risky = false;

        if ((AID)spell.Action.ID is AID.Burst1 or AID.Burst2 or AID.Burst3 or AID.Burst4)
            _predicted.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Touchdown)
        {
            _risky = true;
            SafeDir = default;
        }
    }
}

class BurstAOE(BossModule module) : Components.GroupedAOEs(module, [AID.Burst1, AID.Burst2, AID.Burst3, AID.Burst4], new AOEShapeCircle(11), maxCasts: 3)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (IDs.Contains(spell.Action))
            Casters.SortBy(c => Module.CastFinishAt(c.CastInfo));
    }
}

class Touchdown(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Touchdown, 10)
{
    private readonly Burst Burst = module.FindComponent<Burst>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            if (!IsImmune(slot, src.Activation))
            {
                hints.AddForbiddenZone(ShapeContains.Donut(Arena.Center, 4.5f, 30), src.Activation);
                if (Burst.SafeDir != default)
                    hints.AddForbiddenZone(ShapeContains.InvertedCone(Arena.Center, 30, Burst.SafeDir, 30.Degrees()), src.Activation);
            }
        }
    }
}

class D20ForgivenEmulationStates : StateMachineBuilder
{
    public D20ForgivenEmulationStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<BurstAOE>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<WoodsEmbrace>()
            .ActivateOnEnter<Planting>()
            .ActivateOnEnter<PlantingBait>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1033, NameID = 13973)]
public class D20ForgivenEmulation(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(14.5f));

