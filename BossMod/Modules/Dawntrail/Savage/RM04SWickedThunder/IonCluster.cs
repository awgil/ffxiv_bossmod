namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class StampedingThunder(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? AOE;
    public bool SmallArena;

    private static readonly AOEShapeRect _shape = new(40, 15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IonClusterVisualR:
                AOE = new(_shape, caster.Position - new WDir(5, 0), caster.Rotation, WorldState.FutureTime(2.4f));
                break;
            case AID.IonClusterVisualL:
                AOE = new(_shape, caster.Position + new WDir(5, 0), caster.Rotation, WorldState.FutureTime(2.4f));
                break;
            case AID.StampedingThunderAOE:
                ++NumCasts;
                break;
            case AID.StampedingThunderFinish:
                ++NumCasts;
                AOE = null;
                Module.Arena.Bounds = caster.Position.X < Module.Center.X ? RM04SWickedThunder.P1IonClusterRBounds : RM04SWickedThunder.P1IonClusterLBounds;
                SmallArena = true;
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0 && state is 0x00400004 or 0x00800004)
        {
            Module.Arena.Bounds = RM04SWickedThunder.P1DefaultBounds;
            SmallArena = false;
        }
    }
}

class ElectronStream(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _posCaster;
    private Actor? _negCaster;
    private BitMask _positron;
    private BitMask _negatron;

    private static readonly AOEShapeRect _shape = new(40, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_posCaster?.CastInfo != null)
            yield return new(_shape, _posCaster.Position, _posCaster.CastInfo.Rotation, Module.CastFinishAt(_posCaster.CastInfo), _positron[slot] ? ArenaColor.AOE : ArenaColor.SafeFromAOE, _positron[slot]);
        if (_negCaster?.CastInfo != null)
            yield return new(_shape, _negCaster.Position, _negCaster.CastInfo.Rotation, Module.CastFinishAt(_negCaster.CastInfo), _negatron[slot] ? ArenaColor.AOE : ArenaColor.SafeFromAOE, _negatron[slot]);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        // wild charge
        var offZ = actor.Position.Z - Module.PrimaryActor.Position.Z;
        var sameSideCloser = Raid.WithoutSlot().Where(p => p != actor && p.Position.Z - Module.PrimaryActor.Position.Z is var off && off * offZ > 0 && Math.Abs(off) < Math.Abs(offZ));
        if (actor.Role == Role.Tank)
        {
            if (sameSideCloser.Any())
                hints.Add("Move closer!");
        }
        else
        {
            if (!sameSideCloser.Any(p => p.Role == Role.Tank))
                hints.Add("Hide behind tank!");
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Positron:
                _positron.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.Negatron:
                _negatron.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PositronStream:
                _posCaster = caster;
                break;
            case AID.NegatronStream:
                _negCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PositronStream:
                _posCaster = null;
                break;
            case AID.NegatronStream:
                _negCaster = null;
                break;
        }
    }
}

