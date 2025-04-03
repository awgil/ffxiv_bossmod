namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class Wavelength(BossModule module) : BossComponent(module)
{
    enum Letter { None, A, B }
    struct PlayerState(Letter assignment, DateTime activation, int slot, BitMask players = default, int order = 0)
    {
        public Letter Assignment = assignment;
        public DateTime Activation = activation;
        public int Slot = slot;
        public BitMask Players = players;
        public int Order = order;
    }

    private PlayerState[] PlayerStates = Utils.MakeArray(8, new PlayerState());

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
            return;

        if ((SID)status.ID == SID._Gen_WavelengthA)
            PlayerStates[slot] = new(Letter.A, status.ExpireAt, slot);
        if ((SID)status.ID == SID._Gen_WavelengthB)
            PlayerStates[slot] = new(Letter.B, status.ExpireAt, slot);

        if (PlayerStates.All(p => p.Assignment != Letter.None))
        {
            var ordered = PlayerStates.Select(st => new PlayerState(st.Assignment, st.Activation, st.Slot, st.Players, PlayerStates.Count(p1 => p1.Assignment == st.Assignment && p1.Activation < st.Activation) + 1));
            var masked = ordered.Select(st =>
            {
                var mask = new BitMask();
                foreach (var o in ordered.Where(o => o.Order == st.Order))
                    mask.Set(o.Slot);
                return st with { Players = mask };
            });
            PlayerStates = [.. masked];
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID._Gen_WavelengthA or SID._Gen_WavelengthB)
            PlayerStates[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (NextOrder == 0)
            return PlayerPriority.Normal;

        var ps = PlayerStates[playerSlot];
        if (ps.Players[pcSlot])
            return PlayerPriority.Interesting;

        return ps.Order == NextOrder ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (i, player) in Raid.WithSlot())
        {
            if (!Imminent(PlayerStates[i]))
                continue;

            if (PlayerStates[i].Players[pcSlot])
            {
                if (pcSlot == i)
                {
                    // current slot is us, draw indicator if we are within 2y of any non-stacking player
                    if (PlayersNotInStack(PlayerStates[i]).Any(p => pc.Position.InCircle(p.Item2.Position, 2)))
                        Arena.AddCircle(pc.Position, 2, ArenaColor.Danger);
                }
                else
                    // current slot is partner, draw indicator around them
                    Arena.AddCircle(player.Position, 2, ArenaColor.Safe);
            }
            else
                // current slot is member of other group, draw regular AOE
                Arena.ZoneCircle(player.Position, 2, ArenaColor.AOE);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (i, player) in Raid.WithSlot())
        {
            if (!Imminent(PlayerStates[i]))
                continue;
            if (PlayerStates[i].Players[slot])
            {
                if (slot == i) // current slot is us, avoid other players
                    hints.AddForbiddenZone(ShapeDistance.Union([.. PlayersNotInStack(PlayerStates[i]).Select(p => ShapeDistance.Circle(p.Item2.Position, 2))]), PlayerStates[i].Activation);
                else // current slot is partner, put donut AOE on them so we stack
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(player.Position, 2), PlayerStates[i].Activation);
            }
            else // current slot is other player with imminent aoe
                hints.AddForbiddenZone(ShapeDistance.Circle(player.Position, 2), PlayerStates[i].Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var ps = PlayerStates[slot];
        if (ps.Order > 0)
            hints.Add($"Order: {ps.Order}", false);

        foreach (var (i, player) in Raid.WithSlot())
        {
            if (!Imminent(PlayerStates[i]))
                continue;

            if (i == slot)
            {
                hints.Add("Stack with partner!", !PlayersInStack(PlayerStates[slot]).All(p => p.Item2.Position.InCircle(actor.Position, 2)));
                if (PlayersNotInStack(PlayerStates[slot]).Any(p => p.Item2.Position.InCircle(actor.Position, 2)))
                    hints.Add("GTFO from other players!");
            }
            else if (!PlayerStates[i].Players[slot])
            {
                if (actor.Position.InCircle(player.Position, 2))
                    hints.Add("GTFO from forbidden stacks!");
            }
        }
    }

    private int NextOrder => PlayerStates.Select(p => p.Order).Where(x => x > 0).DefaultIfEmpty(0).Min();

    private IEnumerable<(int, Actor)> PlayersInStack(PlayerState ps) => Raid.WithSlot().IncludedInMask(ps.Players);
    private IEnumerable<(int, Actor)> PlayersNotInStack(PlayerState ps) => Raid.WithSlot().ExcludedFromMask(ps.Players);

    private bool Imminent(PlayerState st) => st.Order > 0 && st.Activation < WorldState.FutureTime(5);
}
