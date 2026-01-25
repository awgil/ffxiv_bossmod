namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class Splattershed(BossModule module) : Components.RaidwideCastDelay(module, (AID)0, AID.SplattershedRaidwide, 2.5f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SplattershedBoss1 or AID.SplattershedBoss2)
            Activation = Module.CastFinishAt(spell, Delay);
    }
}

class GrandEntrance(BossModule module) : Components.GroupedAOEs(module, [AID.GrandEntranceAppear2, AID.GrandEntranceDisappear, AID.GrandEntranceAppear1, AID.GrandEntranceAppear3], new AOEShapeCircle(2));

class BringDownTheHouseSmall(BossModule module) : Components.StandardAOEs(module, AID.BringDownTheHouseSmall, new AOEShapeRect(10, 5));
class BringDownTheHouseMedium(BossModule module) : Components.StandardAOEs(module, AID.BringDownTheHouseMedium, new AOEShapeRect(10, 7.5f));
class BringDownTheHouseLarge(BossModule module) : Components.StandardAOEs(module, AID.BringDownTheHouseLarge, new AOEShapeRect(10, 10));

class BringDownTheHouse : Components.GenericAOEs
{
    DateTime _activation;

    static readonly AOEShape RectSmall = new AOEShapeRect(10, 7.5f);
    static readonly AOEShape RectLarge = new AOEShapeRect(10, 10);

    public readonly RelSimplifiedComplexPolygon CardsShape;
    public readonly RelSimplifiedComplexPolygon IntercardsShape;

    enum Pattern
    {
        None,
        Card,
        Intercard
    }

    Pattern _pattern;

    public BringDownTheHouse(BossModule module) : base(module)
    {
        CardsShape = new PolygonClipper().UnionAll(MakePlatform(15, 0), MakePlatform(-15, 0), MakePlatform(0, -10), MakePlatform(0, 10));
        IntercardsShape = new PolygonClipper().UnionAll(MakePlatform(15, 10), MakePlatform(15, -10), MakePlatform(-15, 10), MakePlatform(-15, -10));
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        switch (_pattern)
        {
            case Pattern.Card:
                yield return new(RectSmall, new(87.5f, 85), default, _activation);
                yield return new(RectSmall, new(87.5f, 105), default, _activation);
                yield return new(RectSmall, new(112.5f, 85), default, _activation);
                yield return new(RectSmall, new(112.5f, 105), default, _activation);
                yield return new(RectLarge, new(100, 95), default, _activation);
                break;
            case Pattern.Intercard:
                yield return new(RectLarge, new(100, 85), default, _activation);
                yield return new(RectLarge, new(90, 95), default, _activation);
                yield return new(RectLarge, new(110, 95), default, _activation);
                yield return new(RectLarge, new(100, 105), default, _activation);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.GrandEntranceAppear2 or AID.GrandEntranceAppear1 or AID.GrandEntranceAppear3 && _pattern == default)
        {
            if (IntercardsShape.Contains(caster.Position - Arena.Center))
                _pattern = Pattern.Card;
            else
                _pattern = Pattern.Intercard;
            _activation = Module.CastFinishAt(spell, 4.3f);
        }

        if ((AID)spell.Action.ID is AID.BringDownTheHouseMedium or AID.BringDownTheHouseLarge or AID.BringDownTheHouseSmall)
            _activation = default;
    }

    PolygonClipper.Operand MakePlatform(float offX, float offZ) => new(CurveApprox.Rect(new WDir(offX, offZ), new WDir(5, 0), new WDir(0, 5)));

    public bool Platforms { get; private set; }

