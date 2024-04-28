namespace BossMod.Endwalker.Savage.P11SThemis;

class DarkAndLight(BossModule module) : BossComponent(module)
{
    public enum TetherType { None, Near, Far }

    public struct PlayerState
    {
        public TetherType Tether;
        public int PartnerSlot;
        public bool TetherBad;
    }

    public bool ShowSafespots = true;
    private readonly PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];

    private const float _farOffset = 13;
    private const float _nearOffset = 7;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var state = _states[slot];
        if (state.Tether != TetherType.None)
            hints.Add($"{state.Tether} tether", state.TetherBad);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (Safespot(slot, actor) is var safespot && safespot != null)
            movementHints.Add(actor.Position, safespot.Value, ArenaColor.Safe);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var pcState = _states[pcSlot];
        return pcState.Tether != TetherType.None && pcState.PartnerSlot == playerSlot ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pcState = _states[pcSlot];
        if (pcState.Tether != TetherType.None && Raid[pcState.PartnerSlot] is var partner && partner != null)
            Arena.AddLine(pc.Position, partner.Position, pcState.TetherBad ? ArenaColor.Danger : ArenaColor.Safe);
        if (Safespot(pcSlot, pc) is var safespot && safespot != null)
            Arena.AddCircle(safespot.Value, 1, ArenaColor.Safe);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch ((TetherID)tether.ID)
        {
            case TetherID.LightLightGood:
            case TetherID.DarkDarkGood:
                UpdateTether(Raid.FindSlot(source.InstanceID), Raid.FindSlot(tether.Target), TetherType.Far, false);
                break;
            case TetherID.LightLightBad:
            case TetherID.DarkDarkBad:
                UpdateTether(Raid.FindSlot(source.InstanceID), Raid.FindSlot(tether.Target), TetherType.Far, true);
                break;
            case TetherID.DarkLightGood:
                UpdateTether(Raid.FindSlot(source.InstanceID), Raid.FindSlot(tether.Target), TetherType.Near, false);
                break;
            case TetherID.DarkLightBad:
                UpdateTether(Raid.FindSlot(source.InstanceID), Raid.FindSlot(tether.Target), TetherType.Near, true);
                break;
        }
    }

    private void UpdateTether(int from, int to, TetherType type, bool bad)
    {
        if (from < 0 || to < 0)
            return;
        _states[from] = new() { Tether = type, PartnerSlot = to, TetherBad = bad };
        _states[to] = new() { Tether = type, PartnerSlot = from, TetherBad = bad };
    }

    // note: this uses default strats (kindred etc)
    private WPos? Safespot(int slot, Actor actor)
    {
        var tether = _states[slot].Tether;
        if (!ShowSafespots || tether == TetherType.None)
            return null;

        bool isFar = tether == TetherType.Far;
        Angle dir = actor.Role switch
        {
            Role.Tank => isFar ? 180.Degrees() : -90.Degrees(),
            Role.Healer => isFar ? 0.Degrees() : 90.Degrees(),
            _ => Raid[_states[slot].PartnerSlot]?.Role == Role.Tank ? (isFar ? -45.Degrees() : -135.Degrees()) : (isFar ? 135.Degrees() : 45.Degrees())
        };
        return Module.Center + (isFar ? _farOffset : _nearOffset) * dir.ToDirection();
    }
}
