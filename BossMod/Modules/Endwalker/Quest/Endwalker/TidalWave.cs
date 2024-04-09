namespace BossMod.Endwalker.Quest.Endwalker;

class TidalWave : Components.KnockbackFromCastTarget
{
    private Megaflare? _megaflare;

    // TODO: Make AI function for Destination Unsafe
    public TidalWave(BossModule module) : base(module, ActionID.MakeSpell(AID.TidalWaveVisual), 25, kind: Kind.DirForward)
    {
        StopAtWall = true;
        _megaflare = module.FindComponent<Megaflare>();
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _megaflare?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}
