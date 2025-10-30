namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class FlamespireClaw(BossModule module) : Components.GenericBaitAway(module, AID.FlamespireClawAOE)
{
    private readonly int[] _order = new int[PartyState.MaxPartySize];
    private BitMask _tethers;

    private static readonly AOEShapeCone _shape = new(20, 45.Degrees()); // TODO: verify angle

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var order = _order[slot];
        if (order != 0 && NumCasts < 8)
        {
            hints.Add($"Order: {order}", false);
            bool shouldBeTethered = order switch
            {
                1 => NumCasts is 1 or 2,
                2 => NumCasts is 2 or 3,
                3 => NumCasts is 3 or 4,
                4 => NumCasts is 4 or 5,
                5 => NumCasts is 5 or 6,
                6 => NumCasts is 6 or 7,
                7 => NumCasts is 7 or 0,
                _ => NumCasts is 0 or 1,
            };
            if (shouldBeTethered != _tethers[slot])
                hints.Add(shouldBeTethered ? "Intercept tether!" : "Pass the tether!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var (_, player) in Raid.WithSlot(true).IncludedInMask(_tethers))
            Arena.AddLine(player.Position, Module.PrimaryActor.Position, ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
        {
            CurrentBaits.Clear();
            var nextSlot = Array.IndexOf(_order, NumCasts + 1);
            var nextTarget = nextSlot >= 0 ? Raid[nextSlot] : null;
            if (nextTarget != null)
                CurrentBaits.Add(new(Module.PrimaryActor, nextTarget, _shape));
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.FlamespireClaw)
            _tethers.Set(Raid.FindSlot(source.InstanceID));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.FlamespireClaw)
            _tethers.Clear(Raid.FindSlot(source.InstanceID));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.FlamespireClaw1 and <= (uint)IconID.FlamespireClaw8)
        {
            var order = (int)iconID - (int)IconID.FlamespireClaw1 + 1;
            if (Raid.TryFindSlot(actor, out var slot))
                _order[slot] = order;
            if (order == 1)
                CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
        }
    }
}
