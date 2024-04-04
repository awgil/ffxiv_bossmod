namespace BossMod.Endwalker.Alliance.A33Oschon;

class Downhill : Components.LocationTargetedAOEs
{
    public Downhill() : base(ActionID.MakeSpell(AID.Downhill2), 6) { }
}

class ClimbingShot : Components.KnockbackFromCastTarget
{
    public ClimbingShot() : base(ActionID.MakeSpell(AID.ClimbingShot), 20) { }
}

class ClimbingShot2 : Components.KnockbackFromCastTarget
{
    public ClimbingShot2() : base(ActionID.MakeSpell(AID.ClimbingShot2), 20) { }
}

class ClimbingShot3 : Components.KnockbackFromCastTarget
{
    public ClimbingShot3() : base(ActionID.MakeSpell(AID.ClimbingShot3), 20) { }
}

class ClimbingShot4 : Components.KnockbackFromCastTarget
{
    public ClimbingShot4() : base(ActionID.MakeSpell(AID.ClimbingShot4), 20) { }
}

class SoaringMinuet1 : Components.SelfTargetedAOEs
{
    public SoaringMinuet1() : base(ActionID.MakeSpell(AID.SoaringMinuet1), new AOEShapeCone(65, 135.Degrees())) { }
}

class SoaringMinuet2 : Components.SelfTargetedAOEs
{
    public SoaringMinuet2() : base(ActionID.MakeSpell(AID.SoaringMinuet2), new AOEShapeCone(65, 135.Degrees())) { }
}