// CONTRIB: made by malediktus, not checked
namespace BossMod.Shadowbringers.HuntA.Sugaar
{
    public enum OID : uint
    {
        Boss = 0x2875,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        NumbingNoise = 18015, // Boss->self (front), 5.0s cast, range 13, 120 degree cone
        TailSnap = 18016, // Boss->self (behind), 5.0s cast, range 18, 120 degree cone
        BodySlam = 18018, // Boss->self, 5.0s cast, range 11 circle
        NumbingNoiseRotation = 18100, // rotation, pulls player in from 30 by max 25 units between hitboxes, 3 attacks of NumbingNoise2
        NumbingNoiseDuringRotation = 18098, // during rotation, 0.3s cast time, 13 range, 120 degree cone
        TailSnapRotation = 18101, // rotation, pulls player in from 30 by max 25 units between hitboxes, 3 attacks of TailSnap2
        TailSnapDuringRotation = 18099, // Boss->self (behind), 0.3s cast, range 18, 120 degree cone
    }

    class NumbingNoise : Components.SelfTargetedAOEs
    {
        public NumbingNoise() : base(ActionID.MakeSpell(AID.NumbingNoise), new AOEShapeCone(13,60.Degrees())) { } 
    }

    class TailSnap : Components.SelfTargetedAOEs
    {
        public TailSnap() : base(ActionID.MakeSpell(AID.TailSnap), new AOEShapeCone(18,60.Degrees())) { }
    }
    class TailSnapRotation : Components.SimpleRotationAOE
    {
        public TailSnapRotation() : base(ActionID.MakeSpell(AID.TailSnapRotation), ActionID.MakeSpell(AID.TailSnapDuringRotation), default, default, new AOEShapeCone(18,60.Degrees()), 3, -120.Degrees(), 180.Degrees()) { }
    }
    class NumbingNoiseRotation : Components.SimpleRotationAOE
    {
        public NumbingNoiseRotation() : base(ActionID.MakeSpell(AID.NumbingNoiseRotation), ActionID.MakeSpell(AID.NumbingNoiseDuringRotation), default, default, new AOEShapeCone(13,60.Degrees()), 3, 120.Degrees()) { }
    }
    class BodySlam : Components.SelfTargetedAOEs
    {
        public BodySlam() : base(ActionID.MakeSpell(AID.BodySlam), new AOEShapeCircle(11)) { }
    }
    class NumbingNoiseRotationAttract : Components.AttractBetweenHitboxes
    {
        public NumbingNoiseRotationAttract() : base(ActionID.MakeSpell(AID.NumbingNoiseRotation), 30, 25, 13) { }
    }
    class TailSnapRotationAttract : Components.AttractBetweenHitboxes
    {
        public TailSnapRotationAttract() : base(ActionID.MakeSpell(AID.TailSnapRotation), 30, 25, 18) { }
    }
    class SugaarStates : StateMachineBuilder
    {
        public SugaarStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<NumbingNoise>()
                .ActivateOnEnter<TailSnap>()
                .ActivateOnEnter<TailSnapRotationAttract>()
                .ActivateOnEnter<NumbingNoiseRotationAttract>()
                .ActivateOnEnter<TailSnapRotation>()
                .ActivateOnEnter<NumbingNoiseRotation>()
                .ActivateOnEnter<BodySlam>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 125)]
    public class Sugaar(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) {}
}
