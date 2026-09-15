namespace BossMod.Endwalker.Unreal.Un4Zurvan;

class P2ExecratedWill(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWill); // hard-hitting add
class P2ExecratedWit(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWit); // high-priority add (casts comets and meteor)
class P2ExecratedWile(BossModule module) : Components.Adds(module, (uint)OID.ExecratedWile); // low-priority add (casts fear, then magical autos)
class P2ExecratedThew(BossModule module) : Components.Adds(module, (uint)OID.ExecratedThew); // small add

class P2Comet(BossModule module) : Components.StandardAOEs(module, AID.Comet, 4);
class P2MeracydianFear(BossModule module) : Components.CastGaze(module, AID.MeracydianFear);
