using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    class SavageBarbery : Components.GenericAOEs
    {
        private List<(Actor Caster, AOEShape Shape)> _casts = new();
        public int NumActiveCasts => _casts.Count;

        public SavageBarbery() : base(new()) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _casts.Select(cs => (cs.Shape, cs.Caster.Position, cs.Caster.CastInfo!.Rotation, cs.Caster.CastInfo.FinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.SavageBarberyDonutAOE => new AOEShapeDonut(6, 20),
                AID.SavageBarberyRectAOE => new AOEShapeRect(20, 6, 20),
                AID.SavageBarberyDonutSword or AID.SavageBarberyRectSword => new AOEShapeCircle(20),
                _ => null
            };
            if (shape != null)
                _casts.Add((caster, shape));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.SavageBarberyDonutAOE or AID.SavageBarberyRectAOE or AID.SavageBarberyDonutSword or AID.SavageBarberyRectSword)
                _casts.RemoveAll(cs => cs.Caster == caster);
        }
    }

    class HairRaid : Components.GenericAOEs
    {
        private List<(Actor Caster, AOEShape Shape)> _casts = new();
        public int NumActiveCasts => _casts.Count;

        public HairRaid() : base(new()) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _casts.Select(cs => (cs.Shape, cs.Caster.Position, cs.Caster.CastInfo!.Rotation, cs.Caster.CastInfo.FinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.HairRaidConeAOE => new AOEShapeCone(40, 60.Degrees()), // TODO: verify angle
                AID.HairRaidDonutAOE => new AOEShapeDonut(6, 20),
                _ => null
            };
            if (shape != null)
                _casts.Add((caster, shape));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.HairRaidConeAOE or AID.HairRaidDonutAOE)
                _casts.RemoveAll(cs => cs.Caster == caster);
        }
    }

    class HairSprayDeadlyTwist : Components.CastStackSpread
    {
        public HairSprayDeadlyTwist() : base(ActionID.MakeSpell(AID.DeadlyTwist), ActionID.MakeSpell(AID.HairSpray), 6, 5, 4) { }
    }
}
