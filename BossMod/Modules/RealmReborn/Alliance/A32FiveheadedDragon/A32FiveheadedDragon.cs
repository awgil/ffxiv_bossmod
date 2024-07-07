namespace BossMod.RealmReborn.Alliance.A32FiveheadedDragon;

class WhiteBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhiteBreath), new AOEShapeCone(30, 60.Degrees()));
class BreathOfFire(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BreathOfFire), 6);
class BreathOfLight(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BreathOfLight), 6);
class BreathOfPoison(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BreathOfPoison), 6);
class BreathOfIce(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BreathOfIce), 6);

class Radiance(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Radiance));
class HeatWave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HeatWave));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3227)]
public class A32FiveheadedDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, 180), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.HeadOfFire), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.HeadOfPoison), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.HeadOfThunder), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.HeadOfIce), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Prominence), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.PoisonSlime), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ToxicSlime), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.DragonfireFly), ArenaColor.Enemy);
    }
}