    public override void OnMapEffect(byte index, uint state)
    {
        switch ((index, state))
        {
            // default
            case (0, 0x00080004):
            case (0, 0x10000004):
                Arena.Bounds = new ArenaBoundsRect(20, 15);
                break;
            // cardinal platforms
            case (0, 0x00020001):
                Platforms = true;
                Arena.Bounds = new ArenaBoundsCustom(20, CardsShape);
                break;
            // intercard platforms
            case (0, 0x02000100):
                Platforms = true;
                Arena.Bounds = new ArenaBoundsCustom(20, IntercardsShape);
                break;
            // inverse of Intercard pattern
            case (0, 0x00200010):
                Arena.Bounds = new ArenaBoundsCustom(20, Arena.Bounds.Clipper.Difference(new(CurveApprox.Rect(new(20, 0), new(0, 15))), new(IntercardsShape)));
                break;
            // inverse of Cardinal pattern
            case (0, 0x08000400):
                Arena.Bounds = new ArenaBoundsCustom(20, Arena.Bounds.Clipper.Difference(new(CurveApprox.Rect(new(20, 0), new(0, 15))), new(CardsShape)));
                break;
        }
    }
}

class MetamitosisProjected(BossModule module) : BossComponent(module)
{
    readonly WDir[] _orientation = new WDir[8];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DirectedGrotesquerieVisual && Raid.TryFindSlot(actor, out var slot))
            _orientation[slot] = status.Extra switch
            {
                0x436 => new(0, -1),
                0x437 => new(1, 0),
                0x438 => new(0, 1),
                0x439 => new(-1, 0),
                _ => default
            };
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MitoticPhase && Raid.TryFindSlot(actor, out var slot))
            _orientation[slot] = default;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (slot, player) in Raid.WithSlot())
        {
            if (player == pc)
                DrawProjected(slot, player, ArenaColor.Danger, true);
            else
                DrawProjected(slot, player, ArenaColor.Safe, false);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
    }

    void DrawProjected(int slot, Actor actor, uint color, bool drawLine)
    {
        var dir = _orientation[slot];
        if (dir == default)
            return;

        var len = MathF.Abs(dir.X) > 0 ? 30 : 20;
        var dest = actor.Position + dir * len;
        if (Arena.Config.ShowOutlinesAndShadows)
            Arena.AddCircle(dest, 3, 0xFF000000, 2);
        Arena.AddCircle(dest, 3, color);
        if (drawLine)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(actor.Position, dest, 0xFF000000, 2);
            Arena.AddLine(actor.Position, dest, color);
        }
    }
}

class MitoticPhaseDramaticLysis(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MitoticPhase)
            Spreads.Add(new(actor, 9, status.ExpireAt.AddSeconds(1.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MitoticPhaseDramaticLysis)
            Spreads.Clear();
    }
}

class MetamitosisTower(BossModule module) : Components.CastTowers(module, AID.MetamitosisTower, 3);

class SplitScourge(BossModule module) : Components.GenericBaitAway(module, AID.SplitScourge)
{
    readonly List<(Actor, DateTime)> _dragons = [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.SnakeHead && id == 0x11D3)
            _dragons.Add((actor, WorldState.CurrentTime));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var (dragon, _) in _dragons)
            Arena.ActorInsideBounds(dragon.Position, dragon.Rotation, ArenaColor.Object);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _dragons.Clear();
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        foreach (var (actor, spawn) in _dragons)
        {
            if (Raid.WithoutSlot().Closest(actor.Position) is { } closest)
                CurrentBaits.Add(new(actor, closest, new AOEShapeRect(60, 5), spawn.AddSeconds(7.4f)));
        }
    }
}

class VenomousScourge : Components.GenericStackSpread
{
    readonly DateTime _activation;

    public int NumCasts { get; private set; }

    public VenomousScourge(BossModule module) : base(module)
    {
        _activation = WorldState.FutureTime(10);
    }

    public override void Update()
    {
        Spreads.Clear();

        foreach (var player in Raid.WithoutSlot().Where(r => r.Position.X < 100).OrderBy(r => r.Position.X).Take(3))
            Spreads.Add(new(player, 5, _activation));
        foreach (var player in Raid.WithoutSlot().Where(r => r.Position.X > 100).OrderByDescending(r => r.Position.X).Take(3))
            Spreads.Add(new(player, 5, _activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.VenomousScourge)
            NumCasts++;
    }
}
