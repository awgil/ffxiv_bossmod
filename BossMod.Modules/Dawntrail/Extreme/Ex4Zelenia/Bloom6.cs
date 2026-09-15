namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class Bloom6Emblazon(BossModule module) : Emblazon(module)
{
    public static readonly BitMask BadTiles = BitMask.Build(1, 2, 5, 6, 9, 10, 13, 14);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var tile = Tiles.GetTile(actor);

        if (Baiters[slot] && !_tiles[tile] && BadTiles[tile])
            hints.Add("Don't connect towers!");
    }
}

class HolyHazard(BossModule module) : Components.StandardAOEs(module, AID.HolyHazard, new AOEShapeCone(24, 60.Degrees()), maxCasts: 2)
{
    private readonly Ex4ZeleniaConfig _config = Service.Config.Get<Ex4ZeleniaConfig>();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _config.ShowHolyHazard ? base.ActiveAOEs(slot, actor) : [];
}
