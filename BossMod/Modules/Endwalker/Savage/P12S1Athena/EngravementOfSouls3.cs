namespace BossMod.Endwalker.Savage.P12S1Athena;

class EngravementOfSouls3Shock(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.Shock), 3)
{
    private BitMask _towers;
    private BitMask _plus;
    private BitMask _cross;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.UmbralbrightSoul:
            case SID.AstralbrightSoul:
                _towers.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.QuarteredSoul:
                _plus.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.XMarkedSoul:
                _cross.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            BitMask forbidden = spell.Location.Z switch
            {
                < 90 => ~_plus, // TODO: technically cross and plus could switch places
                > 110 => ~_cross,
                _ => ~_towers // TODO: assign specific towers based on priorities?
            };
            Towers.Add(new(spell.LocXZ, Radius, forbiddenSoakers: forbidden));
        }
    }
}

class EngravementOfSouls3Spread(BossModule module) : Components.UniformStackSpread(module, 0, 3, alwaysShowSpreads: true, raidwideOnResolve: false)
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

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var soakers = (SID)status.ID switch
        {
            SID.UmbralbrightSoul => EngravementOfSoulsTethers.TetherType.Dark,
            SID.AstralbrightSoul => EngravementOfSoulsTethers.TetherType.Light,
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
        if ((AID)spell.Action.ID is AID.UmbralGlow or AID.AstralGlow)
            Spreads.Clear();
    }
}

class TheosCross(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheosCross), new AOEShapeCross(40, 3));
class TheosSaltire(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheosSaltire), new AOEShapeCross(40, 3));

// TODO: this assumes standard strats, there could be variations i guess...
class EngravementOfSouls3Hints(BossModule module) : BossComponent(module)
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
        foreach (var chain in PositionHints(slot))
        {
            var from = actor.Position;
            var color = ArenaColor.Safe;
            foreach (var offset in chain)
            {
                var to = Module.Center + offset;
                movementHints.Add(from, to, color);
                from = to;
                color = ArenaColor.Danger;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var chain in PositionHints(pcSlot))
            foreach (var offset in chain.Take(1))
                Arena.AddCircle(Module.Center + offset, 1, ArenaColor.Safe);
    }

    // note: these statuses are assigned before any tethers
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.UmbralbrightSoul:
                _towersLight = true;
                SetState(Raid.FindSlot(actor.InstanceID), PlayerState.Tower);
                break;
            case SID.AstralbrightSoul:
                _towersLight = false;
                SetState(Raid.FindSlot(actor.InstanceID), PlayerState.Tower);
                break;
            case SID.QuarteredSoul:
                SetState(Raid.FindSlot(actor.InstanceID), PlayerState.Plus);
                break;
            case SID.XMarkedSoul:
                SetState(Raid.FindSlot(actor.InstanceID), PlayerState.Cross);
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch ((TetherID)tether.ID)
        {
            case TetherID.LightNear:
            case TetherID.LightFar:
                AssignTether(source, Raid.FindSlot(tether.Target), true);
                break;
            case TetherID.DarkNear:
            case TetherID.DarkFar:
                AssignTether(source, Raid.FindSlot(tether.Target), false);
                break;
            case TetherID.UnnaturalEnchainment:
                if (source.Position.Z < 90)
                {
                    _topLeftSafe = source.Position.X > Module.Center.X;
                    AdvanceMechanic(Mechanic.FixedTowers);
                }
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheosCross or AID.TheosSaltire)
            AdvanceMechanic(Mechanic.TowersBait);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Shock:
                AdvanceMechanic(Mechanic.Tethers);
                break;
            case AID.SearingRadiance:
            case AID.Shadowsear:
                AdvanceMechanic(Mechanic.CrossPlusBait);
                break;
            case AID.UmbralGlow:
            case AID.AstralGlow:
                AdvanceMechanic(Mechanic.WhiteFlameBait);
                break;
            case AID.WhiteFlameAOE:
                AdvanceMechanic(Mechanic.TowersResolve);
                break;
            case AID.UmbralAdvance:
            case AID.AstralAdvance:
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
        bool stayLeft = source.Position.X > Module.Center.X;
        bool stayTop = source.Position.Z > Module.Center.Z;
        SetState(slot, stayLeft ? (stayTop ? PlayerState.TetherTL : PlayerState.TetherBL) : (stayTop ? PlayerState.TetherTR : PlayerState.TetherBR));

        bool lightStayLeft = stayLeft == light;
        _leftTowerMatchTether = lightStayLeft == _towersLight;
    }

    private IEnumerable<IEnumerable<WDir>> PositionHints(int slot)
    {
        if (_nextMechanic == Mechanic.Start)
            yield break;

        switch (_playerStates[slot])
        {
            case PlayerState.Tower:
                // TODO: assign left/right based on prios
                yield return PositionHintsTower(true);
                yield return PositionHintsTower(false);
                break;
            case PlayerState.Plus:
                yield return PositionHintsPlusCross(true);
                break;
            case PlayerState.Cross:
                yield return PositionHintsPlusCross(false);
                break;
            case PlayerState.TetherTL:
                yield return PositionHintsTether(true, true);
                break;
            case PlayerState.TetherBL:
                yield return PositionHintsTether(false, true);
                break;
            case PlayerState.TetherTR:
                yield return PositionHintsTether(true, false);
                break;
            case PlayerState.TetherBR:
                yield return PositionHintsTether(false, false);
                break;
        }
    }

    private IEnumerable<WDir> PositionHintsTower(bool left)
    {
        if (_nextMechanic <= Mechanic.FixedTowers)
        {
            yield return new(left ? -16 : 16, (left == _topLeftSafe) ? 5 : -5);
        }
        if (_nextMechanic <= Mechanic.TowersBait)
        {
            if (_leftTowerMatchTether == left)
                yield return new(left ? -1 : 1, (left == _topLeftSafe) ? 1 : -1);
            else
                yield return new(left ? -12 : 12, (left == _topLeftSafe) ? 5 : -5); // 12 is maxmelee
        }
    }

    private IEnumerable<WDir> PositionHintsPlusCross(bool plus)
    {
        // assume plus goes top
        var left = _topLeftSafe == plus;
        if (_nextMechanic <= Mechanic.FixedTowers)
        {
            yield return new(left ? -16 : 16, plus ? -15 : 15);
        }
        if (_nextMechanic <= Mechanic.CrossPlusBait)
        {
            yield return new((left ? -1 : 1) * (plus ? 19 : 1), plus ? -19 : 19);
        }
        if (_nextMechanic <= Mechanic.WhiteFlameBait)
        {
            yield return new(left ? -10 : 10, plus ? -11 : 15);
        }
    }

    private IEnumerable<WDir> PositionHintsTether(bool top, bool left)
    {
        var centerZ = left == _topLeftSafe ? 5 : -5;
        var offZ = centerZ + (top ? -4 : 4);
        var horiz = Math.Abs(offZ) < 5;
        if (_nextMechanic <= Mechanic.Tethers)
        {
            yield return new((left ? -1 : 1) * (horiz ? 10 : 8), offZ);
        }
        bool baitFlames = _leftTowerMatchTether == left;
        if (_nextMechanic <= Mechanic.WhiteFlameBait && baitFlames)
        {
            yield return new((left ? -1 : 1) * (horiz ? 10 : 1), offZ);
        }
        if (_nextMechanic <= Mechanic.TowersResolve && !baitFlames)
        {
            if (horiz)
                yield return new(left ? -1 : 1, offZ);
            else
                yield return new(left ? -12 : 12, centerZ);
        }
    }
}
