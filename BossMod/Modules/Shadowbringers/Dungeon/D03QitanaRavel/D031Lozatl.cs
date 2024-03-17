// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D031Lozatl
{
    public enum OID : uint
    {
        Boss = 0x27AF, //R=4.4
        Helper = 0x233C, //R=0.5
    }

    public enum AID : uint
    {
        AutoAttack = 872, // 27AF->player, no cast, single-target
        Stonefist = 15497, // 27AF->player, 4,0s cast, single-target
        SunToss = 15498, // 27AF->location, 3,0s cast, range 5 circle
        LozatlsScorn = 15499, // 27AF->self, 3,0s cast, range 40 circle
        RonkanLightRight = 15500, // 233C->self, no cast, range 60 width 20 rect
        RonkanLightLeft = 15725, // 233C->self, no cast, range 60 width 20 rect
        HeatUp = 15502, // 27AF->self, 3,0s cast, single-target
        HeatUp2 = 15501, // 27AF->self, 3,0s cast, single-target
        LozatlsFuryA = 15504, // 27AF->self, 4,0s cast, range 60 width 20 rect
        LozatlsFuryB = 15503, // 27AF->self, 4,0s cast, range 60 width 20 rect
    };

    class LozatlsFuryA : Components.SelfTargetedAOEs
    {
        public LozatlsFuryA() : base(ActionID.MakeSpell(AID.LozatlsFuryA), new AOEShapeRect(60, 20, directionOffset: 90.Degrees())) { } // TODO: verify; there should not be an offset in reality here..., also double halfwidth is strange
    }

    class LozatlsFuryB : Components.SelfTargetedAOEs
    {
        public LozatlsFuryB() : base(ActionID.MakeSpell(AID.LozatlsFuryB), new AOEShapeRect(60, 20, directionOffset: -90.Degrees())) { } // TODO: verify; there should not be an offset in reality here..., also double halfwidth is strange
    }

    class Stonefist : Components.SingleTargetCast
    {
        public Stonefist() : base(ActionID.MakeSpell(AID.Stonefist)) { }
    }

    class LozatlsScorn : Components.RaidwideCast
    {
        public LozatlsScorn() : base(ActionID.MakeSpell(AID.LozatlsScorn)) { }
    }

    class SunToss : Components.LocationTargetedAOEs
    {
        public SunToss() : base(ActionID.MakeSpell(AID.SunToss), 5) { }
    }

    class RonkanLight : Components.GenericAOEs
    {
        private bool castingRight;
        private bool castingLeft;
        private static readonly AOEShapeRect rect = new(60, 20); //TODO: double halfwidth is strange
        private DateTime _activation;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (castingRight)
                yield return new(rect, module.Bounds.Center, 90.Degrees(), _activation);
            if (castingLeft)
                yield return new(rect, module.Bounds.Center, -90.Degrees(), _activation);
        }

        public override void OnActorEAnim(BossModule module, Actor actor, uint state)
        {
            if (state == 0x00040008)
            {
                if (actor.Position.AlmostEqual(new(8, 328), 1))
                    castingRight = true;
                if (actor.Position.AlmostEqual(new(-7, 328), 1))
                    castingLeft = true;
                _activation = module.WorldState.CurrentTime.AddSeconds(8);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.RonkanLightLeft or AID.RonkanLightRight)
            {
                castingRight = false;
                castingLeft = false;
            }
        }
    }

    class D031LozatlStates : StateMachineBuilder
    {
        public D031LozatlStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<LozatlsFuryA>()
                .ActivateOnEnter<LozatlsFuryB>()
                .ActivateOnEnter<Stonefist>()
                .ActivateOnEnter<SunToss>()
                .ActivateOnEnter<RonkanLight>()
                .ActivateOnEnter<LozatlsScorn>();
        }
    }

    [ModuleInfo(CFCID = 651, NameID = 8231)]
    public class D031Lozatl : BossModule
    {
        public D031Lozatl(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 315), 20)) { }
    }
}
