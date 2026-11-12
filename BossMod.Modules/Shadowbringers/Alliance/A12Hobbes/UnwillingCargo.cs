namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class UnwillingCargoRing(BossModule module) : Components.GenericAOEs(module)
{
    public bool Active { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active)
            yield return new(new AOEShapeDonut(17.5f, 20), new(-805, -270));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 4)
        {
            if (state == 0x00020001)
                Active = true;
            else if (state == 0x00080004)
                Active = false;
        }
    }
}

class UnwillingCargo(BossModule module) : Components.Knockback(module, AID.UnwillingCargo, stopAtWall: true)
{
    public DateTime Activation { get; private set; }

    // blue = knockback west, pink = knockback east
    enum Pattern
    {
        None,
        Blue,
        Pink,
        BluePink,
        PinkBlue
    }
    private Pattern _pattern;

    enum Color
    {
        None,
        Blue,
        Pink
    }

    static readonly WPos PlatformCenter = new(-805, -270);

    public bool IsAffected(Actor actor) => Activation > WorldState.CurrentTime && actor.Position.AlmostEqual(PlatformCenter, 20);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (!actor.Position.AlmostEqual(PlatformCenter, 20))
            yield break;

        var sign = _pattern is Pattern.Blue or Pattern.BluePink ? 1 : -1;

        switch (_pattern)
        {
            case Pattern.Blue:
            case Pattern.Pink:
                yield return new(PlatformCenter, 15, Activation, new AOEShapeRect(20, 20, 20), sign * 90.Degrees(), Kind.DirForward);
                break;
            case Pattern.BluePink:
            case Pattern.PinkBlue:
                yield return new(PlatformCenter - new WDir(0, 14), 15, Activation, new AOEShapeRect(20, 3.5f, 20), sign * 90.Degrees(), Kind.DirForward);
                yield return new(PlatformCenter - new WDir(0, 7), 15, Activation, new AOEShapeRect(20, 3.5f, 20), -sign * 90.Degrees(), Kind.DirForward);
                yield return new(PlatformCenter, 15, Activation, new AOEShapeRect(20, 3.5f, 20), sign * 90.Degrees(), Kind.DirForward);
                yield return new(PlatformCenter + new WDir(0, 7), 15, Activation, new AOEShapeRect(20, 3.5f, 20), -sign * 90.Degrees(), Kind.DirForward);
                yield return new(PlatformCenter + new WDir(0, 14), 15, Activation, new AOEShapeRect(20, 3.5f, 20), sign * 90.Degrees(), Kind.DirForward);
                break;
        }
    }

    // could return constant 15 to improve perf here, but visual hint would be incorrect (arrow extending outside platform)
    public override VFloat CalculateDistance(WVec position, WVec direction) => Intersect.RayCircleV(position - PlatformCenter, direction, 20);
    public override PointTest DestinationUnsafe(int slot, Actor actor) => ShapeDistance.InvertedCircle(PlatformCenter, 17.5f).Discretize();

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 5)
        {
            switch (state)
            {
                case 0x00200020:
                    SetPattern(Pattern.Blue);
                    break;
                case 0x01000100:
                    SetPattern(Pattern.Pink);
                    break;
                case 0x08000800:
                    SetPattern(Pattern.BluePink);
                    break;
                case 0x40004000:
                    SetPattern(Pattern.PinkBlue);
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _pattern = Pattern.None;
        }
    }

    private void SetPattern(Pattern p)
    {
        _pattern = p;
        Activation = WorldState.FutureTime(6.1f);
    }
}
