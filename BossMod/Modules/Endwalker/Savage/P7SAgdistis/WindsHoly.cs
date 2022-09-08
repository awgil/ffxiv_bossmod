using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class WindsHoly : Components.StackSpread
    {
        public int NumCasts { get; private set; }
        private BitMask[] _futureStacks = new BitMask[4];
        private BitMask[] _futureSpreads = new BitMask[4];

        public WindsHoly() : base(6, 7, 4) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.InviolateWinds1:
                case SID.PurgatoryWinds1:
                    SpreadMask.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.InviolateWinds2:
                case SID.PurgatoryWinds2:
                    _futureSpreads[0].Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.PurgatoryWinds3:
                    _futureSpreads[1].Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.PurgatoryWinds4:
                    _futureSpreads[2].Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.HolyBonds1:
                case SID.HolyPurgation1:
                    StackMask.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.HolyBonds2:
                case SID.HolyPurgation2:
                    _futureStacks[0].Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.HolyPurgation3:
                    _futureStacks[1].Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.HolyPurgation4:
                    _futureStacks[2].Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.HemitheosHolyExpire)
            {
                StackMask = _futureStacks[NumCasts];
                SpreadMask = _futureSpreads[NumCasts];
                ++NumCasts;
            }
        }
    }
}
