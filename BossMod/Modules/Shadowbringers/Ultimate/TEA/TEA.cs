using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1FluidSwing : Components.Cleave
    {
        public P1FluidSwing() : base(ActionID.MakeSpell(AID.FluidSwing), new AOEShapeCone(11.5f, 45.Degrees())) { }
    }

    class P1FluidStrike : Components.Cleave
    {
        public P1FluidStrike() : base(ActionID.MakeSpell(AID.FluidSwing), new AOEShapeCone(11.5f, 45.Degrees()), (uint)OID.LiquidHand) { }
    }

    class P1Sluice : Components.LocationTargetedAOEs
    {
        public P1Sluice() : base(ActionID.MakeSpell(AID.Sluice), 5) { }
    }

    class P1Splash : Components.CastCounter
    {
        public P1Splash() : base(ActionID.MakeSpell(AID.Splash)) { }
    }

    class P1Drainage : Components.TankbusterTether
    {
        public P1Drainage() : base(ActionID.MakeSpell(AID.Drainage), (uint)TetherID.Drainage, 6) { }
    }

    class P2JKick : Components.CastCounter
    {
        public P2JKick() : base(ActionID.MakeSpell(AID.JKick)) { }
    }

    class P2EyeOfTheChakram : Components.SelfTargetedLegacyRotationAOEs
    {
        public P2EyeOfTheChakram() : base(ActionID.MakeSpell(AID.EyeOfTheChakram), new AOEShapeRect(70, 3)) { }
    }

    class P2HawkBlasterOpticalSight : Components.LocationTargetedAOEs
    {
        public P2HawkBlasterOpticalSight() : base(ActionID.MakeSpell(AID.HawkBlasterP2), 10) { }
    }

    class P2SpinCrusher : Components.SelfTargetedLegacyRotationAOEs
    {
        public P2SpinCrusher() : base(ActionID.MakeSpell(AID.SpinCrusher), new AOEShapeCone(10, 45.Degrees())) { } // TODO: verify angle
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.BossP1)]
    public class TEA : BossModule
    {
        private List<Actor> _liquidHand;
        public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
        public Actor? LiquidHand() => _liquidHand.FirstOrDefault();

        private Actor? _bruteJustice;
        private Actor? _cruiseChaser;
        public Actor? BruteJustice() => _bruteJustice;
        public Actor? CruiseChaser() => _cruiseChaser;

        public TEA(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 22))
        {
            _liquidHand = Enemies(OID.LiquidHand);
        }

        protected override void UpdateModule()
        {
            // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
            // the problem is that on wipe, any actor can be deleted and recreated in the same frame
            _bruteJustice ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BruteJustice).FirstOrDefault() : null;
            _cruiseChaser ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.CruiseChaser).FirstOrDefault() : null;
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            switch (StateMachine.ActivePhaseIndex)
            {
                case -1:
                case 0:
                    Arena.Actor(BossP1(), ArenaColor.Enemy);
                    Arena.Actor(LiquidHand(), ArenaColor.Enemy);
                    break;
                case 1:
                    Arena.Actor(_bruteJustice, ArenaColor.Enemy, true);
                    Arena.Actor(_cruiseChaser, ArenaColor.Enemy, true);
                    break;
            }
        }
    }
}
