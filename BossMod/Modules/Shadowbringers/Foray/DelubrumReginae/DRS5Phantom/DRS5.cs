namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

class MaledictionOfAgony : Components.CastCounter
{
    public MaledictionOfAgony() : base(ActionID.MakeSpell(AID.MaledictionOfAgonyAOE)) { }
}

class BloodyWraith : Components.Adds
{
    public BloodyWraith() : base((uint)OID.BloodyWraith) { }
}

class MistyWraith : Components.Adds
{
    public MistyWraith() : base((uint)OID.MistyWraith) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9755)]
public class DRS5 : BossModule
{
    public DRS5(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(202, -370), 24)) { }
}
