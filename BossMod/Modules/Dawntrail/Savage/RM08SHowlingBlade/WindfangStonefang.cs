namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class WindfangStonefangCross(BossModule module) : Components.GroupedAOEs(module, [AID.WindfangCards, AID.WindfangIntercards, AID.StonefangCards, AID.StonefangIntercards], new AOEShapeCross(15, 3));
class WindfangDonut(BossModule module) : Components.StandardAOEs(module, AID.WindfangDonut, new AOEShapeDonut(8, 20));
class StonefangCircle(BossModule module) : Components.StandardAOEs(module, AID.StonefangCircle, new AOEShapeCircle(9));

class WindfangStonefang(BossModule module) : Components.CastCounter(module, default)
{
    private Actor? _source;
    public DateTime Activation { get; private set; }
    public bool Stack { get; private set; }
    public bool Active => _source != null;

    private static readonly AOEShapeCone ActiveShape = new(40, 12.5f.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source != null)
        {
            var pcDir = Angle.FromDirection(actor.Position - _source.Position);
            var clipped = Raid.WithoutSlot().Exclude(actor).InShape(ActiveShape, _source.Position, pcDir);
            if (Stack)
            {
                var cond = clipped.CountByCondition(a => a.Class.IsSupport() == actor.Class.IsSupport());
                hints.Add("Stack in pairs!", cond.match != 0 || cond.mismatch != 1);
            }
            else
            {
                hints.Add("Spread!", clipped.Any());
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_source != null)
            hints.AddPredictedDamage(new(0xff), Activation, type: Stack ? AIHints.PredictedDamageType.Shared : AIHints.PredictedDamageType.Raidwide);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _source == null ? PlayerPriority.Irrelevant : player.Class.IsSupport() == pc.Class.IsSupport() ? PlayerPriority.Normal : PlayerPriority.Interesting;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_source != null)
        {
            var pcDir = Angle.FromDirection(pc.Position - _source.Position);
            ActiveShape.Outline(Arena, _source.Position, pcDir);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StonefangCards or AID.WindfangIntercards or AID.StonefangIntercards or AID.WindfangCards)
        {
            _source = caster;
            Stack = (AID)spell.Action.ID is AID.WindfangIntercards or AID.WindfangCards;
            Activation = Module.CastFinishAt(spell, 0.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.StonefangProtean or AID.WindfangProtean)
        {
            ++NumCasts;
            _source = null;
        }
    }
}

class WindfangStonefangAI(BossModule module) : BossComponent(module)
{
    private readonly RM08SHowlingBladeConfig _config = Service.Config.Get<RM08SHowlingBladeConfig>();
    private readonly WindfangStonefang _ws = module.FindComponent<WindfangStonefang>()!;
    private bool _intercard;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WindfangCards:
            case AID.StonefangCards:
                _intercard = true;
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var clockspot = _config.WindfangStonefangSpots[assignment];
        if (clockspot < 0 || !_ws.Active)
            return;

        var isSupport = actor.Class.IsSupport();

        var assignedQuad = (clockspot + 1) / 2;
        var assignedDirection = (180 - 90 * assignedQuad).Degrees();
        if (_intercard)
            assignedDirection += 45.Degrees();

        hints.AddForbiddenZone(ShapeContains.InvertedCone(Module.PrimaryActor.Position, 12, assignedDirection, 45.Degrees()), _ws.Activation);

        var closestPartner = Module.Raid.WithoutSlot().Where(p => p.Class.IsSupport() != isSupport).Closest(actor.Position);
        if (closestPartner == null)
            return;

        var partnerShape = ShapeContains.Cone(Module.PrimaryActor.Position, 12, Module.PrimaryActor.AngleTo(closestPartner), 15.Degrees());

        if (_ws.Activation < WorldState.FutureTime(0.5f))
        {
            var stack = _ws.Stack;
            hints.AddForbiddenZone(p => stack ? !partnerShape(p) : partnerShape(p), _ws.Activation);
        }
    }
}
