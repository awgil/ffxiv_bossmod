namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class ParhelicCircle : Components.CastCounter
{
    private List<WPos> _positions = new();

    private static readonly float _triRadius = 8;
    private static readonly float _hexRadius = 17;
    private static readonly AOEShapeCircle _aoeShape = new(6);

    public ParhelicCircle() : base(ActionID.MakeSpell(AID.Incandescence)) { }

    public override void Update(BossModule module)
    {
        if (_positions.Count == 0)
        {
            // there are 10 orbs: 1 in center, 3 in vertices of a triangle with radius=8, 6 in vertices of a hexagon with radius=17
            // note: i'm not sure how exactly orientation is determined, it seems to be related to eventobj rotations...
            var hex = module.Enemies(OID.RefulgenceHexagon).FirstOrDefault();
            var tri = module.Enemies(OID.RefulgenceTriangle).FirstOrDefault();
            if (hex != null && tri != null)
            {
                var c = module.Bounds.Center;
                _positions.Add(c);
                _positions.Add(c + _triRadius * (tri.Rotation + 60.Degrees()).ToDirection());
                _positions.Add(c + _triRadius * (tri.Rotation + 180.Degrees()).ToDirection());
                _positions.Add(c + _triRadius * (tri.Rotation - 60.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius *  hex.Rotation.ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation + 60.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation + 120.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation + 180.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation - 120.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation - 60.Degrees()).ToDirection());
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_positions.Any(p => _aoeShape.Check(actor.Position, p)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var p in _positions)
            _aoeShape.Draw(arena, p);
    }
}
