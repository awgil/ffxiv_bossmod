namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class BrutalImpact(BossModule module) : Components.CastCounter(module, AID.BrutalImpactRepeat)
{
    private DateTime Activation;
    private bool ShowHint => Activation != default;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BrutalImpact)
            Activation = Module.CastFinishAt(spell, 0.2f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ShowHint)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Activation);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ShowHint)
            hints.Add("Raidwide");
    }
}
