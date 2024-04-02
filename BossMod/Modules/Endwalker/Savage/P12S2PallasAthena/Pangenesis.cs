namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

// note: this assumes standard strategy, not sure whether alternatives are possible...
// TODO: assign sides...
// TODO: show biochemical factor tethers - not sure how exactly they work...
class Pangenesis : Components.GenericTowers
{
    public enum Color { None, Light, Dark }

    public struct PlayerState
    {
        public int UnstableCount;
        public Color Color;
        public DateTime ColorExpire;
        public int AssignedSide; // -1 if left, +1 if right
        public bool SoakedPrimary; // true if last soaked tower was 'primary' (central/southern)
    }

    private PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];
    private List<Color> _towerColors = new(); // parallel to Towers
    private Color _firstLeftTower;

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.UnstableFactor:
                if (module.Raid.FindSlot(actor.InstanceID) is var slotUnstable && slotUnstable >= 0)
                {
                    _states[slotUnstable].UnstableCount = status.Extra;
                }
                break;
            case SID.UmbralTilt:
            case SID.AstralTilt:
                if (module.Raid.FindSlot(actor.InstanceID) is var slotColor && slotColor >= 0)
                {
                    _states[slotColor].Color = (SID)status.ID == SID.UmbralTilt ? Color.Light : Color.Dark;
                    _states[slotColor].ColorExpire = status.ExpireAt;

                    // update forbidden towers
                    bool isLeft = actor.Position.X < module.Bounds.Center.X;
                    for (int i = 0; i < Towers.Count; ++i)
                        if ((Towers[i].Position.X < module.Bounds.Center.X) == isLeft)  // don't care about towers on other side, keep forbidden
                            Towers.AsSpan()[i].ForbiddenSoakers[slotColor] = _states[slotColor].Color == _towerColors[i];
                }
                break;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.UmbralAdvent or AID.AstralAdvent)
        {
            bool isLight = (AID)spell.Action.ID == AID.UmbralAdvent;
            bool isLeft = caster.Position.X < module.Bounds.Center.X;
            bool isPrimary = caster.Position.Z > 90; // first tower at 91, second/third same color is 94, opposite is 88
            var towerColor = isLight ? Color.Light : Color.Dark;

            if (_firstLeftTower == Color.None)
            {
                _firstLeftTower = isLeft == isLight ? Color.Light : Color.Dark;
                for (int i = 0; i < _states.Length; ++i)
                {
                    _states[i].AssignedSide = _states[i].Color switch
                    {
                        Color.Light => _firstLeftTower == Color.Light ? +1 : -1,
                        Color.Dark => _firstLeftTower == Color.Light ? -1 : +1,
                        _ => 0 // TODO: assign sides for non-colored too (need configurable priorities)
                    };
                }
            }

            var cfg = Service.Config.Get<P12S2PallasAthenaConfig>();
            BitMask forbiddenMask = default;
            for (int i = 0; i < _states.Length; ++i)
            {
                var state = _states[i];
                bool forbidden = isLeft ? state.AssignedSide > 0 : state.AssignedSide < 0; // forbid towers on wrong side
                forbidden |= state.Color == towerColor; // forbid towers of same color
                forbidden |= NumCasts switch
                {
                    < 2 => state.UnstableCount == (cfg.PangenesisFirstStack ? 0 : 1) || state.ColorExpire > spell.NPCFinishAt.AddSeconds(4), // for first towers, forbid players with long debuffs and with improper stack count (0 for 2+1, 1 for 2+0)
                    < 6 => state.Color == Color.None && isPrimary == (cfg.PangenesisFirstStack ? state.UnstableCount == 0 : state.UnstableCount != 1), // for second towers, by default (before colors are assigned) for 2+1 strat only 0 goes to non-primary, for 2+0 strat only 1 goes to primary
                    _ => state.SoakedPrimary != isPrimary // for remaining towers, forbid changing lanes by default; note that this will be updated once colors are assigned
                };
                forbiddenMask[i] = forbidden;
            }

            Towers.Add(new(caster.Position, 3, 2, 2, forbiddenMask));
            _towerColors.Add(towerColor);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.UmbralAdvent or AID.AstralAdvent)
        {
            ++NumCasts;
            var index = Towers.FindIndex(t => t.Position.AlmostEqual(caster.Position, 1));
            if (index >= 0)
            {
                Towers.RemoveAt(index);
                _towerColors.RemoveAt(index);
            }
            // note: tower will assign new color in ~0.4s; clear previous colors immediately, since new towers will start before debuffs are gone
            foreach (var t in spell.Targets)
            {
                var slot = module.Raid.FindSlot(t.ID);
                if (slot >= 0)
                {
                    _states[slot].Color = Color.None;
                    _states[slot].AssignedSide = caster.Position.X < module.Bounds.Center.X ? -1 : 1; // ensure correct side is assigned
                    _states[slot].SoakedPrimary = caster.Position.Z > 90;
                }
            }
        }
    }
}

class FactorIn : Components.GenericBaitAway
{
    private List<(Actor source, Actor target)> _slimes = new();

    private static readonly AOEShapeCircle _shape = new(20);

    public FactorIn() : base(ActionID.MakeSpell(AID.FactorIn), centerAtTarget: true) { }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);
        foreach (var s in _slimes)
        {
            arena.Actor(s.source, ArenaColor.Object, true);
            arena.AddLine(s.source.Position, s.target.Position, ArenaColor.Danger);
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CriticalFactor)
            ForbiddenPlayers.Set(module.Raid.FindSlot(actor.InstanceID));
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.FactorIn && module.WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            _slimes.Add((source, target));
            UpdateBaits();
        }
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.FactorIn)
        {
            _slimes.RemoveAll(s => s.source == source);
            UpdateBaits();
        }
    }

    private void UpdateBaits()
    {
        CurrentBaits.Clear();
        foreach (var s in _slimes)
            if (CurrentBaits.FindIndex(b => b.Target == s.target) < 0)
                CurrentBaits.Add(new(s.source, s.target, _shape));
    }
}
