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
        var tankMimicry = actor.FindStatus(SID.MimicryTank) != null;
        var magic = actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer || actor.Class is Class.BLU && !tankMimicry;
        var target = WorldState.Actors.Find(actor.TargetID);
        if (target == null)
            return;
        if (magic && (OID)target.OID == OID.Tail && target.FindStatus(SID.MantleOfTheWhorl) != null)
            hints.Add("Attack the head! (Attacking the tail will reflect damage onto you)");
        if (actor.Class.GetClassCategory() is ClassCategory.PhysRanged && (OID)target.OID == OID.Boss && target.FindStatus(SID.VeilOfTheWhorl) != null)
            hints.Add("Attack the tail! (Attacking the head will reflect damage onto you)");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var converter = Module.Enemies(OID.Converter).FirstOrDefault();
        var convertertargetable = Module.Enemies(OID.Converter).FirstOrDefault(x => x.IsTargetable);
        if (converter != null && convertertargetable != null)
            Arena.AddCircle(converter.Position, 1.4f, ArenaColor.Safe);
    }
}
