namespace BossMod.Endwalker.Ultimate.DSW2;

// TODO: hints?..
sealed class P4Hatebound(BossModule module) : BossComponent(module)
{
    public enum Color { None, Red, Blue }

    public bool ColorsAssigned;
    private readonly List<(Actor orb, Color color, bool exploded)> _orbs = []; // 'red' is actually 'yellow orb'
    private readonly Color[] _playerColors = new Color[PartyState.MaxPartySize];

    public bool ColorReady(Color c) => _orbs.Any(o => o.color == c && OrbReady(o.orb));
    public bool YellowReady => ColorReady(Color.Red);
    public bool BlueReady => ColorReady(Color.Blue);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerColors[slot] != Color.None)
        {
            hints.Add($"Color: {_playerColors[slot]}", false);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var o in _orbs.Where(o => !o.exploded))
        {
            Arena.Actor(o.orb, Colors.Object, true);
            if (OrbReady(o.orb))
                Arena.ZoneCircleOutline(o.orb.Position, 6, _playerColors[pcSlot] == Color.Red ? Colors.Safe : default);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        var color = actor.OID switch
        {
            (uint)OID.AzurePrice => Color.Blue,
            (uint)OID.GildedPrice => Color.Red,
            _ => Color.None
        };
        if (color != Color.None)
            _orbs.Add((actor, color, false));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var color = status.ID switch
        {
            (uint)SID.Clawbound => Color.Red,
            (uint)SID.Fangbound => Color.Blue,
            _ => Color.None
        };
        if (color != Color.None && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            ColorsAssigned = true;
            _playerColors[slot] = color;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FlareStar or (uint)AID.FlareNova or (uint)AID.FlareNovaFail && _orbs.FindIndex(o => o.orb == caster) is var index && index >= 0)
            _orbs.AsSpan()[index].exploded = true;
    }

    private bool OrbReady(Actor orb) => orb.HitboxRadius > 1.501f; // TODO: verify...
}

sealed class P4MirageDive(BossModule module) : Components.CastCounter(module, (uint)AID.MirageDiveAOE)
{
    private readonly List<int> _targets = [];
    private BitMask _forbidden;
    private BitMask _baiters;

    private const float _radius = 4f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_baiters[slot])
        {
            // note: not showing this hint, since typically pc will wait until someone swaps the color
            //if (_forbidden[slot])
            //    hints.Add("Pass the tether!");
            if (!_forbidden[slot] && Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _radius).Any())
                hints.Add("GTFO from raid!");
        }
        else if (Raid.WithSlot(true, true, true).IncludedInMask(_baiters).ExcludedFromMask(_forbidden).InRadius(actor.Position, _radius).Any())
        {
            hints.Add("GTFO from baiters!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _baiters[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pcCanSwap = !_forbidden[pcSlot] && !_baiters[pcSlot];
        foreach (var (slot, player) in Raid.WithSlot(true, true, true).IncludedInMask(_baiters))
        {
            var canSwap = pcCanSwap && _forbidden[slot];
            Arena.ZoneCircleOutline(player.Position, _radius, canSwap ? Colors.Safe : Colors.Danger);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Clawbound)
            _baiters[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Clawbound)
            _baiters[Raid.FindSlot(actor.InstanceID)] = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            _targets.Add(Raid.FindSlot(spell.MainTargetID));
            _forbidden = default;
            var count = _targets.Count;
            var startIndex = Math.Max(0, count - 4);

            for (var i = startIndex; i < count; ++i)
            {
                _forbidden[_targets[i]] = true;
            }
        }
    }
}
