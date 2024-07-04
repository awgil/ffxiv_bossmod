namespace BossMod.Stormblood.Alliance.A24Yiazmat;

class RakeTB(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.RakeTB));
class RakeSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.RakeSpread), 5);
class RakeAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RakeAOE), new AOEShapeCircle(10));
class RakeLoc1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RakeLoc1), 10);
class RakeLoc2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RakeLoc2), 10);
class StoneBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StoneBreath), new AOEShapeCone(60, 22.5f.Degrees()));
class DustStorm2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DustStorm2));
class WhiteBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhiteBreath), new AOEShapeDonut(10, 60));

class AncientAero(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AncientAero), new AOEShapeRect(40, 3));
class Karma(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Karma), new AOEShapeCone(30, 45.Degrees()));
class UnholyDarkness(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UnholyDarkness), 8);

class SolarStorm1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SolarStorm1));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7070)]
public class A24Yiazmat(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, -400), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.HeartOfTheDragon))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Archaeodemon))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.WindAzer))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
