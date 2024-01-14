using BossMod.Components;
using System.Linq;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

    class Enaero : CastHint
    {
        private int EnaeroBuff;
        private int casting;
        public Enaero() : base(ActionID.MakeSpell(AID.RagingWinds1), "") { }
        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var boss = module.Enemies(OID.Boss).FirstOrDefault();  
            if (actor == boss)
            {if ((SID)status.ID == SID.Enaero)
                {
                    EnaeroBuff = 1;
                }
            }
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.RagingWinds1)
                casting = 1;
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.RagingWinds1)
                casting = 0;
        }
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            var boss = module.Enemies(OID.Boss).FirstOrDefault();  
            if (actor == boss)
            {
                if ((SID)status.ID == SID.Enaero)
                EnaeroBuff = 0;
            }
        }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (casting > 0)
            hints.Add("Applies Enaero to Lyon. Use Dispell to remove it");    
            if (EnaeroBuff == 1)
            hints.Add("Enaero on Lyon. Use Dispell to remove it! You only need to do this once per duel, so you can switch to a different action after removing his buff.");
        }
    }
class HeartOfNatureConcentric : ConcentricAOEs
    {
        private static AOEShape[] _shapes = {new AOEShapeCircle(10), new AOEShapeDonut(10,20), new AOEShapeDonut(20,30)};

        public HeartOfNatureConcentric() : base(_shapes) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.NaturesPulse1)
                AddSequence(caster.Position, spell.FinishAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.NaturesPulse1 => 0,
                AID.NaturesPulse2 => 1,
                AID.NaturesPulse3 => 2,
                _ => -1
            };
            AdvanceSequence(order, caster.Position);
        }
    }
