namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

sealed class Chess(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance>[] AOEs = new List<AOEInstance>[PartyState.MaxAllianceSize];
    private readonly int[] distancesPending = new int[PartyState.MaxAllianceSize];
    private readonly AOEShapeRect square = new(5f, 5f, 5f);
    private DateTime _activation;
    private readonly List<WPos> excludeCrosses = [with(2)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (slot is < 0 or > 23)
        {
            return [];
        }
        var aoesSlot = AOEs[slot];
        if (aoesSlot != default && aoesSlot.Count != 0)
        {
            return CollectionsMarshal.AsSpan(aoesSlot);
        }
        var dist = distancesPending[slot];
        if (dist == default)
        {
            return [];
        }
        var tiles = GetSquaresAtManhattanDistance(actor.Position, dist, excludeCrosses);
        var count = tiles.Count;
        var aoes = new List<AOEInstance>(count);
        var color = Colors.SafeFromAOE;
        for (var i = 0; i < count; ++i)
        {
            aoes.Add(new(square, tiles[i], default, _activation, color, false));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public static List<WPos> GetSquaresAtManhattanDistance(WPos position, int distance, List<WPos> excluded)
    {
        var positions = new List<WPos>();
        var centerX = -272f;
        var centerZ = -415f;

        // Align to nearest square center index
        var originI = (int)Math.Round((position.X - centerX) / 10f);
        var originJ = (int)Math.Round((position.Z - centerZ) / 10f);
        var originExcludeI1 = -9f;
        var originExcludeJ1 = -9f;
        var originExcludeI2 = -9f;
        var originExcludeJ2 = -9f;
        if (excluded.Count == 2)
        {
            var exclude0 = excluded[0];
            originExcludeI1 = (int)Math.Round((exclude0.X - centerX) / 10f);
            originExcludeJ1 = (int)Math.Round((exclude0.Z - centerZ) / 10f);
            var exclude1 = excluded[1];
            originExcludeI2 = (int)Math.Round((exclude1.X - centerX) / 10f);
            originExcludeJ2 = (int)Math.Round((exclude1.Z - centerZ) / 10f);
        }

        for (var dx = -5; dx <= 5; ++dx)
        {
            for (var dy = -5; dy <= 5; ++dy)
            {
                if (Math.Abs(dx) + Math.Abs(dy) != distance)
                {
                    continue;
                }
                var i = originI + dx;
                var j = originJ + dy;
                if (Math.Abs(i) > 2 || Math.Abs(j) > 2)
                {
                    continue; // 5x5 grid = -2 to +2
                }

                if (i != originExcludeI1 && i != originExcludeI2 && j != originExcludeJ1 && j != originExcludeJ2)
                {
                    positions.Add(new(centerX + i * 10f, centerZ + j * 10f));
                }
            }
        }
        return positions;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var id = status.ID;
        if (id == (uint)SID.MovementIndicator && excludeCrosses.Count != 2)
        {
            var distance = status.Extra switch
            {
                0xE2 => 1,
                0xE3 => 2,
                0xE4 => 3,
                _ => 0
            };
            if (distance != 0)
            {
                excludeCrosses.Add(actor.Position + distance * 10f * actor.Rotation.ToDirection());
            }
        }
        else if (id is >= (uint)SID.MovementEdict2 and <= (uint)SID.MovementEdict4)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot is < 0 or > 23)
            {
                return;
            }
            distancesPending[slot] = id switch
            {
                (uint)SID.MovementEdict2 => 2,
                (uint)SID.MovementEdict3 => 3,
                (uint)SID.MovementEdict4 => 4,
                _ => default
            };
            _activation = WorldState.FutureTime(22d);
        }
        else if (id is >= (uint)SID.YourMove2Squares and <= (uint)SID.YourMove4Squares)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot is < 0 or > 23)
            {
                return;
            }

            var distance = id switch
            {
                (uint)SID.YourMove2Squares => 2,
                (uint)SID.YourMove3Squares => 3,
                (uint)SID.YourMove4Squares => 4,
                _ => default
            };
            var tiles = GetSquaresAtManhattanDistance(actor.Position, distance, excludeCrosses);
            var count = tiles.Count;
            var color = Colors.SafeFromAOE;
            if (AOEs[slot] == null)
                AOEs[slot] = [with(count)];
            for (var i = 0; i < count; ++i)
            {
                AOEs[slot].Add(new(square, tiles[i], default, _activation, color));
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var id = status.ID;
        if (id is >= (uint)SID.MovementEdict2 and <= (uint)SID.MovementEdict4)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot is < 0 or > 23)
            {
                return;
            }
            distancesPending[slot] = default;
        }
        else if (id is >= (uint)SID.YourMove2Squares and <= (uint)SID.YourMove4Squares)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot is < 0 or > 23)
            {
                return;
            }
            AOEs[slot].Clear();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EndsKnight or (uint)AID.EndsSoldier or (uint)AID.MeansGunner or (uint)AID.MeansWarrior)
        {
            excludeCrosses.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot is < 0 or > 23)
            return;
        if (distancesPending[slot] != default)
        {
            hints.Add("Prepare for movement!", false);
            return;
        }
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;

        var isInside = false;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(actor.Position))
            {
                isInside = true;
                break;
            }
        }
        hints.Add("Move to a safe tile!", !isInside);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        var forbidden = new List<ShapeDistance>(len);
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Risky)
            {
                forbidden.Add(new SDInvertedRect(aoe.Origin, new WDir(default, 1f), 5f, 5f, 5f));
            }
        }
        hints.AddForbiddenZone(new SDIntersection([.. forbidden]), aoes[0].Activation);
    }
}
