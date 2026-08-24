namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class ColdGrip(BossModule module) : Components.GenericAOEs(module, AID.ColdGripAOE)
{
    private Actor? _leftHand;
    private Actor? _rightHand;

    enum Side
    {
        Unknown,
        Left,
        Right
    }

    private Side _safeSide;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_leftHand is { } h)
            yield return new AOEInstance(new AOEShapeRect(100, 6), h.CastInfo!.LocXZ, h.CastInfo!.Rotation, Module.CastFinishAt(h.CastInfo));
        if (_rightHand is { } h2)
            yield return new AOEInstance(new AOEShapeRect(100, 6), h2.CastInfo!.LocXZ, h2.CastInfo!.Rotation, Module.CastFinishAt(h2.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (spell.LocXZ.X > Arena.Center.X)
                _rightHand = caster;
            else
                _leftHand = caster;
        }

        if ((AID)spell.Action.ID == AID.ColdGripLeftSafe)
            _safeSide = Side.Left;
        if ((AID)spell.Action.ID == AID.ColdGripRightSafe)
            _safeSide = Side.Right;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;

            if (_rightHand == caster)
                _rightHand = null;
            if (_leftHand == caster)
                _leftHand = null;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (_rightHand != null && _leftHand != null)
        {
            var x = _safeSide == Side.Left ? 95 : 105;
            Arena.ZoneRect(new WPos(x, 85), default(Angle), 30, 0, 1, ArenaColor.SafeFromAOE);
        }
    }
}

class ExistentialDread(BossModule module) : Components.GenericAOEs(module, AID.ExistentialDread)
{
    enum Side
    {
        Unknown,
        Left,
        Right
    }

    private Side _safeSide;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            var src = Module.PrimaryActor.Position;
            if (_safeSide == Side.Right)
                src.X -= 6;
            else
                src.X += 6;

            yield return new(new AOEShapeRect(100, 12), src, default, _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ColdGripLeftSafe:
                _safeSide = Side.Left;
                break;
            case AID.ColdGripRightSafe:
                _safeSide = Side.Right;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ColdGripAOE && _activation == default)
            _activation = WorldState.FutureTime(1.6f);

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_safeSide != default)
            hints.Add($"Safe side: {_safeSide}");
    }
}
