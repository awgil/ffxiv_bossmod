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

    public int NumAssigned;
    public int NumCasts;
    private readonly PlayerState[] _playerStates = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { Partner = -1 });
    private readonly List<Actor> _fireTowers = module.Enemies((uint)OID.FireTower);
    private readonly List<Actor> _iceTowers = module.Enemies((uint)OID.IceTower);

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
            Arena.AddLine(pc.Position, partner.Position, state.Color == Color.Fire ? Colors.Object : Colors.Other8, state.TooFar ? 2 : 1);
        }

        for (var i = 0; i < _fireTowers.Count; ++i)
            Arena.ZoneCircleOutline(_fireTowers[i].Position, 2, state.Color == Color.Fire ? Colors.Safe : default);
        for (var i = 0; i < _iceTowers.Count; ++i)
            Arena.ZoneCircleOutline(_iceTowers[i].Position, 2, state.Color == Color.Ice ? Colors.Safe : default);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.InfiniteFire:
                AssignColor(spell.MainTargetID, Color.Fire);
                break;
            case (uint)AID.InfiniteIce:
                AssignColor(spell.MainTargetID, Color.Ice);
                break;
            case (uint)AID.SouthStar:
            case (uint)AID.NorthStar:
            case (uint)AID.SouthStarUnsoaked:
            case (uint)AID.NorthStarUnsoaked:
            case (uint)AID.SouthStarWrong:
            case (uint)AID.NorthStarWrong:
                ++NumCasts;
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.InfiniteAnguish or (uint)TetherID.InfiniteFire or (uint)TetherID.InfiniteIce)
        {
            var from = Raid.FindSlot(source.InstanceID);
            var to = Raid.FindSlot(tether.Target);
            if (from >= 0 && to >= 0)
            {
                _playerStates[from].Partner = to;
                _playerStates[to].Partner = from;
                _playerStates[from].TooFar = _playerStates[to].TooFar = tether.ID == (uint)TetherID.InfiniteAnguish;
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
