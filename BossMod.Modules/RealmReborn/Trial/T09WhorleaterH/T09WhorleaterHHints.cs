namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var converter = Module.Enemies(OID.Converter).FirstOrDefault(x => x.IsTargetable);
        if (converter != null)
            hints.Add($"Activate the {converter.Name} or wipe!");
        if (Module.Enemies(OID.DangerousSahagins).Any(x => x.IsTargetable && !x.IsDead))
            hints.Add("Kill Sahagins or lose control!");
        if (Module.Enemies(OID.Spume).Any(x => x.IsTargetable && !x.IsDead))
            hints.Add("Destroy the spumes!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var tail = Module.Enemies(OID.Tail).FirstOrDefault(x => x.IsTargetable && x.FindStatus(SID.Invincibility) == null && x.FindStatus(SID.MantleOfTheWhorl) != null);
        var TankMimikry = actor.FindStatus(2124); //Bluemage Tank Mimikry
        if (tail != null)
        {
            if ((actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer || (actor.Class is Class.BLU && TankMimikry == null)) && actor.TargetID == Module.Enemies(OID.Tail).FirstOrDefault()?.InstanceID)
                hints.Add("Attack the head! (Attacking the tail will reflect damage onto you)");
            if (actor.Class.GetClassCategory() is ClassCategory.PhysRanged && actor.TargetID == Module.PrimaryActor.InstanceID)
                hints.Add("Attack the tail! (Attacking the head will reflect damage onto you)");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var converter = Module.Enemies(OID.Converter).FirstOrDefault();
        var convertertargetable = Module.Enemies(OID.Converter).FirstOrDefault(x => x.IsTargetable);
        if (converter != null && convertertargetable != null)
            Arena.AddCircle(converter.Position, 1.4f, ArenaColor.Safe);
    }
}
