using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6StygimolochLord
{
    class ThunderousDischarge : Components.CastCounter
    {
        public ThunderousDischarge() : base(ActionID.MakeSpell(AID.ThunderousDischargeAOE)) { }
    }

    class ThousandTonzeSwing : Components.SelfTargetedAOEs
    {
        public ThousandTonzeSwing() : base(ActionID.MakeSpell(AID.ThousandTonzeSwing), new AOEShapeCircle(20)) { }
    }

    class Whack : Components.SelfTargetedAOEs
    {
        public Whack() : base(ActionID.MakeSpell(AID.WhackAOE), new AOEShapeCone(40, 30.Degrees())) { }
    }

    class DevastatingBoltOuter : Components.SelfTargetedAOEs
    {
        public DevastatingBoltOuter() : base(ActionID.MakeSpell(AID.DevastatingBoltOuter), new AOEShapeDonut(25, 30)) { }
    }

    class DevastatingBoltInner : Components.SelfTargetedAOEs
    {
        public DevastatingBoltInner() : base(ActionID.MakeSpell(AID.DevastatingBoltInner), new AOEShapeDonut(12, 17)) { }
    }

    class Electrocution : Components.LocationTargetedAOEs
    {
        public Electrocution() : base(ActionID.MakeSpell(AID.Electrocution), 3) { }
    }

    // TODO: ManaFlame component - show reflect hints
    public class DRS6 : BossModule
    {
        private List<Actor> _monks;
        private List<Actor> _ballsEarth;
        private List<Actor> _ballsFire;

        public DRS6(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-416, -184), 40))
        {
            _monks = Enemies(OID.StygimolochMonk);
            _ballsEarth = Enemies(OID.BallOfEarth);
            _ballsFire = Enemies(OID.BallOfFire);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            base.DrawEnemies(pcSlot, pc);
            Arena.Actors(_monks, ArenaColor.Enemy);
            Arena.Actors(_ballsEarth, ArenaColor.Object);
            Arena.Actors(_ballsFire, ArenaColor.Object);
        }
    }
}
