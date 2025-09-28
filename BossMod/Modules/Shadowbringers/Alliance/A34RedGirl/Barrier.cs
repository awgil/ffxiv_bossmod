namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class BarrierVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    private static readonly ArenaBoundsCustom VoidBounds = new(20, new PolygonClipper().Difference(new(CurveApprox.Rect(new(0, 20), new(20, 0))), new(CurveApprox.Rect(new(0, 2.5f), new(2.5f, 0)))));

    private static readonly AOEShapeCustom ZoneOuter = new(new PolygonClipper().Difference(new(CurveApprox.Rect(new(0, 25), new(25, 0))), new(CurveApprox.Rect(new(0, 20), new(20, 0)))));
    private static readonly AOEShapeRect ZoneInner = new(2.5f, 2.5f, 2.5f);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x1C)
        {
            if (state == 0x00020001)
                _activation = WorldState.FutureTime(5.1f);
            if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsSquare(24.5f);
        }
    }

    public override void Update()
    {
        if (_activation != default && _activation <= WorldState.CurrentTime)
        {
            _activation = default;
            Arena.Bounds = VoidBounds;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation > WorldState.CurrentTime)
        {
            yield return new(ZoneOuter, Arena.Center, Activation: _activation);
            yield return new(ZoneInner, Arena.Center, Activation: _activation);
        }
    }
}

class GenerateBarrier1(BossModule module) : Components.StandardAOEs(module, AID.GenerateBarrierShort, new AOEShapeRect(6, 1.5f));
class GenerateBarrier2(BossModule module) : Components.StandardAOEs(module, AID.GenerateBarrierMidShort, new AOEShapeRect(12, 1.5f));
class GenerateBarrier3(BossModule module) : Components.StandardAOEs(module, AID.GenerateBarrierMidLong, new AOEShapeRect(18, 1.5f));
class GenerateBarrier4(BossModule module) : Components.StandardAOEs(module, AID.GenerateBarrierLong, new AOEShapeRect(24, 1.5f));

class Barrier(BossModule module) : BossComponent(module)
{
    public readonly Shade[] Barriers = new Shade[28];

    public int NumBarriers => Barriers.Count(b => b != default);
    public IEnumerable<(WPos Center, WDir Orientation, Shade Shade)> BarrierPositions => Barriers
        .Select((color, index) => (color, index))
        .Where(c => c.color != default)
        .Select(c =>
        {
            var a = GetBarrierPosition(c.index);
            return (a.Center, a.Orientation, c.color);
        });

    public override void OnMapEffect(byte index, uint state)
    {
        var barrierIndex = index - 0x1D;
        if (barrierIndex is >= 0 and < 28)
        {
            Barriers[barrierIndex] = state switch
            {
                0x00020001 => Shade.Black,
                0x00800040 => Shade.White,
                _ => Shade.None
            };
        }
    }

    private (WPos Center, WDir Orientation) GetBarrierPosition(int i)
    {
        var isCrossPattern = i < 16;
        if (isCrossPattern)
        {
            WDir axis = i < 8 ? new(0, 1) : new(1, 0);
            var displace = axis * 13;
            var midpoint = (i % 8) < 4 ? Arena.Center - displace : Arena.Center + displace;
            var order = axis * (i % 4 - 1.5f) * 6;
            return (midpoint + order, axis);
        }
        else
        {
            i -= 16;
            WDir axis = i % 6 < 3 ? new(1, 0) : new(0, 1);
            var quadrant = i / 3;
            var orderInWall = i % 3 - 1;
            var rotation = 180.Degrees() + (-90 * quadrant).Degrees();
            var offFromCenter = new WDir(1, 9);
            return (Arena.Center + (offFromCenter + new WDir(-6, 0) * orderInWall).Rotate(rotation), axis);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < Barriers.Length; i++)
        {
            if (Barriers[i] != default)
            {
                var (center, orient) = GetBarrierPosition(i);
                Arena.ZoneRect(center, orient, 3, 3, 1, ArenaColor.PlayerGeneric);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < Barriers.Length; i++)
        {
            if (Barriers[i] != default)
            {
                var (center, orient) = GetBarrierPosition(i);
                hints.TemporaryObstacles.Add(ShapeContains.Rect(center, Angle.FromDirection(orient), 3.5f, 3.5f, 1.5f)); // increase size for player hitbox
            }
        }
    }
}
