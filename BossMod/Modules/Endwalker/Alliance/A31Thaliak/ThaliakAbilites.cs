using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A31Thaliak
{   
    class Thlipsis : Components.StackWithCastTargets
    {
        public Thlipsis() : base(ActionID.MakeSpell(AID.ThlipsisHelper), 6) { }
    }
    class Hydroptosis : Components.SpreadFromCastTargets
    {
        public Hydroptosis() : base(ActionID.MakeSpell(AID.HydroptosisHelper), 6) { }
    }
    
    //
    class Rhyton : Components.GenericBaitAway
    {
        private static AOEShapeRect _shape = new(70, 3);

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.RhytonBuster)
                CurrentBaits.Add(new(module.PrimaryActor, actor, _shape));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.RhytonHelper)
            {
                ++NumCasts;
                CurrentBaits.Clear();
            }
        }
    } 
    class LeftBank : Components.SelfTargetedAOEs
    {
        public LeftBank() : base(ActionID.MakeSpell(AID.LeftBank), new AOEShapeCone(60, 90.Degrees())) { }
    }
    class LeftBank2 : Components.SelfTargetedAOEs
    {
        public LeftBank2() : base(ActionID.MakeSpell(AID.LeftBank2), new AOEShapeCone(60, 90.Degrees())) { }
    }
    class RightBank : Components.SelfTargetedAOEs
    {
        public RightBank() : base(ActionID.MakeSpell(AID.RightBank), new AOEShapeCone(60, 90.Degrees())) { }
    }
    class RightBank2 : Components.SelfTargetedAOEs
    {
        public RightBank2() : base(ActionID.MakeSpell(AID.RightBank2), new AOEShapeCone(60, 90.Degrees())) { }
    }
    //
    
    class RheognosisKnockback : KnockbackFromCastTarget
    {
        public RheognosisKnockback() : base(ActionID.MakeSpell(AID.RheognosisKnockback), 25f, kind: Kind.DirForward) { }
    }
}
