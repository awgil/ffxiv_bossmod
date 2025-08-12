namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class MacabreMark(BossModule module) : Components.GenericTowers(module, AID.MacabreMark)
{
    private static readonly (WPos Center, int Number)[] AllTowers = [
        (new(91, 88), 3),
        (new(109, 88), 3),
        (new(100, 94), 4),
        (new(85, 97), 2),
        (new(115, 97), 2),
        (new(85, 103), 2),
        (new(115, 103), 2),
        (new(100, 106), 4),
        (new(91, 112), 3),
        (new(109, 112), 3)
    ];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 0x1A and <= 0x23 && state == 0x00020001)
        {
            var t = AllTowers[index - 0x1A];
            Towers.Add(new(t.Center, 3, t.Number, t.Number, activation: WorldState.FutureTime(30)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(spell.TargetXZ, 1));

            var targetsMask = BitMask.Build([.. spell.Targets.Select(t => Raid.FindSlot(t.ID))]);

            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers |= targetsMask;
        }

        if ((AID)spell.Action.ID == AID.SpreadingFearTower)
            Towers.Clear();
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp && Raid.TryFindSlot(actor, out var slot))
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp && Raid.TryFindSlot(actor, out var slot))
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers.Clear(slot);
    }
}
