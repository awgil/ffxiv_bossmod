namespace BossMod.Endwalker.Hunt.RankS.Armstrong;

public enum OID : uint
{
    Boss = 0x35BE, // R7.800, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    RotateCW = 27470, // Boss->self, no cast, single-target
    RotateCCW = 27471, // Boss->self, no cast, single-target
    MagitekCompressorFirst = 27472, // Boss->self, 6.0s cast, range 50 width 7 cross
    MagitekCompressorReverse = 27473, // Boss->self, 2.0s cast, range 50 width 7 cross
    MagitekCompressorNext = 27474, // Boss->self, 0.5s cast, range 50 width 7 cross
    AcceleratedLanding = 27475, // Boss->location, 4.0s cast, range 6 circle
    CalculatedCombustion = 27476, // Boss->self, 5.0s cast, range 35 circle
    Pummel = 27477, // Boss->player, 5.0s cast, single-target
    SoporificGas = 27478, // Boss->self, 6.0s cast, range 12 circle
}

class MagitekCompressor(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;

    private static readonly AOEShapeCross _shape = new(50, 3.5f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MagitekCompressorFirst)
        {
            NumCasts = 0;
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, _increment, Module.CastFinishAt(spell), 2.1f, 10));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RotateCW:
                _increment = -30.Degrees();
                break;
            case AID.RotateCCW:
                _increment = 30.Degrees();
                break;
            case AID.MagitekCompressorFirst:
            case AID.MagitekCompressorReverse:
            case AID.MagitekCompressorNext:
                if (Sequences.Count > 0)
                {
                    AdvanceSequence(0, WorldState.CurrentTime);
                    if (NumCasts == 5)
                    {
                        ref var s = ref Sequences.Ref(0);
                        s.Increment = -s.Increment;
                        s.NextActivation = WorldState.FutureTime(3.6f);
                    }
                }
                break;
        }
    }
}

class AcceleratedLanding(BossModule module) : Components.StandardAOEs(module, AID.AcceleratedLanding, 6);
class CalculatedCombustion(BossModule module) : Components.RaidwideCast(module, AID.CalculatedCombustion);
class Pummel(BossModule module) : Components.SingleTargetCast(module, AID.Pummel);
class SoporificGas(BossModule module) : Components.StandardAOEs(module, AID.SoporificGas, new AOEShapeCircle(12));

class ArmstrongStates : StateMachineBuilder
{
    public ArmstrongStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekCompressor>()
            .ActivateOnEnter<AcceleratedLanding>()
            .ActivateOnEnter<CalculatedCombustion>()
            .ActivateOnEnter<Pummel>()
            .ActivateOnEnter<SoporificGas>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 10619)]
public class Armstrong(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
