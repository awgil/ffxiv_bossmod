namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

class Incinerate : Components.Cleave
{
    public static readonly AOEShapeCone CleaveShape = new(21, 60.Degrees());

    public Incinerate() : base(ActionID.MakeSpell(AID.Incinerate), CleaveShape) { }
}

class RadiantPlume : Components.LocationTargetedAOEs
{
    public RadiantPlume() : base(ActionID.MakeSpell(AID.RadiantPlumeAOE), 8) { }
}

// TODO: consider showing next charge before its cast starts...
class CrimsonCyclone : Components.SelfTargetedAOEs
{
    public CrimsonCyclone() : base(ActionID.MakeSpell(AID.CrimsonCyclone), new AOEShapeRect(49, 9)) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 63, NameID = 1185)]
public class Ex4Ifrit : BossModule
{
    public IReadOnlyList<Actor> SmallNails;
    public IReadOnlyList<Actor> LargeNails;

    public Ex4Ifrit(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 20))
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
