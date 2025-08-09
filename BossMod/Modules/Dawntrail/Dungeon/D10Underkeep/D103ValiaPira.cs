namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D103ValiaPira;

public enum OID : uint
{
    Boss = 0x478E,
    Helper = 0x233C,
    ExplodingOrb = 0x478F, // R1.600, x6
    TetherSource = 0x47B5, // R1.000, x9
    CoordinateBit = 0x4789, // R1.000, x2
    CoordinateTurret = 0x4793, // R1.000, x4
}

public enum AID : uint
{
    AutoAttack = 42526, // Boss->player, no cast, single-target
    EntropicSphere = 42525, // Boss->self, 5.0s cast, range 40 circle
    CoordinateMarch = 42513, // Boss->self, 4.0s cast, single-target
    Collision = 42514, // ExplodingOrb->CoordinateBit/TetherSource, no cast, single-target
    EnforcementRay = 42737, // CoordinateBit->self, 0.5s cast, range 36 width 9 cross
    OrderedFire = 42508, // Boss->self, no cast, single-target
    OrderedFire2 = 42509, // Helper->Boss, 1.0s cast, single-target
    Electray = 43130, // CoordinateTurret->self, 7.0s cast, range 40 width 9 rect
    ElectricFieldVisual = 42519, // Boss->self, 6.6+0.7s cast, single-target
    ElectricFieldSpread = 43261, // Helper->self, no cast, range 26 ?-degree cone
    ConcurrentField = 42521, // Helper->self, 7.3s cast, range 26 ?-degree cone
    NeutralizeFrontLines = 42738, // Boss->self, 5.0s cast, range 30 180-degree cone
    HyperchargedLight = 42524, // Helper->player, 5.0s cast, range 5 circle
    Bloodmarch = 42739, // Boss->self, 5.0s cast, single-target
    DeterrentPulse = 42540, // Boss->self/players, 5.0s cast, range 40 width 8 rect
}

public enum IconID : uint
{
    ElectricField = 586, // Boss->player
    DeterrentPulse = 525 // Boss->player
}

public enum TetherID : uint
{
    OrbTether = 322, // TetherSource->Boss
    BitTether = 321, // TetherSource->Boss
    OrbTeleport = 282, // TetherSource->ExplodingOrb
}

class EntropicSphere(BossModule module) : Components.RaidwideCast(module, AID.EntropicSphere);
class Electray(BossModule module) : Components.StandardAOEs(module, AID.Electray, new AOEShapeRect(40, 4.5f));
class ConcurrentField(BossModule module) : Components.StandardAOEs(module, AID.ConcurrentField, new AOEShapeCone(26, 25.Degrees()));
class ElectricField(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(26, 25.Degrees()), (uint)IconID.ElectricField, AID.ElectricFieldSpread, 7.4f);
class NeutralizeFrontLines(BossModule module) : Components.StandardAOEs(module, AID.NeutralizeFrontLines, new AOEShapeCone(30, 90.Degrees()));
class DeterrentPulse(BossModule module) : Components.GenericWildCharge(module, 4, AID.DeterrentPulse, 40)
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
class HyperchargedLight(BossModule module) : Components.SpreadFromCastTargets(module, AID.HyperchargedLight, 5);

class CoordinateMarch(BossModule module) : Components.GenericAOEs(module, AID.EnforcementRay)
{
    public int BossCasts;

    private readonly List<Actor> _orbs = [];
    private readonly List<(Actor Bit, WPos Spawn, DateTime MoveStart)> _bits = [];
    private readonly List<(WPos Source, DateTime Activation)> _crosses = [];

