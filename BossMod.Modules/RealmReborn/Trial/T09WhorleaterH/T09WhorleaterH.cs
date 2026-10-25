namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

class GrandFall(BossModule module) : Components.StandardAOEs(module, AID.GrandFall, 8);
class Hydroshot(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID.Hydroshot, OID.HydroshotZone, 0);
class Dreadstorm(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID.Dreadstorm, OID.DreadstormZone, 0);

class T09WhorleaterHStates : StateMachineBuilder
{
    public T09WhorleaterHStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrandFall>()
            .ActivateOnEnter<Hydroshot>()
            .ActivateOnEnter<Dreadstorm>()
            .ActivateOnEnter<BodySlamKB>()
            .ActivateOnEnter<BodySlamAOE>()
            .ActivateOnEnter<SpinningDive>()
            .ActivateOnEnter<SpinningDiveKB>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(Contributors = "taurenkey, Malediktus, Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 72, NameID = 2505)]
public class T09WhorleaterH(ModuleInit init) : BossModule(init, new(-0, 0), new ArenaBoundsRect(14.5f, 20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
        foreach (var s in Enemies(OID.Spume))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var e in Enemies(OID.Tail))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.Sahagin))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.DangerousSahagins))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var c in Enemies(OID.Converter))
            Arena.Actor(c, ArenaColor.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var tankMimicry = actor.FindStatus(SID.MimicryTank) != null;
        var magic = actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer || actor.Class is Class.BLU && !tankMimicry;
        var physRanged = actor.Class.GetClassCategory() is ClassCategory.PhysRanged;
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.DangerousSahagins => 4,
                OID.Spume => 3,
                OID.Sahagin => 2,
                OID.Boss or OID.Tail => 1,
                _ => 0
            };
            if (physRanged && (OID)e.Actor.OID == OID.Boss && e.Actor.FindStatus(SID.VeilOfTheWhorl) != null
                || magic && (OID)e.Actor.OID == OID.Tail && e.Actor.FindStatus(SID.MantleOfTheWhorl) != null)
                e.Priority = AIHints.Enemy.PriorityForbidden;
        }

        if (Enemies(OID.Converter).FirstOrDefault(c => c.IsTargetable) is { } converter
            && Raid.WithoutSlot().Where(p => !p.IsDead).MinBy(converter.DistanceToHitbox) == actor)
            hints.InteractWithTarget = converter;
    }
}
