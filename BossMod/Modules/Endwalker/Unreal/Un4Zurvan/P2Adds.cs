namespace BossMod.Endwalker.Unreal.Un4Zurvan;

// hard-hitting add
class P2ExecratedWill : Components.Adds
{
    public P2ExecratedWill() : base((uint)OID.ExecratedWill) { }
}

// high-priority add (casts comets and meteor)
class P2ExecratedWit : Components.Adds
{
    public P2ExecratedWit() : base((uint)OID.ExecratedWit) { }
}

// low-priority add (casts fear, then magical autos)
class P2ExecratedWile : Components.Adds
{
    public P2ExecratedWile() : base((uint)OID.ExecratedWile) { }
}

// small add
class P2ExecratedThew : Components.Adds
{
    public P2ExecratedThew() : base((uint)OID.ExecratedThew) { }
}

class P2Comet : Components.LocationTargetedAOEs
{
    public P2Comet() : base(ActionID.MakeSpell(AID.Comet), 4) { }
}

class P2MeracydianFear : Components.CastGaze
{
    public P2MeracydianFear() : base(ActionID.MakeSpell(AID.MeracydianFear)) { }
}
