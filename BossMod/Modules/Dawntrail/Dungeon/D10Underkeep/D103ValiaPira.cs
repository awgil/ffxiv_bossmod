namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D103ValiaPira;

public enum OID : uint
{
    Boss = 0x478E,
    Helper = 0x233C,
    _Gen_ = 0x47B5, // R1.000, x9
    RedExplodey = 0x478F, // R1.600, x6
    _Gen_CoordinateBit = 0x4789, // R1.000, x2
    _Gen_CoordinateTurret = 0x4793, // R1.000, x4
}

public enum AID : uint
{
    _AutoAttack_ = 42526, // Boss->player, no cast, single-target
    _Spell_EntropicSphere = 42525, // Boss->self, 5.0s cast, range 40 circle
    _Ability_CoordinateMarch = 42513, // Boss->self, 4.0s cast, single-target
    Collision = 42514, // 478F->4789/47B5, no cast, single-target
    _Ability_EnforcementRay = 42737, // 4789->self, 0.5s cast, range 36 width 9 cross
    _Ability_OrderedFire = 42508, // Boss->self, no cast, single-target
    _Ability_OrderedFire2 = 42509, // Helper->Boss, 1.0s cast, single-target
    _Ability_Electray = 43130, // 4793->self, 7.0s cast, range 40 width 9 rect
    _Spell_ElectricField = 42519, // Boss->self, 6.6+0.7s cast, single-target
    _Spell_ElectricField1 = 43261, // Helper->self, no cast, range 26 50-degree cone
    _Spell_ConcurrentField = 42521, // Helper->self, 7.3s cast, range 26 50-degree cone
    _Ability_NeutralizeFrontLines = 42738, // Boss->self, 5.0s cast, range 30 180-degree cone
    _Spell_HyperchargedLight = 42524, // Helper->player, 5.0s cast, range 5 circle
    _Ability_Bloodmarch = 42739, // Boss->self, 5.0s cast, single-target
    _Ability_DeterrentPulse = 42540, // Boss->self/players, 5.0s cast, range 40 width 8 rect
}

public enum IconID : uint
{
    ElectricField = 586, // Boss->player
    DeterrentPulse = 525 // Boss->player
}

public enum TetherID : uint
{
    _Gen_Tether_322 = 322, // _Gen_->Boss
    _Gen_Tether_321 = 321, // _Gen_->Boss
    OrbTeleport = 282, // _Gen_->_Gen_1
}

class EntropicSphere(BossModule module) : Components.RaidwideCast(module, AID._Spell_EntropicSphere);
class Electray(BossModule module) : Components.StandardAOEs(module, AID._Ability_Electray, new AOEShapeRect(40, 4.5f));
class ConcurrentField(BossModule module) : Components.StandardAOEs(module, AID._Spell_ConcurrentField, new AOEShapeCone(26, 25.Degrees()));
class ElectricField(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(26, 25.Degrees()), (uint)IconID.ElectricField, AID._Spell_ElectricField1, 7.4f);
class NeutralizeFrontLines(BossModule module) : Components.StandardAOEs(module, AID._Ability_NeutralizeFrontLines, new AOEShapeCone(30, 90.Degrees()));
class DeterrentPulse(BossModule module) : Components.GenericWildCharge(module, 4, AID._Ability_DeterrentPulse, 40)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.DeterrentPulse)
        {
            Source = Module.PrimaryActor;
            Activation = WorldState.FutureTime(5.3f);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = targetID == p.InstanceID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Source = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}
class HyperchargedLight(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_HyperchargedLight, 5);

