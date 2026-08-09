namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class ChasmOfVollokFangSmall(BossModule module) : Components.GenericAOEs(module, (uint)AID.ChasmOfVollokFangSmallAOE)
{
    public readonly List<AOEInstance> AOEs = [];
    private const float platformOffset = 21.2132f;
    private static readonly AOEShapeRect _shape = new(5f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChasmOfVollokFangSmall)
        {
            // the visual cast happens on one of the side platforms at intercardinals, offset by 30
            var pos = spell.LocXZ;
            var offset = new WDir(pos.X > Arena.Center.X ? -platformOffset : +platformOffset, pos.Z > Arena.Center.Z ? -platformOffset : +platformOffset);
            AOEs.Add(new(_shape, (pos + offset).Quantized(), spell.Rotation, Module.CastFinishAt(spell)));
        }
    }
}

// note: we can start showing aoes earlier, right when fang actors spawn
sealed class ChasmOfVollokFangLarge(BossModule module) : Components.GenericAOEs(module, (uint)AID.ChasmOfVollokFangLargeAOE)
{
    public readonly List<AOEInstance> AOEs = [];

    private readonly AOEShapeRect _shape = new(10f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VollokLargeAOE)
        {
            var act = Module.CastFinishAt(spell);
            var center = Arena.Center;
            var rot = spell.Rotation;
            AOEs.Add(new(_shape, spell.LocXZ, rot, act));
            var pos = spell.LocXZ;
            var mainOffset = new WPos(100f, 100f) - center;
            var fangOffset = pos - center;
            var mirrorOffset = fangOffset.Dot(mainOffset) > 0f ? -2f * mainOffset : 2f * mainOffset;
            AOEs.Add(new(_shape, (pos + mirrorOffset).Quantized(), rot, act));
        }
    }
}

sealed class ChasmOfVollokPlayer(BossModule module) : Components.GenericAOEs(module, (uint)AID.ChasmOfVollokPlayer, "GTFO from occupied cell!")
{
    public bool Active;
    private readonly List<Actor> _targets = [with(8)];
    private DateTime _activation;

    private readonly AOEShapeRect _shape = new(2.5f, 2.5f, 2.5f);
    private readonly WDir _localX = (-135f).Degrees().ToDirection();
    private readonly WDir _localZ = 135f.Degrees().ToDirection();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            return [];
        var aoes = new List<AOEInstance>();
        var mid = new WPos(100f, 100f);
        var platformOffset = 2f * (Arena.Center - mid);
        var a45 = 45f.Degrees();
        var count = _targets.Count;
        for (var i = 0; i < count; ++i)
        {
            var t = _targets[i];
            if (t == actor)
            {
                continue;
            }
            var playerOffset = t.Position - mid;
            var playerX = _localX.Dot(playerOffset);
            var playerZ = _localZ.Dot(playerOffset);
            if (Math.Abs(playerX) >= 15f || Math.Abs(playerZ) >= 15f)
            {
                playerOffset -= platformOffset;
                playerX = _localX.Dot(playerOffset);
                playerZ = _localZ.Dot(playerOffset);
            }
            var cellX = CoordinateToCell(playerX);
            var cellZ = CoordinateToCell(playerZ);
            var cellCenter = mid + _localX * CellCenterCoordinate(cellX) + _localZ * CellCenterCoordinate(cellZ);

            aoes.Add(new(_shape, cellCenter, a45, _activation));
            if (platformOffset != default)
            {
                aoes.Add(new(_shape, cellCenter + platformOffset, a45, _activation));
            }
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void Update()
    {
        // assume that if player dies, he won't participate in the mechanic
        var count = _targets.Count;
        if (count == 0)
            return;
        for (var i = count - 1; i >= 0; --i)
        {
            if (_targets[i].IsDead)
                _targets.RemoveAt(i);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ChasmOfVollok)
        {
            _targets.Add(actor);
            _activation = WorldState.FutureTime(6.1d);
        }
    }

    private static int CoordinateToCell(float x) => x switch
    {
        < -5f => 0,
        < 0f => 1,
        < 5f => 2,
        _ => 3
    };

    private static float CellCenterCoordinate(int c) => -7.5f + c * 5f;
}
