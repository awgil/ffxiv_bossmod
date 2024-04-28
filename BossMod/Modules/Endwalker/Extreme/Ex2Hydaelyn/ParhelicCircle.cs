namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class ParhelicCircle(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Incandescence))
{
    private readonly List<WPos> _positions = [];

    private const float _triRadius = 8;
    private const float _hexRadius = 17;
    private static readonly AOEShapeCircle _aoeShape = new(6);

    public override void Update()
    {
        if (_positions.Count == 0)
        {
            // there are 10 orbs: 1 in center, 3 in vertices of a triangle with radius=8, 6 in vertices of a hexagon with radius=17
            // note: i'm not sure how exactly orientation is determined, it seems to be related to eventobj rotations...
            var hex = Module.Enemies(OID.RefulgenceHexagon).FirstOrDefault();
            var tri = Module.Enemies(OID.RefulgenceTriangle).FirstOrDefault();
            if (hex != null && tri != null)
            {
                var c = Module.Center;
                _positions.Add(c);
                _positions.Add(c + _triRadius * (tri.Rotation + 60.Degrees()).ToDirection());
                _positions.Add(c + _triRadius * (tri.Rotation + 180.Degrees()).ToDirection());
                _positions.Add(c + _triRadius * (tri.Rotation - 60.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * hex.Rotation.ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation + 60.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation + 120.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation + 180.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation - 120.Degrees()).ToDirection());
                _positions.Add(c + _hexRadius * (hex.Rotation - 60.Degrees()).ToDirection());
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_positions.Any(p => _aoeShape.Check(actor.Position, p)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var p in _positions)
            _aoeShape.Draw(Arena, p);
    }
}
