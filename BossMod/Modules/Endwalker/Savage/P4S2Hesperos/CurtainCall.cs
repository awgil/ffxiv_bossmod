namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to curtain call mechanic
class CurtainCall(BossModule module) : BossComponent(module)
{
    private readonly int[] _playerOrder = new int[8];
    private List<Actor>? _playersInBreakOrder;
    private int _numCasts;

    public override void Update()
    {
        _playersInBreakOrder ??= [.. Raid.WithSlot(true).WhereSlot(i => _playerOrder[i] != 0).OrderBy(ip => _playerOrder[ip.Item1]).Actors()];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerOrder[slot] > _numCasts)
        {
            var relOrder = _playerOrder[slot] - _numCasts;
            hints.Add($"Tether break order: {relOrder}", relOrder == 1);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_playersInBreakOrder != null)
            hints.Add($"Order: {string.Join(" -> ", _playersInBreakOrder.Skip(_numCasts).Select(OrderTextForPlayer))}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw other players
        foreach ((int slot, var player) in Raid.WithSlot().Exclude(pc))
            Arena.Actor(player, _playerOrder[slot] == _numCasts + 1 ? ArenaColor.Danger : ArenaColor.PlayerGeneric);

        // tether
        var tetherTarget = WorldState.Actors.Find(pc.Tether.Target);
        if (tetherTarget != null)
            Arena.AddLine(pc.Position, tetherTarget.Position, pc.Tether.ID == (uint)TetherID.WreathOfThorns ? ArenaColor.Danger : ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Thornpricked && Raid.TryFindSlot(actor, out var slot))
        {
            _playerOrder[slot] = 2 * (int)((status.ExpireAt - WorldState.CurrentTime).TotalSeconds / 10); // 2/4/6/8
            bool ddFirst = Service.Config.Get<P4S2Config>().CurtainCallDDFirst;
            if (ddFirst != actor.Role is Role.Tank or Role.Healer)
                --_playerOrder[slot];
            _playersInBreakOrder = null;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Thornpricked)
            ++_numCasts;
    }

    private string OrderTextForPlayer(Actor player)
    {
        //return player.Name;
        var status = player.FindStatus((uint)SID.Thornpricked);
        var remaining = status != null ? (status.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds : 0;
        return $"{player.Name} ({remaining:f1}s)";
    }
}