    private SourceSide _sourceSide;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _crosses.Take(1).Select(c => new AOEInstance(new AOEShapeCross(36, 4.5f), c.Source, Activation: c.Activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CoordinateMarch)
            BossCasts++;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.OrbTeleport)
        {
            _orbs.Add(source);
            _orbs.RemoveAll(o => o.InstanceID == tether.Target);
            Predict();
        }
    }

    private WPos ClampToArena(WPos p) => new(Math.Clamp(p.X, Arena.Center.X - 17.5f, Arena.Center.X + 17.5f), Math.Clamp(p.Z, Arena.Center.Z - 17.5f, Arena.Center.Z + 17.5f));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        switch (((OID)actor.OID, id))
        {
            case (OID.ExplodingOrb, 0x11DA):
                _orbs.Add(actor);
                break;
            case (OID.CoordinateBit, 0x11D5):
                if (_sourceSide == SourceSide.None)
                    _sourceSide = actor.Position.Z < -347 ? SourceSide.North : SourceSide.East;
                _bits.Add((actor, ClampToArena(actor.Position), default));
                break;
            case (OID.CoordinateBit, 0x24E1):
                _bits.RemoveAll(b => b.Bit == actor);
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.CoordinateBit && status.ID == 2056)
        {
            var bit = _bits.FindIndex(b => b.Bit == actor);
            if (bit >= 0)
            {
                _bits.Ref(bit).MoveStart = WorldState.CurrentTime;
                Predict();
            }
        }
    }

    enum SourceSide
    {
        None,
        North,
        East
    }

    // arrays of tile indices, indexed by starting tile
    public static readonly byte[][] PathsFirstN = [
        [], [1,5,9,8,12], [2,6,10,11,15], [],
        [],[],[],[],
        [],[],[],[],
        [],[],[],[]
    ];
    public static readonly byte[][] PathsFirstE = [
        [],[],[],[],
        [],[],[], Rotate(PathsFirstN[1]),
        [],[],[], Rotate(PathsFirstN[2]),
        [],[],[],[]
    ];
    public static readonly byte[][] PathsRestN = [
        [0,1,5,4,8,9,13,12], [1,0,4,5,9,8,12,13], [2,3,7,6,10,11,15,14], [3,2,6,7,11,10,14,15],
        [],[],[],[],
        [],[],[],[],
        [],[],[],[]
    ];
    public static readonly byte[][] PathsRestE = [
        [],[],[],Rotate(PathsRestN[0]),
        [],[],[],Rotate(PathsRestN[1]),
        [],[],[],Rotate(PathsRestN[2]),
        [],[],[],Rotate(PathsRestN[3])
    ];

    private static byte[] Rotate(byte[] tiles)
    {
        var ret = new byte[tiles.Length];
        for (var i = 0; i < tiles.Length; i++)
        {
            var t = tiles[i];
            ret[i] = (byte)(3 - (t / 4) + (4 * (t % 4)));
        }
        return ret;
    }

    private byte FindTile(WPos p)
    {
        var off = (p - Arena.Center) / 9f + new WDir(2, 2);
        var col = (int)off.X;
        var row = (int)off.Z;
        return (byte)(row * 4 + col);
    }

    private void Predict()
    {
        _crosses.Clear();

        if (_bits.Any(b => b.MoveStart == default))
            // second bit hasn't moved yet, we need to use that timestamp to predict activations
            return;

        var collisionCounter = new int[16]; // indexed by path

        const float travelBase = 1.75f; // estimate

        var curPaths = (BossCasts, _sourceSide) switch
        {
            ( <= 1, SourceSide.North) => PathsFirstN,
            ( <= 1, SourceSide.East) => PathsFirstE,
            _ => _sourceSide == SourceSide.North ? PathsRestN : PathsRestE
        };

        List<(Actor orb, Actor bit, DateTime bitSpawn, int path, int distance)> collisions = [];

        foreach (var o in _orbs)
        {
            var thisTile = FindTile(o.Position);
            var thisBit = _bits.First(b => curPaths[FindTile(b.Spawn)].Contains(thisTile));
            var thisPathIx = FindTile(thisBit.Spawn);

            var dist = Array.IndexOf(curPaths[thisPathIx], thisTile);
            collisions.Add((o, thisBit.Bit, thisBit.MoveStart, thisPathIx, dist));
        }

        collisions.SortBy(c => c.distance);

        foreach (var coll in collisions)
        {
            var travelTime = travelBase;
            travelTime += coll.distance * 2; // bits travel about half a tile per second
            travelTime += collisionCounter[coll.path] * 2.5f; // bits pause about 2.5s during collision

            _crosses.Add((coll.orb.Position, coll.bitSpawn.AddSeconds(travelTime)));
            collisionCounter[coll.path]++;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Collision)
        {
            _sourceSide = SourceSide.None;
            _orbs.RemoveAll(o => o.Position.AlmostEqual(caster.Position, 1));
        }

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var ix = _crosses.FindIndex(c => c.Source.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
            {
                if (Service.IsDev)
                {
                    var cross = _crosses[ix];
                    Service.Log($"predicted activation {cross.Activation}, actual activation {WorldState.CurrentTime}, difference {(cross.Activation - WorldState.CurrentTime).TotalSeconds:f3}");
                }
                _crosses.RemoveAt(ix);
            }
        }
    }
}

class Debugger(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _orbs = [];
    private readonly List<Actor> _bits = [];
    private readonly List<(Actor From, Actor To)> _links = [];

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
            case (OID.ExplodingOrb, 0x11DA):
                _orbs.Add(actor);
                _links.RemoveAll(t => t.From.Position.AlmostEqual(actor.Position, 0.1f));
                break;
            case (OID.CoordinateBit, 0x11D5):
                _bits.Add(actor);
                break;
            case (OID.CoordinateBit, 0x24E1):
                _bits.Remove(actor);
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.OrbTeleport)
            _links.Add((source, WorldState.Actors.Find(tether.Target)!));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Collision)
            _orbs.Remove(caster);
    }

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
            Arena.ZoneRect(b.Position, new WDir(0, 1), 1, 1, 1, 0xFFFF0080);

        foreach (var b in _orbs)
            Arena.ZoneCircle(b.Position, 1, 0xFF8000FF);
    }

    private WPos TileCenter(int tile)
    {
        var col = tile % 4;
        var row = tile / 4;
        var off = new WDir(-13.5f, -13.5f) + new WDir(9 * col, 9 * row);
        return Arena.Center + off;
    }
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
            .ActivateOnEnter<CoordinateMarch>();
        // .ActivateOnEnter<Debugger>(Service.IsDev);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027, NameID = 13749)]
public class D103ValiaPira(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -331), new ArenaBoundsSquare(17.5f));

