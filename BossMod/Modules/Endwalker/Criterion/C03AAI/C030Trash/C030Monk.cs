namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash1;

class Hydroshot : Components.Knockback
{
    private Actor? _caster;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (_caster?.CastInfo?.TargetID == actor.InstanceID)
            yield return new(_caster.Position, 10, _caster.CastInfo?.NPCFinishAt ?? default);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NHydroshot or AID.SHydroshot)
            _caster = caster;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_caster == caster)
            _caster = null;
    }
}

class C030MonkStates : StateMachineBuilder
{
    private bool _savage;

    public C030MonkStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Hydroshot>()
            .ActivateOnEnter<Twister>();
    }

    private void SinglePhase(uint id)
    {
        Hydroshot(id, 9.1f);
        CrossAttack(id + 0x10000, 3.4f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Hydroshot(uint id, float delay)
    {
        Cast(id, _savage ? AID.SHydroshot : AID.NHydroshot, delay, 5, "Single-target 1");
        Cast(id + 0x10, _savage ? AID.SHydroshot : AID.NHydroshot, 1.5f, 5, "Single-target 2");
        Cast(id + 0x20, _savage ? AID.SHydroshot : AID.NHydroshot, 1.5f, 5, "Single-target 3");
    }

    private void CrossAttack(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCrossAttack : AID.NCrossAttack, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }
}
class C030NMonkStates : C030MonkStates { public C030NMonkStates(BossModule module) : base(module, false) { } }
class C030SMonkStates : C030MonkStates { public C030SMonkStates(BossModule module) : base(module, true) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NMonk, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12631, SortOrder = 4)]
public class C030NMonk : C030Trash1
{
    public C030NMonk(WorldState ws, Actor primary) : base(ws, primary) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SMonk, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12631, SortOrder = 4)]
public class C030SMonk : C030Trash1
{
    public C030SMonk(WorldState ws, Actor primary) : base(ws, primary) { }
}
