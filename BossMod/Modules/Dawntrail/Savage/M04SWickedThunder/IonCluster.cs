namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class StampedingThunder(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];
    public bool SmallArena;

    private static readonly AOEShapeRect _shape = new(40f, 15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IonClusterVisualR:
                AOE = [new(_shape, caster.Position - new WDir(5f, default), caster.Rotation, WorldState.FutureTime(2.4d))];
                break;
            case (uint)AID.IonClusterVisualL:
                AOE = [new(_shape, caster.Position + new WDir(5f, default), caster.Rotation, WorldState.FutureTime(2.4d))];
                break;
            case (uint)AID.StampedingThunderAOE:
                ++NumCasts;
                break;
            case (uint)AID.StampedingThunderFinish:
                ++NumCasts;
                AOE = [];
                Arena.Bounds = M04SWickedThunder.IonClusterBounds;
                Arena.Center = new(M04SWickedThunder.P1DefaultCenter.X + 3f * (M04SWickedThunder.P1DefaultCenter.X - caster.PosRot.X), M04SWickedThunder.P1DefaultCenter.Z);
                SmallArena = true;
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state is 0x00400004u or 0x00800004u)
        {
            Arena.Bounds = M04SWickedThunder.P1DefaultBounds;
            Arena.Center = M04SWickedThunder.P1DefaultCenter;
            SmallArena = false;
        }
    }
}

sealed class ElectronStream(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _posCaster;
    private Actor? _negCaster;
    private BitMask _positron;
    private BitMask _negatron;

    private static readonly AOEShapeRect _shape = new(40f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        if (_posCaster?.CastInfo != null)
            aoes.Add(new(_shape, _posCaster.Position, _posCaster.CastInfo.Rotation, Module.CastFinishAt(_posCaster.CastInfo), _positron[slot] ? default : Colors.SafeFromAOE, _positron[slot]));
        if (_negCaster?.CastInfo != null)
            aoes.Add(new(_shape, _negCaster.Position, _negCaster.CastInfo.Rotation, Module.CastFinishAt(_negCaster.CastInfo), _negatron[slot] ? default : Colors.SafeFromAOE, _negatron[slot]));
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        // wild charge
        var offZ = actor.PosRot.Z - Module.PrimaryActor.PosRot.Z;
        var sameSideCloser = Raid.WithoutSlot(false, true, true).Where(p => p != actor && p.PosRot.Z - Module.PrimaryActor.PosRot.Z is var off && off * offZ > 0f && Math.Abs(off) < Math.Abs(offZ));
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

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.Positron:
                _positron[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.Negatron:
                _negatron[Raid.FindSlot(actor.InstanceID)] = true;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.PositronStream:
                _posCaster = caster;
                break;
            case (uint)AID.NegatronStream:
                _negCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.PositronStream:
                _posCaster = null;
                break;
            case (uint)AID.NegatronStream:
                _negCaster = null;
                break;
        }
    }
}

