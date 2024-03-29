namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class GreaterFlamesent : Components.Adds
{
    public GreaterFlamesent() : base((uint)OID.GreaterFlamesent) { }
}

class FlamesentNS : Components.Adds
{
    public FlamesentNS() : base((uint)OID.FlamesentNS) { }
}

class FlamesentSS : Components.Adds
{
    public FlamesentSS() : base((uint)OID.FlamesentSS) { }
}

class FlamesentNC : Components.Adds
{
    public FlamesentNC() : base((uint)OID.FlamesentNC) { }
}

class GhastlyTorch : Components.RaidwideCast
{
    public GhastlyTorch() : base(ActionID.MakeSpell(AID.GhastlyTorch)) { }
}

class ShatteringHeatAdd : Components.TankbusterTether
{
    public ShatteringHeatAdd() : base(ActionID.MakeSpell(AID.ShatteringHeatAdd), (uint)TetherID.ShatteringHeatAdd, 3) { }
}

class GhastlyWind : Components.BaitAwayTethers
{
    public GhastlyWind() : base(new AOEShapeCone(40, 15.Degrees()), (uint)TetherID.GhastlyWind, ActionID.MakeSpell(AID.GhastlyWind)) { } // TODO: verify angle
}

class GhastlyFlame : Components.LocationTargetedAOEs
{
    public GhastlyFlame() : base(ActionID.MakeSpell(AID.GhastlyFlameAOE), 5) { }
}
