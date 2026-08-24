namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class HurricaneWingRaidwide(BossModule module) : Components.CastCounterMulti(module, [AID.HurricaneWingRaidwideAOE1, AID.HurricaneWingRaidwideAOE2, AID.HurricaneWingRaidwideAOE3, AID.HurricaneWingRaidwideAOE4, AID.HurricaneWingRaidwideAOE5, AID.HurricaneWingRaidwideAOE6, AID.HurricaneWingRaidwideAOE7, AID.HurricaneWingRaidwideAOE8, AID.HurricaneWingRaidwideAOE9]);

class HurricaneWingAOE(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(9), new AOEShapeDonut(9, 16), new AOEShapeDonut(16, 23), new AOEShapeDonut(23, 30)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = ShapeForAction(spell.Action);
        if (shape != null)
        {
            NumCasts = 0;
            AOEs.Add(new(shape, caster.Position, default, Module.CastFinishAt(spell)));
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = ShapeForAction(spell.Action);
        if (shape != null)
        {
            AOEs.RemoveAll(aoe => aoe.Shape == shape);
            ++NumCasts;
        }
    }

    private AOEShape? ShapeForAction(ActionID aid) => (AID)aid.ID switch
    {
        AID.HurricaneWingLongExpanding1 or AID.HurricaneWingShortExpanding1 or AID.HurricaneWingLongShrinking4 or AID.HurricaneWingShortShrinking4 => _shapes[0],
        AID.HurricaneWingLongExpanding2 or AID.HurricaneWingShortExpanding2 or AID.HurricaneWingLongShrinking3 or AID.HurricaneWingShortShrinking3 => _shapes[1],
        AID.HurricaneWingLongExpanding3 or AID.HurricaneWingShortExpanding3 or AID.HurricaneWingLongShrinking2 or AID.HurricaneWingShortShrinking2 => _shapes[2],
        AID.HurricaneWingLongExpanding4 or AID.HurricaneWingShortExpanding4 or AID.HurricaneWingLongShrinking1 or AID.HurricaneWingShortShrinking1 => _shapes[3],
        _ => null
    };
}

class GreatWhirlwind(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<(Actor wind, AOEShape shape, DateTime activation)> AOEs = [];

    private static readonly AOEShapeCircle _shapeLarge = new(10);
    private static readonly AOEShapeCircle _shapeSmall = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Select(aoe => new AOEInstance(aoe.shape, aoe.wind.Position, default, aoe.activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = (AID)spell.Action.ID switch
        {
            AID.GreatWhirlwindLarge => _shapeLarge,
            AID.GreatWhirlwindSmall => _shapeSmall,
            _ => null
        };
        if (shape != null)
        {
            AOEs.Add((caster, shape, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GreatWhirlwindLarge or AID.GreatWhirlwindLargeAOE or AID.GreatWhirlwindSmall or AID.GreatWhirlwindSmallAOE)
            ++NumCasts;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BitingWind && id == 0x1E3C || (OID)actor.OID == OID.RavagingWind && id == 0x1E39)
            AOEs.RemoveAll(aoe => aoe.wind == actor);
    }
}

class HorridRoarPuddle(BossModule module) : Components.StandardAOEs(module, AID.HorridRoarPuddle, 4);
class HorridRoarSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.HorridRoarSpread, 8);
