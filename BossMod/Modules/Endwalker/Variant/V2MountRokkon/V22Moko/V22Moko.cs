namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V22Moko;

sealed class AzureAuspice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AzureAuspice, new AOEShapeDonut(6f, 60f));
sealed class KenkiReleaseMoonlessNight(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.KenkiRelease, (uint)AID.MoonlessNight]);
sealed class IronRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronRain, 10f);
sealed class Unsheathing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Unsheathing, 3f);
sealed class VeilSever(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VeilSever, new AOEShapeRect(40f, 2.5f));
sealed class ScarletAuspice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScarletAuspice, 6f);
sealed class Clearout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Clearout, new AOEShapeCone(22f, 90f.Degrees()));
sealed class BoundlessScarletAzure(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BoundlessScarlet, (uint)AID.BoundlessAzure], new AOEShapeRect(60f, 5f));

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeRect(60f, 15f), 2)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(Casters);

        var hasDifferentRotations = false;
        if (count > 1)
        {
            ref var aoe0 = ref aoes[0];
            ref var aoe1 = ref aoes[1];
            hasDifferentRotations = aoe0.Rotation != aoe1.Rotation;
        }

        var max = count > MaxCasts ? MaxCasts : count;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            aoe.Color = i == 0 && count > i ? Colors.Danger : default;
            aoe.Risky = i == 0 || hasDifferentRotations;
        }
        return aoes[..max];
    }
}

sealed class GhastlyGrasp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyGrasp, 5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Moko, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945u, NameID = 12357u, SortOrder = 3, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V22MokoOtherPaths(WorldState ws, Actor primary) : V22Moko(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.MokoP2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945u, NameID = 12357u, SortOrder = 4, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V22MokoPath2(WorldState ws, Actor primary) : V22Moko(ws, primary);

public abstract class V22Moko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700f, 540f), new ArenaBoundsSquare(24.5f));
