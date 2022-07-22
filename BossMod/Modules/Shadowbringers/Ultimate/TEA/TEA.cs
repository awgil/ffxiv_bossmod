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

    [ModuleInfo(PrimaryActorOID = (uint)OID.BossP1)]
    public class TEA : BossModule
    {
        private List<Actor> _liquidHand;
        public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
        public Actor? LiquidHand() => _liquidHand.FirstOrDefault();

        public TEA(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 22))
        {
            _liquidHand = Enemies(OID.LiquidHand);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(BossP1(), ArenaColor.Enemy);
            Arena.Actor(LiquidHand(), ArenaColor.Enemy);
        }
    }
}
