namespace BossMod.Dawntrail.Trial.T03Everkeep;

// Spread on icon 376; 8 helpers instant-cast Fire III (37752) ~5s later. Forbidden radius is 7m
// (2m wider than the 5m hit) so the AI doesn't clip a neighbor while routing.
class FireIII(BossModule module) : Components.SpreadFromIcon(module, 376, AID.FireIII, 5, 5.1f);
