namespace BossMod.Endwalker.Unreal.Un4Zurvan;

// this is used purely for tracking phase transitions
class P2Eidos : BossComponent
{
    public int PhaseIndex { get; private set; }

    public P2Eidos(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var nextPhase = (AID)spell.Action.ID switch
        {
            AID.Eidos1 => 1,
            AID.Eidos2 => 2,
            AID.Eidos3 => 3,
            _ => 0
        };
        if (nextPhase > PhaseIndex)
            PhaseIndex = nextPhase;
    }
}
