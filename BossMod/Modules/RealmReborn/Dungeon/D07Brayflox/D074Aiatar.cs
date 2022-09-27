namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D074Aiatar
{
    public enum OID : uint
    {
        Boss = 0x38C5, // x1
        Helper = 0x233C, // x14
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast
        SalivousSnap = 28659, // Boss->player, 5.0s cast, tankbuster
        ToxicVomit = 28656, // Boss->self, 3.0s cast, visual
        ToxicVomitAOE = 28657, // Helper->self, 5.0s cast, range 2 aoe
        Burst = 28658, // Helper->self, 9.0s cast, range 10 aoe
        DragonBreath = 28660, // Boss->self, 3.0s cast, range 30 width 8 rect
    };

    class SalivousSnap : Components.SingleTargetCast
    {
        public SalivousSnap() : base(ActionID.MakeSpell(AID.SalivousSnap)) { }
    }

    class ToxicVomit : Components.SelfTargetedAOEs
    {
        public ToxicVomit() : base(ActionID.MakeSpell(AID.ToxicVomitAOE), new AOEShapeCircle(2)) { }
    }

    class Burst : Components.SelfTargetedAOEs
    {
        public Burst() : base(ActionID.MakeSpell(AID.Burst), new AOEShapeCircle(10), 4) { }
    }

    class DragonBreath : Components.SelfTargetedLegacyRotationAOEs
    {
        public DragonBreath() : base(ActionID.MakeSpell(AID.DragonBreath), new AOEShapeRect(30, 4)) { }
    }

    class D074AiatarStates : StateMachineBuilder
    {
        public D074AiatarStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<SalivousSnap>()
                .ActivateOnEnter<ToxicVomit>()
                .ActivateOnEnter<Burst>()
                .ActivateOnEnter<DragonBreath>();
        }
    }

    public class D074Aiatar : BossModule
    {
        public D074Aiatar(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-25, -235), 20)) { }
    }
}
