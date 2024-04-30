namespace BossMod.Endwalker.Ultimate.DSW1;

class EmptyDimension(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EmptyDimension), new AOEShapeDonut(6, 70));
class FullDimension(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FullDimension), new AOEShapeCircle(6));
class HoliestHallowing(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.HoliestHallowing), "Interrupt!");

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SerAdelphel, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 788)]
public class DSW1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(22))
{
    private Actor? _grinnaux;
    private Actor? _charibert;
    public Actor? SerAdelphel() => PrimaryActor;
    public Actor? SerGrinnaux() => _grinnaux;
    public Actor? SerCharibert() => _charibert;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _grinnaux ??= Enemies(OID.SerGrinnaux).FirstOrDefault();
        _charibert ??= Enemies(OID.SerCharibert).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(SerAdelphel(), ArenaColor.Enemy);
        Arena.Actor(SerGrinnaux(), ArenaColor.Enemy);
        Arena.Actor(SerCharibert(), ArenaColor.Enemy);
    }
}
