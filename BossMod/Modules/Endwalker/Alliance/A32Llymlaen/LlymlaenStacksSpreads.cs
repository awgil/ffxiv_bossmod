using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A32Llymlaen
{   
   class DeepDiveStack : Components.UniformStackSpread
    {
        public DeepDiveStack() : base(6,0) { }
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.DiveStack)
                AddStack(actor, module.WorldState.CurrentTime.AddSeconds(9f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.DeepDiveStack1 or AID.DeepDiveStack2)
                Stacks.Clear();
        }
    }
    class HardWaterStack : Components.UniformStackSpread
    {
        public HardWaterStack() : base(6,0) { }
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.HardWaterStack)
                AddStack(actor, module.WorldState.CurrentTime.AddSeconds(7f));
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HardWaterStack1 or AID.HardWaterStack2)
                Stacks.Clear();
        }
    }
    class StormwindsSpread : Components.SpreadFromCastTargets
    {
        public StormwindsSpread() : base(ActionID.MakeSpell(AID.StormwindsSpread), 6) { }
    }   
}
