using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.HuntS.Burfurlur
{
    public enum OID : uint
    {
        Boss = 0x360A, // R6.000, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Trolling = 27316, // Boss->self, no cast, range 10 circle
        QuintupleInhale1 = 27307, // Boss->self, 4.0s cast, range 27 45-degree cone
        QuintupleInhale24 = 27308, // Boss->self, 0.5s cast, range 27 45-degree cone
        QuintupleSneeze1 = 27309, // Boss->self, 5.0s cast, range 40 45-degree cone
        QuintupleSneeze24 = 27310, // Boss->self, 0.5s cast, range 40 45-degree cone
        QuintupleInhale35 = 27692, // Boss->self, 0.5s cast, range 27 45-degree cone
        QuintupleSneeze35 = 27693, // Boss->self, 0.5s cast, range 40 45-degree cone
        Uppercut = 27314, // Boss->self, 3.0s cast, range 15 120-degree cone
        RottenSpores = 27313, // Boss->location, 3.0s cast, range 6 circle
    };

    class QuintupleSneeze : Components.GenericRotatingAOE
    {
        private Angle _referenceAngle;
        private List<Angle> _pendingOffsets = new();
        private static AOEShapeCone _shape = new(40, 45.Degrees());
        private bool _sneezing;
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_sneezing)
            {
                if (_pendingOffsets.Count > 0)
                foreach (var s in Sequences)
                {
                    int num = Math.Min(s.NumRemainingCasts, s.MaxShownAOEs);
                    var time = s.NextActivation > module.WorldState.CurrentTime ? s.NextActivation : module.WorldState.CurrentTime;
                    for (int i = 1; i < num; ++i)
                    {
                        time = time.AddSeconds(s.SecondsBetweenActivations);
                        yield return new(s.Shape, s.Origin, _referenceAngle + _pendingOffsets[0], time, ImminentColor);
                    }
                }
                if (_pendingOffsets.Count > 1)
                foreach (var s in Sequences)
                    if (s.NumRemainingCasts > 0)
                        yield return new(s.Shape, s.Origin, _referenceAngle + _pendingOffsets[1], s.NextActivation, FutureColor);
            }
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.QuintupleInhale1:
                    _referenceAngle = spell.Rotation;
                    _pendingOffsets.Clear();
                    _pendingOffsets.Add(new());
                    _sneezing = false;
                    break;
                case AID.QuintupleInhale24:
                case AID.QuintupleInhale35:
                    _pendingOffsets.Add(spell.Rotation - _referenceAngle);
                    break;
                case AID.QuintupleSneeze1:
                    _sneezing = true;
                    Sequences.Add(new(_shape, caster.Position, _referenceAngle, _pendingOffsets[1], spell.FinishAt, 2.2f, 6));
                    _referenceAngle = spell.Rotation;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster == module.PrimaryActor && (AID)spell.Action.ID is AID.QuintupleSneeze1 or AID.QuintupleSneeze24 or AID.QuintupleSneeze35)
            {
                AdvanceSequence(0, module.WorldState.CurrentTime);
                _pendingOffsets.RemoveAt(0);
                _sneezing = _pendingOffsets.Count > 0;
            }
        }
    }

    class Uppercut : Components.SelfTargetedAOEs
    {
        public Uppercut() : base(ActionID.MakeSpell(AID.Uppercut), new AOEShapeCone(15, 60.Degrees())) { }
    }

    class RottenSpores : Components.LocationTargetedAOEs
    {
        public RottenSpores() : base(ActionID.MakeSpell(AID.RottenSpores), 6) { }
    }

    class BurfurlurStates : StateMachineBuilder
    {
        public BurfurlurStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<QuintupleSneeze>()
                .ActivateOnEnter<Uppercut>()
                .ActivateOnEnter<RottenSpores>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 176)]
    public class Burfurlur : SimpleBossModule
    {
        public Burfurlur(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
