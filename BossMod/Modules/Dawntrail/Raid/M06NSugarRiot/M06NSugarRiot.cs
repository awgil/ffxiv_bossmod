namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

sealed class SprayPain : Components.SimpleAOEs
{
    public SprayPain(BossModule module) : base(module, (uint)AID.SprayPain, 10f, 10)
    {
        MaxDangerColor = 5;
    }
}

sealed class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 4f);

abstract class ColorRiot(BossModule module, uint aid, bool showhint) : Components.BaitAwayCast(module, aid, 4f, tankbuster: showhint);
sealed class WarmBomb(BossModule module) : ColorRiot(module, (uint)AID.WarmBomb, true);
sealed class CoolBomb(BossModule module) : ColorRiot(module, (uint)AID.CoolBomb, false);

sealed class MousseTouchUp(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MousseTouchUp, 6f);
sealed class TasteOfThunder(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.TasteOfThunder, 6f);
sealed class TasteOfFire(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.TasteOfFire, 6f, 4, 4);

sealed class MousseMural(BossModule module) : Components.RaidwideCast(module, (uint)AID.MousseMural);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1021u, NameID = 13822u)]
public sealed class M06NSugarRiot(WorldState ws, Actor primary) : SugarRiotSharedBounds(ws, primary);
