namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class LightningStorm(BossModule module) : Components.BaitAwayIcon(module, 8f, (uint)IconID.LightningStorm, (uint)AID.LightningStorm, 9.1f);

sealed class LightningStormHint(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoeRisk = [];
    private AOEInstance[] _aoeSave = [];
    private BitMask targets;
    private readonly PolygonCustom bridgeEast = new([new(102.813f, 107.202f), new(106.781f, 110.245f), new(111.651f, 103.898f), new(107.684f, 100.853f)]);
    private readonly PolygonCustom bridgeNorth = new([new(104.831f, 93.963f), new(105.482f, 89.005f), new(97.550f, 87.961f), new(96.903f, 92.876f)]);
    private readonly PolygonCustom bridgeWest = new([new(92.318f, 98.853f), new(87.738f, 100.750f), new(90.838f, 108.124f), new(95.419f, 106.228f)]);
    private readonly Highlightning _aoe = module.FindComponent<Highlightning>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (targets[slot])
        {
            return _aoeRisk;
        }
        else
        {
            return _aoeSave;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LightningStorm)
        {
            targets.Set(Raid.FindSlot(actor.InstanceID));
            if (_aoeRisk.Length == 0)
            {
                UpdateAOE();
            }
        }
    }

    public void UpdateAOE()
    {
        if (targets == default || _aoe.AOE.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoe.AOE[0];
        var center = Arena.Center;
        AOEShapeCustom shape = new(center, [bridgeEast, bridgeNorth, bridgeWest], [new Polygon(aoe.Origin, 21f, 10)]);
        var act = WorldState.FutureTime(9.8d);
        _aoeRisk = [new(shape, center, default, act, shapeDistance: shape.Distance(center, default))];
        _aoeSave = [new(shape, center, default, act, Colors.SafeFromAOE, shapeDistance: shape.InvertedDistance(center, default))];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.LevinDrop or (uint)AID.LevinMerengue)
        {
            _aoeRisk = [];
            _aoeSave = [];
            targets = default;
            ++NumCasts;
            if (id == (uint)AID.LevinMerengue) // if merengue happens it only casts once instead of twice
            {
                ++NumCasts;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoeRisk.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoeRisk[0];
        var isRisky = aoe.Check(actor.Position);
        if (targets[slot])
        {
            hints.Add("Stay away from bridges and go to different islands!", isRisky);
        }
        else
        {
            hints.Add("Stay on bridges!", !isRisky);
        }
    }
}
