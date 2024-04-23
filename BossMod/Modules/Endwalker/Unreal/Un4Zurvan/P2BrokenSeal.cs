namespace BossMod.Endwalker.Unreal.Un4Zurvan;

class P2BrokenSeal(BossModule module) : BossComponent(module)
{
    public enum Color { None, Fire, Ice }

    public struct PlayerState
    {
        public Color Color;
        public int Partner;
        public bool TooFar;
    }

    public int NumAssigned { get; private set; }
    public int NumCasts { get; private set; }
    private readonly PlayerState[] _playerStates = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { Partner = -1 });
    private readonly IReadOnlyList<Actor> _fireTowers = module.Enemies(OID.FireTower);
    private readonly IReadOnlyList<Actor> _iceTowers = module.Enemies(OID.IceTower);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts > 0)
            return;

        if (_playerStates[slot].TooFar)
            hints.Add("Move closer to partner!");

        var towers = _playerStates[slot].Color switch
        {
            Color.Fire => _fireTowers,
            Color.Ice => _iceTowers,
            _ => null
        };
        if (towers?.Count > 0 && !towers.Any(t => actor.Position.InCircle(t.Position, 2)))
            hints.Add("Soak the tower!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _playerStates[pcSlot].Color != Color.None && _playerStates[pcSlot].Partner == playerSlot ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts > 0)
            return;

        var state = _playerStates[pcSlot];
        var partner = state.Color != Color.None && state.Partner >= 0 ? Raid[state.Partner] : null;
        if (partner != null)
        {
            Arena.AddLine(pc.Position, partner.Position, state.Color == Color.Fire ? 0xff0080ff : 0xffff8000, state.TooFar ? 2 : 1);
        }

        foreach (var t in _fireTowers)
            Arena.AddCircle(t.Position, 2, state.Color == Color.Fire ? ArenaColor.Safe : ArenaColor.Danger);
        foreach (var t in _iceTowers)
            Arena.AddCircle(t.Position, 2, state.Color == Color.Ice ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.InfiniteFire:
                AssignColor(spell.MainTargetID, Color.Fire);
                break;
            case AID.InfiniteIce:
                AssignColor(spell.MainTargetID, Color.Ice);
                break;
            case AID.SouthStar:
            case AID.NorthStar:
            case AID.SouthStarUnsoaked:
            case AID.NorthStarUnsoaked:
            case AID.SouthStarWrong:
            case AID.NorthStarWrong:
                ++NumCasts;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.InfiniteAnguish or TetherID.InfiniteFire or TetherID.InfiniteIce)
        {
            var from = Raid.FindSlot(source.InstanceID);
            var to = Raid.FindSlot(tether.Target);
            if (from >= 0 && to >= 0)
            {
                _playerStates[from].Partner = to;
                _playerStates[to].Partner = from;
                _playerStates[from].TooFar = _playerStates[to].TooFar = (TetherID)tether.ID == TetherID.InfiniteAnguish;
            }
        }
    }

    private void AssignColor(ulong playerID, Color color)
    {
        ++NumAssigned;
        var slot = Raid.FindSlot(playerID);
        if (slot >= 0)
            _playerStates[slot].Color = color;
    }
}
