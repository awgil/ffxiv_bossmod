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

    private const float _farOffset = 13f;
    private const float _nearOffset = 7f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var state = _states[slot];
        if (state.Tether != TetherType.None)
            hints.Add($"{state.Tether} tether", state.TetherBad);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (Safespot(slot, actor) is var safespot && safespot != null)
            movementHints.Add(actor.Position, safespot.Value, Colors.Safe);
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
            Arena.AddLine(pc.Position, partner.Position, pcState.TetherBad ? default : Colors.Safe);
        if (Safespot(pcSlot, pc) is var safespot && safespot != null)
            Arena.ZoneCircleOutline(safespot.Value, 1f, Colors.Safe);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        switch (tether.ID)
        {
            case (uint)TetherID.LightLightGood:
            case (uint)TetherID.DarkDarkGood:
                UpdateTether(Raid.FindSlot(source.InstanceID), Raid.FindSlot(tether.Target), TetherType.Far, false);
                break;
            case (uint)TetherID.LightLightBad:
            case (uint)TetherID.DarkDarkBad:
                UpdateTether(Raid.FindSlot(source.InstanceID), Raid.FindSlot(tether.Target), TetherType.Far, true);
                break;
            case (uint)TetherID.DarkLightGood:
                UpdateTether(Raid.FindSlot(source.InstanceID), Raid.FindSlot(tether.Target), TetherType.Near, false);
                break;
            case (uint)TetherID.DarkLightBad:
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

        var isFar = tether == TetherType.Far;
        var dir = actor.Role switch
        {
            Role.Tank => isFar ? 180f.Degrees() : -90f.Degrees(),
            Role.Healer => isFar ? default : 90f.Degrees(),
            _ => Raid[_states[slot].PartnerSlot]?.Role == Role.Tank ? (isFar ? -45f.Degrees() : -135f.Degrees()) : (isFar ? 135f.Degrees() : 45f.Degrees())
        };
        return Arena.Center + (isFar ? _farOffset : _nearOffset) * dir.ToDirection();
    }
}
