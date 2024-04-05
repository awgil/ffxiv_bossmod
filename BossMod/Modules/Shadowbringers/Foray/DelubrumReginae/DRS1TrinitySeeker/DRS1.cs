namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class VerdantTempest : Components.CastCounter
{
    public VerdantTempest() : base(ActionID.MakeSpell(AID.VerdantTempestAOE)) { }
}

class MercifulBreeze : Components.SelfTargetedAOEs
{
    public MercifulBreeze() : base(ActionID.MakeSpell(AID.MercifulBreeze), new AOEShapeRect(50, 2.5f)) { }
}

class MercifulBlooms : Components.SelfTargetedAOEs
{
    public MercifulBlooms() : base(ActionID.MakeSpell(AID.MercifulBlooms), new AOEShapeCircle(20)) { }
}

class MercifulArc : Components.BaitAwayIcon
{
    public MercifulArc() : base(new AOEShapeCone(12, 45.Degrees()), (uint)IconID.MercifulArc, ActionID.MakeSpell(AID.MercifulArc)) { } // TODO: verify angle
}

// TODO: depending on phantom edge, it's either a shared tankbuster cleave or a weird cleave ignoring closest target (?)
class BalefulOnslaught1 : Components.Cleave
{
    public BalefulOnslaught1() : base(ActionID.MakeSpell(AID.BalefulOnslaughtAOE1), new AOEShapeCone(10, 45.Degrees())) { } // TODO: verify angle
}
class BalefulOnslaught2 : Components.Cleave
{
    public BalefulOnslaught2() : base(ActionID.MakeSpell(AID.BalefulOnslaughtAOE2), new AOEShapeCone(10, 45.Degrees())) { } // TODO: verify angle
}

class BurningChains : Components.Chains
{
    public BurningChains() : base((uint)TetherID.BurningChains, ActionID.MakeSpell(AID.ScorchingShackle)) { }
}

// TODO: it's a line stack, but I don't think there's a way to determine cast target - so everyone should just stack?..
class IronImpact : Components.CastCounter
{
    public IronImpact() : base(ActionID.MakeSpell(AID.IronImpact)) { }
}

class IronRose : Components.SelfTargetedAOEs
{
    public IronRose() : base(ActionID.MakeSpell(AID.IronRose), new AOEShapeRect(50, 4)) { }
}

class DeadIron : Components.BaitAwayTethers
{
    public DeadIron() : base(new AOEShapeCone(50, 15.Degrees()), (uint)TetherID.DeadIron, ActionID.MakeSpell(AID.DeadIronAOE)) { DrawTethers = false; }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9834)]
public class DRS1 : BossModule
{
    public static readonly float BarricadeRadius = 20;

    public DRS1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 278), 25)) { }

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (int i = 0; i < 4; ++i)
        {
            var center = (45 + i * 90).Degrees();
            Arena.PathArcTo(Bounds.Center, BarricadeRadius, (center - 22.5f.Degrees()).Rad, (center + 22.5f.Degrees()).Rad);
            Arena.PathStroke(false, ArenaColor.Border, 2);
        }
    }
}
