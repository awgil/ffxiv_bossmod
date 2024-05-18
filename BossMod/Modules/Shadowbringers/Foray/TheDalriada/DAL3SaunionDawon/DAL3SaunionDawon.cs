namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

class HighPoweredMagitekRay(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HighPoweredMagitekRay));
class ToothAndTalon(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ToothAndTalon));
class PentagustAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PentagustAOE), new AOEShapeCone(50, 10.Degrees()));
class RawHeat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RawHeat), new AOEShapeCircle(10));
class SurfaceMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SurfaceMissile), 6);
class SwoopingFrenzyAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SwoopingFrenzyAOE), 12);
class Touchdown3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Touchdown3));
class MissileSalvo(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.MissileSalvo), 6);
class MagitekCrossray(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekCrossray), new AOEShapeCross(60, 8));
class MagitekHalo(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekHalo), new AOEShapeDonut(7, 60));
class FrigidPulseAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FrigidPulseAOE), new AOEShapeDonut(7, 60));
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AntiPersonnelMissile), 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 32, SortOrder = 4)] //BossNameID = 10192 & 10006
public class DAL3SaunionDawon : BossModule
{
    public readonly IReadOnlyList<Actor> Boss;
    public readonly IReadOnlyList<Actor> Dawon;

    public DAL3SaunionDawon(WorldState ws, Actor primary) : base(ws, primary, new(650, -659), new ArenaBoundsSquare(25))
    {
        Boss = Enemies(OID.Boss);
        Dawon = Enemies(OID.Dawon);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Boss, ArenaColor.Enemy);
        Arena.Actors(Dawon, ArenaColor.Enemy);
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
    }
}
