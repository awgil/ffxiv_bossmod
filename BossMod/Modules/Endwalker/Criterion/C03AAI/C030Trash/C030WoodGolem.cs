namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash2;

class Tornado(BossModule module, AID aid) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(aid), 4);
class NTornado(BossModule module) : Tornado(module, AID.NTornado);
class STornado(BossModule module) : Tornado(module, AID.STornado);

class Ovation(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(12, 2));
class NOvation(BossModule module) : Ovation(module, AID.NOvation);
class SOvation(BossModule module) : Ovation(module, AID.SOvation);

class C030WoodGolemStates : StateMachineBuilder
{
    private readonly bool _savage;

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
class C030NWoodGolemStates(BossModule module) : C030WoodGolemStates(module, false);
class C030SWoodGolemStates(BossModule module) : C030WoodGolemStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NWoodGolem, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12560, SortOrder = 6)]
public class C030NWoodGolem(WorldState ws, Actor primary) : C030Trash2(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SWoodGolem, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12560, SortOrder = 6)]
public class C030SWoodGolem(WorldState ws, Actor primary) : C030Trash2(ws, primary);
