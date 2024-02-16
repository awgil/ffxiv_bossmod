using System;
using System.Collections.Generic;
using System.Dynamic;

namespace BossMod.Endwalker.HuntS.Armstrong
{
    public enum OID : uint
    {
        Boss = 0x35BE, // R7.800, x1
    };

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
    };

    class MagitekCompressor : Components.GenericRotatingAOE
    {
        private Angle _starting;
        private Angle _increment;
        private static AOEShapeCross _shape = new(50, 3.5f);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {

            foreach (var s in Sequences)
            {
                if (s.NumRemainingCasts > 1)
                {
                    var rot = s.Rotation;
                    var time = s.NextActivation > module.WorldState.CurrentTime ? s.NextActivation : module.WorldState.CurrentTime;
                    
                    if (s.NumRemainingCasts > 5)
                        rot += s.Increment;
                    if (s.NumRemainingCasts == 5)
                    {
                        rot -= s.Increment;
                        time = time.AddSeconds(3.6f);
                    }
                    if (s.NumRemainingCasts < 5)
                        rot -= s.Increment;
                    if (s.NumRemainingCasts != 5)
                        time = time.AddSeconds(s.SecondsBetweenActivations);                
                        yield return new(s.Shape, s.Origin, rot, time, FutureColor);
                }          
                if (s.NumRemainingCasts > 0)
                    yield return new(s.Shape, s.Origin, s.Rotation, s.NextActivation, ImminentColor);
            }
        }
        public void AdvanceSequenceAltered(int index, DateTime currentTime, bool removeWhenFinished = true)
        {
            ++NumCasts;

            ref var s = ref Sequences.AsSpan()[index];
            if (--s.NumRemainingCasts <= 0 && removeWhenFinished)
            {
                Sequences.RemoveAt(index);
            }
            else
            {
                if(s.NumRemainingCasts > 5)
                {
                   s.Rotation += s.Increment;
                   s.NextActivation = currentTime.AddSeconds(s.SecondsBetweenActivations);
                }
                if(s.NumRemainingCasts == 5)
                {
                   s.Rotation += s.Increment;
                   s.NextActivation = currentTime.AddSeconds(3.6f);
                }
                if(s.NumRemainingCasts < 5)
                {
                   s.Rotation -= s.Increment;
                   s.NextActivation = currentTime.AddSeconds(s.SecondsBetweenActivations);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.MagitekCompressorFirst)
            {
                _starting = spell.Rotation;
                Sequences.Add(new(_shape, caster.Position, _starting, _increment, spell.FinishAt, 2.1f, 10));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (Sequences.Count == 0)
                switch ((AID)spell.Action.ID)
                {
                    case AID.RotateCW:
                        _increment = -30.Degrees();
                        break;
                    case AID.RotateCCW:
                        _increment = 30.Degrees();
                        break;
                }
            if (Sequences.Count > 0)
                switch ((AID)spell.Action.ID)
                {
                    case AID.MagitekCompressorFirst:
                    case AID.MagitekCompressorReverse:
                    case AID.MagitekCompressorNext:
                        AdvanceSequenceAltered(0, module.WorldState.CurrentTime);
                        break;
                }
        }
    }

    class AcceleratedLanding : Components.LocationTargetedAOEs
    {
        public AcceleratedLanding() : base(ActionID.MakeSpell(AID.AcceleratedLanding), 6) { }
    }

    class CalculatedCombustion : Components.RaidwideCast
    {
        public CalculatedCombustion() : base(ActionID.MakeSpell(AID.CalculatedCombustion)) { }
    }

    class Pummel : Components.SingleTargetCast
    {
        public Pummel() : base(ActionID.MakeSpell(AID.Pummel)) { }
    }

    class SoporificGas : Components.SelfTargetedAOEs
    {
        public SoporificGas() : base(ActionID.MakeSpell(AID.SoporificGas), new AOEShapeCircle(12)) { }
    }

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
    [ModuleInfo(NotoriousMonsterID = 196)]
    public class Armstrong : SimpleBossModule
    {
        public Armstrong(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
