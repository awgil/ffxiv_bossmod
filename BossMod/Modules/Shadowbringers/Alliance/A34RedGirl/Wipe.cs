namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class Wipe(BossModule module) : Components.GenericAOEs(module)
{
    record class AOEShapeTri(WDir A, WDir B, WDir C) : AOEShape
    {
        public AOEShapeTri(WPos source, WPos a, WPos b) : this(default, a - source, b - source) { }

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
        // drawing double meteors makes minimap hard to decode during the Point mechanic
        if (!_risky)
            yield break;

        foreach (var m in _meteors)
        {
            for (var i = 0; i < m.Blockers.Count - 1; i++)
            {
                yield return new AOEInstance(new AOEShapeTri(m.Caster.Position, m.Blockers[i], m.Blockers[i + 1]), m.Caster.Position, default, Activation: Module.CastFinishAt(m.Caster.CastInfo));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var color = (AID)spell.Action.ID switch
        {
            AID.WipeWhite => Shade.White,
            AID.WipeBlack => Shade.Black,
            _ => default
        };
        if (color != default)
        {
            _meteors.Add(new(caster, color, []));
            UpdateWalls();
        }
    }

    public override void OnMapEffect(byte index, uint state)
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
            List<Visibility.LineSegment> segments = [];

            void addBlock(WPos center, IEnumerable<WDir> corners)
            {
                var c = corners.ToList();
                for (var i = 0; i < c.Count - 1; i++)
                    segments.Add(new(c[i] + center, c[i + 1] + center));
                segments.Add(new(c[^1] + center, c[0] + center));
            }

            foreach (var block in _barriers.BarrierPositions.Where(b => b.Shade == m.Shade))
                addBlock(block.Center, [block.Orientation * 3, block.Orientation * -3]);
            // use larger radius than arena max, since the algorithm is not designed to work with intersecting line segments - outermost blocks can extend outside arena
            addBlock(Arena.Center, CurveApprox.Rect(new(30, 0), new(0, 30)));

            m.Blockers.AddRange(Visibility.VisibilityPolygon(m.Caster.Position, segments));

            if (m.Blockers.Count > 0)
                m.Blockers.Add(m.Blockers[0]);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WipeWhite or AID.WipeBlack)
        {
            _meteors.RemoveAll(m => m.Caster == caster);
            if (_meteors.Count == 0)
                _risky = false;
        }
    }
}
