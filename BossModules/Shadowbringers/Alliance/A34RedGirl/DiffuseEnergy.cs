namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class DiffuseEnergy(BossModule module) : Components.GenericRotatingAOE(module)
{
    enum Direction
    {
        Unknown,
        CW,
        CCW
    }

    private readonly Dictionary<ulong, Direction> _direction = [];
    private readonly List<Actor> _casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DiffuseEnergy)
        {
            _casters.Add(caster);
            if (_direction.TryGetValue(caster.InstanceID, out var dir))
                AddCaster(caster, dir);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.TurnLeft:
                if (_casters.Contains(actor))
                    AddCaster(actor, Direction.CCW);
                else
                    _direction[actor.InstanceID] = Direction.CCW;
                break;
            case IconID.TurnRight:
                if (_casters.Contains(actor))
                    AddCaster(actor, Direction.CW);
                else
                    _direction[actor.InstanceID] = Direction.CW;
                break;
        }
    }

    private void AddCaster(Actor caster, Direction dir)
    {
        Sequences.Add(new(new AOEShapeCone(12, 60.Degrees()), caster.Position, caster.Rotation, dir == Direction.CCW ? 120.Degrees() : -120.Degrees(), Module.CastFinishAt(caster.CastInfo), 2.8f, 6));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DiffuseEnergy or AID.DiffuseEnergyRepeat)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}
