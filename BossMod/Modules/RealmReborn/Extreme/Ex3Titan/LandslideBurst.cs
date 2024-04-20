namespace BossMod.RealmReborn.Extreme.Ex3Titan;

// burst (bomb explosion) needs to be shown in particular moment (different for different patterns) so that ai can avoid them nicely
class LandslideBurst(BossModule module) : Components.GenericAOEs(module)
{
    public int MaxBombs = 9;
    private readonly List<Actor> _landslides = [];
    private readonly List<Actor> _bursts = []; // TODO: reconsider: we can start showing bombs even before cast starts...
    public int NumActiveBursts => _bursts.Count;

    private static readonly AOEShapeRect _shapeLandslide = new(40.25f, 3);
    private static readonly AOEShapeCircle _shapeBurst = new(6.3f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var l in _landslides)
            yield return new(_shapeLandslide, l.Position, l.CastInfo!.Rotation, l.CastInfo.NPCFinishAt);
        foreach (var b in _bursts.Take(MaxBombs))
            yield return new(_shapeBurst, b.Position, b.CastInfo!.Rotation, b.CastInfo.NPCFinishAt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LandslideBoss:
            case AID.LandslideHelper:
            case AID.LandslideGaoler:
                _landslides.Add(caster);
                break;
            case AID.Burst:
                _bursts.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LandslideBoss:
            case AID.LandslideHelper:
            case AID.LandslideGaoler:
                _landslides.Remove(caster);
                break;
            case AID.Burst:
                _bursts.Remove(caster);
                break;
        }
    }
}
