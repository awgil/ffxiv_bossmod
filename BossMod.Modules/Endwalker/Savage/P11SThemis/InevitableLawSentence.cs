namespace BossMod.Endwalker.Savage.P11SThemis;

// note: currently we start showing stacks right after previous mechanic ends
class InevitableLawSentence(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DivisiveOverrulingSoloLight:
            case AID.InnerLight:
            case AID.DivisiveOverrulingBossLight:
                AddPartyStacks();
                break;
            case AID.DivisiveOverrulingSoloDark:
            case AID.OuterDark:
            case AID.DivisiveOverrulingBossDark:
                AddPairStacks();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.JuryOverrulingProteanLight:
            case AID.UpheldOverrulingAOELight:
                AddPartyStacks();
                break;
            case AID.JuryOverrulingProteanDark:
            case AID.UpheldOverrulingAOEDark:
                AddPairStacks();
                break;
            case AID.InevitableLaw:
            case AID.InevitableSentence:
                Stacks.Clear();
                break;
        }
    }

    private void AddPartyStacks()
    {
        Stacks.Clear();
        foreach (var t in Raid.WithoutSlot(true).Where(t => t.Role == Role.Healer))
            Stacks.Add(new(t, 6, 4, activation: Module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide)));
    }

    private void AddPairStacks()
    {
        Stacks.Clear();
        foreach (var t in Raid.WithoutSlot(true).Where(t => t.Class.IsDD()))
            Stacks.Add(new(t, 3, 2, activation: Module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide)));
    }
}
