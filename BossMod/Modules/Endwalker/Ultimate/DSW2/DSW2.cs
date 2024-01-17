using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2AscalonsMercyConcealed : Components.SelfTargetedAOEs
    {
        public P2AscalonsMercyConcealed() : base(ActionID.MakeSpell(AID.AscalonsMercyConcealedAOE), new AOEShapeCone(50, 15.Degrees())) { }
    }

    class P2AscalonMight : Components.Cleave
    {
        public P2AscalonMight() : base(ActionID.MakeSpell(AID.AscalonsMight), new AOEShapeCone(50, 30.Degrees()), (uint)OID.BossP2) { }
    }

    class P2UltimateEnd : Components.CastCounter
    {
        public P2UltimateEnd() : base(ActionID.MakeSpell(AID.UltimateEndAOE)) { }
    }

    class P3Drachenlance : Components.SelfTargetedAOEs
    {
        public P3Drachenlance() : base(ActionID.MakeSpell(AID.DrachenlanceAOE), new AOEShapeCone(13, 45.Degrees())) { }
    }

    class P3SoulTether : Components.TankbusterTether
    {
        public P3SoulTether() : base(ActionID.MakeSpell(AID.SoulTether), (uint)TetherID.HolyShieldBash, 5) { }
    }

    class P4Resentment : Components.CastCounter
    {
        public P4Resentment() : base(ActionID.MakeSpell(AID.Resentment)) { }
    }

    class P5TwistingDive : Components.SelfTargetedAOEs
    {
        public P5TwistingDive() : base(ActionID.MakeSpell(AID.TwistingDive), new AOEShapeRect(60, 5)) { }
    }

    class P5Cauterize1 : Components.SelfTargetedAOEs
    {
        public P5Cauterize1() : base(ActionID.MakeSpell(AID.Cauterize1), new AOEShapeRect(48, 10)) { }
    }

    class P5Cauterize2 : Components.SelfTargetedAOEs
    {
        public P5Cauterize2() : base(ActionID.MakeSpell(AID.Cauterize2), new AOEShapeRect(48, 10)) { }
    }

    class P5SpearOfTheFury : Components.SelfTargetedAOEs
    {
        public P5SpearOfTheFury() : base(ActionID.MakeSpell(AID.SpearOfTheFuryP5), new AOEShapeRect(50, 5)) { }
    }

    class P5AscalonMight : Components.Cleave
    {
        public P5AscalonMight() : base(ActionID.MakeSpell(AID.AscalonsMight), new AOEShapeCone(50, 30.Degrees()), (uint)OID.BossP5) { }
    }

    class P5Surrender : Components.CastCounter
    {
        public P5Surrender() : base(ActionID.MakeSpell(AID.Surrender)) { }
    }

    class P6SwirlingBlizzard : Components.SelfTargetedAOEs
    {
        public P6SwirlingBlizzard() : base(ActionID.MakeSpell(AID.SwirlingBlizzard), new AOEShapeDonut(20, 35)) { }
    }

    class P7Shockwave : Components.CastCounter
    {
        public P7Shockwave() : base(ActionID.MakeSpell(AID.ShockwaveP7)) { }
    }

    class P7AlternativeEnd : Components.CastCounter
    {
        public P7AlternativeEnd() : base(ActionID.MakeSpell(AID.AlternativeEnd)) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.BossP2, CFCID = 788)]
    public class DSW2 : BossModule
    {
        public static ArenaBoundsCircle BoundsCircle = new ArenaBoundsCircle(new (100, 100), 21); // p2, intermission
        public static ArenaBoundsSquare BoundsSquare = new ArenaBoundsSquare(new (100, 100), 21); // p3, p4

        private Actor? _arenaFeatures;
        private Actor? _bossP3;
        private Actor? _leftEyeP4;
        private Actor? _rightEyeP4;
        private Actor? _nidhoggP4;
        private Actor? _serCharibert;
        private Actor? _spear;
        private Actor? _bossP5;
        private Actor? _nidhoggP6;
        private Actor? _hraesvelgrP6;
        private Actor? _bossP7;
        public Actor? ArenaFeatures => _arenaFeatures;
        public Actor? BossP2() => PrimaryActor;
        public Actor? BossP3() => _bossP3;
        public Actor? LeftEyeP4() => _leftEyeP4;
        public Actor? RightEyeP4() => _rightEyeP4;
        public Actor? NidhoggP4() => _nidhoggP4;
        public Actor? SerCharibert() => _serCharibert;
        public Actor? Spear() => _spear;
        public Actor? BossP5() => _bossP5;
        public Actor? NidhoggP6() => _nidhoggP6;
        public Actor? HraesvelgrP6() => _hraesvelgrP6;
        public Actor? BossP7() => _bossP7;

        public DSW2(WorldState ws, Actor primary) : base(ws, primary, BoundsCircle) { }

        protected override void UpdateModule()
        {
            // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
            // the problem is that on wipe, any actor can be deleted and recreated in the same frame
            _arenaFeatures ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.ArenaFeatures).FirstOrDefault() : null;
            _bossP3 ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.BossP3).FirstOrDefault() : null;
            _leftEyeP4 ??= StateMachine.ActivePhaseIndex == 2 ? Enemies(OID.LeftEye).FirstOrDefault() : null;
            _rightEyeP4 ??= StateMachine.ActivePhaseIndex == 2 ? Enemies(OID.RightEye).FirstOrDefault() : null;
            _nidhoggP4 ??= StateMachine.ActivePhaseIndex == 2 ? Enemies(OID.NidhoggP4).FirstOrDefault() : null;
            _serCharibert ??= StateMachine.ActivePhaseIndex == 3 ? Enemies(OID.SerCharibert).FirstOrDefault() : null;
            _spear ??= StateMachine.ActivePhaseIndex == 3 ? Enemies(OID.SpearOfTheFury).FirstOrDefault() : null;
            _bossP5 ??= StateMachine.ActivePhaseIndex == 4 ? Enemies(OID.BossP5).FirstOrDefault() : null;
            _nidhoggP6 ??= StateMachine.ActivePhaseIndex == 5 ? Enemies(OID.NidhoggP6).FirstOrDefault() : null;
            _hraesvelgrP6 ??= StateMachine.ActivePhaseIndex == 5 ? Enemies(OID.HraesvelgrP6).FirstOrDefault() : null;
            _bossP7 ??= StateMachine.ActivePhaseIndex == 6 ? Enemies(OID.DragonKingThordan).FirstOrDefault() : null;
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            Arena.Actor(_bossP3, ArenaColor.Enemy);
            Arena.Actor(_leftEyeP4, ArenaColor.Enemy);
            Arena.Actor(_rightEyeP4, ArenaColor.Enemy);
            Arena.Actor(_nidhoggP4, ArenaColor.Enemy);
            Arena.Actor(_serCharibert, ArenaColor.Enemy);
            Arena.Actor(_spear, ArenaColor.Enemy);
            Arena.Actor(_bossP5, ArenaColor.Enemy);
            Arena.Actor(_nidhoggP6, ArenaColor.Enemy);
            Arena.Actor(_hraesvelgrP6, ArenaColor.Enemy);
            Arena.Actor(_bossP7, ArenaColor.Enemy);
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
            //Arena.AddLine(PrimaryActor.Position, PrimaryActor.Position + GeometryUtils.DirectionToVec3(PrimaryActor.Rotation) * 5, ArenaColor.Enemy);
        }
    }
}
