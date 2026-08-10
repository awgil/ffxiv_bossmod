namespace BossMod.Endwalker.Alliance.A11Byregot;

class HammersCells(BossModule module) : Components.GenericAOEs(module, (uint)AID.DestroySideTiles, "GTFO from dangerous tile!")
{
    public bool Active;
    public bool MovementPending;
    public readonly int[] LineOffset = new int[5];
    public readonly int[] LineMovement = new int[5];
    private readonly AOEShapeRect _shape = new(5f, 5f, 5f);
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!MovementPending)
        {
            return [];
        }

        var aoes = new List<AOEInstance>();
        for (var z = -2; z <= 2; ++z)
        {
            for (var x = -2; x <= 2; ++x)
            {
                var center = CellCenter(x, z);
                if (Arena.InBounds(center) && CellDangerous(x, z))
                {
                    aoes.Add(new(_shape, center, default, activation));
                }
            }
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Active = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Arena.Bounds = new ArenaBoundsRect(15f, 25f);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x07 and <= 0x0B)
        {
            (var offset, var movement) = state switch
            {
                0x00020001u => (0, +1),
                0x08000400u => (-1, +1),
                0x00800040u => (0, -1),
                0x80004000u => (+1, -1),
                _ => default
            };
            if (movement == default)
                return;
            var i = index - 0x07;
            LineOffset[i] = offset;
            LineMovement[i] = movement;
            MovementPending = true;
            activation = WorldState.FutureTime(16d);
        }
        else if (index == 0x1A)
        {
            MovementPending = false;
            for (var i = 0; i < 5; ++i)
            {
                LineOffset[i] += LineMovement[i];
                LineMovement[i] = 0;
            }
            var rects = new Rectangle[5];
            var start = new WPos(default, 680f);
            for (var i = 0; i < 5; ++i)
            {
                rects[i] = new(start + new WDir(LineOffset[i] * 10f, 10f * i), 15f, 5f);
            }
            var arena = new ArenaBoundsCustom(rects);
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
        }
        else if (index == 0x4F && state == 0x00080004u)
        {
            Active = false;
            new Span<int>(LineOffset).Clear();
            new Span<int>(LineMovement).Clear();
        }
    }

    public static WPos CellCenter(int x, int z) => new WPos(0f, 700f) + 10f * new WDir(x, z);

    private bool CellDangerous(int x, int z)
    {
        var off = LineOffset[z + 2] + LineMovement[z + 2];
        return Math.Abs(x - off) > 1;
    }
}

abstract class Rect(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(50f, 5f));
class DestroySideTiles(BossModule module) : Rect(module, (uint)AID.DestroySideTiles);
class HammersLevinforge(BossModule module) : Rect(module, (uint)AID.Levinforge);

class HammersSpire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ByregotSpire, new AOEShapeRect(50f, 15f))
{
    private WPos? _safespot;
    private readonly HammersCells _cells = module.FindComponent<HammersCells>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _safespot != null ? [] : CollectionsMarshal.AsSpan(Casters);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            ref var aoe = ref CollectionsMarshal.AsSpan(Casters)[0];
            var safespots = new List<WPos>();
            {
                for (var z = -2; z <= 2; ++z)
                {
                    for (var x = -2; x <= 2; ++x)
                    {
                        var center = HammersCells.CellCenter(x, z);
                        if (!CellSafe(x, z, true) && CellSafe(x, z, false) && !aoe.Check(center))
                            safespots.Add(center);
                    }
                }
                var count = safespots.Count;
                var minDistSq = float.MaxValue;
                var primaryPos = Module.PrimaryActor.Position;
                for (var i = 0; i < count; ++i)
                {
                    var safespot = safespots[i];
                    var distSq = (safespot - primaryPos).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        _safespot = safespot;
                    }
                }

                bool CellSafe(int x, int z, bool future)
                {
                    var off = _cells.LineOffset[z + 2];
                    if (future)
                        off += _cells.LineMovement[z + 2];
                    return Math.Abs(x - off) > 1;
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_safespot is WPos pos)
        {
            Arena.AddRect(pos, new(default, 1), 5f, 5f, 5f, Colors.Safe, 2f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_safespot is WPos pos)
        {
            hints.GoalZones.Add(AIHints.GoalRectangle(pos, new WDir(default, 1f), 5f, 5f, 10f));
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        _safespot = null;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_safespot != null)
        {
            hints.Add("Prepare to go to upcoming safespot!");
        }
    }
}
