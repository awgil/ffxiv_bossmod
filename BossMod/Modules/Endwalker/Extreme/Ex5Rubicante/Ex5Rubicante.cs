namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class ShatteringHeatBoss : Components.SpreadFromCastTargets
{
    public ShatteringHeatBoss() : base(ActionID.MakeSpell(AID.ShatteringHeatBoss), 4) { }
}

class BlazingRapture : Components.CastCounter
{
    public BlazingRapture() : base(ActionID.MakeSpell(AID.BlazingRaptureAOE)) { }
}

class InfernoSpread : Components.SpreadFromCastTargets
{
    public InfernoSpread() : base(ActionID.MakeSpell(AID.InfernoSpreadAOE), 5) { }
}

[ConfigDisplay(Order = 0x050, Parent = typeof(EndwalkerConfig))]
public class Ex5RubicanteConfig : CooldownPlanningConfigNode
{
    public Ex5RubicanteConfig() : base(90) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 924, NameID = 12057)]
public class Ex5Rubicante : BossModule
{
    public Ex5Rubicante(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
}
