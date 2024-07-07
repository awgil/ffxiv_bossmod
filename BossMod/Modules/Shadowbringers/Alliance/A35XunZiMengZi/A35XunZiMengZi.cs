namespace BossMod.Shadowbringers.Alliance.A35XunZiMengZi;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.XunZi, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9921)]
public class A35XunZiMengZi(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 800), new ArenaBoundsSquare(20))
{
    private Actor? _mengZi;

    public Actor? XunZi() => PrimaryActor;
    public Actor? MengZi() => _mengZi;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _mengZi ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.MengZi).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_mengZi, ArenaColor.Enemy);
    }
}
