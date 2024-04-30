namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

class Incinerate(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Incinerate), CleaveShape)
{
    public static readonly AOEShapeCone CleaveShape = new(21, 60.Degrees());
}

class RadiantPlume(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RadiantPlumeAOE), 8);

// TODO: consider showing next charge before its cast starts...
class CrimsonCyclone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrimsonCyclone), new AOEShapeRect(49, 9));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 63, NameID = 1185)]
public class Ex4Ifrit : BossModule
{
    public IReadOnlyList<Actor> SmallNails;
    public IReadOnlyList<Actor> LargeNails;

    public Ex4Ifrit(WorldState ws, Actor primary) : base(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
    {
        SmallNails = Enemies(OID.InfernalNailSmall);
        LargeNails = Enemies(OID.InfernalNailLarge);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(SmallNails, ArenaColor.Object);
        Arena.Actors(LargeNails, ArenaColor.Object);
    }
}
