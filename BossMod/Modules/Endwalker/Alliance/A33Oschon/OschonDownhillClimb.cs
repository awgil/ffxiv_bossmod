namespace BossMod.Endwalker.Alliance.A33Oschon;

class Downhill2 : Components.LocationTargetedAOEs
{
    public Downhill2() : base(ActionID.MakeSpell(AID.Downhill2), 6) { }
}

class ClimbingShot : Components.KnockbackFromCastTarget
{
    public ClimbingShot() : base(ActionID.MakeSpell(AID.ClimbingShot), 20) { }
}

class ClimbingShot2 : Components.KnockbackFromCastTarget
{
    public ClimbingShot2() : base(ActionID.MakeSpell(AID.ClimbingShot2), 20) { }
}

class SoaringMinuet1 : Components.SelfTargetedAOEs
{
    public SoaringMinuet1() : base(ActionID.MakeSpell(AID.SoaringMinuet1), new AOEShapeCone(65, 135.Degrees())) { }
}
class SoaringMinuet2 : Components.SelfTargetedAOEs
{
    public SoaringMinuet2() : base(ActionID.MakeSpell(AID.SoaringMinuet2), new AOEShapeCone(65, 135.Degrees())) { }
}

class Ability1 : Components.LocationTargetedAOEs
{
    public Ability1() : base(ActionID.MakeSpell(AID.Ability1), 6) { }
}
class Ability2 : Components.SelfTargetedAOEs
{
    public Ability2() : base(ActionID.MakeSpell(AID.Ability2), new AOEShapeCone(65, 135.Degrees())) { }
}
class Ability3 : Components.SelfTargetedAOEs
{
    public Ability3() : base(ActionID.MakeSpell(AID.Ability3), new AOEShapeCone(65, 135.Degrees())) { }
}
class Ability4 : Components.SelfTargetedAOEs
{
    public Ability4() : base(ActionID.MakeSpell(AID.Ability4), new AOEShapeCone(65, 135.Degrees())) { }
}