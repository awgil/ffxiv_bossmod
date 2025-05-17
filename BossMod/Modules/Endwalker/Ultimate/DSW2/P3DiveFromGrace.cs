namespace BossMod.Endwalker.Ultimate.DSW2;

class P3Geirskogul(BossModule module) : Components.StandardAOEs(module, AID.Geirskogul, new AOEShapeRect(62, 4))
{
    private readonly List<Actor> _predicted = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in _predicted)
        {
            Arena.Actor(p, ArenaColor.Object, true);
            var target = Raid.WithoutSlot().Closest(p.Position);
            if (target != null)
                Shape.Outline(Arena, p.Position, Angle.FromDirection(target.Position - p.Position));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            _predicted.Clear();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if ((AID)spell.Action.ID == AID.DarkdragonDive)
            _predicted.Add(caster);
    }
}

class P3GnashAndLash(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _aoeGnash = new(8);
    private static readonly AOEShapeDonut _aoeLash = new(8, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        (AOEShape? first, AOEShape? second) = (AID)spell.Action.ID switch
        {
            AID.GnashAndLash => (_aoeGnash, _aoeLash),
            AID.LashAndGnash => (_aoeLash, _aoeGnash),
            _ => ((AOEShape?)null, (AOEShape?)null)
        };
        if (first != null && second != null)
        {
            _aoes.Clear(); // just a precaution, in one pull i had unfortunate cast time updates which 'restarted' the spell several times
            // note: marking aoes as non-risky, so that we don't spam warnings - reconsider (maybe mark as risky when cast ends?)
            _aoes.Add(new(first, caster.Position, default, WorldState.FutureTime(3.7f), Risky: false));
            _aoes.Add(new(second, caster.Position, default, WorldState.FutureTime(6.8f), Risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GnashingWheel or AID.LashingWheel)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

// currently we make some arbitrary decisions:
// 1. raid stacks to the north
// 2. if there are forward/backward jumps at given order, forward takes W spot, backward takes E spot (center takes S) - this can be changed by config
// 3. otherwise, no specific assignments are assumed until player baits or soaks the tower
// TODO: split into towers & bait-away?
class P3DiveFromGrace(BossModule module) : Components.CastTowers(module, AID.DarkdragonDive, 5)
{
    private struct PlayerState
    {
        public int JumpOrder; // 0 if unassigned, otherwise [1,3]
        public int JumpDirection; // -1 for backward, +1 for forward
        public int AssignedSpot; // 0 if unassigned, 1 for 'backward' spot, 2 for 'center' spot, 3 for 'forward' spot
        //public string Hint = "";
        //public WPos SafeSpot;
        //public bool IsBaitingJump;

        public readonly bool CanBait(int order, int spot) => JumpOrder == order && (AssignedSpot == 0 || AssignedSpot == spot);
    }

    public int NumJumps { get; private set; }
    private readonly DSW2Config _config = Service.Config.Get<DSW2Config>();
    private bool _haveDirections;
    private readonly PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
    private readonly BitMask[] _orderPlayers = new BitMask[3]; // [0] = players with order 1, etc.
    private BitMask _ordersWithArrows; // bit 1 = order 1, etc.
    private readonly List<Tower> _predictedTowers = [];

    private const float _towerOffset = 14;
    private const float _spotOffset = 7f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var state = _playerStates[slot];
        if (state.JumpOrder > 0)
            hints.Add($"Order: {state.JumpOrder}", false);
        if (_haveDirections)
            hints.Add($"Spot: {state.AssignedSpot switch
            {
                1 => _config.P3DiveFromGraceLookWest ? "W" : "E",
                2 => "S",
                3 => _config.P3DiveFromGraceLookWest ? "E" : "W",
                _ => "flex"
            }}", false);

        base.AddHints(slot, actor, hints);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var s in SafeSpots(slot))
            movementHints.Add(actor.Position, s, ArenaColor.Safe);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_haveDirections)
            hints.Add($"Arrows for: {(_ordersWithArrows.Any() ? string.Join(", ", _ordersWithArrows.SetBits()) : "none")}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // TODO: forbidden directions
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _playerStates[playerSlot].JumpOrder == CurrentBaitOrder() ? PlayerPriority.Interesting : PlayerPriority.Normal;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var t in _predictedTowers)
            DrawTower(Arena, t.Position, t.Radius, !t.ForbiddenSoakers[pcSlot]);

        // draw baited jumps
        var baitOrder = CurrentBaitOrder();
        foreach (var (slot, player) in Raid.WithSlot(true).WhereSlot(i => _playerStates[i].JumpOrder == baitOrder))
        {
            var pos = player.Position + _playerStates[slot].JumpDirection * player.Rotation.ToDirection() * _towerOffset;
            Arena.AddCircle(pos, Radius, ArenaColor.Object);
            if (slot == pcSlot)
                Arena.AddLine(pc.Position, pos, ArenaColor.Object);
        }

        // safe spots
        foreach (var s in SafeSpots(pcSlot))
            Arena.AddCircle(s, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Jump1:
                AssignJumpOrder(actor, 1);
                break;
            case SID.Jump2:
                AssignJumpOrder(actor, 2);
                break;
            case SID.Jump3:
                AssignJumpOrder(actor, 3);
                break;
            case SID.JumpBackward:
                AssignJumpDirection(actor, -1);
                break;
            case SID.JumpCenter:
                AssignJumpDirection(actor, 0);
                break;
            case SID.JumpForward:
                AssignJumpDirection(actor, +1);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _predictedTowers.Clear();
            Towers.Add(CreateTower(caster.Position));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DarkdragonDive:
                foreach (var t in spell.Targets)
                    AssignLateSpot(t.ID, caster.Position);
                ++NumCasts;
                break;
            case AID.DarkHighJump:
            case AID.DarkSpineshatterDive:
            case AID.DarkElusiveJump:
                ++NumJumps;
                AssignLateSpot(spell.MainTargetID, caster.Position);
                var offset = (AID)spell.Action.ID != AID.DarkHighJump ? _towerOffset * caster.Rotation.ToDirection() : new();
                _predictedTowers.Add(CreateTower(caster.Position + offset));
                break;
        }
    }

    private void AssignJumpOrder(Actor actor, int order)
    {
        if (Raid.TryFindSlot(actor, out var slot))
        {
            _playerStates[slot].JumpOrder = order;
            _orderPlayers[order - 1].Set(slot);
        }
    }

    private void AssignJumpDirection(Actor actor, int direction)
    {
        _haveDirections = true;
        if (Raid.TryFindSlot(actor, out var slot))
        {
            _playerStates[slot].JumpDirection = direction;
            if (direction != 0 && _playerStates[slot].JumpOrder is var order && order > 0)
            {
                _ordersWithArrows.Set(order);
                foreach (var p in _orderPlayers[order - 1].SetBits())
                {
                    _playerStates[p].AssignedSpot = _playerStates[p].JumpDirection + 2;
                }
            }
        }
    }

    private void AssignLateSpot(ulong target, WPos pos)
    {
        if (Raid.TryFindSlot(target, out var slot) && _playerStates[slot].AssignedSpot == 0)
            _playerStates[slot].AssignedSpot = TowerSpot(pos);
    }

    private int CurrentBaitOrder() => _haveDirections ? NumJumps switch
    {
        < 3 => 1,
        < 5 => 2,
        < 8 => 3,
        _ => -1
    } : -1;

    private int TowerSpot(WPos pos)
    {
        var towerOffset = pos - Module.Center;
        var toStack = DirectionForStack();
        var dotForward = DirectionForForwardArrow().Dot(towerOffset);
        return -toStack.Dot(towerOffset) > Math.Abs(dotForward) ? 2 : dotForward > 0 ? 3 : 1;
    }

    private Tower CreateTower(WPos pos)
    {
        var spot = TowerSpot(pos);
        var soakerOrder = NumCasts switch
        {
            < 3 => 3,
            < 5 => 1,
            < 8 => spot != 2 ? 2 : 1,
            _ => -1
        };
        var forbidden = Raid.WithSlot(true).WhereSlot(i => !_playerStates[i].CanBait(soakerOrder, spot)).Mask();
        return new(pos, Radius, forbiddenSoakers: forbidden);
    }

    private WDir DirectionForStack() => new(0, -_spotOffset); // TODO: this is arbitrary
    private WDir DirectionForForwardArrow() => _config.P3DiveFromGraceLookWest ? DirectionForStack().OrthoR() : DirectionForStack().OrthoL();

    private IEnumerable<WPos> SafeSpots(int slot)
    {
        if (!_haveDirections)
            yield break;

        // show safespot hints only if there are no towers to soak (TODO: or geirskoguls to bait?..)
        var state = _playerStates[slot];
        if (state.JumpOrder == CurrentBaitOrder())
        {
            var origin = Module.Center;
            if (state.JumpOrder == 2)
                origin += DirectionForStack() * 0.8f; // TODO: the coefficient is arbitrary

            if (state.AssignedSpot is 0 or 1)
                yield return origin - DirectionForForwardArrow();
            if (state.AssignedSpot is 0 or 2 && state.JumpOrder != 2)
                yield return origin - DirectionForStack();
            if (state.AssignedSpot is 0 or 3)
                yield return origin + DirectionForForwardArrow();
        }
        else if (NumJumps < (state.JumpOrder == 3 ? 3 : 8))
        {
            yield return Module.Center + DirectionForStack();
        }
    }
}
