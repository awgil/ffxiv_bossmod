namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", PrimaryActorOID = (uint)OID.Gretel, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9990)]
public class A32HanselGretel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -950), new ArenaBoundsCircle(30))
{
    private Actor? _hansel;

    public Actor? Gretel() => PrimaryActor;
    public Actor? Hansel() => _hansel;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _hansel ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Hansel).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_hansel, ArenaColor.Enemy);
    }
}
