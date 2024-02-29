using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P1Plummet : Components.Cleave
    {
        public P1Plummet() : base(ActionID.MakeSpell(AID.Plummet), new AOEShapeCone(12, 60.Degrees()), (uint)OID.Twintania) { }
    }

    class P1Twister : Components.CastTwister
    {
        public P1Twister() : base(2, (uint)OID.VoidzoneTwister, ActionID.MakeSpell(AID.Twister), 0.3f, 0.5f) { KeepOnPhaseChange = true; } // TODO: verify radius
    }

    class P1LiquidHell : Components.PersistentVoidzoneAtCastTarget
    {
        public P1LiquidHell() : base(6, ActionID.MakeSpell(AID.LiquidHell), m => m.Enemies(OID.VoidzoneLiquidHell).Where(z => z.EventState != 7), 1.3f) { KeepOnPhaseChange = true; }
        public void Reset() => NumCasts = 0;
    }

    class P2BahamutsClaw : Components.CastCounter
    {
        public P2BahamutsClaw() : base(ActionID.MakeSpell(AID.BahamutsClaw)) { }
    }

    class P3FlareBreath : Components.Cleave
    {
        public P3FlareBreath() : base(ActionID.MakeSpell(AID.FlareBreath), new AOEShapeCone(29.2f, 45.Degrees()), (uint)OID.BahamutPrime) { } // TODO: verify angle
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.Twintania)]
    public class UCOB : BossModule
    {
        private IReadOnlyList<Actor> _nael;
        private IReadOnlyList<Actor> _bahamutPrime;

        public Actor? Twintania() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
        public Actor? Nael() => _nael.FirstOrDefault();
        public Actor? BahamutPrime() => _bahamutPrime.FirstOrDefault();

        public UCOB(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 21))
        {
            _nael = Enemies(OID.NaelDeusDarnus);
            _bahamutPrime = Enemies(OID.BahamutPrime);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(Twintania(), ArenaColor.Enemy);
            Arena.Actor(Nael(), ArenaColor.Enemy);
            Arena.Actor(BahamutPrime(), ArenaColor.Enemy);
        }
    }
}
