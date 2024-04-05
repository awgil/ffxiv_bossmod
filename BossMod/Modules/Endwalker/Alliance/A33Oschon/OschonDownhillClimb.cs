namespace BossMod.Endwalker.Alliance.A33Oschon;

class DownhillP1 : Components.LocationTargetedAOEs
{
    public DownhillP1() : base(ActionID.MakeSpell(AID.Downhill), 6) { }
}

class ClimbingShot : Components.KnockbackFromCastTarget
{
    public ClimbingShot() : base(ActionID.MakeSpell(AID.ClimbingShot), 20) { }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<DownhillP1>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false || !module.Bounds.Contains(pos);
}

class ClimbingShot2 : Components.KnockbackFromCastTarget
{
    public ClimbingShot2() : base(ActionID.MakeSpell(AID.ClimbingShot2), 20) { }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<DownhillP1>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false || !module.Bounds.Contains(pos);
}

class ClimbingShot3 : Components.KnockbackFromCastTarget
{
    public ClimbingShot3() : base(ActionID.MakeSpell(AID.ClimbingShot3), 20) { }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<DownhillP1>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false || !module.Bounds.Contains(pos);
}

class ClimbingShot4 : Components.KnockbackFromCastTarget
{
    public ClimbingShot4() : base(ActionID.MakeSpell(AID.ClimbingShot4), 20) { }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<DownhillP1>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false || !module.Bounds.Contains(pos);
}

class SoaringMinuet1 : Components.SelfTargetedAOEs
{
    public SoaringMinuet1() : base(ActionID.MakeSpell(AID.SoaringMinuet1), new AOEShapeCone(65, 135.Degrees())) { }
}

class SoaringMinuet2 : Components.SelfTargetedAOEs
{
    public SoaringMinuet2() : base(ActionID.MakeSpell(AID.SoaringMinuet2), new AOEShapeCone(65, 135.Degrees())) { }
}