namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash1;

class LeadHook : Components.CastCounter
{
    public LeadHook() : base(default) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NLeadHook or AID.NLeadHookAOE1 or AID.NLeadHookAOE2 or AID.SLeadHook or AID.SLeadHookAOE1 or AID.SLeadHookAOE2)
            ++NumCasts;
    }
}

class TailScrew : Components.LocationTargetedAOEs
{
    public TailScrew(AID aid) : base(ActionID.MakeSpell(aid), 4) { }
}
class NTailScrew : TailScrew { public NTailScrew() : base(AID.NTailScrew) { } }
class STailScrew : TailScrew { public STailScrew() : base(AID.STailScrew) { } }

class C030KiwakinStates : StateMachineBuilder
{
    private bool _savage;

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
class C030NKiwakinStates : C030KiwakinStates { public C030NKiwakinStates(BossModule module) : base(module, false) { } }
class C030SKiwakinStates : C030KiwakinStates { public C030SKiwakinStates(BossModule module) : base(module, true) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NKiwakin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12632, SortOrder = 1)]
public class C030NKiwakin : C030Trash1
{
    public C030NKiwakin(WorldState ws, Actor primary) : base(ws, primary) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SKiwakin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12632, SortOrder = 1)]
public class C030SKiwakin : C030Trash1
{
    public C030SKiwakin(WorldState ws, Actor primary) : base(ws, primary) { }
}
