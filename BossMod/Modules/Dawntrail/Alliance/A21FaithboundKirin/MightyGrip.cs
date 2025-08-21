namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

class ChiseledArm(BossModule module) : Components.AddsMulti(module, [OID.ChiseledArm3, OID.ChiseledArm4], 1);

class Shockwave(BossModule module) : Components.RaidwideInstant(module, AID.Shockwave, 9)
{
    public void Predict(float delay) => Activation = WorldState.FutureTime(delay);
}

class StandingFirm(BossModule module) : Components.GenericTowers(module, AID.Bury)
{
    private static readonly WPos[] _towers = [
        new(-858, 781),
        new(-858, 789),
        new(-842, 781)
    ];

    public override void OnEventEnvControl(byte index, uint state)
    {
        // ignore tower 0x51, the NPC helper soaks it
        if (index is >= 0x4E and <= 0x50)
        {
            if (state == 0x00020001)
                Towers.Add(new(_towers[index - 0x4E], 3, 1, 1, Raid.WithSlot().WhereActor(r => r.Role != Role.Tank).Mask(), WorldState.FutureTime(11)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(spell.TargetXZ, 1));
            NumCasts++;
        }
    }
}

class MightyGrip : Components.GenericAOEs
{
    private DateTime _activation;

    public bool Transformed { get; private set; }

    private readonly AOEShapeCustom _borderShape;

    public MightyGrip(BossModule module) : base(module, AID.MightyGrip)
    {
        var blank = new PolygonClipper.Operand(CurveApprox.Rect(new(0, 30), new(30, 0)));
        var safe = new PolygonClipper.Operand(CurveApprox.Rect(new(12.5f, 0), new(0, 15)).Select(d => d + new WDir(0, 5)));
        var hole = module.Bounds.Clipper.Difference(blank, safe);
        _borderShape = new(hole);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(_borderShape, Arena.Center, Activation: _activation);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x46)
        {
            if (state == 0x00200010)
                _activation = WorldState.FutureTime(11.1f);

            if (state == 0x00020001)
            {
                Transformed = true;
                _activation = default;
                Arena.Bounds = new ArenaBoundsRect(12.5f, 15);
                Arena.Center = new(-850, 785);
            }

            if (state == 0x00080004)
            {
                Transformed = false;
                Arena.Bounds = new ArenaBoundsCircle(29.5f);
                Arena.Center = new(-850, 780);
            }
        }
    }
}
