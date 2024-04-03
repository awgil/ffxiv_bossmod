namespace BossMod.Endwalker.Alliance.A35Eulogia;

class SolarFans : BossComponent
{
    private readonly List<(Actor, AOEShapeRect)> _start = [];
    private bool _rhythmActive;
    private readonly List<Actor> _finish = [];

    private static readonly float _flightRadiusInner = 20; // TODO: check whether this is correct
    private static readonly float _flightRadiusOuter = 40;
    private static readonly AOEShapeCircle _aoeFinish = new(25);

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var (fan, shape) in _start)
        {
            shape.Draw(arena, fan);
        }

        if (_rhythmActive)
        {
            foreach (var flame in module.Enemies(OID.WardensFlame))
            {
                var dir = Angle.FromDirection(flame.Position - module.Bounds.Center) + 45.Degrees();
                arena.ZoneCone(module.Bounds.Center, _flightRadiusInner, _flightRadiusOuter, dir, 45.Degrees(), ArenaColor.AOE);
            }
        }

        foreach (var finish in _finish)
        {
            _aoeFinish.Draw(arena, finish);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SolarFansAOE:
                AOEShapeRect shape = new(0, 5);
                shape.SetEndPointFromCastLocation(caster);
                _start.Add((caster, shape));
                break;
            case AID.RadiantFlourish:
                _rhythmActive = false;
                _finish.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SolarFansAOE:
                _start.RemoveAll(e => e.Item1 == caster);
                _rhythmActive = true;
                break;
            case AID.RadiantFlourish:
                _finish.Remove(caster);
                break;
        }
    }

    private bool ActorInRhythmAOE(WPos center, Actor flame, Actor player)
    {
        return !player.Position.InCircle(center, _flightRadiusInner) && player.Position.InCone(center, Angle.FromDirection(flame.Position - center) + 45.Degrees(), 45.Degrees());
    }
}