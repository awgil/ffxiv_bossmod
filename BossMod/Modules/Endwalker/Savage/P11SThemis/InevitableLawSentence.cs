namespace BossMod.Endwalker.Savage.P11SThemis;

// note: currently we start showing stacks right after previous mechanic ends
class InevitableLawSentence : Components.GenericStackSpread
{
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DivisiveOverrulingSoloLight:
            case AID.InnerLight:
            case AID.DivisiveOverrulingBossLight:
                AddPartyStacks(module);
                break;
            case AID.DivisiveOverrulingSoloDark:
            case AID.OuterDark:
            case AID.DivisiveOverrulingBossDark:
                AddPairStacks(module);
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.JuryOverrulingProteanLight:
            case AID.UpheldOverrulingAOELight:
                AddPartyStacks(module);
                break;
            case AID.JuryOverrulingProteanDark:
            case AID.UpheldOverrulingAOEDark:
                AddPairStacks(module);
                break;
            case AID.InevitableLaw:
            case AID.InevitableSentence:
                Stacks.Clear();
                break;
        }
    }

    private void AddPartyStacks(BossModule module)
    {
        Stacks.Clear();
        foreach (var t in module.Raid.WithoutSlot(true).Where(t => t.Role == Role.Healer))
            Stacks.Add(new(t, 6, 4, activation: module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide)));
    }

    private void AddPairStacks(BossModule module)
    {
        Stacks.Clear();
        foreach (var t in module.Raid.WithoutSlot(true).Where(t => t.Class.IsDD()))
            Stacks.Add(new(t, 3, 2, activation: module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide)));
    }
}
