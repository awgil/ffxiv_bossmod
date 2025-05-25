namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class Wipe(BossModule module) : Components.GenericAOEs(module)
{
    record class AOEShapeTri(WDir A, WDir B, WDir C) : AOEShape
    {
        public override bool Check(WPos position, WPos origin, Angle rotation) => position.InTri(origin + A.Rotate(rotation), origin + B.Rotate(rotation), origin + C.Rotate(rotation));
        public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.ZoneTri(origin + A.Rotate(rotation), origin + B.Rotate(rotation), origin + C.Rotate(rotation), color);
        public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = 0) => arena.AddTriangle(origin + A.Rotate(rotation), origin + B.Rotate(rotation), origin + C.Rotate(rotation), color);
    }

    record class Meteor(Actor Caster, Shade Shade, ArcList AnglesUnblocked, List<(WPos, WPos)> BlockingWalls);

    private readonly List<Meteor> _meteors = [];
    private readonly Barrier _barriers = module.FindComponent<Barrier>()!;
    private bool _risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var m in _meteors)
        {
            foreach (var (min, max) in m.AnglesUnblocked.Allowed(default))
                yield return new AOEInstance(new AOEShapeCone(50, (max - min) * 0.5f), m.Caster.Position, (max + min) * 0.5f, Module.CastFinishAt(m.Caster.CastInfo), Risky: _risky);

            foreach (var tri in m.BlockingWalls)
                yield return new AOEInstance(new AOEShapeTri(default, tri.Item1 - m.Caster.Position, tri.Item2 - m.Caster.Position), m.Caster.Position, Activation: Module.CastFinishAt(m.Caster.CastInfo), Risky: _risky);
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
            _meteors.Add(new(caster, color, new(caster.Position, 50), []));
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

        foreach (var m in _meteors)
        {
            var list = m.AnglesUnblocked;
            list.Forbidden.Clear();
            m.BlockingWalls.Clear();
            foreach (var (bcenter, borient, bcol) in _barriers.BarrierPositions)
            {
                if (bcol != m.Shade)
                    continue;

                var rectFaceToCaster = borient.OrthoR();
                if (rectFaceToCaster.Dot(bcenter - m.Caster.Position) > 0)
                    rectFaceToCaster *= -1;

                var corner1 = bcenter + rectFaceToCaster.OrthoL() * 3 + rectFaceToCaster;
                var corner2 = bcenter + rectFaceToCaster.OrthoR() * 3 + rectFaceToCaster;
                var angle1 = Angle.FromDirection(corner1 - m.Caster.Position).Normalized();
                var angle2 = Angle.FromDirection(corner2 - m.Caster.Position).Normalized();

                if (angle2.Rad < -MathF.PI * 0.5f && angle1.Rad > MathF.PI * 0.5f)
                {
                    var amax2 = angle2 + 360.Degrees();
                    angle2 = angle1;
                    angle1 = amax2;
                }

                var center = (angle1.Rad + angle2.Rad) * 0.5f;
                var width = MathF.Abs(angle2.Rad - center);
                list.ForbidArcByLength(center.Radians().Normalized(), width.Radians());
                m.BlockingWalls.Add((corner1, corner2));
            }
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
