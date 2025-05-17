namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash1;

class LeadHook(BossModule module) : Components.CastCounter(module, default)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NLeadHook or AID.NLeadHookAOE1 or AID.NLeadHookAOE2 or AID.SLeadHook or AID.SLeadHookAOE1 or AID.SLeadHookAOE2)
            ++NumCasts;
    }
}

class TailScrew(BossModule module, AID aid) : Components.StandardAOEs(module, aid, 4);
class NTailScrew(BossModule module) : TailScrew(module, AID.NTailScrew);
class STailScrew(BossModule module) : TailScrew(module, AID.STailScrew);

class C030KiwakinStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030KiwakinStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<NTailScrew>(!_savage)
            .ActivateOnEnter<STailScrew>(_savage)
            .ActivateOnEnter<NWater>(!_savage) // note: second pack is often pulled together with first one
            .ActivateOnEnter<SWater>(_savage)
            .ActivateOnEnter<BubbleShowerCrabDribble>()
            .ActivateOnEnter<Twister>();
    }

    private void SinglePhase(uint id)
    {
        LeadHook(id, 8.1f);
        SharpStrike(id + 0x10000, 3.4f);
        TailScrew(id + 0x20000, 4.2f);
        LeadHook(id + 0x30000, 15.1f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void LeadHook(uint id, float delay)
    {
        Cast(id, _savage ? AID.SLeadHook : AID.NLeadHook, delay, 4)
            .ActivateOnEnter<LeadHook>();
        ComponentCondition<LeadHook>(id + 2, 0.1f, comp => comp.NumCasts > 0, "Mini tankbuster hit 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<LeadHook>(id + 3, 1.1f, comp => comp.NumCasts > 1)
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<LeadHook>(id + 4, 1.1f, comp => comp.NumCasts > 2, "Mini tankbuster hit 3")
            .DeactivateOnExit<LeadHook>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SharpStrike(uint id, float delay)
    {
        Cast(id, _savage ? AID.SSharpStrike : AID.NSharpStrike, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TailScrew(uint id, float delay)
    {
        Cast(id, _savage ? AID.STailScrew : AID.NTailScrew, delay, 5, "AOE");
    }
}
class C030NKiwakinStates(BossModule module) : C030KiwakinStates(module, false);
class C030SKiwakinStates(BossModule module) : C030KiwakinStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NKiwakin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12632, SortOrder = 1)]
public class C030NKiwakin(WorldState ws, Actor primary) : C030Trash1(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SKiwakin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12632, SortOrder = 1)]
public class C030SKiwakin(WorldState ws, Actor primary) : C030Trash1(ws, primary);
