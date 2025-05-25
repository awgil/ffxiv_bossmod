namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class Wipe(BossModule module) : Components.GenericAOEs(module)
{
    record class AOEShapeTri(WDir A, WDir B, WDir C) : AOEShape
    {
        public override bool Check(WPos position, WPos origin, Angle rotation) => position.InTri(origin + A.Rotate(rotation), origin + B.Rotate(rotation), origin + C.Rotate(rotation));
        public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.ZoneTri(origin + A.Rotate(rotation), origin + B.Rotate(rotation), origin + C.Rotate(rotation), color);
        public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.AddTriangle(origin + A.Rotate(rotation), origin + B.Rotate(rotation), origin + C.Rotate(rotation), color);
    }

    record class Meteor(Actor Caster, Shade Shade, List<WPos> Blockers);

    private readonly List<Meteor> _meteors = [];
    private readonly Barrier _barriers = module.FindComponent<Barrier>()!;
    private bool _risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_risky)
            yield break;

        foreach (var m in _meteors)
        {
            for (var i = 0; i < m.Blockers.Count - 1; i++)
            {
                yield return new AOEInstance(new AOEShapeTri(default, m.Blockers[i] - m.Caster.Position, m.Blockers[i + 1] - m.Caster.Position), m.Caster.Position, default, Activation: Module.CastFinishAt(m.Caster.CastInfo));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var color = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_WipeWhite => Shade.White,
            AID._Weaponskill_WipeBlack => Shade.Black,
            _ => default
        };
        if (color != default)
        {
            _meteors.Add(new(caster, color, []));
            UpdateWalls();
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index - 0x1D is >= 0 and < 28)
            UpdateWalls();
    }

    private void UpdateWalls()
    {
        if (_meteors.Count == 1)
            _risky = _barriers.NumBarriers < 12;
        else if (_meteors.Count == 2)
            _risky = !(Module.FindComponent<Point>()?.Active ?? true);

        foreach (var m in _meteors)
        {
            m.Blockers.Clear();
            List<(WPos, WPos)> segments = [];

            void addBlock(WPos center, IEnumerable<WDir> corners)
            {
                var c = corners.ToList();
                for (var i = 0; i < c.Count - 1; i++)
                    segments.Add((c[i] + center, c[i + 1] + center));
                segments.Add((c[^1] + center, c[0] + center));
            }

            foreach (var block in _barriers.BarrierPositions.Where(b => b.Shade == m.Shade))
                addBlock(block.Center, CurveApprox.Rect(block.Orientation, 1, 3));
            addBlock(Arena.Center, CurveApprox.Rect(new(Arena.Bounds.Radius, 0), new(0, Arena.Bounds.Radius)));

            foreach (var point in Visibility.Compute(m.Caster.Position, segments))
            {
                if (m.Blockers.Count == 0 || !m.Blockers[^1].AlmostEqual(point, 0.1f))
                    m.Blockers.Add(point);
            }

            if (m.Blockers.Count > 0)
                m.Blockers.Add(m.Blockers[0]);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WipeWhite or AID._Weaponskill_WipeBlack)
        {
            _meteors.RemoveAll(m => m.Caster == caster);
            if (_meteors.Count == 0)
                _risky = false;
        }
    }
}
