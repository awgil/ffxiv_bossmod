namespace BossMod.Dawntrail.Alliance.A11Prishe;

class CrystallineThorns(BossModule module) : Components.CastCounter(module, AID.Thornbite)
{
    public DateTime Activation;
    private RelSimplifiedComplexPolygon? _poly;
    private List<RelTriangle>? _triangulation;

    public bool Active => Activation != default;
    public bool Dangerous(WPos pos) => _poly?.Contains(pos - Module.Center) ?? false;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Dangerous(actor.Position))
            hints.Add("GTFO from spikes!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_triangulation != null)
            Arena.Zone(_triangulation, ArenaColor.AOE);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 1)
            return;

        switch (state)
        {
            case 0x00020001: // telegraph
            case 0x02000100:
                Activation = WorldState.FutureTime(5.1f);
                _poly = BuildPolygon(state == 0x02000100);
                _triangulation = _poly.Triangulate();
                break;
            case 0x00200010: // activation
            case 0x08000400:
                Activation = WorldState.CurrentTime;
                break;
            case 0x00080004: // deactivation
            case 0x00800004:
                Activation = default;
                _poly = null;
                _triangulation = null;
                break;
        }
    }

    private static RelSimplifiedComplexPolygon BuildPolygon(bool rotate)
    {
        // exterior
        var poly = new RelPolygonWithHoles([new(-35, +35), new(-35, -5), new(-25, -5), new(-25, -25), new(+5, -25), new(+5, -35), new(+35, -35), new(+35, +5), new(+25, +5), new(+25, +25), new(-5, +25), new(-5, +35)]);
        // hole
        poly.HoleStarts.Add(poly.Vertices.Count);
        poly.Vertices.AddRange([new(-15, +15), new(-15, -5), new(-5, -5), new(-5, -15), new(+15, -15), new(+15, +5), new(+5, +5), new(+5, +15)]);
        if (rotate)
            foreach (ref var v in poly.Vertices.AsSpan())
                v = v.OrthoR();
        return new([poly]);
    }
}

class AuroralUppercut(BossModule module) : Components.Knockback(module, AID.AuroralUppercutAOE, true)
{
    private readonly CrystallineThorns? _thorns = module.FindComponent<CrystallineThorns>();
    private float _distance;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_distance > 0)
            yield return new(Module.Center, _distance, _activation);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_thorns?.Dangerous(pos) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = (AID)spell.Action.ID switch
        {
            AID.AuroralUppercut1 => 12,
            AID.AuroralUppercut2 => 24,
            AID.AuroralUppercut3 => 36,
            _ => 0
        };
        if (distance > 0)
        {
            _distance = distance;
            _activation = Module.CastFinishAt(spell, 1.4f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _distance = 0;
            _activation = default;
        }
    }
}
