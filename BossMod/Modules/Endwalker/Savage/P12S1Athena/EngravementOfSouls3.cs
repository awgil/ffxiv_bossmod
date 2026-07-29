namespace BossMod.Endwalker.Savage.P12S1Athena;

sealed class EngravementOfSouls3Shock(BossModule module) : Components.CastTowers(module, (uint)AID.Shock, 3f)
{
    private BitMask _towers;
    private BitMask _plus;
    private BitMask _cross;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.UmbralbrightSoul:
            case (uint)SID.AstralbrightSoul:
                _towers[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.QuarteredSoul:
                _plus[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.XMarkedSoul:
                _cross[Raid.FindSlot(actor.InstanceID)] = true;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var forbidden = spell.Location.Z switch
            {
                < 90f => ~_plus, // TODO: technically cross and plus could switch places
                > 110f => ~_cross,
                _ => ~_towers // TODO: assign specific towers based on priorities?
            };
            Towers.Add(new(spell.LocXZ, Radius, forbiddenSoakers: forbidden));
        }
    }
}

sealed class EngravementOfSouls3Spread(BossModule module) : Components.UniformStackSpread(module, default, 3f, raidwideOnResolve: false)
{
    private readonly EngravementOfSoulsTethers? _tethers = module.FindComponent<EngravementOfSoulsTethers>();
    private EngravementOfSoulsTethers.TetherType _soakers;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (IsSpreadTarget(pc))
            return _tethers?.States[playerSlot].Tether == _soakers ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        else
            return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var soakers = status.ID switch
        {
            (uint)SID.UmbralbrightSoul => EngravementOfSoulsTethers.TetherType.Dark,
            (uint)SID.AstralbrightSoul => EngravementOfSoulsTethers.TetherType.Light,
            _ => EngravementOfSoulsTethers.TetherType.None
        };
        if (soakers != EngravementOfSoulsTethers.TetherType.None)
        {
            _soakers = soakers;
            AddSpread(actor); // TODO: activation
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.UmbralGlow or (uint)AID.AstralGlow)
        {
            Spreads.Clear();
        }
    }
}

sealed class TheosCrossSaltire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TheosCross, (uint)AID.TheosSaltire], new AOEShapeCross(40f, 3f));

// TODO: this assumes standard strats, there could be variations i guess...
sealed class EngravementOfSouls3Hints(BossModule module) : BossComponent(module)
{
    public enum PlayerState { None, Tower, Plus, Cross, TetherTL, TetherBL, TetherTR, TetherBR }
    public enum Mechanic { Start, FixedTowers, Tethers, CrossPlusBait, TowersBait, WhiteFlameBait, TowersResolve }

