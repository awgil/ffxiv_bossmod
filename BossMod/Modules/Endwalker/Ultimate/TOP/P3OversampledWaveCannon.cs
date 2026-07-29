namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P3OversampledWaveCannon(BossModule module) : BossComponent(module)
{
    private Actor? _boss;
    private Angle _bossAngle;
    private readonly Angle[] _playerAngles = new Angle[PartyState.MaxPartySize];
    private readonly int[] _playerOrder = new int[PartyState.MaxPartySize];
    private int _numPlayerAngles;
    private readonly List<int> _monitorOrder = [];
    private readonly TOPConfig _config = Service.Config.Get<TOPConfig>();

    private DateTime _resolve;
    private readonly ArcList[] _safeAngles = Utils.GenArray(PartyState.MaxPartySize, () => new ArcList(default, 50f));

    private static readonly AOEShapeCone _shape = new(40f, 90f.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerOrder[slot] != 0)
            hints.Add($"Order: {(IsMonitor(slot) != default ? "M" : "N")}{_playerOrder[slot]}", false);

        var numHitBy = AOEs(slot).Count(a => !a.source && _shape.Check(actor.Position, a.origin, a.rot));
        if (numHitBy != 1)
            hints.Add($"Hit by {numHitBy} monitors!");
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots(slot).Where(p => p.assigned))
            movementHints.Add(actor.Position, p.pos, Colors.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PredictedDamage.Add(new(Raid.WithSlot().Mask(), _resolve, AIHints.PredictedDamageType.Raidwide));

        if (!IsMonitor(slot) || !_config.P3MonitorForbiddenDirections)
            return;

        var safeCW = _bossAngle.Rad > 0;

        var targetCW = (_config.P3LastMonitorSouth, _playerOrder[slot]) switch
        {
            (_, 1) => safeCW,
            (_, 3) => !safeCW,
            (true, 2) => !safeCW,
            (false, 2) => safeCW,
            _ => false
        };

        var al = _safeAngles[slot];
        al.Forbidden.Clear();
        al.Center = actor.Position;

        var safeConePlayers = Raid.WithoutSlot().ClockOrder(actor, Arena.Center, !targetCW).Skip(2).Take(2).ToList();
        if (targetCW)
            safeConePlayers.Reverse();

        var angleRight = actor.AngleTo(safeConePlayers[0]);
        var angleLeft = actor.AngleTo(safeConePlayers[1]);

        // forbid angle ranges that don't face the player toward or away from their intended targets
        al.ForbidArc(angleLeft, (angleRight + 180f.Degrees()).Normalized());
        al.ForbidArc((angleLeft + 180f.Degrees()).Normalized(), angleRight);

        // forbid any angle that would hit the boss with the monitor; eliminates one of the two remaining facing cones
        var dirToUnsafeCleave = actor.DirectionTo(Arena.Center).ToAngle() - _playerAngles[slot];
        al.ForbidArc(dirToUnsafeCleave - 90.Degrees(), dirToUnsafeCleave + 90f.Degrees());

        foreach (var (min, max) in al.Allowed(2f.Degrees()))
            hints.ForbiddenDirections.Add(((max + min) / 2f, (max - min) / 2f, _resolve));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var a in AOEs(pcSlot))
        {
            if (a.source)
                _shape.Outline(Arena, a.origin, a.rot);
            else
                _shape.Draw(Arena, a.origin, a.rot, a.safe ? Colors.SafeFromAOE : default);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in SafeSpots(pcSlot))
            Arena.ZoneCircleOutline(p.pos, 1f, p.assigned ? Colors.Safe : default);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var angle = status.ID switch
        {
            (uint)SID.OversampledWaveCannonLoadingL => 90f.Degrees(),
            (uint)SID.OversampledWaveCannonLoadingR => -90f.Degrees(),
            _ => default
        };
        if (angle != default && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _playerAngles[slot] = angle;
            if (++_numPlayerAngles == 3)
            {
                int n = 0, m = 0;
                foreach (var sg in Service.Config.Get<TOPConfig>().P3MonitorsAssignments.Resolve(Raid).OrderBy(sg => sg.group))
                {
                    _playerOrder[sg.slot] = IsMonitor(sg.slot) ? ++m : ++n;
                    if (IsMonitor(sg.slot))
                        _monitorOrder.Add(sg.slot);
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var angle = spell.Action.ID switch
        {
            (uint)AID.OversampledWaveCannonL => 90f.Degrees(),
            (uint)AID.OversampledWaveCannonR => -90f.Degrees(),
            _ => default
        };
        if (angle != default)
        {
            _boss = caster;
            _bossAngle = angle;
            _resolve = Module.CastFinishAt(spell);
        }
    }

    private bool IsMonitor(int slot) => _playerAngles[slot] != default;

    private List<(WPos pos, bool assigned)> SafeSpots(int slot)
    {
        if (_numPlayerAngles < 3 || _bossAngle == default)
            return [];

        WPos adjust(float x, float z) => Arena.Center + new WDir(_bossAngle.Rad < 0 ? -x : x, z);
        var safespots = new List<(WPos, bool)>(5);
        if (IsMonitor(slot))
        {
            var nextSlot = 0;
            if (!_config.P3LastMonitorSouth)
                safespots.Add((adjust(10f, -11f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(-11f, -9f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(-11f, +9f), _playerOrder[slot] == ++nextSlot));
            if (_config.P3LastMonitorSouth)
                safespots.Add((adjust(10f, 11f), _playerOrder[slot] == ++nextSlot));
        }
        else
        {
            var nextSlot = 0;
            safespots.Add((adjust(1f, -15f), _playerOrder[slot] == ++nextSlot));
            if (_config.P3LastMonitorSouth)
                safespots.Add((adjust(10f, -11f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(15f, -4f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(15f, +4f), _playerOrder[slot] == ++nextSlot));
            if (!_config.P3LastMonitorSouth)
                safespots.Add((adjust(10f, 11f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(1f, 15f), _playerOrder[slot] == ++nextSlot));
        }
        return safespots;
    }

    private (WPos origin, Angle rot, bool safe, bool source)[] AOEs(int slot)
    {
        var isMonitor = IsMonitor(slot);
        int order;
        if (_config.P3LastMonitorSouth)
        {
            // NA strat, M3 is SE/SW
            order = (isMonitor, _playerOrder[slot]) switch
            {
                (true, 1 or 2) => 0, // M1/M2 are hit by boss
                (true, 3) => 2, // M3 is hit by M2

                (_, 1 or 2) => 1, // N1/N2 are hit by M1
                (_, 3 or 4) => 3, // N3/N4 are hit by M3
                _ => 2 // N5 is hit by M2
            };
        }
        else
        {
            // EU strat, M1 is NE/NW
            order = (isMonitor, _playerOrder[slot]) switch
            {
                (_, 1) => 2, // N1/M1 are hit by M2
                (true, _) => 0, // M2/M3 are hit by boss
                (_, 2 or 3) => 1, // N2/N3 are hit by M1
                _ => 3, // N4/N5 are hit by M3
            };
        }
        var aoes = AOEs();
        var len = aoes.Length;
        var aoesNew = new (WPos, Angle, bool, bool)[len];
        var index = 0;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.origin != null)
            {
                aoesNew[index++] = (aoe.origin.Position, aoe.origin.Rotation + aoe.offset, aoe.order == order, isMonitor && aoe.order == _playerOrder[slot]);
            }
        }
        return aoesNew[..index];
    }

    private (Actor? origin, Angle offset, int order)[] AOEs()
    {
        var count = _monitorOrder.Count;
        var aoes = new (Actor?, Angle, int)[count + 1];
        aoes[0] = (_boss, _bossAngle, 0);
        for (var i = 0; i < _monitorOrder.Count; ++i)
        {
            var slot = _monitorOrder[i];
            aoes[i + 1] = (Raid[slot], _playerAngles[slot], i + 1);
        }
        return aoes;
    }
}

sealed class P3OversampledWaveCannonSpread(BossModule module) : Components.UniformStackSpread(module, default, 7f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OversampledWaveCannonR or (uint)AID.OversampledWaveCannonL)
            AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OversampledWaveCannonAOE)
            Spreads.Clear();
    }
}
