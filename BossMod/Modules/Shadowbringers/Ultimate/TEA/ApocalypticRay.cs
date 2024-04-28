namespace BossMod.Shadowbringers.Ultimate.TEA;

class ApocalypticRay(BossModule module, bool faceCenter) : Components.GenericAOEs(module)
{
    public Actor? Source { get; private set; }
    private readonly bool _faceCenter = faceCenter;
    private Angle _rotation;
    private DateTime _activation;

    private readonly AOEShapeCone _shape = new(25.5f, 45.Degrees()); // TODO: verify angle

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Source != null)
            yield return new(_shape, Source.Position, _rotation, _activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ApocalypticRay:
                Source = caster;
                if (_faceCenter)
                {
                    _rotation = Angle.FromDirection(Module.Center - caster.Position);
                }
                else
                {
                    var target = WorldState.Actors.Find(caster.TargetID);
                    _rotation = target != null ? Angle.FromDirection(target.Position - caster.Position) : caster.Rotation; // this seems to be how it is baited
                }
                _activation = WorldState.FutureTime(0.6f);
                break;
            case AID.ApocalypticRayAOE:
                ++NumCasts;
                _activation = WorldState.FutureTime(1.1f);
                _rotation = caster.Rotation; // fix possible mistake
                break;
        }
    }
}

class P2ApocalypticRay(BossModule module) : ApocalypticRay(module, false);
class P3ApocalypticRay(BossModule module) : ApocalypticRay(module, true);
