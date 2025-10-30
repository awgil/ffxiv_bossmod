namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster;

class CrypticFlames(BossModule module) : BossComponent(module)
{
    public bool ReadyToBreak { get; private set; }
    private readonly int[] _playerOrder = new int[4];
    private readonly List<(Actor laser, int order)> _lasers = [];
    private int _numBrokenLasers;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var order = _playerOrder[slot];
        if (order != 0)
            hints.Add($"Break order: {order}", order == CurrentBreakOrder);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var order = _playerOrder[pcSlot];
        foreach (var l in _lasers)
        {
            var dir = l.laser.Rotation.ToDirection();
            var extent = 2 * dir * dir.Dot(Module.Center - l.laser.Position);
            var color = l.order != _playerOrder[pcSlot] ? ArenaColor.Enemy : order == CurrentBreakOrder ? ArenaColor.Safe : ArenaColor.Danger;
            Arena.AddLine(l.laser.Position, l.laser.Position + extent, color, 2);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.FirstBrand:
                SetPlayerOrder(actor, 1);
                break;
            case SID.SecondBrand:
                SetPlayerOrder(actor, 2);
                break;
            case SID.ThirdBrand:
                SetPlayerOrder(actor, 3);
                break;
            case SID.FourthBrand:
                SetPlayerOrder(actor, 4);
                break;
            case SID.FirstFlame:
            case SID.SecondFlame:
            case SID.ThirdFlame:
            case SID.FourthFlame:
                ReadyToBreak = true;
                break;
            case SID.Counter:
                var order = status.Extra switch
                {
                    0x1C1 => -1, // unbreakable
                    0x1C2 or 0x1C6 => 1,
                    0x1C3 or 0x1C7 => 2,
                    0x1C4 or 0x1C8 => 3,
                    0x1C5 or 0x1C9 => 4,
                    _ => 0
                };
                if (order != 0)
                    _lasers.Add((actor, order));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Counter)
        {
            _numBrokenLasers += _lasers.RemoveAll(l => l.laser == actor);
        }
    }

    private void SetPlayerOrder(Actor player, int order)
    {
        if (Raid.TryFindSlot(player, out var slot) && slot < _playerOrder.Length)
            _playerOrder[slot] = order;
    }

    private int CurrentBreakOrder => _numBrokenLasers switch
    {
        < 4 => _numBrokenLasers + 1,
        < 8 => _numBrokenLasers - 4 + 1,
        _ => 0
    };
}
