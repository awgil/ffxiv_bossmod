namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash2;

class Tornado : Components.SpreadFromCastTargets
{
    public Tornado(AID aid) : base(ActionID.MakeSpell(aid), 4) { }
}
class NTornado : Tornado { public NTornado() : base(AID.NTornado) { } }
class STornado : Tornado { public STornado() : base(AID.STornado) { } }

class Ovation : Components.SelfTargetedAOEs
{
    public Ovation(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(12, 2)) { }
}
class NOvation : Ovation { public NOvation() : base(AID.NOvation) { } }
class SOvation : Ovation { public SOvation() : base(AID.SOvation) { } }

class C030WoodGolemStates : StateMachineBuilder
{
    private bool _savage;

    public C030WoodGolemStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        AncientAero(id, 11.5f);
        Tornado(id + 0x10000, 2);
        Ovation(id + 0x20000, 2.7f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void AncientAero(uint id, float delay)
    {
        CastStart(id, _savage ? AID.SAncientAero : AID.NAncientAero, delay);

        var castEnd = SimpleState(id + 1, 5, "Interruptible raidwide"); // note: we use custom state instead of cast-end, since cast-end happens whenever anyone presses interrupt - and if not interrupted, spell finish can be slightly delayed
        castEnd.Raw.Comment = "Interruptible cast end";
        castEnd.Raw.Update = timeSinceTransition => Module.PrimaryActor.CastInfo == null && timeSinceTransition >= castEnd.Raw.Duration ? 0 : -1;
    }

    private void Tornado(uint id, float delay)
    {
        Cast(id, _savage ? AID.STornado : AID.NTornado, delay, 5, "Spread")
            .ActivateOnEnter<NTornado>(!_savage)
            .ActivateOnEnter<STornado>(_savage)
            .DeactivateOnExit<Tornado>();
    }

    private void Ovation(uint id, float delay)
    {
        Cast(id, _savage ? AID.SOvation : AID.NOvation, delay, 4, "Line")
            .ActivateOnEnter<NOvation>(!_savage)
            .ActivateOnEnter<SOvation>(_savage)
            .DeactivateOnExit<Ovation>();
    }
}
class C030NWoodGolemStates : C030WoodGolemStates { public C030NWoodGolemStates(BossModule module) : base(module, false) { } }
class C030SWoodGolemStates : C030WoodGolemStates { public C030SWoodGolemStates(BossModule module) : base(module, true) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NWoodGolem, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12560, SortOrder = 6)]
public class C030NWoodGolem : C030Trash2
{
    public C030NWoodGolem(WorldState ws, Actor primary) : base(ws, primary) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SWoodGolem, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12560, SortOrder = 6)]
public class C030SWoodGolem : C030Trash2
{
    public C030SWoodGolem(WorldState ws, Actor primary) : base(ws, primary) { }
}
