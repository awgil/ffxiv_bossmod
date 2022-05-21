using System;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2AscalonMercy : CommonComponents.SelfTargetedAOE
    {
        public P2AscalonMercy() : base(ActionID.MakeSpell(AID.AscalonsMercyConcealedAOE), new AOEShapeCone(50, MathF.PI / 12)) { }
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

        protected override void DrawArenaForegroundPre(int pcSlot, Actor pc)
        {
            //Arena.Actor(Enemies(OID.SerJanlenoux).FirstOrDefault(), 0xffffffff);
            //Arena.Actor(Enemies(OID.SerVellguine).FirstOrDefault(), 0xff0000ff);
            //Arena.Actor(Enemies(OID.SerPaulecrain).FirstOrDefault(), 0xff00ff00);
            //Arena.Actor(Enemies(OID.SerIgnasse).FirstOrDefault(), 0xffff0000);
            //Arena.Actor(Enemies(OID.SerHermenost).FirstOrDefault(), 0xff00ffff);
            //Arena.Actor(Enemies(OID.SerGuerrique).FirstOrDefault(), 0xffff00ff);
            //Arena.Actor(Enemies(OID.SerHaumeric).FirstOrDefault(), 0xffffff00);
            //Arena.Actor(Enemies(OID.SerNoudenet).FirstOrDefault(), 0xffffff80);
            //Arena.Actor(Enemies(OID.SerZephirin).FirstOrDefault(), 0xff8080ff);
            //Arena.Actor(Enemies(OID.SerAdelphel).FirstOrDefault(), 0xff80ff80);
            //Arena.Actor(Enemies(OID.SerGrinnaux).FirstOrDefault(), 0xffff8080);
            //Arena.Actor(Enemies(OID.SerCharibert).FirstOrDefault(), 0xff80ffff);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            //Arena.AddLine(PrimaryActor.Position, PrimaryActor.Position + GeometryUtils.DirectionToVec3(PrimaryActor.Rotation) * 5, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