sealed class ElectronStreamCurrent(BossModule module) : Components.GenericAOEs(module, (uint)AID.AxeCurrent)
{
    private readonly uint[] _status = new uint[PartyState.MaxPartySize];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shapeCircle = new(2f);
    private static readonly AOEShapeDonut _shapeDonut = new(10f, 25f);
    private static readonly AOEShapeCone _shapeBait = new(50f, 12.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var actorOffset = actor.PosRot.Z - Module.PrimaryActor.PosRot.Z;
        var aoes = new List<AOEInstance>();
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            switch (_status[i])
            {
                case (uint)SID.RemoteCurrent:
                    if (_status[slot] == (uint)SID.ColliderConductor && (p.PosRot.Z - Module.PrimaryActor.PosRot.Z) * actorOffset < 0f)
                        break; // we're gonna bait this
                    if (FindBaitTarget(i, p) is var tf && tf != null)
                        aoes.Add(new(_shapeBait, p.Position, Angle.FromDirection(tf.Position - p.Position), _activation, risky: _status[slot] != (uint)SID.RemoteCurrent)); // common strat has two remotes hitting each other, which is fine
                    break;
                case (uint)SID.ProximateCurrent:
                    if (_status[slot] == (uint)SID.ColliderConductor && (p.PosRot.Z - Module.PrimaryActor.PosRot.Z) * actorOffset > 0f)
                        break; // we're gonna bait this
                    if (FindBaitTarget(i, p) is var tc && tc != null)
                        aoes.Add(new(_shapeBait, p.Position, Angle.FromDirection(tc.Position - p.Position), _activation));
                    break;
                case (uint)SID.SpinningConductor:
                    aoes.Add(new(_shapeCircle, p.Position, default, _activation));
                    break;
                case (uint)SID.RoundhouseConductor:
                    aoes.Add(new(_shapeDonut, p.Position, default, _activation));
                    break;
            }
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_status[slot] == (uint)SID.ColliderConductor)
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
            case (uint)SID.RemoteCurrent:
                if (FindBaitTarget(pcSlot, pc) is var tf && tf != null)
                    _shapeBait.Outline(Arena, pc.Position, Angle.FromDirection(tf.Position - pc.Position));
                break;
            case (uint)SID.ProximateCurrent:
                if (FindBaitTarget(pcSlot, pc) is var tc && tc != null)
                    _shapeBait.Outline(Arena, pc.Position, Angle.FromDirection(tc.Position - pc.Position));
                break;
            case (uint)SID.SpinningConductor:
                _shapeCircle.Outline(Arena, pc);
                break;
            case (uint)SID.RoundhouseConductor:
                _shapeDonut.Outline(Arena, pc);
                break;
            case (uint)SID.ColliderConductor:
                var source = FindDesignatedBaitSource(pc);
                if (source.actor != null && FindBaitTarget(source.slot, source.actor) is var target && target != null)
                    _shapeBait.Outline(Arena, source.actor.Position, Angle.FromDirection(target.Position - source.actor.Position), Colors.Safe);
                break;
        }

        foreach (var p in SafeSpots(pcSlot, pc))
            Arena.ZoneCircleOutline(p, 1, Colors.Safe);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots(slot, actor))
            movementHints.Add(actor.Position, p, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.RemoteCurrent or (uint)SID.ProximateCurrent or (uint)SID.SpinningConductor or (uint)SID.RoundhouseConductor or (uint)SID.ColliderConductor)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _status[slot] = status.ID;
            _activation = status.ExpireAt;
        }
    }

    private (int slot, Actor? actor) FindDesignatedBaitSource(Actor target)
    {
        var targetOffset = target.PosRot.Z - Module.PrimaryActor.PosRot.Z;
        bool isBaiter(int slot, Actor actor) => _status[slot] switch
        {
            (uint)SID.RemoteCurrent => (actor.PosRot.Z - Module.PrimaryActor.PosRot.Z) * targetOffset < 0,
            (uint)SID.ProximateCurrent => (actor.PosRot.Z - Module.PrimaryActor.PosRot.Z) * targetOffset > 0,
            _ => false
        };
        return Raid.WithSlot(false, true, true).FirstOrDefault(ip => isBaiter(ip.Item1, ip.Item2));
    }

    private Actor? FindBaitTarget(int slot, Actor source) => _status[slot] switch
    {
        (uint)SID.RemoteCurrent => Raid.WithoutSlot(false, true, true).Exclude(source).Farthest(source.Position),
        (uint)SID.ProximateCurrent => Raid.WithoutSlot(false, true, true).Exclude(source).Closest(source.Position),
        _ => null
    };

    private List<WPos> SafeSpots(int slot, Actor actor)
    {
        var dirZ = actor.PosRot.Z - Module.PrimaryActor.PosRot.Z > 0f ? 1f : -1f;
        var positions = new List<WPos>(2);
        switch (_status[slot])
        {
            case (uint)SID.RemoteCurrent:
            case (uint)SID.ProximateCurrent:
                positions.Add(Module.PrimaryActor.Position + new WDir(default, 5f * dirZ));
                break;
            case (uint)SID.SpinningConductor:
            case (uint)SID.RoundhouseConductor:
                positions.Add(Module.PrimaryActor.Position + new WDir(-2.5f, 2.5f * dirZ));
                positions.Add(Module.PrimaryActor.Position + new WDir(+2.5f, 2.5f * dirZ));
                break;
            case (uint)SID.ColliderConductor:
                positions.Add(Module.PrimaryActor.Position + new WDir(default, 6f * dirZ));
                break;
        }
        return positions;
    }
}
