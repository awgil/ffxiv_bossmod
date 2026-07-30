namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

sealed class IronRainStorm(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];
    private readonly IaiGiriBait? _bait = module.FindComponent<IaiGiriBait>();

    private static readonly AOEShapeCircle _shapeRain = new(10f);
    private static readonly AOEShapeCircle _shapeStorm = new(20f);
    private static readonly WDir[] _safespotDirections = [new(1f, default), new(-1f, default), new(default, 1f), new(default, -1f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // draw safespots (TODO: consider assigning specific side)
        var bait = _bait?.Instances.Find(i => i.Target == pc);
        if (bait?.DirOffsets.Count == 2)
        {
            var offset = bait.DirOffsets[1].Rad > 0f ? 5f : -5f;
            var count = AOEs.Count;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < 4; ++i)
            {
                var dir = _safespotDirections[i];
                var safespot = Arena.Center + 19f * dir;
                var safespotSafe = true;
                for (var j = 0; j < count; ++j)
                {
                    ref var aoe = ref aoes[j];
                    if (aoe.Check(safespot))
                    {
                        safespotSafe = false;
                        break;
                    }
                }
                if (safespotSafe)
                {
                    Arena.ZoneCircleOutline(safespot + offset * dir.OrthoR(), 1f, Colors.Safe);
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.NIronRainFirst or (uint)AID.SIronRainFirst => _shapeRain,
            (uint)AID.NIronStormFirst or (uint)AID.SIronStormFirst => _shapeStorm,
            _ => null
        };
        if (shape != null)
            AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NIronRainFirst:
            case (uint)AID.SIronRainFirst:
            case (uint)AID.NIronStormFirst:
            case (uint)AID.SIronStormFirst:
                ++NumCasts;
                var count = AOEs.Count;
                var aoes = CollectionsMarshal.AsSpan(AOEs);
                var act = WorldState.FutureTime(6.2d);
                for (var j = 0; j < count; ++j)
                {
                    ref var aoe = ref aoes[j];
                    aoe.Activation = act; // second aoe will happen at same location
                }
                break;
            case (uint)AID.NIronRainSecond:
            case (uint)AID.SIronRainSecond:
            case (uint)AID.NIronStormSecond:
            case (uint)AID.SIronStormSecond:
                ++NumCasts;
                AOEs.Clear();
                break;
        }
    }
}
