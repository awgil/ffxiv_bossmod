namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class ChasmOfVollokFangSmall(BossModule module) : Components.GenericAOEs(module, AID.ChasmOfVollokFangSmallAOE)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(2.5f, 2.5f, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChasmOfVollokFangSmall)
        {
            // the visual cast happens on one of the side platforms at intercardinals, offset by 30
            var platformOffset = 30 / 1.41421356f;
            var offset = new WDir(caster.Position.X > Module.Center.X ? -platformOffset : +platformOffset, caster.Position.Z > Module.Center.Z ? -platformOffset : +platformOffset);
            AOEs.Add(new(_shape, caster.Position + offset, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }
}

// note: we can start showing aoes earlier, right when fang actors spawn
class ChasmOfVollokFangLarge(BossModule module) : Components.GenericAOEs(module, AID.ChasmOfVollokFangLargeAOE)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VollokLargeAOE)
        {
            AOEs.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));

            var mainOffset = Ex2ZoraalJa.NormalCenter - Module.Center;
            var fangOffset = caster.Position - Module.Center;
            var mirrorOffset = fangOffset.Dot(mainOffset) > 0 ? -2 * mainOffset : 2 * mainOffset;
            AOEs.Add(new(_shape, caster.Position + mirrorOffset, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }
}

class ChasmOfVollokPlayer(BossModule module) : Components.GenericAOEs(module, AID.ChasmOfVollokPlayer, "GTFO from occupied cell!")
{
    public bool Active;
    private readonly List<Actor> _targets = [];
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(2.5f, 2.5f, 2.5f);
    private static readonly WDir _localX = (-135).Degrees().ToDirection();
    private static readonly WDir _localZ = 135.Degrees().ToDirection();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield break;
        var platformOffset = 2 * (Module.Center - Ex2ZoraalJa.NormalCenter);
        foreach (var t in _targets.Exclude(actor))
        {
            var playerOffset = t.Position - Ex2ZoraalJa.NormalCenter;
            var playerX = _localX.Dot(playerOffset);
            var playerZ = _localZ.Dot(playerOffset);
            if (Math.Abs(playerX) >= 15 || Math.Abs(playerZ) >= 15)
            {
                playerOffset -= platformOffset;
                playerX = _localX.Dot(playerOffset);
                playerZ = _localZ.Dot(playerOffset);
            }
            var cellX = CoordinateToCell(playerX);
            var cellZ = CoordinateToCell(playerZ);
            var cellCenter = Ex2ZoraalJa.NormalCenter + _localX * CellCenterCoordinate(cellX) + _localZ * CellCenterCoordinate(cellZ);

            yield return new(_shape, cellCenter, 45.Degrees(), _activation);
            if (platformOffset != default)
                yield return new(_shape, cellCenter + platformOffset, 45.Degrees(), _activation);
        }
    }

    public override void Update()
    {
        // assume that if player dies, he won't participate in the mechanic
        _targets.RemoveAll(t => t.IsDead);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ChasmOfVollok)
        {
            _targets.Add(actor);
            _activation = WorldState.FutureTime(6.1f);
        }
    }

    private int CoordinateToCell(float x) => x switch
    {
        < -5 => 0,
        < 0 => 1,
        < 5 => 2,
        _ => 3
    };

    private float CellCenterCoordinate(int c) => -7.5f + c * 5;
}
