namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class CrueltyP1(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.CrueltyVisualP1, (uint)AID.Cruelty, 0.1d);
sealed class CrueltyP2(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.CrueltyVisualP2, (uint)AID.Cruelty, 0.1d);
sealed class SublimeTranscendence(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.SublimeTranscendenceVisual, (uint)AID.SublimeTranscendence, 0.1d);
sealed class ManipulateEnergy(BossModule module) : Components.BaitAwayIcon(module, 3f, (uint)IconID.ManipulateEnergy, (uint)AID.ManipulateEnergy, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class GenerateBarrier1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual1, new AOEShapeRect(18f, 1.5f));
sealed class GenerateBarrier2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual2, new AOEShapeRect(24f, 1.5f));
sealed class GenerateBarrier3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual3, new AOEShapeRect(12f, 1.5f));
sealed class GenerateBarrier4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual4, new AOEShapeRect(6f, 1.5f));

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 9f, riskyWithSecondsLeft: 5d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9920, SortOrder = 4)]
public sealed class A33RedGirl(WorldState ws, Actor primary) : BossModule(ws, primary, new(845f, -851f), new ArenaBoundsSquare(24.5f))
{
    public Actor? BossP2;
    public Actor? RedSphere;

    protected override void UpdateModule()
    {
        RedSphere ??= GetActor((uint)OID.RedSphere);
        if (StateMachine.ActivePhaseIndex >= 1)
        {
            BossP2 ??= GetActor((uint)OID.BossP2);
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(PrimaryActor);
                break;
            case 1:
                Arena.Actor(RedSphere);
                Arena.Actors(Enemies((uint)OID.BlackPylon));
                Arena.Actors(Enemies((uint)OID.WhitePylon));
                Arena.Actors(Enemies((uint)OID.BlackWall), Colors.Object);
                Arena.Actors(Enemies((uint)OID.WhiteWall), Colors.Object);
                break;
            case 2:
                Arena.Actor(BossP2);
                break;
        }
    }
}
