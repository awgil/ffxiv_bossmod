namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D062Peacekeeper;

public enum OID : uint
{
    Boss = 0x34C6,
    Helper = 0x233C,
    ElectromagneticRepellant = 0x1EB5F7
}

public enum AID : uint
{
    Decimation = 25936, // Boss->self, 5.0s cast, range 40 circle
    ElectromagneticRepellant = 28360, // Boss->self, 4.0s cast, range 9 circle
    DisengageHatch = 28356, // Boss->self, no cast, single-target
    SmallBoreLaser = 28352, // 384B->self, 5.0s cast, range 20 width 4 rect
    OrderToFire = 28351, // Boss->self, 5.0s cast, single-target
    InfantryDeterrent = 28359, // Helper->player, 5.0s cast, range 6 circle
    NoFutureAOE = 25927, // Helper->self, 4.0s cast, range 6 circle
    NoFutureSpread = 25928, // Helper->player, 5.0s cast, range 6 circle
    Peacefire = 25934, // Helper->self, 7.0s cast, range 10 circle
    EclipsingExhaust = 25931, // Boss->self, 5.0s cast, range 40 circle
    Elimination = 25935, // Boss->self/player, 5.0s cast, range 46 width 10 rect
}

class Elimination(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Elimination), new AOEShapeRect(46, 5), endsOnCastEvent: true);
class Decimation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Decimation));
class ElectromagneticRepellant(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 9, ActionID.MakeSpell(AID.ElectromagneticRepellant), m => m.Enemies(OID.ElectromagneticRepellant).Where(e => e.EventState != 7), 0);
class NoFutureGround(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NoFutureAOE), new AOEShapeCircle(6));
class NoFutureSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.NoFutureSpread), 6);
class Peacefire(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Peacefire), 10);
class SmallBoreLaser(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SmallBoreLaser), new AOEShapeRect(20, 2));
class InfantryDeterrent(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.InfantryDeterrent), 6);
class EclipsingExhaust(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.EclipsingExhaust), 11)
{
    private Peacefire? _peacefire;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _peacefire ??= Module.FindComponent<Peacefire>();

        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.FirstOrDefault() is Actor c)
        {
            if (IsImmune(slot, Module.CastFinishAt(c.CastInfo)))
                return;

            hints.AddForbiddenZone(pos =>
            {
                if ((pos - Arena.Center).Length() > 5)
                    return -1;

                var proj = pos + (pos - Arena.Center).Normalized() * 11;

                foreach (var p in _peacefire?.Casters ?? [])
                    if (proj.InCircle(p.CastInfo!.LocXZ, 10))
                        return -1;

                return 0;
            }, Module.CastFinishAt(c.CastInfo));
        }
    }
}
class Firewall(BossModule module) : Components.GenericAOEs(module)
{
    private bool Active;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x17)
        {
            Active = state == 0x00020001;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active)
            yield return new AOEInstance(new AOEShapeDonut(16, 25), Arena.Center);
    }
}

class PeacekeeperStates : StateMachineBuilder
{
    public PeacekeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Decimation>()
            .ActivateOnEnter<ElectromagneticRepellant>()
            .ActivateOnEnter<NoFutureGround>()
            .ActivateOnEnter<NoFutureSpread>()
            .ActivateOnEnter<SmallBoreLaser>()
            .ActivateOnEnter<InfantryDeterrent>()
            .ActivateOnEnter<Peacefire>()
            .ActivateOnEnter<Firewall>()
            .ActivateOnEnter<EclipsingExhaust>()
            .ActivateOnEnter<Elimination>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10315)]
public class Peacekeeper(WorldState ws, Actor primary) : BossModule(ws, primary, new(-105, -210), new ArenaBoundsCircle(20));

