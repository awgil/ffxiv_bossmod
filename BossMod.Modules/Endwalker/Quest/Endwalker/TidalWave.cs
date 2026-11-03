namespace BossMod.Endwalker.Quest.Endwalker;

// TODO: Make AI function for Destination Unsafe
class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWaveVisual, 25, kind: Kind.DirForward, stopAtWall: true)
{
    private Megaflare? Megaflare => Module.FindComponent<Megaflare>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Megaflare?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}
