namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class ProsecutionOfWar(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.ProsecutionOfWar), ActionID.MakeSpell(AID.ProsecutionOfWar), ActionID.MakeSpell(AID.ProsecutionOfWarAOE), 3.1f, null, true);
class DyingMemory(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DyingMemory));
class DyingMemoryLast(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DyingMemoryLast));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1017, NameID = 13029)]
public class Ex3Sphene(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    private Actor? _bossP2;
    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex > 0 ? Enemies(OID.BossP2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
    }
}
