namespace BossMod.Dawntrail.Dungeon.D13Clyteum.D131EyeOfTheScorpion;

public enum OID : uint
{
    Boss = 0x4C2C, // R6.000, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    MotionScanner = 0x4C2D, // R1.000, x2
}

public enum AID : uint
{
    AutoAttack = 50110, // Boss->player, no cast, single-target
    EyesOnMe = 48896, // Boss->self, 5.0s cast, range 35 circle
    PetrifyingBeamCast1 = 50175, // Boss->self, 8.0+0.5s cast, single-target
    PetrifyingBeamCast2 = 50176, // Boss->self, 8.0+0.5s cast, single-target
    PetrifyingBeam1 = 50177, // Helper->self, 8.5s cast, range 70 100-degree cone
    PetrifyingBeam2 = 50178, // Helper->self, 8.5s cast, range 70 100-degree cone
    MotionScanner = 48893, // Boss->self, 4.0s cast, single-target
    Launch = 48895, // Helper->player, no cast, single-target
    BallisticMissile = 48897, // Boss->self, no cast, single-target
    PenetratorMissile = 48901, // Helper->players, 5.0s cast, range 6 circle
    SurfaceMissile = 48898, // Helper->location, 3.0s cast, range 5 circle
    AntiPersonnelMissile = 48899, // Helper->player, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    MotionTracker = 5191, // none->player, extra=0x0
}

public enum IconID : uint
{
    Checkmark = 136, // player->self
    Stack = 62, // player->self
    Spread = 139, // player->self
}

class EyesOnMe(BossModule module) : Components.RaidwideCast(module, AID.EyesOnMe);
class PetrifyingBeam(BossModule module) : Components.GroupedAOEs(module, [AID.PetrifyingBeam1, AID.PetrifyingBeam2], new AOEShapeCone(70, 50.Degrees()));
class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID.SurfaceMissile, 5);
class PenetratorMissile(BossModule module) : Components.StackWithCastTargets(module, AID.PenetratorMissile, 5);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID.AntiPersonnelMissile, 6);
class MotionScanner(BossModule module) : Components.StayMove(module)
{
    Actor? _scanner;
    BitMask _exclude;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MotionTracker && Raid.TryFindSlot(actor, out var slot))
            SetState(slot, new(Requirement.Stay, WorldState.CurrentTime, 1));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MotionTracker && Raid.TryFindSlot(actor, out var slot))
        {
            _exclude.Set(slot);
            ClearState(slot);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.MotionScanner && id == 0x248B)
            _scanner = actor;

        if ((OID)actor.OID == OID.MotionScanner && id == 0x1E3C)
        {
            _scanner = null;
            _exclude.Reset();
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_scanner != null)
            Arena.ZoneRect(_scanner.Position, _scanner.Rotation, 7.5f, 7.5f, 20, 0x807969D2);
    }

    // we have to anticipate scanner appearing, since reacting to status gain is too late
    public override void Update()
    {
        if (_scanner != null)
        {
            foreach (var (i, _) in Raid.WithSlot().ExcludedFromMask(_exclude).InShape(new AOEShapeRect(9.5f, 15, 7.5f), _scanner))
                SetState(i, new(Requirement.Stay, WorldState.CurrentTime));
        }
    }
}

class D131EyeOfTheScorpionStates : StateMachineBuilder
{
    public D131EyeOfTheScorpionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EyesOnMe>()
            .ActivateOnEnter<PetrifyingBeam>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<PenetratorMissile>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<MotionScanner>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1011, NameID = 14716)]
public class D131EyeOfTheScorpion(WorldState ws, Actor primary) : BossModule(ws, primary, new(-615, 575), new ArenaBoundsSquare(20));
