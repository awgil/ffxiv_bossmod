namespace BossMod.Dawntrail.Foray.ForkedTower.FT01DemonTablet;

class DemonicDarkII(BossModule module) : Components.RaidwideCastDelay(module, AID.DemonicDarkIICast, AID.DemonicDarkII, 0.8f);
class LandingBoss(BossModule module) : Components.StandardAOEs(module, AID.LandingBoss, new AOEShapeRect(6, 15));
class LandingNear(BossModule module) : Components.StandardAOEs(module, AID.LandingNear, new AOEShapeRect(15, 15));
class RayOfIgnorance(BossModule module) : Components.StandardAOEs(module, AID.RayOfIgnorance, new AOEShapeRect(30, 15));
class LandingKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.LandingKnockback, 25, kind: Kind.DirForward, shape: new AOEShapeRect(30, 15));
class OccultChisel(BossModule module) : Components.BaitAwayCast(module, AID.OccultChisel, new AOEShapeCircle(5), true, true);
class Rotation(BossModule module) : Components.StandardAOEs(module, AID.RotationCone, new AOEShapeCone(37, 45.Degrees()));
class Rotation1(BossModule module) : Components.GroupedAOEs(module, [AID.RotationBoss1, AID.RotationBoss2], new AOEShapeRect(33, 1.5f));
class LacunateStream(BossModule module) : Components.GenericAOEs(module, AID.LacunateStreamHelper)
{
    private AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RotateLeft:
                _predicted = new(new AOEShapeRect(34, 15), caster.Position, caster.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 5.1f));
                break;
            case AID.RotateRight:
                _predicted = new(new AOEShapeRect(34, 15), caster.Position, caster.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 5.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _predicted = null;
        }
    }
}
class Summon(BossModule module) : Components.StandardAOEs(module, AID.SummonRect, new AOEShapeRect(36, 15));
class Summons(BossModule module) : Components.AddsMulti(module, [OID.SummonedDemon, OID.SummonedArchdemon], 2);
class DarkDefenses(BossModule module) : Components.DispelHint(module, (uint)SID.DarkDefenses, includeTargetName: true);
class LandingStatue(BossModule module) : Components.StandardAOEs(module, AID.LandingStatue, new AOEShapeCircle(18));

class BossCollision(BossModule module) : Components.CastCounter(module, AID.LandingBoss)
{
    public bool Grounded { get; private set; } = true;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Grounded)
            hints.TemporaryObstacles.Add(ShapeContains.Rect(Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, 3.5f, 3.5f, 15.5f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RayOfDangersNear or AID.RayOfExpulsionAfar or AID.DemonographOfDangersNear or AID.DemonographOfExpulsionAfar or AID.CometeorOfExpulsionAfar or AID.CometeorOfDangersNear or AID.GravityOfExpulsionAfar or AID.GravityOfDangersNear or AID.SummonRect)
            Grounded = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Grounded = true;
        }
    }
}

static class AssignmentUtil
{
    private static WPos Center => FT01DemonTablet.ArenaCenter;

    public static ForkedTowerConfig.Alliance GetTowerAssignment(WPos pos)
    {
        var letters = pos.Z < Center.Z;
        var off = pos - Center;
        if (letters)
            off = -off;

        ForkedTowerConfig.Alliance ass;
        if (off.Z > 20)
            ass = letters ? ForkedTowerConfig.Alliance.B : ForkedTowerConfig.Alliance.E2;
        else if (off.X > 0)
            ass = letters ? ForkedTowerConfig.Alliance.C : ForkedTowerConfig.Alliance.F3;
        else
            ass = letters ? ForkedTowerConfig.Alliance.A : ForkedTowerConfig.Alliance.D1;

        return ass;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13760)]
public class FT01DemonTablet(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(15, 33))
{
    public static readonly WPos ArenaCenter = new(700, 379);

    public override bool DrawAllPlayers => true;
}

