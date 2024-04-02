namespace BossMod.Endwalker.Quest.Endwalker;

class TidalWave : Components.KnockbackFromCastTarget
{
    private Megaflare? _megaflare;

    // TODO: Make AI function for Destination Unsafe
    public TidalWave() : base(ActionID.MakeSpell(AID.TidalWaveVisual), 25, kind: Kind.DirForward) { StopAtWall = true; }

    public override void Init(BossModule module) => _megaflare = module.FindComponent<Megaflare>();

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => _megaflare?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}
