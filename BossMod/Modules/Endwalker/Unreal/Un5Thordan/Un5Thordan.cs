namespace BossMod.Endwalker.Unreal.Un5Thordan;

class AscalonsMight : Components.Cleave
{
    public AscalonsMight() : base(ActionID.MakeSpell(AID.AscalonsMight), new AOEShapeCone(8 + 3.8f, 45.Degrees())) { }
}

class Meteorain : Components.LocationTargetedAOEs
{
    public Meteorain() : base(ActionID.MakeSpell(AID.MeteorainAOE), 6) { }
}

class AscalonsMercy : Components.SelfTargetedAOEs
{
    public AscalonsMercy() : base(ActionID.MakeSpell(AID.AscalonsMercy), new AOEShapeCone(34.8f, 10.Degrees())) { }
}

class AscalonsMercyHelper : Components.SelfTargetedAOEs
{
    public AscalonsMercyHelper() : base(ActionID.MakeSpell(AID.AscalonsMercyAOE), new AOEShapeCone(34.5f, 10.Degrees())) { }
}

class DragonsRage : Components.StackWithCastTargets
{
    public DragonsRage() : base(ActionID.MakeSpell(AID.DragonsRage), 6, 6) { }
}

class Heavensflame : Components.LocationTargetedAOEs
{
    public Heavensflame() : base(ActionID.MakeSpell(AID.HeavensflameAOE), 6) { }
}

class Conviction : Components.CastTowers
{
    public Conviction() : base(ActionID.MakeSpell(AID.ConvictionAOE), 3) { }
}

class SerZephirin : Components.Adds
{
    public SerZephirin() : base((uint)OID.Zephirin) { }
}

// TODO: show knockback 3 from [-0.8, -16.3]
class LightOfAscalon : Components.CastCounter
{
    public LightOfAscalon() : base(ActionID.MakeSpell(AID.LightOfAscalon)) { }
}

class UltimateEnd : Components.CastCounter
{
    public UltimateEnd() : base(ActionID.MakeSpell(AID.UltimateEndAOE)) { }
}

class HeavenswardLeap : Components.CastCounter
{
    public HeavenswardLeap() : base(ActionID.MakeSpell(AID.HeavenswardLeap)) { }
}

class PureOfSoul : Components.CastCounter
{
    public PureOfSoul() : base(ActionID.MakeSpell(AID.PureOfSoul)) { }
}

class AbsoluteConviction : Components.CastCounter
{
    public AbsoluteConviction() : base(ActionID.MakeSpell(AID.AbsoluteConviction)) { }
}

[ConfigDisplay(Order = 0x350, Parent = typeof(EndwalkerConfig))]
public class Un5ThordanConfig : CooldownPlanningConfigNode
{
    public Un5ThordanConfig() : base(90) { }
}

public class Un5Thordan : BossModule
{
    public Un5Thordan(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 21)) { }
}
