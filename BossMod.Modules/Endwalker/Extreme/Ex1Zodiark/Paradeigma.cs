namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to paradeigma and astral flow mechanics
class Paradeigma(BossModule module) : BossComponent(module)
{
    public enum FlowDirection { None, CW, CCW }

    private FlowDirection _flow;
    private readonly List<WDir> _birds = [];
    private readonly List<WDir> _behemoths = [];
    private readonly List<(WDir, Angle)> _snakes = [];
    private readonly List<WDir> _fireLine = [];

    private const float _birdBehemothOffset = 10.5f;
    private const float _snakeNOffset = 5.5f;
    private const float _snakeFOffset = 15.5f;
    private const float _snakeOrthoOffset = 21;
    private static readonly AOEShapeDonut _birdAOE = new(5, 15);
    private static readonly AOEShapeCircle _behemothAOE = new(15);
    private static readonly AOEShapeRect _snakeAOE = new(42, 5.5f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (RotatedBirds().Any(b => _birdAOE.Check(actor.Position, b)) || RotatedBehemoths().Any(b => _behemothAOE.Check(actor.Position, b)))
            hints.Add("GTFO from bird/behemoth aoe!");
        if (RotatedSnakes().Any(s => _snakeAOE.Check(actor.Position, s.Item1, s.Item2)))
            hints.Add("GTFO from snake aoe!");
        if (_fireLine.Any(c => InFireAOE(c, actor.Position)))
            hints.Add("GTFO from fire aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in RotatedBirds())
            _birdAOE.Draw(Arena, b);
        foreach (var b in RotatedBehemoths())
            _behemothAOE.Draw(Arena, b);
        foreach (var s in RotatedSnakes())
            _snakeAOE.Draw(Arena, s.Item1, s.Item2);
        foreach (var c in _fireLine)
            Arena.ZoneTri(Module.Center + c, RotatedPosition(c), Module.Center, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_fireLine.Count == 2)
            Arena.AddLine(Module.Center + _fireLine[0], Module.Center + _fireLine[1], ArenaColor.Danger);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // notable env controls that we don't care too much about:
        // 1: common for all flows, 00020001 = activate, 00080004 = deactivate
        // 3: common for all flows, happens a bit after cast start, always 00010001
        if (index == 2)
        {
            // flow rotation arrows (note that we could also rely on cast id for them...)
            if (state == 0x00020001)
                _flow = FlowDirection.CW;
            else if (state == 0x00200010)
                _flow = FlowDirection.CCW;
            // other states: 00080004, 00400004 - deactivation
        }
        else if (index == 5)
        {
            switch (state)
            {
                case 0x00020001:
                    _fireLine.Add(new(+Module.Bounds.Radius, -Module.Bounds.Radius));
                    _fireLine.Add(new(-Module.Bounds.Radius, +Module.Bounds.Radius));
                    break;
                case 0x00400020:
                    _fireLine.Add(new(-Module.Bounds.Radius, -Module.Bounds.Radius));
                    _fireLine.Add(new(+Module.Bounds.Radius, +Module.Bounds.Radius));
                    break;
            }
        }
        else if (index >= 9 && index <= 24 && state == 0x00200010)
        {
            // birds, behemoths and snakes; other states: 20001000 = color change, 40000004 = disappear
            switch (index)
            {
                case 09: _behemoths.Add(new(-_birdBehemothOffset, -_birdBehemothOffset)); break;
                case 10: _behemoths.Add(new(+_birdBehemothOffset, -_birdBehemothOffset)); break;
                case 11: _behemoths.Add(new(-_birdBehemothOffset, +_birdBehemothOffset)); break;
                case 12: _behemoths.Add(new(+_birdBehemothOffset, +_birdBehemothOffset)); break;
                case 13:
                    _snakes.Add((new(-_snakeFOffset, -_snakeOrthoOffset), 0.Degrees()));
                    _snakes.Add((new(+_snakeNOffset, -_snakeOrthoOffset), 0.Degrees()));
                    break;
                case 14:
                    _snakes.Add((new(-_snakeNOffset, -_snakeOrthoOffset), 0.Degrees()));
                    _snakes.Add((new(+_snakeFOffset, -_snakeOrthoOffset), 0.Degrees()));
                    break;
                case 15:
                    _snakes.Add((new(-_snakeFOffset, _snakeOrthoOffset), 180.Degrees()));
                    _snakes.Add((new(+_snakeNOffset, _snakeOrthoOffset), 180.Degrees()));
                    break;
                case 16:
                    _snakes.Add((new(-_snakeNOffset, _snakeOrthoOffset), 180.Degrees()));
                    _snakes.Add((new(+_snakeFOffset, _snakeOrthoOffset), 180.Degrees()));
                    break;
                case 17:
                    _snakes.Add((new(-_snakeOrthoOffset, -_snakeFOffset), 90.Degrees()));
                    _snakes.Add((new(-_snakeOrthoOffset, +_snakeNOffset), 90.Degrees()));
                    break;
                case 18:
                    _snakes.Add((new(-_snakeOrthoOffset, -_snakeNOffset), 90.Degrees()));
                    _snakes.Add((new(-_snakeOrthoOffset, +_snakeFOffset), 90.Degrees()));
                    break;
                case 19:
                    _snakes.Add((new(_snakeOrthoOffset, -_snakeFOffset), -90.Degrees()));
                    _snakes.Add((new(_snakeOrthoOffset, +_snakeNOffset), -90.Degrees()));
                    break;
                case 20:
                    _snakes.Add((new(_snakeOrthoOffset, -_snakeNOffset), -90.Degrees()));
                    _snakes.Add((new(_snakeOrthoOffset, +_snakeFOffset), -90.Degrees()));
                    break;
                case 21: _birds.Add(new(-_birdBehemothOffset, -_birdBehemothOffset)); break;
                case 22: _birds.Add(new(+_birdBehemothOffset, -_birdBehemothOffset)); break;
                case 23: _birds.Add(new(-_birdBehemothOffset, +_birdBehemothOffset)); break;
                case 24: _birds.Add(new(+_birdBehemothOffset, +_birdBehemothOffset)); break;
            }
        }
    }

    private WPos RotatedPosition(WDir offset)
    {
        return _flow switch
        {
            FlowDirection.CW => Module.Center + offset.OrthoR(),
            FlowDirection.CCW => Module.Center + offset.OrthoL(),
            _ => Module.Center + offset
        };
    }

    private (WPos, Angle) RotatedPosRot((WDir, Angle) posRot)
    {
        return _flow switch
        {
            FlowDirection.CW => (Module.Center + posRot.Item1.OrthoR(), posRot.Item2 - 90.Degrees()),
            FlowDirection.CCW => (Module.Center + posRot.Item1.OrthoL(), posRot.Item2 + 90.Degrees()),
            _ => (Module.Center + posRot.Item1, posRot.Item2)
        };
    }

    private IEnumerable<WPos> RotatedBirds() => _birds.Select(RotatedPosition);
    private IEnumerable<WPos> RotatedBehemoths() => _behemoths.Select(RotatedPosition);
    private IEnumerable<(WPos, Angle)> RotatedSnakes() => _snakes.Select(RotatedPosRot);

    private bool InFireAOE(WDir corner, WPos pos)
    {
        var p1 = Module.Center + corner;
        var p2 = RotatedPosition(corner);
        var pMid = WPos.Lerp(p1, p2, 0.5f);
        var dirMid = (pMid - Module.Center).Normalized();
        return pos.InCone(Module.Center, dirMid, 45.Degrees());
    }
}
