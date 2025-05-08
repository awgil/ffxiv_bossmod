namespace BossMod.Dawntrail.Hunt.RankS.Neyoozoteel;

public enum OID : uint
{
    Boss = 0x4233, // R6.500, x1
}

// 7.11+
// RLB -> 37370 > 37371 > 42173
// LBR -> 37396 > 37394 > 37395
// LBL -> 37396 > 37394 > 42174
public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    NoxiousSap = 37308, // Boss->self, 5.0s cast, range 30 120-degree cone
    WhirlingOmenLBR = 37376, // Boss->self, 3.0s cast, single-target, visual (apply omens: left-back-right)
    WhirlingOmenBRR = 37377, // Boss->self, 3.0s cast, single-target, visual (apply omens: back-right-right)
    WhirlingOmenRLB = 37378, // Boss->self, 3.0s cast, single-target, visual (apply omens: right-left-back)
    WhirlingOmenLBL = 37379, // Boss->self, 3.0s cast, single-target, visual (apply omens: left-back-left)
    SapSpiller = 37397, // Boss->self, 12.0s cast, single-target, visual (consume omens, cast instant saps)
    NoxiousSapR1 = 37370, // Boss->self, no cast, range 30 120-degree cone (consume first right?)
    NoxiousSapL2 = 37371, // Boss->self, no cast, range 30 120-degree cone (consume second left?)
    NoxiousSapB2 = 37394, // Boss->self, no cast, range 30 120-degree cone (consume second rear?)
    NoxiousSapR3 = 37395, // Boss->self, no cast, range 30 120-degree cone (consume third right?)
    NoxiousSapL1 = 37396, // Boss->self, no cast, range 30 120-degree cone (consume first left?)
    NoxiousSapR2 = 42172, // Boss->self, no cast, range 30 120-degree cone (???)
    NoxiousSapB3 = 42173, // Boss->self, no cast, range 30 120-degree cone (consume third back?)
    NoxiousSapL3 = 42174, // Boss->self, no cast, range 30 120-degree cone (consume third left?)
    Neurotoxify = 38331, // Boss->self, 5.0s cast, range 40 circle, raidwide + apply delayed stun
    Cocopult = 37307, // Boss->players, 5.0s cast, range 5 circle stack
    RavagingRootsCW = 37373, // Boss->self, 5.0s cast, range 30 width 6 cross
    RavagingRootsCCW = 37374, // Boss->self, 5.0s cast, range 30 width 6 cross
    RavagingRootsAOE = 37375, // Boss->self, no cast, range 30 width 6 cross
}

public enum SID : uint
{
    DelayedNeurotoxicity = 3940, // Boss->player, extra=0x0
    DownForTheCount = 3908, // Boss->player, extra=0xEC7
    Heavy = 2391, // Boss->player, extra=0x3C
}

public enum IconID : uint
{
    Cocopult = 161, // player
    RotateCW = 167, // Boss
    RotateCCW = 168, // Boss
}

class NoxiousSap(BossModule module) : Components.StandardAOEs(module, AID.NoxiousSap, new AOEShapeCone(30, 60.Degrees()));

class SapSpiller(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Angle> _futureRotations = [];
    private AOEInstance? _nextAOE;

    private static readonly AOEShapeCone _shape = new(30, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_nextAOE != null)
        {
            var futureDir = _nextAOE.Value.Rotation;
            var futureActivation = _nextAOE.Value.Activation;
            foreach (var offset in _futureRotations)
            {
                futureDir += offset;
                futureActivation = futureActivation.AddSeconds(2.2f);
                yield return new(_shape, Module.PrimaryActor.Position, futureDir, futureActivation);
            }
            yield return _nextAOE.Value;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WhirlingOmenLBR:
                _futureRotations.AddRange([90.Degrees(), 180.Degrees(), -90.Degrees()]);
                break;
            case AID.WhirlingOmenBRR:
                _futureRotations.AddRange([180.Degrees(), -90.Degrees(), -90.Degrees()]);
                break;
            case AID.WhirlingOmenRLB:
                _futureRotations.AddRange([-90.Degrees(), 90.Degrees(), 180.Degrees()]);
                break;
            case AID.WhirlingOmenLBL:
                _futureRotations.AddRange([90.Degrees(), 180.Degrees(), 90.Degrees()]);
                break;
            case AID.SapSpiller:
                if (_futureRotations.Count > 0)
                {
                    _nextAOE = new(_shape, caster.Position, spell.Rotation + _futureRotations[0], Module.CastFinishAt(spell, 0.6f), ArenaColor.Danger);
                    _futureRotations.RemoveAt(0);
                }
                else
                {
                    ReportError("Unexpected sap spiller without omens");
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NoxiousSapR1 or AID.NoxiousSapR2 or AID.NoxiousSapR3 or AID.NoxiousSapL1 or AID.NoxiousSapL2 or AID.NoxiousSapL3 or AID.NoxiousSapB2 or AID.NoxiousSapB3 && _nextAOE != null)
        {
            if (!_nextAOE.Value.Rotation.AlmostEqual(spell.Rotation, 0.1f))
                ReportError($"Unexpected rotation: got {spell.Rotation}, expected {_nextAOE.Value.Rotation}");
            if (_futureRotations.Count > 0)
            {
                _nextAOE = _nextAOE.Value with { Rotation = _nextAOE.Value.Rotation + _futureRotations[0], Activation = WorldState.FutureTime(2.2f) };
                _futureRotations.RemoveAt(0);
            }
            else
            {
                _nextAOE = null;
            }
        }
    }
}

class Neurotoxify(BossModule module) : Components.RaidwideCast(module, AID.Neurotoxify, "Raidwide + delayed stun");
class Cocopult(BossModule module) : Components.StackWithCastTargets(module, AID.Cocopult, 5, 4);

class RavagingRoots(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCross _shape = new(30, 3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = (AID)spell.Action.ID switch
        {
            AID.RavagingRootsCW => -45.Degrees(),
            AID.RavagingRootsCCW => 45.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, increment, Module.CastFinishAt(spell), 2.5f, 8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0 && (AID)spell.Action.ID is AID.RavagingRootsCW or AID.RavagingRootsCCW or AID.RavagingRootsAOE)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}

class NeyoozoteelStates : StateMachineBuilder
{
    public NeyoozoteelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NoxiousSap>()
            .ActivateOnEnter<SapSpiller>()
            .ActivateOnEnter<Neurotoxify>()
            .ActivateOnEnter<Cocopult>()
            .ActivateOnEnter<RavagingRoots>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 12754)]
public class Neyoozoteel(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
