namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class RainingCatsTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(100, 35.Degrees()), (uint)TetherID.RainingCats, AID.RainingCatsTether) // TODO: verify angle
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Module.Raid.FindSlot(t.ID));
        }
    }
}

class RainingCatsStack(BossModule module) : Components.UniformStackSpread(module, 4, 0, 3)
{
    private readonly RainingCatsTether? _tether = module.FindComponent<RainingCatsTether>();
    private DateTime _activation;

    public override void Update()
    {
        Stacks.Clear();
        if (_activation != default)
        {
            var tetherTargets = new BitMask();
            if (_tether != null)
                foreach (var t in _tether.CurrentBaits)
                    tetherTargets.Set(Module.Raid.FindSlot(t.Target.InstanceID));

            var closest = Module.Raid.WithoutSlot().Closest(Module.PrimaryActor.Position);
            var farthest = Module.Raid.WithoutSlot().Farthest(Module.PrimaryActor.Position);
            if (closest != null)
                AddStack(closest, _activation, tetherTargets);
            if (farthest != null)
                AddStack(farthest, _activation, tetherTargets);
        }
        base.Update();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RainingCatsFirst or AID.RainingCatsMid or AID.RainingCatsLast)
            _activation = Module.CastFinishAt(spell, 0.8f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RainingCatsStack)
            _activation = default;
    }
}
