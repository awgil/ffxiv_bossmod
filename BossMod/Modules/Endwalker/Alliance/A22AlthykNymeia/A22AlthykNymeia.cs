namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class MythrilGreataxe(BossModule module) : Components.StandardAOEs(module, AID.MythrilGreataxe, new AOEShapeCone(71, 30.Degrees()));
class Hydroptosis(BossModule module) : Components.SpreadFromCastTargets(module, AID.HydroptosisAOE, 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Althyk, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12244)]
public class A22AlthykNymeia(WorldState ws, Actor primary) : BossModule(ws, primary, new(50, -750), new ArenaBoundsSquare(25))
{
    private Actor? _nymeia;

    public Actor? Althyk() => PrimaryActor;
    public Actor? Nymeia() => _nymeia;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _nymeia ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Nymeia).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_nymeia, ArenaColor.Enemy);
    }
}
