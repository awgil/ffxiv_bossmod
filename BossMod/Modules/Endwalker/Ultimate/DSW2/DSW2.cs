using System;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2AscalonMercy : CommonComponents.SelfTargetedAOE
    {
        public P2AscalonMercy() : base(ActionID.MakeSpell(AID.AscalonsMercyConcealedAOE), new AOEShapeCone(50, MathF.PI / 12)) { }
    }

    // TODO: improve component to show aoe...
    class P2AscalonMight : CommonComponents.CastCounter
    {
        public P2AscalonMight() : base(ActionID.MakeSpell(AID.AscalonsMight)) { }
    }

    public class DSW2 : BossModule
    {
        public DSW2(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            Arena.WorldHalfSize = 22;
            Arena.IsCircle = true;
            InitStates(new DSW2States(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
