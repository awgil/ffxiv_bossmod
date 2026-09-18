namespace BossMod.Global.Crucible.CatoblepasPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x18, Helper type
    Boss = 0x4C9B, // R4.600, x1
    DemonicEyeCircle = 0x4C9C, // R1.500, x0 (spawn during fight)
    DemonicEyeRing = 0x4C9D, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 49682, // Boss->player, no cast, single-target
    BestialRoar = 48504, // Boss->self, 3.0s cast, range 60 circle
    Nearburst = 48505, // 4C9C->self, no cast, single-target
    NearburstAOE = 48506, // Helper->self, 0.5s cast, range 25 circle
    Farburst = 48507, // 4C9D->self, no cast, single-target
    FarburstAOE = 48508, // Helper->self, 0.5s cast, range 5-50 donut
    ShiftingGazeBoss = 48509, // Boss->self, 3.0s cast, single-target
    ShiftingGaze = 48954, // Boss->4C9C/4C9D, no cast, single-target
    FalseDemonEye = 48510, // Helper->self, no cast, range 100 circle, gaze
    SinisterGleamCast = 48511, // Boss->self, 6.0s cast, single-target
    SinisterGleam = 48512, // Helper->self, 6.5s cast, range 60 180-degree cone
}

public enum SID : uint
{
    Petrification = 4891, // Helper->player, extra=0x0
    Gaze = 2056, // Boss->4C9C/4C9D, extra=0xAE
}

public enum TetherID : uint
{
    EyeTether = 195, // 4C9C/4C9D->Boss
}

class BestialRoar(BossModule module) : Components.RaidwideCast(module, AID.BestialRoar);

// eyes spawn and move in a straight line to a point 135 degrees away from their starting position and 21 units away from center
// eyes pop if you touch them, so avoid the non-donut ones since they cover the whole arena
class DemonicEye(BossModule module) : BossComponent(module)
{
    public record struct Eye(Actor Actor, WPos Destination, DateTime Arrival, bool Gaze);

    readonly List<Actor> _unassigned = [];
    public readonly List<Eye> Eyes = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.DemonicEyeRing or OID.DemonicEyeCircle)
            _unassigned.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NearburstAOE or AID.FarburstAOE)
        {
            for (var i = 0; i < Eyes.Count; i++)
                if (Eyes[i].Actor.Position.AlmostEqual(spell.LocXZ, 1))
                {
                    Eyes.Ref(i).Destination = spell.LocXZ;
                    Eyes.Ref(i).Arrival = Module.CastFinishAt(spell);
                }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NearburstAOE or AID.FarburstAOE)
            Eyes.RemoveAll(e => e.Actor.Position.AlmostEqual(caster.Position, 1));
    }

    public override void Update()
    {
        for (var i = _unassigned.Count - 1; i >= 0; i--)
        {
            var actor = _unassigned[i];
            if (!actor.Position.InCircle(Arena.Center, 20))
                continue;

            var dir = actor.Rotation.ToDirection();
            var len = Intersect.RayCircle(actor.Position - Arena.Center, dir, 21);
            var dest = Arena.Center + (MathF.Round((actor.Position + dir * len - Arena.Center).ToAngle().Rad / (MathF.PI / 4)) * MathF.PI / 4).Radians().ToDirection() * 21;

            Eyes.Add(new(actor, dest, WorldState.FutureTime(19.1f), actor.FindStatus(SID.Gaze) != null));
            Eyes.SortBy(e => e.Arrival);
            _unassigned.RemoveAt(i);
        }
    }
}

class DemonicEyeVoidzone(BossModule module) : Components.Voidzone(module, 2, (uint)OID.DemonicEyeCircle, moveHintLength: 10)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NearburstAOE)
        {
            var matched = Sources.Where(s => s.Position.AlmostEqual(spell.LocXZ, 1)).ToList();
            foreach (var m in matched)
                RemoveSource(m);
        }
    }
}

class Nearburst(BossModule module) : Components.GenericAOEs(module, AID.NearburstAOE)
{
    readonly DemonicEye _eyes = module.FindComponent<DemonicEye>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _eyes.Eyes.Where(e => e.Actor.OID == (uint)OID.DemonicEyeCircle).Take(1).Select(e => new AOEInstance(new AOEShapeCircle(25), e.Destination, default, e.Arrival));
}

class Farburst(BossModule module) : Components.GenericAOEs(module, AID.FarburstAOE)
{
    readonly DemonicEye _eyes = module.FindComponent<DemonicEye>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _eyes.Eyes.Where(e => e.Actor.OID == (uint)OID.DemonicEyeRing).Take(1).Select(e => new AOEInstance(new AOEShapeDonut(5, 50), e.Destination, default, e.Arrival));
}

class FalseDemonEye(BossModule module) : Components.GenericGaze(module, AID.FalseDemonEye)
{
    readonly DemonicEye _eyes = module.FindComponent<DemonicEye>()!;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => _eyes.Eyes.Where(e => e.Gaze).Take(1).Select(e => new Eye(e.Destination, e.Arrival));
}

class SinisterGleam(BossModule module) : Components.StandardAOEs(module, AID.SinisterGleam, new AOEShapeCone(60, 90.Degrees()));

class CatoblepasPieceStates : StateMachineBuilder
{
    public CatoblepasPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BestialRoar>()
            .ActivateOnEnter<DemonicEye>()
            .ActivateOnEnter<DemonicEyeVoidzone>()
            .ActivateOnEnter<Nearburst>()
            .ActivateOnEnter<Farburst>()
            .ActivateOnEnter<FalseDemonEye>()
            .ActivateOnEnter<SinisterGleam>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14577)]
public class CatoblepasPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

