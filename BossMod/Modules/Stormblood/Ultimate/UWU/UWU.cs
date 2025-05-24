namespace BossMod.Stormblood.Ultimate.UWU;

class P1Slipstream(BossModule module) : Components.StandardAOEs(module, AID.Slipstream, new AOEShapeCone(11.7f, 45.Degrees()));
class P1Downburst(BossModule module) : Components.Cleave(module, AID.Downburst, new AOEShapeCone(11.7f, 45.Degrees()));
class P1EyeOfTheStorm(BossModule module) : Components.StandardAOEs(module, AID.EyeOfTheStorm, new AOEShapeDonut(12, 25));
class P1Gigastorm(BossModule module) : Components.StandardAOEs(module, AID.Gigastorm, new AOEShapeCircle(6.5f));
class P2RadiantPlume(BossModule module) : Components.StandardAOEs(module, AID.RadiantPlumeAOE, 8);
class P2Incinerate(BossModule module) : Components.Cleave(module, AID.Incinerate, new AOEShapeCone(15, 60.Degrees()), (uint)OID.Ifrit);
class P3RockBuster(BossModule module) : Components.Cleave(module, AID.RockBuster, new AOEShapeCone(10.55f, 60.Degrees()), (uint)OID.Titan); // TODO: verify angle
class P3MountainBuster(BossModule module) : Components.Cleave(module, AID.MountainBuster, new AOEShapeCone(15.55f, 45.Degrees()), (uint)OID.Titan); // TODO: verify angle
class P3WeightOfTheLand(BossModule module) : Components.StandardAOEs(module, AID.WeightOfTheLandAOE, 6);
class P3Upheaval(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Upheaval, 24, true);
class P3Tumult(BossModule module) : Components.CastCounter(module, AID.Tumult);
class P4Blight(BossModule module) : Components.CastCounter(module, AID.Blight);
class P4HomingLasers(BossModule module) : Components.SpreadFromCastTargets(module, AID.HomingLasers, 4);
class P4DiffractiveLaser(BossModule module) : Components.Cleave(module, AID.DiffractiveLaser, new AOEShapeCone(18, 45.Degrees()), (uint)OID.UltimaWeapon); // TODO: verify angle
class P5MistralSongCone(BossModule module) : Components.StandardAOEs(module, AID.MistralSongCone, new AOEShapeCone(21.7f, 75.Degrees()));
class P5AetherochemicalLaserCenter(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalLaserCenter, new AOEShapeRect(46, 4, 6));
class P5AetherochemicalLaserRight(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalLaserRight, new AOEShapeRect(46, 4, 6));
class P5AetherochemicalLaserLeft(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalLaserLeft, new AOEShapeRect(46, 4, 6));
class P5LightPillar(BossModule module) : Components.StandardAOEs(module, AID.LightPillarAOE, 3); // TODO: consider showing circle around baiter
class P5AethericBoom(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AethericBoom, 10);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Garuda, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 539, PlanLevel = 70)]
public class UWU : BossModule
{
    private readonly IReadOnlyList<Actor> _titan;
    private readonly IReadOnlyList<Actor> _lahabrea;
    private readonly IReadOnlyList<Actor> _ultima;
    private Actor? _mainIfrit;

    public IReadOnlyList<Actor> Ifrits { get; }

    public Actor? Garuda() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Ifrit() => _mainIfrit;
    public Actor? Titan() => _titan.FirstOrDefault();
    public Actor? Lahabrea() => _lahabrea.FirstOrDefault();
    public Actor? Ultima() => _ultima.FirstOrDefault();

    public UWU(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
    {
        Ifrits = Enemies(OID.Ifrit);
        _titan = Enemies(OID.Titan);
        _lahabrea = Enemies(OID.Lahabrea);
        _ultima = Enemies(OID.UltimaWeapon);
    }

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void UpdateModule()
    {
        if (StateMachine.ActivePhaseIndex == 1)
            _mainIfrit ??= Ifrits.FirstOrDefault(a => a.IsTargetable);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Garuda(), ArenaColor.Enemy);
        Arena.Actor(Ifrit(), ArenaColor.Enemy);
        Arena.Actor(Titan(), ArenaColor.Enemy);
        Arena.Actor(Lahabrea(), ArenaColor.Enemy);
        Arena.Actor(Ultima(), ArenaColor.Enemy);
    }
}
