namespace BossMod.Shadowbringers.Ultimate.TEA;

class ApocalypticRay : Components.GenericAOEs
{
    public Actor? Source { get; private set; }
    private bool _faceCenter;
    private Angle _rotation;
    private DateTime _activation;

    private AOEShapeCone _shape = new(25.5f, 45.Degrees()); // TODO: verify angle

    public ApocalypticRay(bool faceCenter)
    {
        _faceCenter = faceCenter;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (Source != null)
            yield return new(_shape, Source.Position, _rotation, _activation);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ApocalypticRay:
                Source = caster;
                if (_faceCenter)
                {
                    _rotation = Angle.FromDirection(module.Bounds.Center - caster.Position);
                }
                else
                {
                    var target = module.WorldState.Actors.Find(caster.TargetID);
                    _rotation = target != null ? Angle.FromDirection(target.Position - caster.Position) : caster.Rotation; // this seems to be how it is baited
                }
                _activation = module.WorldState.CurrentTime.AddSeconds(0.6f);
                break;
            case AID.ApocalypticRayAOE:
                ++NumCasts;
                _activation = module.WorldState.CurrentTime.AddSeconds(1.1f);
                _rotation = caster.Rotation; // fix possible mistake
                break;
        }
    }
}

class P2ApocalypticRay : ApocalypticRay
{
    public P2ApocalypticRay() : base(false) { }
}

class P3ApocalypticRay : ApocalypticRay
{
    public P3ApocalypticRay() : base(true) { }
}
