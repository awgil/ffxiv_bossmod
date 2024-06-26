namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

abstract class Chess(BossModule module) : Components.GenericAOEs(module)
{
    public struct GuardState
    {
        public Actor? Actor;
        public WPos FinalPosition;
    }

    protected GuardState[] GuardStates = new GuardState[4];
    protected static readonly AOEShapeCross Shape = new(60, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts >= 4)
            yield break;

        IEnumerable<GuardState> imminent = NumCasts < 2 ? GuardStates.Take(2) : GuardStates.Skip(2);
        foreach (var g in imminent)
            if (g.Actor != null)
                yield return new(Shape, g.FinalPosition, g.Actor.Rotation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MovementIndicator)
        {
            var distance = status.Extra switch
            {
                0xE2 => 1,
                0xE3 => 2,
                0xE4 => 3,
                _ => 0
            };
            var index = GuardIndex(actor);
            if (distance != 0 && index >= 0 && GuardStates[index].Actor == null)
            {
                GuardStates[index] = new() { Actor = actor, FinalPosition = actor.Position + distance * 10 * actor.Rotation.ToDirection() };
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EndsKnight or AID.MeansWarrior or AID.EndsSoldier or AID.MeansGunner)
            ++NumCasts;
    }

    protected int GuardIndex(Actor actor) => (OID)actor.OID switch
    {
        OID.QueensKnight => 0,
        OID.QueensWarrior => 1,
        OID.QueensSoldier => 2,
        OID.QueensGunner => 3,
        _ => -1
    };
}

class QueensWill(BossModule module) : Chess(module) { }

// TODO: enumerate all possible safespots instead? after first pair of casts, select still suitable second safespots
class QueensEdict(BossModule module) : Chess(module)
{
    public class PlayerState
    {
        public int FirstEdict;
        public int SecondEdict;
        public List<WPos> Safespots = [];
    }

    public int NumStuns { get; private set; }
    private readonly Dictionary<ulong, PlayerState> _playerStates = [];
    private int _safespotZOffset;

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var m in GetSafeSpotMoves(actor))
            movementHints.Add(m);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in GetSafeSpotMoves(pc))
            Arena.AddLine(m.from, m.to, m.color);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        base.OnStatusGain(actor, status);
        switch ((SID)status.ID)
        {
            case SID.Stun:
                ++NumStuns;
                break;
            case SID.MovementEdictShort2:
                _playerStates.GetOrAdd(actor.InstanceID).FirstEdict = 2;
                break;
            case SID.MovementEdictShort3:
                _playerStates.GetOrAdd(actor.InstanceID).FirstEdict = 3;
                break;
            case SID.MovementEdictShort4:
                _playerStates.GetOrAdd(actor.InstanceID).FirstEdict = 4;
                break;
            case SID.MovementEdictLong2:
                _playerStates.GetOrAdd(actor.InstanceID).SecondEdict = 2;
                break;
            case SID.MovementEdictLong3:
                _playerStates.GetOrAdd(actor.InstanceID).SecondEdict = 3;
                break;
            case SID.MovementEdictLong4:
                _playerStates.GetOrAdd(actor.InstanceID).SecondEdict = 4;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Stun)
            --NumStuns;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is 0x1C or 0x1D && state == 0x00020001)
            _safespotZOffset = index == 0x1D ? 2 : -2;
    }

    private IEnumerable<(WPos from, WPos to, uint color)> GetSafeSpotMoves(Actor actor)
    {
        var state = _playerStates.GetValueOrDefault(actor.InstanceID);
        if (state == null)
            yield break;

        if (state.Safespots.Count == 0)
        {
            // try initializing safespots on demand
            if (_safespotZOffset == 0 || state.FirstEdict == 0 || state.SecondEdict == 0 || GuardStates.Any(g => g.Actor == null))
                yield break; // not ready yet...

            // initialize second safespot: select cells that are SecondEdict distance from safespot and not in columns clipped by second set of guards
            int forbiddenCol1 = OffsetToCell(GuardStates[2].FinalPosition.X - Module.Center.X);
            int forbiddenCol2 = OffsetToCell(GuardStates[3].FinalPosition.X - Module.Center.X);
            int forbiddenRow1 = OffsetToCell(GuardStates[0].FinalPosition.Z - Module.Center.Z);
            int forbiddenRow2 = OffsetToCell(GuardStates[1].FinalPosition.Z - Module.Center.Z);
            foreach (var s2 in CellsAtManhattanDistance((0, _safespotZOffset), state.SecondEdict).Where(s2 => s2.x != forbiddenCol1 && s2.x != forbiddenCol2))
            {
                foreach (var s1 in CellsAtManhattanDistance(s2, state.FirstEdict).Where(s1 => s1.z != forbiddenRow1 && s1.z != forbiddenRow2))
                {
                    state.Safespots.Add(CellCenter(s1));
                    state.Safespots.Add(CellCenter(s2));
                    state.Safespots.Add(CellCenter((0, _safespotZOffset)));
                    break;
                }
                if (state.Safespots.Count > 0)
                    break;
            }
        }

        uint color = ArenaColor.Safe;
        var from = actor.Position;
        foreach (var p in state.Safespots.Skip(NumCasts / 2))
        {
            yield return (from, p, color);
            from = p;
            color = ArenaColor.Danger;
        }
    }

    private int OffsetToCell(float offset) => offset switch
    {
        < -25 => -3,
        < -15 => -2,
        < -5 => -1,
        < 5 => 0,
        < 15 => 1,
        < 25 => 2,
        _ => 3
    };

    private WPos CellCenter((int x, int z) cell) => Module.Center + 10 * new WDir(cell.x, cell.z);

    private IEnumerable<(int x, int z)> CellsAtManhattanDistance((int x, int z) origin, int distance)
    {
        for (int x = -2; x <= 2; ++x)
        {
            var dz = distance - Math.Abs(x - origin.x);
            if (dz == 0)
            {
                yield return (x, origin.z);
            }
            else if (dz > 0)
            {
                var z1 = origin.z - dz;
                var z2 = origin.z + dz;
                if (z1 >= -2)
                    yield return (x, z1);
                if (z2 <= +2)
                    yield return (x, z2);
            }
        }
    }
}
