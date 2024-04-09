namespace BossMod.Endwalker.Unreal.Un4Zurvan;

// hard-hitting add
class P2ExecratedWill(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWill);

// high-priority add (casts comets and meteor)
class P2ExecratedWit(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWit);

// low-priority add (casts fear, then magical autos)
class P2ExecratedWile(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWile);

// small add
class P2ExecratedThew(BossModule module) : Components.Adds(module, (uint)OID.ExecratedThew);

class P2Comet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Comet), 4);

class P2MeracydianFear(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.MeracydianFear));
