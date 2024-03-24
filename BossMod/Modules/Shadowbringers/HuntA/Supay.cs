// CONTRIB: made by malediktus, not checked

namespace BossMod.Shadowbringers.HuntA.Supay
{
    public enum OID : uint
    {
        Boss = 0x2839, // R=3.6
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        BlasphemousHowl = 17858, // Boss->players, 3,0s cast, range 8 circle, spread, applies terror
        PetroEyes = 17856, // Boss->self, 3,0s cast, range 40 circle, gaze, inflicts petrification
        Beakaxe = 17857, // Boss->player, no cast, single-target, instantlyy kills petrified players
    };

    class BlasphemousHowl : Components.SpreadFromCastTargets
    {
        public BlasphemousHowl() : base(ActionID.MakeSpell(AID.BlasphemousHowl), 8) { }
    }

    class PetroEyes : Components.CastGaze
    {
        public PetroEyes() : base(ActionID.MakeSpell(AID.PetroEyes)) { }
    }

    class SupayStates : StateMachineBuilder
    {
        public SupayStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<PetroEyes>()
                .ActivateOnEnter<BlasphemousHowl>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 134)]
    public class Supay(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) {}
}
