using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A35Eulogia
{   
        class SunbeamSelf : Components.BaitAwayCast
    {
        public SunbeamSelf() : base(ActionID.MakeSpell(AID.SunbeamTankBuster), new AOEShapeCircle(6), true) { }
    }
        class DestructiveBoltStack : Components.UniformStackSpread
    {
        public DestructiveBoltStack() : base(6,0) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Stackmarker)
                AddStack(actor, module.WorldState.CurrentTime.AddSeconds(6.9f));
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.DestructiveBoltStack)
                Stacks.Clear();
        }
    }

    [ModuleInfo(CFCID = 962, PrimaryActorOID = 0x4086)]
    public class A35Eulogia : BossModule
    {
        public A35Eulogia(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(944.976f, -945.006f), 30f)) { }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.Helper))
                Arena.Actor(s, ArenaColor.Object, true);
        }
    }  
}