    private Mechanic _nextMechanic;
    private bool _topLeftSafe;
    private bool _towersLight;
    private bool _leftTowerMatchTether;
    private readonly PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        var hints = PositionHints(slot);
        var count = hints.Count;
        if (count != 0)
        {
            for (var i = 0; i < count; ++i)
            {
                var chain = hints[i];
                var count2 = chain.Count;
                var from = actor.Position;
                var color = Colors.Safe;
                for (var j = 0; j < count2; ++j)
                {
                    var to = Arena.Center + chain[j];
                    movementHints.Add(from, to, color);
                    from = to;
                    color = Colors.Danger;
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var hints = PositionHints(pcSlot);
        var count = hints.Count;
        for (var i = 0; i < count; ++i)
        {
            var chain = hints[i];
            if (chain.Count > 0)
            {
                Arena.ZoneCircleOutline(Arena.Center + chain[0], 1f, Colors.Safe);
            }
        }
    }

    // note: these statuses are assigned before any tethers
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.UmbralbrightSoul:
                _towersLight = true;
                SetState(Raid.FindSlot(actor.InstanceID), PlayerState.Tower);
                break;
            case (uint)SID.AstralbrightSoul:
                _towersLight = false;
                SetState(Raid.FindSlot(actor.InstanceID), PlayerState.Tower);
                break;
            case (uint)SID.QuarteredSoul:
                SetState(Raid.FindSlot(actor.InstanceID), PlayerState.Plus);
                break;
            case (uint)SID.XMarkedSoul:
                SetState(Raid.FindSlot(actor.InstanceID), PlayerState.Cross);
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        switch (tether.ID)
        {
            case (uint)TetherID.LightNear:
            case (uint)TetherID.LightFar:
                AssignTether(source, Raid.FindSlot(tether.Target), true);
                break;
            case (uint)TetherID.DarkNear:
            case (uint)TetherID.DarkFar:
                AssignTether(source, Raid.FindSlot(tether.Target), false);
                break;
            case (uint)TetherID.UnnaturalEnchainment:
                if (source.PosRot.Z < 90f)
                {
                    _topLeftSafe = source.PosRot.X > Arena.Center.X;
                    AdvanceMechanic(Mechanic.FixedTowers);
                }
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TheosCross or (uint)AID.TheosSaltire)
        {
            AdvanceMechanic(Mechanic.TowersBait);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Shock:
                AdvanceMechanic(Mechanic.Tethers);
                break;
            case (uint)AID.SearingRadiance:
            case (uint)AID.Shadowsear:
                AdvanceMechanic(Mechanic.CrossPlusBait);
                break;
            case (uint)AID.UmbralGlow:
            case (uint)AID.AstralGlow:
                AdvanceMechanic(Mechanic.WhiteFlameBait);
                break;
            case (uint)AID.WhiteFlameAOE:
                AdvanceMechanic(Mechanic.TowersResolve);
                break;
            case (uint)AID.UmbralAdvance:
            case (uint)AID.AstralAdvance:
                _nextMechanic = Mechanic.Start;
                break;
        }
    }

    private void AdvanceMechanic(Mechanic next)
    {
        if (_nextMechanic < next)
            _nextMechanic = next;
    }

    private void SetState(int slot, PlayerState state)
    {
        if (slot < 0)
        {
            ReportError("Failed to assign state");
            return;
        }
        if (_playerStates[slot] != PlayerState.None)
            ReportError($"State reassignment: {_playerStates[slot]} -> {state}");
        _playerStates[slot] = state;
    }

    private void AssignTether(Actor source, int slot, bool light)
    {
        var stayLeft = source.PosRot.X > Arena.Center.X;
        var stayTop = source.PosRot.Z > Arena.Center.Z;
        SetState(slot, stayLeft ? (stayTop ? PlayerState.TetherTL : PlayerState.TetherBL) : (stayTop ? PlayerState.TetherTR : PlayerState.TetherBR));

        var lightStayLeft = stayLeft == light;
        _leftTowerMatchTether = lightStayLeft == _towersLight;
    }

    private List<List<WDir>> PositionHints(int slot)
    {
        var dirs = new List<List<WDir>>();
        if (_nextMechanic == Mechanic.Start)
        {
            return dirs;
        }

        switch (_playerStates[slot])
        {
            case PlayerState.Tower:
                // TODO: assign left/right based on prios
                dirs.Add(PositionHintsTower(true));
                dirs.Add(PositionHintsTower(false));
                break;
            case PlayerState.Plus:
                dirs.Add(PositionHintsPlusCross(true));
                break;
            case PlayerState.Cross:
                dirs.Add(PositionHintsPlusCross(false));
                break;
            case PlayerState.TetherTL:
                dirs.Add(PositionHintsTether(true, true));
                break;
            case PlayerState.TetherBL:
                dirs.Add(PositionHintsTether(false, true));
                break;
            case PlayerState.TetherTR:
                dirs.Add(PositionHintsTether(true, false));
                break;
            case PlayerState.TetherBR:
                dirs.Add(PositionHintsTether(false, false));
                break;
        }
        return dirs;
    }

    private List<WDir> PositionHintsTower(bool left)
    {
        var dirs = new List<WDir>(2);
        if (_nextMechanic <= Mechanic.FixedTowers)
        {
            dirs.Add(new(left ? -16f : 16f, (left == _topLeftSafe) ? 5f : -5f));
        }
        if (_nextMechanic <= Mechanic.TowersBait)
        {
            if (_leftTowerMatchTether == left)
            {
                dirs.Add(new(left ? -1f : 1f, (left == _topLeftSafe) ? 1f : -1f));
            }
            else
            {
                dirs.Add(new(left ? -12f : 12f, (left == _topLeftSafe) ? 5f : -5f)); // 12 is maxmelee
            }
        }
        return dirs;
    }

    private List<WDir> PositionHintsPlusCross(bool plus)
    {
        // assume plus goes top
        var left = _topLeftSafe == plus;
        var dirs = new List<WDir>(3);
        if (_nextMechanic <= Mechanic.FixedTowers)
        {
            dirs.Add(new(left ? -16f : 16f, plus ? -15f : 15f));
        }
        if (_nextMechanic <= Mechanic.CrossPlusBait)
        {
            dirs.Add(new((left ? -1f : 1f) * (plus ? 19f : 1f), plus ? -19f : 19f));
        }
        if (_nextMechanic <= Mechanic.WhiteFlameBait)
        {
            dirs.Add(new(left ? -10f : 10f, plus ? -11f : 15f));
        }
        return dirs;
    }

    private List<WDir> PositionHintsTether(bool top, bool left)
    {
        var centerZ = left == _topLeftSafe ? 5f : -5f;
        var offZ = centerZ + (top ? -4f : 4f);
        var horiz = Math.Abs(offZ) < 5f;
        var dirs = new List<WDir>(3);
        if (_nextMechanic <= Mechanic.Tethers)
        {
            dirs.Add(new((left ? -1f : 1f) * (horiz ? 10f : 8f), offZ));
        }
        var baitFlames = _leftTowerMatchTether == left;
        if (_nextMechanic <= Mechanic.WhiteFlameBait && baitFlames)
        {
            dirs.Add(new((left ? -1f : 1f) * (horiz ? 10f : 1f), offZ));
        }
        if (_nextMechanic <= Mechanic.TowersResolve && !baitFlames)
        {
            if (horiz)
            {
                dirs.Add(new(left ? -1f : 1f, offZ));
            }
            else
            {
                dirs.Add(new(left ? -12f : 12f, centerZ));
            }
        }
        return dirs;
    }
}
