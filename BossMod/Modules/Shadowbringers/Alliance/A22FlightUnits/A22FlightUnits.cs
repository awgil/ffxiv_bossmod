namespace BossMod.Shadowbringers.Alliance.A22FlightUnits;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", PrimaryActorOID = (uint)OID.FlightUnitALpha, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9140)]
public class A22FlightUnits(WorldState ws, Actor primary) : BossModule(ws, primary, new(-230, -180), new ArenaBoundsCircle(30))
{
    private Actor? _beta;
    private Actor? _chi;

    public Actor? FlightUnitALpha() => PrimaryActor;
    public Actor? FlightUnitBEta() => _beta;
    public Actor? FlightUnitCHi() => _chi;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _beta ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.FlightUnitBEta).FirstOrDefault() : null;
        _chi ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.FlightUnitCHi).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_beta, ArenaColor.Enemy);
        Arena.Actor(_chi, ArenaColor.Enemy);
    }
}
