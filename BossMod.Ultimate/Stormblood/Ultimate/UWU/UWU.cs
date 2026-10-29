namespace BossMod.Stormblood.Ultimate.UWU;

class P1Slipstream(BossModule module) : Components.StandardAOEs(module, AID.Slipstream, new AOEShapeCone(11.7f, 45.Degrees()));
class P1Downburst(BossModule module) : Components.Cleave(module, AID.Downburst, new AOEShapeCone(11.7f, 65.Degrees()));
class P1EyeOfTheStorm(BossModule module) : Components.StandardAOEs(module, AID.EyeOfTheStorm, new AOEShapeDonut(12, 25));
class P1Gigastorm(BossModule module) : Components.StandardAOEs(module, AID.Gigastorm, new AOEShapeCircle(6.5f));
class P2RadiantPlume(BossModule module) : Components.StandardAOEs(module, AID.RadiantPlumeAOE, 8);
class P2Incinerate(BossModule module) : Components.Cleave(module, AID.Incinerate, new AOEShapeCone(15, 60.Degrees()), (uint)OID.Ifrit);
class P3RockBuster(BossModule module) : Components.Cleave(module, AID.RockBuster, new AOEShapeCone(10.55f, 60.Degrees()), (uint)OID.Titan); // TODO: verify angle
class P3MountainBuster(BossModule module) : Components.Cleave(module, AID.MountainBuster, new AOEShapeCone(15.55f, 45.Degrees()), (uint)OID.Titan); // TODO: verify angle
class P3WeightOfTheLand(BossModule module) : Components.StandardAOEs(module, AID.WeightOfTheLandAOE, 6);
class P3Tumult(BossModule module) : Components.CastCounter(module, AID.Tumult);
class P4Blight(BossModule module) : Components.CastCounter(module, AID.Blight);
class P4HomingLasers(BossModule module) : Components.SpreadFromCastTargets(module, AID.HomingLasers, 4);
class P4DiffractiveLaser(BossModule module) : Components.Cleave(module, AID.DiffractiveLaser, new AOEShapeCone(18, 45.Degrees()), (uint)OID.UltimaWeapon); // TODO: verify angle
class P5MistralSongCone(BossModule module) : Components.StandardAOEs(module, AID.MistralSongCone, new AOEShapeCone(21.7f, 75.Degrees()));
class P5AetherochemicalLaserCenter(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalLaserCenter, new AOEShapeRect(46, 4));
class P5AetherochemicalLaserRight(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalLaserRight, new AOEShapeRect(46, 4));
class P5AetherochemicalLaserLeft(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalLaserLeft, new AOEShapeRect(46, 4));
class P5LightPillar(BossModule module) : Components.StandardAOEs(module, AID.LightPillarAOE, 3); // TODO: consider showing circle around baiter
class P5AethericBoom(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AethericBoom, 10);

class BossPositioning : BossComponent
{
    public Actor? Boss;
    public WPos? DesiredPosition;
    public Angle? DesiredRotation = 180.Degrees();

    public BossPositioning(BossModule module) : base(module)
    {
        Boss = module.PrimaryActor;
        DesiredPosition = module.Arena.Center;
        KeepOnPhaseChange = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(Boss) is { } target)
        {
            target.DesiredPosition = DesiredPosition;
            target.DesiredRotation = DesiredRotation;

            if (target.Actor.PendingHPRatio < 0.02f && target.Actor.FindStatus(SID.Woken) == null)
                target.Priority = AIHints.Enemy.PriorityForbidden;
        }
    }

    public void Clear()
    {
        DesiredPosition = null;
        DesiredRotation = null;
    }

    public void P2MoveToIntercard()
    {
        if (DesiredPosition is not { } pos || Boss == null)
        {
            ReportError("Trying to set new Ifrit goal position, but component state is wrong, doing nothing");
            return;
        }

        var destAngle = (MathF.Ceiling((pos - Arena.Center).ToAngle().Deg / 90) * 90 - 45).Degrees();
        DesiredPosition = Arena.Center + destAngle.ToDirection() * 11;
        DesiredRotation = destAngle;
    }
}

class PartyPositioning : BossComponent
{
    public PartyPositioning(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
        _lastTitan = module.Arena.Center;
    }

    int _numGeocrushes;
    WPos _lastTitan;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Geocrush2)
            _lastTitan = caster.Position;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Geocrush1)
            _numGeocrushes++;

        if ((AID)spell.Action.ID == AID.Tumult)
            _lastTitan = Arena.Center;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var uwu = (UWU)Module;

        switch (Module.StateMachine.ActivePhaseIndex)
        {
            // garuda
            case 0:
                if (Module.PrimaryActor.IsTargetable)
                    return;

                hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, Module.PrimaryActor.HitboxRadius + 3 + actor.HitboxRadius));
                break;

            // ifrit
            case 1:
                // pre dash/plume start, we haven't picked an ifrit yet (there are 4 identical ones in the object table)
                if (uwu.Ifrit() is not { } ifrit)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 1), DateTime.MaxValue);
                else if (!ifrit.IsTargetable)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, ifrit.HitboxRadius + 3 + actor.HitboxRadius));
                break;

            // titan
            case 2:
                if (uwu.Titan() is not { } titan)
                    return;

                // everyone should stack together for the first proximity raidwide so healers don't get fucked
                if (_numGeocrushes == 0 && Module.FindComponent<BossPositioning>() is { DesiredRotation: { } relNorth } && !titan.IsTargetable)
                {
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center + relNorth.ToDirection() * 20, 5));
                    return;
                }

                // drawing much smaller than max melee so i can avoid writing extra code to make the party stack mid before cardinal geocrush - not enough time to react just using the castbar and we can't tell what cardinal he's going to
                if (!titan.IsTargetable)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(_lastTitan, 4));

                // just stay near titan tbh all of these mechanics are annoying
                if (titan.IsTargetable)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(titan.Position, titan.HitboxRadius + 3 + actor.HitboxRadius, 0.1f));
                break;
        }
    }
}

[ModuleInfo(PrimaryActorOID = (uint)OID.Garuda, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 539, PlanLevel = 70)]
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

    public UWU(ModuleInit init) : base(init, new(100, 100), new ArenaBoundsCircle(19.5f))
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
