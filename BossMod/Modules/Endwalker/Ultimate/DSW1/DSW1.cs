using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    class EmptyDimension : CommonComponents.SelfTargetedAOE
    {
        public EmptyDimension() : base(ActionID.MakeSpell(AID.EmptyDimension), new AOEShapeDonut(6, 70)) {}
    }

    class FullDimension : CommonComponents.SelfTargetedAOE
    {
        public FullDimension() : base(ActionID.MakeSpell(AID.FullDimension), new AOEShapeCircle(6)) { }
    }

    class HoliestHallowing : CommonComponents.Interruptible
    {
        public HoliestHallowing() : base(ActionID.MakeSpell(AID.HoliestHallowing)) { }
    }

    public class DSW1 : BossModule
    {
        private List<Actor> GrinnauxList;
        private List<Actor> CharibertList;
        public Actor? SerAdelphel() => PrimaryActor;
        public Actor? SerGrinnaux() => GrinnauxList.FirstOrDefault();
        public Actor? SerCharibert() => CharibertList.FirstOrDefault();

        public DSW1(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            GrinnauxList = Enemies(OID.SerGrinnaux);
            CharibertList = Enemies(OID.SerCharibert);
            Arena.WorldHalfSize = 22;
            InitStates(new DSW1States(this).Initial);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(SerAdelphel(), Arena.ColorEnemy);
            Arena.Actor(SerGrinnaux(), Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
