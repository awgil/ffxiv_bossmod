namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

// envcontrols:
// 1F-2E = 1-man towers
// - 00020001 - appear
// - 00200010 - occupied
// - 00080004 - disappear
// - 08000001 - ? (spot animation)
// - arrangement:
//      25             26
//   21 xx 1F xx xx 20 xx 22
//      23             24
//      xx             xx
//      xx             xx
//      2B             2C
//   29 xx 27 xx xx 28 xx 2A
//      2D             2E
// 2F-3E = 2-man towers
// - 00020001 - appear
// - 00200010 - occupied by 1
// - 00800040 - occupied by 2
// - 00080004 - disappear
// - 08000001 - ? (spot animations)
// - arrangement (also covers intersecting square):
//      35             36
//   31 xx 2F xx xx 30 xx 32
//      33             34
//      xx             xx
//      xx             xx
//      3B             3C
//   39 xx 37 xx xx 38 xx 3A
//      3D             3E
// 3F-46 = 3-man towers
// - 00020001 - appear
// - 00200010 - occupied by 1
// - 00800040 - occupied by 2
// - 02000100 - occupied by 3
// - 00080004 - disappear
// - 08000001 - ? (spot animations)
// - arrangement:
//     3F         43
//   42  40     44  46
//     41         45
// 47-56 = 1-man tower falling orb
// 57-66 = 2-man tower falling orb
// 67-6E = 3-man tower falling orb
class ParticleConcentration(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.ParticleBeam1)) { }
