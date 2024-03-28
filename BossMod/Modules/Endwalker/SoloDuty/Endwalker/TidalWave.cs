using System.Linq;

namespace BossMod.Modules.Endwalker.SoloDuty.Endwalker;
class TidalWave : Components.KnockbackFromCastTarget
{
    // TODO: Make AI function for Destination Unsafe
    public TidalWave() : base(ActionID.MakeSpell(AID.TidalWaveVisual), 25, kind: Kind.DirForward) { StopAtWall = true; }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos)
    {
        if (module.FindComponent<Megaflare>() != null && module.FindComponent<Megaflare>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
            return true;
        else
            return false;
    }
}
