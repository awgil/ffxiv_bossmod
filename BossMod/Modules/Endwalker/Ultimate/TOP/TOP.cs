using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class SolarRayM : Components.BaitAwayCast
    {
        public SolarRayM() : base(ActionID.MakeSpell(AID.SolarRayM), new AOEShapeCircle(5), true) { }
    }

    class SolarRayF : Components.BaitAwayCast
    {
        public SolarRayF() : base(ActionID.MakeSpell(AID.SolarRayF), new AOEShapeCircle(5), true) { }
    }

    class P4BlueScreen : Components.CastCounter
    {
        public P4BlueScreen() : base(ActionID.MakeSpell(AID.BlueScreenAOE)) { }
    }

    public class TOP : BossModule
    {
        private Actor? _opticalUnit;
        private Actor? _omegaM;
        private Actor? _omegaF;
        private Actor? _bossP3;
        private Actor? _omegaMP5;
        public Actor? BossP1() => PrimaryActor;
        public Actor? OpticalUnit() => _opticalUnit; // we use this to distinguish P1 wipe vs P1 kill - primary actor can be destroyed before P2 bosses spawn
        public Actor? BossP2M() => _omegaM;
        public Actor? BossP2F() => _omegaF;
        public Actor? BossP3() => _bossP3;
        public Actor? BossP5M() => _omegaMP5;

        public TOP(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void UpdateModule()
        {
            // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
            // the problem is that on wipe, any actor can be deleted and recreated in the same frame
            _opticalUnit ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.OpticalUnit).FirstOrDefault() : null;
            _omegaM ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.OmegaM).FirstOrDefault() : null;
            _omegaF ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.OmegaF).FirstOrDefault() : null;
            _bossP3 ??= StateMachine.ActivePhaseIndex == 2 ? Enemies(OID.BossP3).FirstOrDefault() : null;
            _omegaMP5 ??= StateMachine.ActivePhaseIndex == 4 ? Enemies(OID.OmegaMP5).FirstOrDefault() : null;
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actor(_omegaM, ArenaColor.Enemy);
            Arena.Actor(_omegaF, ArenaColor.Enemy);
            Arena.Actor(_bossP3, ArenaColor.Enemy);
            Arena.Actor(_omegaMP5, ArenaColor.Enemy);
        }
    }
}
