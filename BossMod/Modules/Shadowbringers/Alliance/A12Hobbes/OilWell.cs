namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class OilWell(BossModule module) : Components.GenericAOEs(module, AID.OilWell)
{
    public DateTime Activation { get; private set; }
    private bool _inverted;
    private static readonly List<WPos> Platforms = MakePlatforms();

    private static List<WPos> MakePlatforms()
    {
        var centers = CurveApprox.Rect(new(8, 0), new(0, 8));
        WPos platformCenter = new(-779, -225);
        return [.. centers.Select(c => platformCenter + c.Rotate(-120.Degrees()))];
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation != default)
        {
            foreach (var c in Platforms)
                yield return new AOEInstance(new AOEShapeCircle(6), c, Activation: Activation, Risky: !_inverted, Color: _inverted ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default && Relevant(actor))
        {
            hints.AddForbiddenZone(p => _inverted ? !OnPlatform(p) : OnPlatform(p), Activation);
        }
    }

    bool Relevant(Actor a) => a.Position.InCircle(new(-779, -225), 20);
    bool OnPlatform(WPos a) => Platforms.Any(p => p.InCircle(a, 6));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (Activation != default && _inverted && Relevant(actor) && !OnPlatform(actor.Position))
            hints.Add("Go to platform!");
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 6)
        {
            switch (state)
            {
                case 0x01000080:
                    Activation = WorldState.FutureTime(4.2f);
                    _inverted = false;
                    break;
                case 0x00200010:
                    Activation = WorldState.FutureTime(4.2f);
                    _inverted = true;
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Activation = default;
        }
    }
}
