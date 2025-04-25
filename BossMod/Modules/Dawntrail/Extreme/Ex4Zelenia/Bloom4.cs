namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class Bloom4Emblazon(BossModule module) : Components.CastCounter(module, AID.Emblazon)
{
    public BitMask Baiters;
    private DateTime Activation;
    private readonly Tiles Tiles = module.FindComponent<Tiles>()!;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Rose)
        {
            Baiters.Set(Raid.FindSlot(actor.InstanceID));
            Activation = WorldState.FutureTime(6.7f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.Emblazon)
        {
            Baiters.Clear(Raid.FindSlot(spell.MainTargetID));
            if (!Baiters.Any())
                Activation = default;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (slot, player) in Raid.WithSlot().IncludedInMask(Baiters))
        {
            var tile = Tiles.GetTile(player);
            if (slot == pcSlot)
                Tiles.DrawTile(Arena, tile, ArenaColor.Danger);
            else
                Tiles.ZoneTile(Arena, tile, ArenaColor.AOE);
        }

        Tiles.ZoneTiles(Arena, Tiles.Mask, ArenaColor.AOE);
    }
}
