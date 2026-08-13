namespace BossMod.Modules.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

sealed class HeatBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatBurst);

sealed class BurningGleam(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BurningGleam, (uint)AID.BurningGleam1, (uint)AID.BurningGleam2], new AOEShapeCross(40f, 5f));

sealed class CharmedChains(BossModule module) : Components.Chains(module, (uint)TetherID.CharmedChain);

sealed class SimpleFableFlight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftFableflight, (uint)AID.RightFableflight], new AOEShapeCone(60f, 90f.Degrees()));

sealed class FireOfVictory(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FireOfVictory, 4f);

sealed class FellSpark(BossModule module) : Components.InterceptTetherStatus(module, (uint)AID.FellSpark, (uint)TetherID.FellSpark, (uint)SID.DarkResistanceDown);

sealed class CurseOfCompanionshipSolitude(BossModule module) : Components.StatusStackSpread(module, (uint)SID.CurseOfCompanionship, (uint)SID.CurseOfSolitude, 15f, 15f);

sealed class SpurningFlames(BossModule module) : Components.RaidwideCast(module, (uint)AID.SpurningFlames);
sealed class ImpassionedSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpassionedSparks3, 8f);
sealed class BurningPillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningPillar, 10f);
sealed class SparkPuddle(BossModule module) : Components.Voidzone(module, 10f, GetPuddles)
{
    private static Actor[] GetPuddles(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SparkPuddle);
        var count = enemies.Count;
        var index = 0;
        var puddles = new Actor[count];
        for (var i = 0; i < count; i++)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                puddles[index++] = z;
            }
        }
        return puddles[..index];
    }
}

sealed class FireWell(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, (uint)AID.FireWell, 6f, 3d);

sealed class ScouringScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScouringScorn);

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(PariOfPlentyStates),
ConfigType = null, // replace null with typeof(PariOfPlentyConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
IconIDType = null, // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.PariOfPlenty,
Contributors = "HerStolenLight",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1084u,
NameID = 14274u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));
