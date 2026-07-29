namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

// common features for ruby glow mechanics
// quadrants:
// 0 1
// 2 3
// for diagonal, quadrant for coordinate is whatever cell is fully contained (so e.g. for DiagNW quadrant is either 1 or 2)
abstract class RubyGlowCommon(BossModule module, uint watchedAction = default) : Components.GenericAOEs(module, watchedAction)
{
    public enum ArenaState { Normal, Cells, DiagNW, DiagNE } // DiagNW == NW to SE, etc

    public ArenaState State;
    private readonly List<Actor> _magicStones = [];
    private readonly List<Actor> _poisonStones = [];
    public List<Actor> MagicStones => GetStones(_magicStones);
    public List<Actor> PoisonStones => GetStones(_poisonStones);

    private List<Actor> GetStones(List<Actor> Stones)
    {
        var count = Stones.Count;
        if (count == 0)
            return [];

        var stones = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = Stones[i];
            if (!z.IsDead)
                stones.Add(z);
        }
        return stones;
    }

    public static readonly AOEShape ShapeQuadrant = new AOEShapeRect(7.5f, 7.5f, 7.5f);
    public static readonly AOEShape ShapeHalf = new AOEShapeRect(45f, 45f);
    public static readonly AOEShape ShapePoison = new AOEShapeCircle(13f);

    public int QuadrantForPosition(WPos pos)
    {
        var offset = pos - Arena.Center;
        return State switch
        {
            ArenaState.Cells => (offset.X < 0 ? 0 : 1) | (offset.Z < 0 ? 0 : 2),
            ArenaState.DiagNW => offset.X > offset.Z ? 1 : 2,
            ArenaState.DiagNE => offset.X > -offset.Z ? 3 : 0,
            _ => 0
        };
    }

    public WDir QuadrantDir(int q) => new((q & 1) != 0 ? +1 : -1, (q & 2) != 0 ? +1 : -1); // both coords are +-1
    public WPos QuadrantCenter(int q) => Arena.Center + Arena.Bounds.Radius * 0.5f * QuadrantDir(q);

    public Waymark WaymarkForQuadrant(int q)
    {
        var c = QuadrantCenter(q);
        var w = Waymark.Count;
        var wd = float.MaxValue;
        for (var i = Waymark.A; i < Waymark.Count; ++i)
        {
            var pos = WorldState.Waymarks[i];
            var dist = pos != null ? (new WPos(pos.Value.XZ()) - c).LengthSq() : float.MaxValue;
            if (dist < wd)
            {
                w = i;
                wd = dist;
            }
        }
        return w;
    }

    public AOEInstance[] ActivePoisonAOEs()
    {
        // TODO: correct explosion time
        var stones = PoisonStones;
        var count = stones.Count;
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(ShapePoison, stones[i].Position);
        }
        return aoes;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var center = Arena.Center;
        var radius = Arena.Bounds.Radius;
        var color = Colors.Border;
        switch (State)
        {
            case ArenaState.Cells:
                Arena.AddLine(center - new WDir(radius, default), center + new WDir(radius, default), color);
                Arena.AddLine(center - new WDir(default, radius), center + new WDir(default, radius), color);
                break;
            case ArenaState.DiagNW:
                Arena.AddLine(center - new WDir(radius, radius), center + new WDir(radius, radius), color);
                break;
            case ArenaState.DiagNE:
                Arena.AddLine(center - new WDir(radius, -radius), center + new WDir(radius, -radius), color);
                break;
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.TopazStoneAny)
            return;

        switch (state)
        {
            case 0x00010002: // dormant -> magic
                _magicStones.Add(actor);
                break;
            case 0x00100080: // dormant -> poison
                _poisonStones.Add(actor);
                break;
            case 0x00100020: // magic -> poison
                _magicStones.Remove(actor);
                _poisonStones.Add(actor);
                break;
            case 0x00040400: // magic -> dormant
                _magicStones.Remove(actor);
                break;
            case 0x00040200: // poison -> dormant
                _poisonStones.Remove(actor);
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var astate = index switch
        {
            0 => ArenaState.DiagNE,
            1 => ArenaState.DiagNW,
            2 => ArenaState.Cells,
            _ => ArenaState.Normal
        };
        if (astate == ArenaState.Normal)
            return;

        switch (state)
        {
            // 0x00100020 - happens ~1s after activation
            case 0x00020001:
                if (State != ArenaState.Normal)
                    ReportError($"Active state {State} while state {astate} is activated");
                State = astate;
                break;
            case 0x00080004:
                if (State != astate)
                    ReportError($"Active state {State} while state {astate} is deactivated");
                State = ArenaState.Normal;
                break;
        }
    }
}

// common features for ruby glow 4 & 6 (ones that feature recoloring)
// this includes venom pools and raging claw/searing ray aoes
// note: we show circles around healers until cast happens
abstract class RubyGlowRecolor(BossModule module, int expectedMagicStones) : RubyGlowCommon(module, (uint)AID.VenomPoolRecolorAOE)
{
    public enum RecolorState { BeforeStones, BeforeRecolor, Done }

    public RecolorState CurRecolorState;
    public int AOEQuadrant;
    private readonly int _expectedMagicStones = expectedMagicStones;

    private const float _recolorRadius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (VenomPoolActive && Raid.WithoutSlot(false, true, true).Where(a => a.Role == Role.Healer).InRadius(actor.Position, _recolorRadius).Count() != 1)
            hints.Add("Stack with healer!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => VenomPoolActive && player.Role == Role.Healer ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurRecolorState == RecolorState.BeforeRecolor)
            foreach (var o in MagicStones)
                if (QuadrantForPosition(o.Position) != AOEQuadrant)
                    Arena.Actor(o, Colors.Vulnerable, true);

        if (VenomPoolActive)
            foreach (var a in Raid.WithoutSlot(false, true, true).Where(a => a.Role == Role.Healer))
                Arena.ZoneCircleOutline(a.Position, _recolorRadius, Colors.Safe);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        base.OnActorEAnim(actor, state);
        switch (CurRecolorState)
        {
            case RecolorState.BeforeStones:
                if (MagicStones.Count == _expectedMagicStones)
                {
                    var counts = new int[4];
                    foreach (var o in MagicStones)
                        ++counts[QuadrantForPosition(o.Position)];
                    AOEQuadrant = Array.IndexOf(counts, 3);
                    CurRecolorState = RecolorState.BeforeRecolor;
                }
                break;
            case RecolorState.BeforeRecolor:
                if (PoisonStones.Count != 0)
                {
                    CurRecolorState = RecolorState.Done;
                }
                break;
        }
    }

    private bool VenomPoolActive => NumCasts == 0 && CurRecolorState == RecolorState.BeforeRecolor;
}
