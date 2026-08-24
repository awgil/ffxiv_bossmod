namespace BossMod.Shadowbringers.Alliance.A23SuperiorFlightUnit;

class SlidingSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, Angle side, DateTime activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select((c, i) => new AOEInstance(new AOEShapeCone(200, 90.Degrees()), c.caster.Position, c.caster.Rotation + c.side, c.activation, Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE)).Take(2).Reverse();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var side = (AID)spell.Action.ID switch
        {
            AID.AlphaSlidingSwipeRight or AID.BetaSlidingSwipeRight or AID.ChiSlidingSwipeRight => -90.Degrees(),
            AID.AlphaSlidingSwipeLeft or AID.BetaSlidingSwipeLeft or AID.ChiSlidingSwipeLeft => 90.Degrees(),
            _ => default
        };
        if (side != default)
            _casters.Add((caster, side, Module.CastFinishAt(spell, 0.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SlidingSwipeLeft or AID.SlidingSwipeRight)
        {
            NumCasts++;
            if (_casters.Count > 0)
                _casters.RemoveAt(0);
        }
    }
}
