namespace BossMod.Global.Crucible.CatoblepasPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x18, Helper type
    Boss = 0x4C9B, // R4.600, x1
    _Gen_DemonicEye = 0x4C9D, // R1.500, x0 (spawn during fight)
    _Gen_DemonicEye1 = 0x4C9C, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 49682, // Boss->player, no cast, single-target
    _Weaponskill_BestialRoar = 48504, // Boss->self, 3.0s cast, range 60 circle
    _Weaponskill_Nearburst = 48505, // 4C9C->self, no cast, single-target
    _Weaponskill_Farburst = 48507, // 4C9D->self, no cast, single-target
    _Weaponskill_Nearburst1 = 48506, // Helper->self, 0.5s cast, range 25 circle
    _Weaponskill_Farburst1 = 48508, // Helper->self, 0.5s cast, range 5-50 donut
    _Weaponskill_ShiftingGaze = 48509, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ShiftingGaze1 = 48954, // Boss->4C9C/4C9D, no cast, single-target
    _Weaponskill_FalseDemonEye = 48510, // Helper->self, no cast, range 100 circle
    _Weaponskill_SinisterGleam = 48511, // Boss->self, 6.0s cast, single-target
    _Weaponskill_SinisterGleam1 = 48512, // Helper->self, 6.5s cast, range 60 180-degree cone
}

public enum SID : uint
{
    _Gen_Petrification = 4891, // Helper->player, extra=0x0
    _Gen_ = 2056, // Boss->4C9C/4C9D, extra=0xAE
}

public enum TetherID : uint
{
    _Gen_Tether_chn_ice_mouth01x = 195, // 4C9C/4C9D->Boss
}

// eyes spawn and move in a straight line to a point 135 degrees away from their starting position and 21 units away from center
// if an eye has a gaze attached, it triggers early when it is 18 units away from the center, so it's not aligned with a compass point
class Farburst(BossModule module) : Components.DebugCasts(module, [AID._Weaponskill_Farburst1], new AOEShapeDonut(5, 50));

class DemonicEye(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _unassigned = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID._Gen_DemonicEye or OID._Gen_DemonicEye1)
            _unassigned.Add(actor);
    }

    public override void Update()
    {
        for (var i = _unassigned.Count - 1; i >= 0; i--)
        {
            var actor = _unassigned[i];
            if (!actor.Position.InCircle(Arena.Center, 20))
                continue;

            var start = Module.StateMachine.TimeSinceActivation;

            var dir = actor.Rotation.ToDirection();
            var len = Intersect.RayCircle(actor.Position - Arena.Center, dir, 21);
            var dest = Arena.Center + (MathF.Round((actor.Position + dir * len - Arena.Center).ToAngle().Rad / (MathF.PI / 4)) * MathF.PI / 4).Radians().ToDirection() * 21;

            if (actor.FindStatus(SID._Gen_) != null)
            {
                // stop early
                var dirActual = dest - actor.Position;
                var len2 = Intersect.RayCircle(actor.Position - Arena.Center, dirActual.Normalized(), 18);
                var d2 = actor.Position + dirActual.Normalized() * len2;
                Service.Log($"actor {actor} will likely arrive at {d2} (+{start})");
            }
            else
            {
                // delay is 19.1s or so
                // TODO: it seems like eyeballs also trigger if you touch them?
                Service.Log($"actor {actor} will likely arrive at {dest} (+{start})");
            }
            _unassigned.RemoveAt(i);
        }
    }
}

class CatoblepasPieceStates : StateMachineBuilder
{
    public CatoblepasPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
        //.ActivateOnEnter<Farburst>()
        //.ActivateOnEnter<DemonicEye>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14577)]
public class CatoblepasPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

