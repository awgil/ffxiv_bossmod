namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN1TrinitySeeker;

class MercifulBreeze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MercifulBreeze), new AOEShapeRect(50, 2.5f));
class MercifulBlooms(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MercifulBlooms), new AOEShapeCircle(20));
class MercifulArc(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(12, 45.Degrees()), (uint)IconID.MercifulArc, ActionID.MakeSpell(AID.MercifulArc)); // TODO: verify angle

class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.ScorchingShackle));

class IronImpact(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.IronImpact));
class IronRose(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IronRose), new AOEShapeRect(50, 4));

class DeadIron : Components.BaitAwayTethers
{
    public DeadIron(BossModule module) : base(module, new AOEShapeCone(50, 15.Degrees()), (uint)TetherID.DeadIron, ActionID.MakeSpell(AID.DeadIronAOE)) { DrawTethers = false; }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9834)]
public class DRN1TrinitySeeker(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 278), new ArenaBoundsCircle(25))
{
    public const float BarricadeRadius = 20;

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < 4; ++i)
        {
            var center = (45 + i * 90).Degrees();
            Arena.PathArcTo(Center, BarricadeRadius, (center - 22.5f.Degrees()).Rad, (center + 22.5f.Degrees()).Rad);
            MiniArena.PathStroke(false, ArenaColor.Border, 2);
        }
    }
}
