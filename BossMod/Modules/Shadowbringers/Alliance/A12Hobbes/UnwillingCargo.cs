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

    private static WDir ExpectedDirection(Pattern pattern, WPos p)
    {
        var stripe = MathF.Abs(p.Z + 270) switch
        {
            < 3.5f => 1,
            < 10.5f => 0,
            < 17.5f => 1,
            _ => -1
        };
        if (stripe < 0)
            return default;

        var blue = new WDir(-15, 0);
        var pink = new WDir(15, 0);

        return pattern switch
        {
            Pattern.Blue => blue,
            Pattern.Pink => pink,
            Pattern.BluePink => stripe == 1 ? blue : pink,
            Pattern.PinkBlue => stripe == 1 ? pink : blue,
            _ => default
        };

    }

    public bool IsAffected(Actor actor) => Activation > WorldState.CurrentTime && MathF.Abs(actor.Position.Z + 270) < 20;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !pos.InCircle(new WPos(-805, -270), 17.5f);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (IsImmune(slot, Activation))
            yield break;

        var dir = ExpectedDirection(_pattern, actor.Position);
        if (dir != default)
            yield return new(actor.Position, 15, Activation, Direction: Angle.FromDirection(dir), Kind: Kind.DirForward);
    }

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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            var dir = src.Direction.ToDirection() * src.Distance;
            var pat = _pattern;
            hints.AddForbiddenZone(p =>
            {
                var dir = ExpectedDirection(pat, p);
                return dir != default && !(p + dir).InCircle(new WPos(-805, -270), 17.5f);
            }, Activation);
        }
    }
}
