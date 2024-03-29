namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko;

class IronRainStorm : Components.GenericAOEs
{
    public List<AOEInstance> AOEs = new();
    private IaiGiriBait? _bait;

    private static readonly AOEShapeCircle _shapeRain = new(10);
    private static readonly AOEShapeCircle _shapeStorm = new(20);
    private static readonly WDir[] _safespotDirections = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => AOEs;

    public override void Init(BossModule module)
    {
        _bait = module.FindComponent<IaiGiriBait>();
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);

        // draw safespots (TODO: consider assigning specific side)
        var bait = _bait?.Instances.Find(i => i.Target == pc);
        if (bait?.DirOffsets.Count == 2)
        {
            var offset = bait.DirOffsets[1].Rad > 0 ? 5 : -5;
            foreach (var dir in _safespotDirections)
            {
                var safespot = module.Bounds.Center + 19 * dir;
                if (!AOEs.Any(aoe => aoe.Check(safespot)))
                {
                    arena.AddCircle(safespot + offset * dir.OrthoR(), 1, ArenaColor.Safe);
                }
            }
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        AOEShapeCircle? shape = (AID)spell.Action.ID switch
        {
            AID.NIronRainFirst or AID.SIronRainFirst => _shapeRain,
            AID.NIronStormFirst or AID.SIronStormFirst => _shapeStorm,
            _ => null
        };
        if (shape != null)
            AOEs.Add(new(shape, spell.LocXZ, default, spell.NPCFinishAt));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NIronRainFirst:
            case AID.SIronRainFirst:
            case AID.NIronStormFirst:
            case AID.SIronStormFirst:
                ++NumCasts;
                foreach (ref var aoe in AOEs.AsSpan())
                    aoe.Activation = module.WorldState.CurrentTime.AddSeconds(6.2f); // second aoe will happen at same location
                break;
            case AID.NIronRainSecond:
            case AID.SIronRainSecond:
            case AID.NIronStormSecond:
            case AID.SIronStormSecond:
                ++NumCasts;
                AOEs.Clear();
                break;
        }
    }
}
