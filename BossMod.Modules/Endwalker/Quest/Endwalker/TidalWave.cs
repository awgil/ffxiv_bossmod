namespace BossMod.Endwalker.Quest.Endwalker;

// TODO: Make AI function for Destination Unsafe
class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWaveVisual, 25, kind: Kind.DirForward, stopAtWall: true);
