namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D60AncestralMaliktender;

public enum OID : uint
{
    Boss = 0x49DE,
    Helper = 0x233C,
    SabotenderAmir = 0x49DF, // R1.200, x12
    FlowertenderAmirah = 0x49E0, // R2.400, x4
}

public enum AID : uint
{
    AutoAttack = 45130, // Boss->player, no cast, single-target
    SpineshotCast = 44866, // Boss->self, 5.0s cast, single-target
    Spineshot = 44872, // Helper->self, no cast, range 60 60-degree cone
    BranchOut = 44857, // Boss->self, 3.0s cast, single-target
    SeveralThousandNeedlesSlow = 44858, // SabotenderAmir->self, 5.0s cast, range 10 width 10 rect
    LongRangeNeedlesSlow = 44859, // FlowertenderAmirah->self, 5.0s cast, range 30 width 30 rect
    OneStoneMarch = 44862, // Boss->self, 14.0s cast, single-target
    TwoStoneMarch = 44863, // Boss->self, 14.0s cast, single-target
    SeveralThousandNeedlesCast = 44864, // SabotenderAmir->self, 12.7s cast, single-target
    LongRangeNeedlesCast = 44865, // FlowertenderAmirah->self, 12.7s cast, single-target
    SeveralThousandNeedlesFast = 44860, // SabotenderAmir->self, 1.0s cast, range 10 width 10 rect
    LongRangeNeedlesFast = 44861, // FlowertenderAmirah->self, 1.0s cast, range 30 width 30 rect
    SpinningNeedles = 44909, // Helper->self, no cast, range 60 60-degree cone
    SpinningNeedlesCCW = 44867, // Boss->self, 5.0s cast, single-target
    SpinningNeedlesCW = 44868, // Boss->self, 5.0s cast, single-target
    SpinningNeedlesBoss1 = 44869, // Boss->self, no cast, single-target
    SpinningNeedlesBoss2 = 44870, // Boss->self, no cast, single-target
    SpinningNeedlesBoss3 = 44871, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    TurnRight = 167, // Boss->self
    TurnLeft = 168, // Boss->self
}

class SeveralThousandNeedles(BossModule module) : Components.StandardAOEs(module, AID.SeveralThousandNeedlesSlow, new AOEShapeRect(10, 5), maxCasts: 6, highlightImminent: true);
class LongRangeNeedles(BossModule module) : Components.StandardAOEs(module, AID.LongRangeNeedlesSlow, new AOEShapeRect(30, 15), maxCasts: 2, highlightImminent: true);

class Spineshot(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpineshotCast)
        {
            _predicted.Add(new(new AOEShapeCone(60, 30.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 0.1f)));
            _predicted.Add(new(new AOEShapeCone(60, 30.Degrees()), spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 0.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Spineshot)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class March(BossModule module) : Components.GenericAOEs(module)
{
    private int _march;
    private DateTime _root;
    private int _direction;

    public class Caster
    {
        public required Actor Actor;
        public WPos Destination;
        public bool Moved;
    }

    private readonly List<Caster> _adds = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var add in _adds.Where(a => a.Moved))
        {
            var radius = add.Actor.OID == (uint)OID.FlowertenderAmirah ? 15 : 5;
            yield return new AOEInstance(new AOEShapeRect(radius, radius, radius), add.Destination, default, _root);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0)
        {
            switch (state)
            {
                case 0x00080004:
                    _direction = 0;
                    break;
                case 0x00200010:
                    _direction = 1;
                    MoveCasters();
                    break;
                case 0x00020001:
                    _direction = -1;
                    MoveCasters();
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OneStoneMarch:
                _march = 1;
                _root = WorldState.FutureTime(14.7f);
                break;
            case AID.TwoStoneMarch:
                _march = 2;
                _root = WorldState.FutureTime(14.7f);
                break;

            case AID.SeveralThousandNeedlesCast:
            case AID.LongRangeNeedlesCast:
                var c = new Caster()
                {
                    Actor = caster,
                    Destination = _direction != 0 ? MoveCaster(caster) : default,
                    Moved = _direction != 0
                };
                _adds.Add(c);
                break;
        }
    }

    private void MoveCasters()
    {
        foreach (var c in _adds)
        {
            if (c.Moved)
                continue;

            c.Destination = MoveCaster(c.Actor);
            c.Moved = true;
        }
    }

    private WPos MoveCaster(Actor actor)
    {
        WDir rotate(WDir d) => _direction > 0 ? d.OrthoL() : d.OrthoR();
        WPos proj;
        if (actor.Position.AlmostEqual(Arena.Center, 10))
        {
            var off = actor.Position - Arena.Center;
            proj = Arena.Center + (_march == 1 ? rotate(off) : -off);
        }
        else
        {
            var ang = MathF.Round(((actor.Position - Arena.Center).ToAngle() + (10 * _direction).Degrees()).Normalized().Deg / 90f);
            var dir = rotate((ang * MathF.PI / 2).Radians().ToDirection()) * 10;
            proj = actor.Position + dir;
            if (_march == 2)
            {
                if (Arena.InBounds(proj + dir))
                    proj += dir;
                else
                    proj += rotate(dir);
            }
        }

        return proj;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LongRangeNeedlesFast or AID.SeveralThousandNeedlesFast)
        {
            NumCasts++;
            if (_adds.Count > 0)
            {
                _adds.RemoveAt(0);
                if (_adds.Count == 0)
                {
                    _march = 0;
                    _root = default;
                }
            }
        }
    }
}

class SpinningNeedles(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SpinningNeedlesCW:
                Predict(Module.CastFinishAt(spell, 0.1f), -1);
                break;
            case AID.SpinningNeedlesCCW:
                Predict(Module.CastFinishAt(spell, 0.1f), 1);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpinningNeedles)
        {
            NumCasts++;
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }

    private void Predict(DateTime start, int dir)
    {
        Sequence seq = new(new AOEShapeCone(60, 30.Degrees()), Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo!.Rotation, (60 * dir).Degrees(), start, 1.1f, 10);
        Sequences.Add(seq);
        seq.Rotation += 180.Degrees();
        Sequences.Add(seq);
    }
}

class D60AncestralMaliktenderStates : StateMachineBuilder
{
    public D60AncestralMaliktenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spineshot>()
            .ActivateOnEnter<SeveralThousandNeedles>()
            .ActivateOnEnter<LongRangeNeedles>()
            .ActivateOnEnter<March>()
            .ActivateOnEnter<SpinningNeedles>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1037, NameID = 14097)]
public class D60AncestralMaliktender(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsSquare(20));