class ElectronStreamCurrent(BossModule module) : Components.GenericAOEs(module, AID.AxeCurrent)
{
    private readonly SID[] _status = new SID[PartyState.MaxPartySize];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shapeCircle = new(2);
    private static readonly AOEShapeDonut _shapeDonut = new(10, 25);
    private static readonly AOEShapeCone _shapeBait = new(50, 12.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var actorOffset = actor.Position.Z - Module.PrimaryActor.Position.Z;
        foreach (var (i, p) in Raid.WithSlot().Exclude(slot))
        {
            switch (_status[i])
            {
                case SID.RemoteCurrent:
                    if (_status[slot] == SID.ColliderConductor && (p.Position.Z - Module.PrimaryActor.Position.Z) * actorOffset < 0)
                        break; // we're gonna bait this
                    if (FindBaitTarget(i, p) is var tf && tf != null)
                        yield return new(_shapeBait, p.Position, Angle.FromDirection(tf.Position - p.Position), _activation, Risky: _status[slot] != SID.RemoteCurrent); // common strat has two remotes hitting each other, which is fine
                    break;
                case SID.ProximateCurrent:
                    if (_status[slot] == SID.ColliderConductor && (p.Position.Z - Module.PrimaryActor.Position.Z) * actorOffset > 0)
                        break; // we're gonna bait this
                    if (FindBaitTarget(i, p) is var tc && tc != null)
                        yield return new(_shapeBait, p.Position, Angle.FromDirection(tc.Position - p.Position), _activation);
                    break;
                case SID.SpinningConductor:
                    yield return new(_shapeCircle, p.Position, default, _activation);
                    break;
                case SID.RoundhouseConductor:
                    yield return new(_shapeDonut, p.Position, default, _activation);
                    break;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_status[slot] == SID.ColliderConductor)
        {
            var source = FindDesignatedBaitSource(actor);
            if (source.actor != null)
            {
                var target = FindBaitTarget(source.slot, source.actor);
                if (target != null && !_shapeBait.Check(actor.Position, source.actor.Position, Angle.FromDirection(target.Position - source.actor.Position)))
                    hints.Add("Get hit by the cone!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        switch (_status[pcSlot])
        {
            case SID.RemoteCurrent:
                if (FindBaitTarget(pcSlot, pc) is var tf && tf != null)
                    _shapeBait.Outline(Arena, pc.Position, Angle.FromDirection(tf.Position - pc.Position));
                break;
            case SID.ProximateCurrent:
                if (FindBaitTarget(pcSlot, pc) is var tc && tc != null)
                    _shapeBait.Outline(Arena, pc.Position, Angle.FromDirection(tc.Position - pc.Position));
                break;
            case SID.SpinningConductor:
                _shapeCircle.Outline(Arena, pc);
                break;
            case SID.RoundhouseConductor:
                _shapeDonut.Outline(Arena, pc);
                break;
            case SID.ColliderConductor:
                var source = FindDesignatedBaitSource(pc);
                if (source.actor != null && FindBaitTarget(source.slot, source.actor) is var target && target != null)
                    _shapeBait.Outline(Arena, source.actor.Position, Angle.FromDirection(target.Position - source.actor.Position), ArenaColor.Safe);
                break;
        }

        foreach (var p in SafeSpots(pcSlot, pc))
            Arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots(slot, actor))
            movementHints.Add(actor.Position, p, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.RemoteCurrent or SID.ProximateCurrent or SID.SpinningConductor or SID.RoundhouseConductor or SID.ColliderConductor)
        {
            if (Raid.TryFindSlot(actor.InstanceID, out var slot))
                _status[slot] = (SID)status.ID;
            _activation = status.ExpireAt;
        }
    }

    private (int slot, Actor? actor) FindDesignatedBaitSource(Actor target)
    {
        var targetOffset = target.Position.Z - Module.PrimaryActor.Position.Z;
        bool isBaiter(int slot, Actor actor) => _status[slot] switch
        {
            SID.RemoteCurrent => (actor.Position.Z - Module.PrimaryActor.Position.Z) * targetOffset < 0,
            SID.ProximateCurrent => (actor.Position.Z - Module.PrimaryActor.Position.Z) * targetOffset > 0,
            _ => false
        };
        return Raid.WithSlot().FirstOrDefault(ip => isBaiter(ip.Item1, ip.Item2));
    }

    private Actor? FindBaitTarget(int slot, Actor source) => _status[slot] switch
    {
        SID.RemoteCurrent => Raid.WithoutSlot().Exclude(source).Farthest(source.Position),
        SID.ProximateCurrent => Raid.WithoutSlot().Exclude(source).Closest(source.Position),
        _ => null
    };

    private IEnumerable<WPos> SafeSpots(int slot, Actor actor)
    {
        var dirZ = actor.Position.Z - Module.PrimaryActor.Position.Z > 0 ? 1 : -1;
        switch (_status[slot])
        {
            case SID.RemoteCurrent:
            case SID.ProximateCurrent:
                yield return Module.PrimaryActor.Position + new WDir(0, 5 * dirZ);
                break;
            case SID.SpinningConductor:
            case SID.RoundhouseConductor:
                yield return Module.PrimaryActor.Position + new WDir(-2.5f, 2.5f * dirZ);
                yield return Module.PrimaryActor.Position + new WDir(+2.5f, 2.5f * dirZ);
                break;
            case SID.ColliderConductor:
                yield return Module.PrimaryActor.Position + new WDir(0, 6 * dirZ);
                break;
        }
    }
}
