namespace BossMod.Endwalker.Unreal.Un3Sophia;

class ThunderDonut : Components.SelfTargetedAOEs
{
    public ThunderDonut() : base(ActionID.MakeSpell(AID.ThunderDonut), new AOEShapeDonut(5, 20)) { }
}

class ExecuteDonut : Components.SelfTargetedAOEs
{
    public ExecuteDonut() : base(ActionID.MakeSpell(AID.ExecuteDonut), new AOEShapeDonut(5, 20)) { }
}

class Aero : Components.SelfTargetedAOEs
{
    public Aero() : base(ActionID.MakeSpell(AID.Aero), new AOEShapeCircle(10)) { }
}

class ExecuteAero : Components.SelfTargetedAOEs
{
    public ExecuteAero() : base(ActionID.MakeSpell(AID.ExecuteAero), new AOEShapeCircle(10)) { }
}

class ThunderCone : Components.SelfTargetedAOEs
{
    public ThunderCone() : base(ActionID.MakeSpell(AID.ThunderCone), new AOEShapeCone(20, 45.Degrees())) { }
}

class ExecuteCone : Components.SelfTargetedAOEs
{
    public ExecuteCone() : base(ActionID.MakeSpell(AID.ExecuteCone), new AOEShapeCone(20, 45.Degrees())) { }
}

class LightDewShort : Components.SelfTargetedAOEs
{
    public LightDewShort() : base(ActionID.MakeSpell(AID.LightDewShort), new AOEShapeRect(55, 9, 5)) { }
}

class LightDewLong : Components.SelfTargetedAOEs
{
    public LightDewLong() : base(ActionID.MakeSpell(AID.LightDewLong), new AOEShapeRect(55, 9, 5)) { }
}

class Onrush : Components.SelfTargetedAOEs
{
    public Onrush() : base(ActionID.MakeSpell(AID.Onrush), new AOEShapeRect(55, 8, 5)) { }
}

class Gnosis : Components.KnockbackFromCastTarget
{
    public Gnosis() : base(ActionID.MakeSpell(AID.Gnosis), 25) { }
}

// note: ~4.2s before first cast boss gets model state 5
class Cintamani : Components.CastCounter
{
    public Cintamani() : base(ActionID.MakeSpell(AID.Cintamani)) { }
}

class QuasarProximity1 : Components.LocationTargetedAOEs
{
    public QuasarProximity1() : base(ActionID.MakeSpell(AID.QuasarProximity1), 15) { }
}

class QuasarProximity2 : Components.LocationTargetedAOEs
{
    public QuasarProximity2() : base(ActionID.MakeSpell(AID.QuasarProximity2), 15) { } // TODO: reconsider distance
}

[ConfigDisplay(Order = 0x330, Parent = typeof(EndwalkerConfig))]
public class Un3SophiaConfig : CooldownPlanningConfigNode
{
    public Un3SophiaConfig() : base(90) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 926, NameID = 5199)]
public class Un3Sophia : BossModule
{
    public Un3Sophia(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 0), 20, 15)) { }
}