class Coordinates(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _orbs = [];
    private readonly List<(Actor From, Actor To)> _links = [];

    public readonly List<(int Tile, DateTime Activation)> Crosses = [];
    record struct Bit(Actor Actor, DateTime MoveStart, List<int> Path);
    private readonly List<Bit> _bits = [];

    [Flags]
    public enum Tile
    {
        None,
        N = 1,
        E = 1 << 1,
        S = 1 << 2,
        W = 1 << 3
    }

    // starting at top left from 0
    // bit 0 = N, bit 1 = E, bit 2 = S, bit 3 = W
    public readonly Tile[] Tiles = new Tile[16];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Crosses.Take(1).Select(c => new AOEInstance(new AOEShapeCross(36, 4.5f), TileCenter(c.Tile), default, c.Activation));

    public override void OnEventEnvControl(byte index, uint state)
    {
        var tile = index switch
        {
            0x1D => 0,
            0x1E => 1,
            >= 0x27 and <= 0x34 => index - 0x25,
            _ => -1
        };
        if (tile >= 0)
        {
            Tiles[tile] = state switch
            {
                0x00020001 => Tile.N | Tile.S,
                0x00200010 => Tile.E | Tile.W,
                0x00800040 => Tile.N | Tile.E,
                0x02000100 => Tile.E | Tile.S,
                0x08000400 => Tile.S | Tile.W,
                0x20001000 => Tile.N | Tile.W,
                _ => default
            };
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        switch (((OID)actor.OID, id))
        {
            case (OID.RedExplodey, 0x11DA):
                _orbs.Add(actor);
                _links.RemoveAll(t => t.From.Position.AlmostEqual(actor.Position, 0.1f));
                break;
            case (OID._Gen_CoordinateBit, 0x11D5):
                _bits.Add(new(actor, default, FindPath(actor)));
                break;
            case (OID._Gen_CoordinateBit, 0x24E1):
                _bits.RemoveAll(b => b.Actor == actor);
                break;
        }
    }

    private List<int> FindPath(Actor bit)
    {
        var t1 = FindTile(bit.Position);
        List<int> path = [];
        Tile prevConn;
        if (bit.Position.Z < Arena.Center.Z - 17.5f)
            prevConn = Tile.N;
        else if (bit.Position.X < Arena.Center.X - 17.5f)
            prevConn = Tile.W;
        else if (bit.Position.X > Arena.Center.X + 17.5f)
            prevConn = Tile.E;
        else if (bit.Position.Z > Arena.Center.Z + 17.5f)
            prevConn = Tile.S;
        else
        {
            prevConn = Tile.N | Tile.E | Tile.S | Tile.W;
            ReportError("don't know connection");
        }

        while (t1 >= 0)
        {
            if (path.Contains(t1))
            {
                ReportError($"loop in path at tile {t1}");
                return [];
            }
            path.Add(t1);
            var (t2, nc2) = NextTile(t1, prevConn);
            prevConn = nc2;
            t1 = t2;
        }

        return path;
    }

    private (int, Tile) NextTile(int prevTile, Tile prevDirection)
    {
        var t = Tiles[prevTile];
        t &= ~prevDirection;
        if (t.HasFlag(Tile.N) && prevTile > 3)
            return (prevTile - 4, Tile.S);
        if (t.HasFlag(Tile.E) && prevTile % 4 != 3)
            return (prevTile + 1, Tile.W);
        if (t.HasFlag(Tile.W) && prevTile % 4 != 0)
            return (prevTile - 1, Tile.E);
        if (t.HasFlag(Tile.S) && prevTile < 12)
            return (prevTile + 4, Tile.N);
        return (-1, default);
    }

    private int FindTile(WPos p)
    {
        var off = (p - Arena.Center) / 9f + new WDir(2, 2);
        var col = (int)off.X;
        var row = (int)off.Z;
        return row * 4 + col;
    }

    public override void Update()
    {
        var update = false;
        for (var i = 0; i < _bits.Count; i++)
        {
            if (_bits[i].MoveStart == default && _bits[i].Actor.LastFrameMovement != default)
            {
                _bits.Ref(i).MoveStart = WorldState.CurrentTime;
                update = true;
            }
        }

        if (update && _bits.All(b => b.MoveStart != default))
            UpdatePredictions();
    }

    private void UpdatePredictions()
    {
        Crosses.Clear();
        var orbTiles = new BitMask();
        foreach (var (f, _) in _links)
            orbTiles.Set(FindTile(f.Position));

        foreach (var o in _orbs)
        {
            if (_links.Any(l => l.To == o))
                continue;
            orbTiles.Set(FindTile(o.Position));
        }

        for (var i = 0; i < _bits.Count; i++)
        {
            var bit = _bits[i];
            var travelTime = 0.5f;
            foreach (var p in bit.Path)
            {
                travelTime += 1f;
                if (orbTiles[p])
                {
                    // estimated 2 seconds of being stationary during AOE activation, plus ~0.5s of acceleration
                    Crosses.Add((p, bit.MoveStart.AddSeconds(travelTime)));
                    travelTime += 2.5f;
                }
                travelTime += 1f;
            }
        }

        Crosses.SortBy(c => c.Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Collision)
        {
            _orbs.Remove(caster);
        }

        if ((AID)spell.Action.ID == AID._Ability_EnforcementRay)
            Crosses.RemoveAll(c => c.Tile == FindTile(caster.Position));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.OrbTeleport)
        {
            _links.Add((source, WorldState.Actors.Find(tether.Target)!));
            if (_bits.All(b => b.MoveStart != default))
                UpdatePredictions();
        }
    }

    private WPos TileCenter(int tile)
    {
        var col = tile % 4;
        var row = tile / 4;
        var off = new WDir(-13.5f, -13.5f) + new WDir(9 * col, 9 * row);
        return Arena.Center + off;
    }

#if DEBUG
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < 16; i++)
        {
            var t = Tiles[i];
            var center = TileCenter(i);
            if (t.HasFlag(Tile.N))
                Arena.AddLine(center, center + new WDir(0, -4.5f), ArenaColor.Danger);
            if (t.HasFlag(Tile.E))
                Arena.AddLine(center, center + new WDir(4.5f, 0), ArenaColor.Danger);
            if (t.HasFlag(Tile.S))
                Arena.AddLine(center, center + new WDir(0, 4.5f), ArenaColor.Danger);
            if (t.HasFlag(Tile.W))
                Arena.AddLine(center, center + new WDir(-4.5f, 0), ArenaColor.Danger);
        }

        foreach (var (f, t) in _links)
        {
            Arena.AddLine(f.Position, t.Position, ArenaColor.Danger, 2);
            Arena.ZoneRect(f.Position, new WDir(0, 1), 1, 1, 1, 0x808000FF);
        }

        foreach (var b in _bits)
            Arena.ZoneRect(b.Actor.Position, new WDir(0, 1), 1, 1, 1, 0xFFFF0080);

        foreach (var b in _orbs)
            Arena.ZoneRect(b.Position, new WDir(0, 1), 1, 1, 1, 0xFF8000FF);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add(string.Join(", ", Crosses.Select(a => $"{a.Tile} @ +{(a.Activation - WorldState.CurrentTime).TotalSeconds:f3}")));
    }
#endif
}

class D103ValiaPiraStates : StateMachineBuilder
{
    public D103ValiaPiraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EntropicSphere>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<ConcurrentField>()
            .ActivateOnEnter<ElectricField>()
            .ActivateOnEnter<NeutralizeFrontLines>()
            .ActivateOnEnter<DeterrentPulse>()
            .ActivateOnEnter<HyperchargedLight>()
            .ActivateOnEnter<Coordinates>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027, NameID = 13749)]
public class D103ValiaPira(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -331), new ArenaBoundsSquare(17.5f));

