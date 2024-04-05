namespace BossMod.Endwalker.Savage.P9SKokytos;

class GluttonysAugur : Components.CastCounter
{
    public GluttonysAugur() : base(ActionID.MakeSpell(AID.GluttonysAugurAOE)) { }
}

class SoulSurge : Components.CastCounter
{
    public SoulSurge() : base(ActionID.MakeSpell(AID.SoulSurge)) { }
}

class BeastlyFury : Components.CastCounter
{
    public BeastlyFury() : base(ActionID.MakeSpell(AID.BeastlyFuryAOE)) { }
}

[ConfigDisplay(Order = 0x190, Parent = typeof(EndwalkerConfig))]
public class P9SKokytosConfig : CooldownPlanningConfigNode
{
    public P9SKokytosConfig() : base(90) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 937, NameID = 12369)]
public class P9SKokytos : BossModule
{
    public P9SKokytos(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
}
