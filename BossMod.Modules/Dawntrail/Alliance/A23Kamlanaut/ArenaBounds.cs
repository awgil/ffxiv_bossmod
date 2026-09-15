namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

class ProvingGround(BossModule module) : Components.GenericAOEs(module, AID.ProvingGround)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new AOEInstance(new AOEShapeCircle(5), Arena.Center, Activation: _activation);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.ProvingGround)
            _activation = default;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell);
    }
}

class ArenaBounds(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new AOEInstance(new AOEShapeDonut(20, 50), Arena.Center, default, _activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 2 && state == 0x00010001)
        {
            var arenaSmall = CurveApprox.Circle(20, 1 / 90f);
            IEnumerable<WDir> bridge(Angle a) => CurveApprox.Rect(new(5, 0), new(0, 20)).Select(d => (d + new WDir(0, 20)).Rotate(a));

            var oper = new PolygonClipper.Operand(bridge(180.Degrees()));
            oper.AddContour(bridge(60.Degrees()));
            oper.AddContour(bridge(-60.Degrees()));

            var arenaBig = Arena.Bounds.Clipper.Union(new(arenaSmall), oper);
            Arena.Bounds = new ArenaBoundsCustom(30, arenaBig);
        }

        if (index == 0x63)
        {
            if (state == 0x00200010)
                _activation = WorldState.FutureTime(4.3f);

            if (state == 0x00080004)
                _activation = default;
        }
    }
}
