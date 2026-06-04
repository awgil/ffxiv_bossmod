namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1TelePortent(BossModule module) : BossComponent(module)
{
    [Flags]
    enum Direction : byte
    {
        None,
        Up = 1,
        Down = 2,
        Right = 4,
        Left = 8
    }

    class Directions
    {
        public Direction All;

        public (Direction Dir, DateTime Time) D1;
        public (Direction Dir, DateTime Time) D2;
    }

    readonly Directions[] _debuffs = Utils.GenArray<Directions>(8, () => new());
    readonly List<(Direction, WPos)>[] _hintSpots = Utils.GenArray<List<(Direction, WPos)>>(8, () => []);
    readonly int[] _timesHit = new int[8];

    static WDir ToWDir(Direction d)
    {
        var wd = new WDir(0, 0);
        if (d.HasFlag(Direction.Up))
            wd.Z -= 1;
        if (d.HasFlag(Direction.Down))
            wd.Z += 1;
        if (d.HasFlag(Direction.Right))
            wd.X += 1;
        if (d.HasFlag(Direction.Left))
            wd.X -= 1;
        return wd;
    }

    static Direction FromWDir(WDir d)
    {
        d = d.Normalized().Rounded();
        if (d.X < 0)
            return Direction.Left;
        if (d.X > 0)
            return Direction.Right;
        if (d.Z > 0)
            return Direction.Down;
        if (d.Z < 0)
            return Direction.Up;
        return Direction.None;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var newDir = (SID)status.ID switch
        {
            SID.TelePortentN1 or SID.TelePortentN2 => Direction.Up,
            SID.TelePortentS1 or SID.TelePortentS2 => Direction.Down,
            SID.TelePortentE1 or SID.TelePortentE2 => Direction.Right,
            SID.TelePortentW1 or SID.TelePortentW2 => Direction.Left,
            _ => default
        };

        if (newDir == default || !Raid.TryFindSlot(actor.InstanceID, out var slot))
            return;

        var dir = _debuffs[slot];

        dir.All |= newDir;

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8)
            dir.D2 = (newDir, status.ExpireAt);
        else
            dir.D1 = (newDir, status.ExpireAt);

        if (dir.D1 == default || dir.D2 == default)
            return;

        var wd = ToWDir(dir.All);

        if (dir.All == dir.D1.Dir)
        {
            // matched arrows
            var cardinal = wd.OrthoL() * 12;
            var preCardinal = (wd + wd.OrthoL() * 0.5f).OrthoL() * 12;
            _hintSpots[slot].AddRange([(dir.All, Arena.Center + cardinal), (dir.All, Arena.Center + preCardinal)]);
        }
        else
        {
            // unmatched arrows
            var intercard = wd.OrthoL() * 6;
            var preIntercard = intercard + (wd + wd.OrthoL()).Sign().OrthoL() * 6;
            _hintSpots[slot].AddRange([(FromWDir(wd.Rotate(-45.Degrees())), Arena.Center + intercard * 2), (FromWDir(wd.Rotate(45.Degrees())), Arena.Center + preIntercard)]);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var d = _debuffs[slot];
        hints.Add($"{d.D1.Dir} => {d.D2.Dir}", false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (sd, sp) in _hintSpots[pcSlot])
        {
            var toCheck = _timesHit[pcSlot] == 0 ? _debuffs[pcSlot].D1.Dir : _debuffs[pcSlot].D2.Dir;

            Arena.AddCircle(sp, 0.75f, sd == toCheck ? ArenaColor.Safe : ArenaColor.Danger);
            //Arena.TextWorld(sp, sd.ToString(), ArenaColor.Object);
        }
    }
}
