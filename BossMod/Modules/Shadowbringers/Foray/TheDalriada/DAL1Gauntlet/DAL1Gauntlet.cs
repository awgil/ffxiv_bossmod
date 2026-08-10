namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class NihilitysSong(BossModule module) : Components.RaidwideCast(module, (uint)AID.NihilitysSong);
sealed class SanctifiedQuakeIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.SanctifiedQuakeIII);
sealed class BroadsideBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BroadsideBarrage, new AOEShapeRect(40f, 20f));
sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 6f);
sealed class CeruleumExplosion(BossModule module) : Components.CastHint(module, (uint)AID.CeruleumExplosion, "Enrage!", true);
sealed class FlamingCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlamingCyclone, 10f);
sealed class SeventyFourDegrees(BossModule module) : Components.DonutStack(module, (uint)AID.SeventyFourDegrees, (uint)IconID.SeventyFourDegrees, 4f, 8f, 9f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.TheDalriada, GroupID = 778, NameID = 10212, SortOrder = 1)]
public sealed class DAL1Gauntlet(WorldState ws, Actor primary) : BossModule(ws, primary, new(222f, -689f), new ArenaBoundsSquare(29.5f))
{
    public Actor? BossAugur;
    public Actor? BossAlkonost;
    public Actor? BossCrow;

    protected override void UpdateModule()
    {
        BossAugur ??= GetActor((uint)OID.ForthLegionAugur1);
        if (StateMachine.ActivePhaseIndex >= 1)
        {
            BossAlkonost ??= GetActor((uint)OID.TamedAlkonost);
            BossCrow ??= GetActor((uint)OID.TamedCrow);
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(PrimaryActor);
                Arena.Actors(Enemies((uint)OID.ForthLegionInfantry));
                break;
            case 1:
                Arena.Actor(BossAugur);
                Arena.Actors(Enemies((uint)OID.WaveborneZirnitra));
                Arena.Actors(Enemies((uint)OID.FlameborneZirnitra));
                break;
            case 2:
                Arena.Actor(BossAlkonost);
                Arena.Actor(BossCrow);
                break;
        }
    }
}
