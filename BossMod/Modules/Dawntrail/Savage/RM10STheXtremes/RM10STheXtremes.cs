namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class DiversDare1(BossModule module) : Components.RaidwideCast(module, AID.DiversDareRed);
class DiversDare2(BossModule module) : Components.RaidwideCast(module, AID.DiversDareBlue);

class XtremeSpectacularProximity(BossModule module) : Components.StandardAOEs(module, AID.XtremeSpectacularProximity, new AOEShapeRect(50, 15));
class XtremeSpectacularRaidwideFirst(BossModule module) : Components.RaidwideCast(module, AID.XtremeSpectacularProximity);
class XtremeSpectacularRaidwideLast(BossModule module) : Components.RaidwideInstant(module, AID.XtremeSpectacularFinal, 4.9f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.XtremeSpectacularProximity)
            Activation = WorldState.FutureTime(Delay);

        if (spell.Action == WatchedAction)
        {
            Activation = default;
            NumCasts++;
        }
    }
}

class FiresnakingRaidwide(BossModule module) : Components.RaidwideCast(module, AID.Firesnaking);
class XtremeFiresnakingRaidwide(BossModule module) : Components.RaidwideCast(module, AID.XtremeFiresnaking);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1071, NameID = 14370, PrimaryActorOID = (uint)OID.RedHot, PlanLevel = 100)]
public class RM10STheXtremes(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    public Actor? B1() => PrimaryActor;
    public Actor? DeepBlue;
    public Actor? B2() => DeepBlue;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(DeepBlue, ArenaColor.Enemy);
    }

    protected override void UpdateModule()
    {
        DeepBlue ??= Enemies(OID.DeepBlue).FirstOrDefault();
    }
}
