namespace BossMod.Endwalker.Criterion.C01ASS.C010Dullahan
{
    public enum OID : uint
    {
        Boss = 0x3AD7, // R2.500, x1
    };

    public enum AID : uint
    {
        AutoAttack = 31318, // Boss->player, no cast, single-target
        BlightedGloom = 31078, // Boss->self, 4.0s cast, range 10 circle aoe
        KingsWill = 31080, // Boss->self, 2.5s cast, single-target damage up
        InfernalPain = 31081, // Boss->self, 5.0s cast, raidwide
    };

    class BlightedGloom : Components.SelfTargetedAOEs
    {
        public BlightedGloom() : base(ActionID.MakeSpell(AID.BlightedGloom), new AOEShapeCircle(10)) { }
    }

    class KingsWill : Components.CastHint
    {
        public KingsWill() : base(ActionID.MakeSpell(AID.KingsWill), "Damage increase buff") { }
    }

    class InfernalPain : Components.RaidwideCast
    {
        public InfernalPain() : base(ActionID.MakeSpell(AID.InfernalPain)) { }
    }

    class C010DullahanStates : StateMachineBuilder
    {
        public C010DullahanStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<BlightedGloom>()
                .ActivateOnEnter<KingsWill>()
                .ActivateOnEnter<InfernalPain>();
        }
    }

    public class C010Dullahan : SimpleBossModule
    {
        public C010Dullahan(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
