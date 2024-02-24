using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.HuntA.MoussePrincess
{
    public enum OID : uint
    {
        Boss = 0x360B, // R6.000, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        PrincessThrenodyPrepare = 27318, // Boss->self, 4.0s cast, range 40 120-degree cone
        PrincessThrenodyResolve = 27319, // Boss->self, 1.0s cast, range 40 120-degree cone
        WhimsyAlaMode = 27320, // Boss->self, 4.0s cast, single-target
        AmorphicFlail = 27321, // Boss->self, 5.0s cast, range 9 circle
        PrincessCacophony = 27322, // Boss->location, 5.0s cast, range 12 circle
        Banish = 27323, // Boss->player, 5.0s cast, single-target
        RemoveWhimsy = 27634, // Boss->self, no cast, single-target, removes whimsy debuffs
    };

    public enum SID : uint
    {
        RightwardWhimsy = 2840,
        LeftwardWhimsy = 2841,
        BackwardWhimsy = 2842,
        ForwardWhimsy = 2958,
    }

    class PrincessThrenody : Components.GenericAOEs
    {
        private Angle _direction;
        private Angle _offset;
        private bool casting;
        private DateTime _activation;
        private static readonly AOEShapeCone _shape = new(40, 60.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (casting)
                yield return new(_shape, module.PrimaryActor.Position, _direction + _offset, _activation);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.PrincessThrenodyPrepare)
            {
                foreach (var s in module.PrimaryActor.Statuses)
                {
                    if ((SID)s.ID == SID.RightwardWhimsy)
                        _offset = -90.Degrees();
                    if ((SID)s.ID == SID.LeftwardWhimsy)
                        _offset = 90.Degrees();
                    if ((SID)s.ID == SID.BackwardWhimsy)
                        _offset = 180.Degrees();
                    if ((SID)s.ID == SID.ForwardWhimsy)
                        _offset = 0.Degrees();                    
                }
                casting = true;
                _direction = spell.Rotation;
                _activation = module.WorldState.CurrentTime.AddSeconds(6); //times observed between 6.07s and 6.2s
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {          
            if ((AID)spell.Action.ID == AID.PrincessThrenodyResolve)
                casting = false;
        }
    }

    class WhimsyAlaMode : Components.CastHint
    {
        public WhimsyAlaMode() : base(ActionID.MakeSpell(AID.WhimsyAlaMode), "Select direction") { }
    }

    class AmorphicFlail : Components.SelfTargetedAOEs
    {
        public AmorphicFlail() : base(ActionID.MakeSpell(AID.AmorphicFlail), new AOEShapeCircle(9)) { }
    }

    class PrincessCacophony : Components.LocationTargetedAOEs
    {
        public PrincessCacophony() : base(ActionID.MakeSpell(AID.PrincessCacophony), 12) { }
    }

    class Banish : Components.SingleTargetCast
    {
        public Banish() : base(ActionID.MakeSpell(AID.Banish)) { }
    }

    class MoussePrincessStates : StateMachineBuilder
    {
        public MoussePrincessStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<PrincessThrenody>()
                .ActivateOnEnter<WhimsyAlaMode>()
                .ActivateOnEnter<AmorphicFlail>()
                .ActivateOnEnter<PrincessCacophony>()
                .ActivateOnEnter<Banish>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 204)]
    public class MoussePrincess : SimpleBossModule
    {
        public MoussePrincess(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
