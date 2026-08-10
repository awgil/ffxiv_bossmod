namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class PureFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PureFire, 6f);

sealed class FiresteelFracture(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FiresteelFracture, new AOEShapeCone(50f, 45f.Degrees()), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class ShowOfStrength(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShowOfStrength);
sealed class CastShadow(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CastShadow1, (uint)AID.CastShadow2], new AOEShapeCone(50f, 15f.Degrees()), 6, 12);

public abstract class VCZelessGah(WorldState ws, Actor primary) : BossModule(ws, primary, new(289f, -105f), new ArenaBoundsRect(24.5f, 29.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.ZelessGah, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11393, SortOrder = 5, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V14ZelessGah(WorldState ws, Actor primary) : VCZelessGah(ws, primary);
