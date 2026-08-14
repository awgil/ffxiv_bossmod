namespace BossMod.Dawntrail.Savage.M12SLindwurm;

sealed class GrotesquerieAct3(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { Unknown, CardinalSafe, IntercardinalSafe }
    public enum ArenaPhase { Normal, Disconnected, PartiallyConnected }

    private const ushort DirNorth = 1078;
    private const ushort DirEast = 1079;
    private const ushort DirSouth = 1080;
    private const ushort DirWest = 1081;
    private static readonly float SafeSpotRadius = 1f;
    private static readonly WPos CardinalNorthSupp = new(96f, 86f);
    private static readonly WPos CardinalNorthDPS = new(104f, 94f);
    private static readonly WPos CardinalSouthSupp = new(96f, 106f);
    private static readonly WPos CardinalSouthDPS = new(104f, 114f);
    private static readonly WPos CardinalEastSupp = new(111f, 104f);
    private static readonly WPos CardinalEastDPS = new(119f, 96f);
    private static readonly WPos CardinalWestSupp = new(81f, 104f);
    private static readonly WPos CardinalWestDPS = new(89f, 96f);
    private static readonly WPos IntercardNESupp = new(111f, 94f);
    private static readonly WPos IntercardNEDPS = new(119f, 86f);
    private static readonly WPos IntercardSWSupp = new(81f, 114f);
    private static readonly WPos IntercardSWDPS = new(89f, 106f);
    private static readonly WPos IntercardSESupp = new(111f, 114f);
    private static readonly WPos IntercardSEDPS = new(119f, 106f);
    private static readonly WPos IntercardNWSupp = new(81f, 94f);
    private static readonly WPos IntercardNWDPS = new(89f, 86f);

    private Pattern _pattern = Pattern.Unknown;
    private ArenaPhase _arenaPhase = ArenaPhase.Normal;
    private readonly List<AOEInstance> _aoes = [];

    private readonly ushort[] _direction = new ushort[8];
    private readonly DateTime[] _mitoticExpire = new DateTime[8];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot is < 0 or >= 8)
            return;

        if (status.ID == (uint)SID._Gen_Direction && status.Extra is >= DirNorth and <= DirWest)
            _direction[slot] = status.Extra;

        if (status.ID == (uint)SID.MitoticPhase)
            _mitoticExpire[slot] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot is < 0 or >= 8)
            return;

        if (status.ID == (uint)SID._Gen_Direction)
            _direction[slot] = 0;

        if (status.ID == (uint)SID.MitoticPhase)
            _mitoticExpire[slot] = default;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_pattern == Pattern.Unknown)
        {
            if (spell.Action.ID == (uint)AID.GrandEntrance1)
            {
                _pattern = Pattern.CardinalSafe;
                QueueDestructionAOEs();
            }
            else if (spell.Action.ID == (uint)AID.GrandEntrance2)
            {
                _pattern = Pattern.IntercardinalSafe;
                QueueDestructionAOEs();
            }
        }

        if (spell.Action.ID == (uint)AID.FeralFission && _pattern != Pattern.Unknown)
        {
            QueueDestructionAOEs();
        }

        if (spell.Action.ID is (uint)AID.BringDownTheHouse0 or (uint)AID.BringDownTheHouse1 or (uint)AID.BringDownTheHouse2)
        {
            _aoes.Clear();
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0)
            return;

        switch (state)
        {
            // Normal -> Disconnected
            case 0x02000100u:
            case 0x00020001u:
                if (_pattern != Pattern.Unknown)
                {
                    _arenaPhase = ArenaPhase.Disconnected;
                    UpdateArenaBounds();
                }
                break;

            // Disconnected -> PartiallyConnected
            case 0x08000400u:
            case 0x00200010u:
                if (_pattern != Pattern.Unknown)
                {
                    _arenaPhase = ArenaPhase.PartiallyConnected;
                    UpdateArenaBounds();
                }
                break;

            // PartiallyConnected -> Normal
            case 0x00080004u:
            case 0x10000004u:
                Arena.Bounds = M12SLindwurm.DefaultBounds;
                _pattern = Pattern.Unknown;
                _arenaPhase = ArenaPhase.Normal;
                _aoes.Clear();
                break;
        }
    }

    private void QueueDestructionAOEs()
    {
        if (_arenaPhase != ArenaPhase.Normal)
            return;

        _aoes.Clear();
        var activation = WorldState.FutureTime(10f);

        if (_pattern == Pattern.CardinalSafe)
        {
            _aoes.Add(new(new AOEShapeRect(10f, 10f), new WPos(100f, 95f), default, activation)); // c
            _aoes.Add(new(new AOEShapeRect(10f, 10f), new WPos(85f, 85f), default, activation)); // nw
            _aoes.Add(new(new AOEShapeRect(10f, 10f), new WPos(115f, 85f), default, activation)); // ne
            _aoes.Add(new(new AOEShapeRect(10f, 10f), new WPos(85f, 105f), default, activation)); // sw
            _aoes.Add(new(new AOEShapeRect(10f, 10f), new WPos(115f, 105f), default, activation)); // se
        }
        else if (_pattern == Pattern.IntercardinalSafe)
        {
            _aoes.Add(new(new AOEShapeRect(10f, 10f), new WPos(100f, 85f), default, activation)); // n
            _aoes.Add(new(new AOEShapeRect(10f, 10f), new WPos(100f, 95f), default, activation)); // c
            _aoes.Add(new(new AOEShapeRect(10f, 10f), new WPos(100f, 105f), default, activation)); // s
            _aoes.Add(new(new AOEShapeRect(10f, 5f), new WPos(85f, 95f), default, activation)); // w
            _aoes.Add(new(new AOEShapeRect(10f, 5f), new WPos(115f, 95f), default, activation)); // e
        }
    }

    private void UpdateArenaBounds()
    {
        if (_arenaPhase == ArenaPhase.Disconnected)
        {
            if (_pattern == Pattern.CardinalSafe)
            {
                Arena.Bounds = new ArenaBoundsCustom(
                [
                    new Rectangle(new WPos(100f, 90f), 5f, 5f),   // n
                    new Rectangle(new WPos(100f, 110f), 5f, 5f),  // s
                    new Rectangle(new WPos(85f, 100f), 5f, 5f),   // w
                    new Rectangle(new WPos(115f, 100f), 5f, 5f),  // e
                ],
                []);
            }
            else if (_pattern == Pattern.IntercardinalSafe)
            {
                Arena.Bounds = new ArenaBoundsCustom(
                [
                    new Rectangle(new WPos(85f, 90f), 5f, 5f),   // nw
                    new Rectangle(new WPos(115f, 90f), 5f, 5f),  // ne
                    new Rectangle(new WPos(85f, 110f), 5f, 5f),  // sw
                    new Rectangle(new WPos(115f, 110f), 5f, 5f), // se
                ],
                []);
            }
        }
        else if (_arenaPhase == ArenaPhase.PartiallyConnected)
        {
            if (_pattern == Pattern.CardinalSafe)
            {
                Arena.Bounds = new ArenaBoundsCustom(
                [
                    new Rectangle(new WPos(100f, 100f), 10f, 5f),  // c
                    new Rectangle(new WPos(100f, 90f), 10f, 5f),   // n
                    new Rectangle(new WPos(100f, 110f), 10f, 5f),  // s
                    new Rectangle(new WPos(85f, 100f), 5f, 5f),   // w
                    new Rectangle(new WPos(115f, 100f), 5f, 5f),  // e
                ],
                []);
            }
            else if (_pattern == Pattern.IntercardinalSafe)
            {
                Arena.Bounds = new ArenaBoundsCustom(
                [
                    new Rectangle(new WPos(92.5f, 100f), 2.5f, 15f),  // w
                    new Rectangle(new WPos(107.5f, 100f), 2.5f, 15f), // e
                    new Rectangle(new WPos(100f, 100f), 5f, 5f),      // c
                    new Rectangle(new WPos(85f, 90f), 5f, 5f),        // nw
                    new Rectangle(new WPos(115f, 90f), 5f, 5f),       // ne
                    new Rectangle(new WPos(85f, 110f), 5f, 5f),       // sw
                    new Rectangle(new WPos(115f, 110f), 5f, 5f),      // se
                ],
                []);
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_pattern != Pattern.Unknown)
            hints.Add(_pattern == Pattern.CardinalSafe ? "Cardinals Safe" : "Intercardinals Safe");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_pattern == Pattern.Unknown)
            return;

        //DrawAllSafeSpots();

        var dir = _direction[pcSlot];
        if (dir == 0)
            return;

        var isSupport = pc.Class.IsSupport();
        var safeSpot = GetSafeSpot(dir, isSupport);
        if (safeSpot != default)
        {
            var isInside = pc.Position.InCircle(safeSpot, SafeSpotRadius);
            Arena.ZoneCircleOutline(safeSpot, SafeSpotRadius, isInside ? Colors.Danger : Colors.Safe);
        }

        DrawAllDefamations();

        var towerPos = GetTowerPosition(pc.Position, dir);
        Arena.ZoneCircleOutline(towerPos, 3f, Colors.Object);
    }

    private void DrawAllDefamations()
    {
        foreach (var actor in Raid.WithoutSlot(false, true, true))
        {
            var status = actor.FindStatus((uint)SID.MitoticPhase);
            if (status == null)
                continue;

            var remaining = StatusDuration(status.Value.ExpireAt);
            if (remaining is > 0 and < 3)
                Arena.ZoneCircleOutline(actor.Position, 9f, Colors.Danger); // DramaticLysis1
        }
    }

    private float StatusDuration(DateTime expireAt) => Math.Max(0, (float)(expireAt - WorldState.CurrentTime).TotalSeconds);

    private WPos GetSafeSpot(ushort direction, bool isSupport)
    {
        if (_pattern == Pattern.CardinalSafe)
        {
            return direction switch
            {
                DirSouth => isSupport ? CardinalNorthSupp : CardinalNorthDPS,
                DirNorth => isSupport ? CardinalSouthSupp : CardinalSouthDPS,
                DirWest => isSupport ? CardinalEastSupp : CardinalEastDPS,
                DirEast => isSupport ? CardinalWestSupp : CardinalWestDPS,
                _ => default
            };
        }
        else // IntercardinalSafe
        {
            return direction switch
            {
                DirSouth => isSupport ? IntercardNESupp : IntercardNEDPS,
                DirNorth => isSupport ? IntercardSWSupp : IntercardSWDPS,
                DirWest => isSupport ? IntercardSESupp : IntercardSEDPS,
                DirEast => isSupport ? IntercardNWSupp : IntercardNWDPS,
                _ => default
            };
        }
    }

    private static WPos GetTowerPosition(WPos playerPos, ushort direction)
    {
        return direction switch
        {
            DirNorth => playerPos + new WDir(0, -20f),
            DirEast => playerPos + new WDir(20f, 0),
            DirSouth => playerPos + new WDir(0, 20f),
            DirWest => playerPos + new WDir(-20f, 0),
            _ => playerPos
        };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_arenaPhase == ArenaPhase.Normal || _pattern == Pattern.Unknown)
            return;

        var dir = _direction[slot];
        if (dir == 0)
            return;

        var dirName = dir switch
        {
            DirNorth => "North",
            DirEast => "East",
            DirSouth => "South",
            DirWest => "West",
            _ => "?"
        };

        var isSupport = actor.Class.IsSupport();
        var role = isSupport ? "Support" : "DPS";
        hints.Add($"Add facing {dirName}, {role}", false);
    }
}
