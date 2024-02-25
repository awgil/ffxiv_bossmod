using System.Linq;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            var converter = module.Enemies(OID.Converter).Where(x => x.IsTargetable).FirstOrDefault();
            if (converter != null)
                hints.Add($"Activate the {converter.Name} or wipe!");
            if (module.Enemies(OID.DangerousSahagins).Any(x => x.IsTargetable && !x.IsDead))
                hints.Add("Kill Sahagins or lose control!");
            if (module.Enemies(OID.Spume).Any(x => x.IsTargetable && !x.IsDead))
                hints.Add("Destroy the spumes!");
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var tail = module.Enemies(OID.Tail).Where(x => x.IsTargetable && x.FindStatus(775) == null && x.FindStatus(477) != null).FirstOrDefault();
            var TankMimikry = actor.FindStatus(2124); //Bluemage Tank Mimikry
            if (tail != null)
            {
                if ((actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer || (actor.Class is Class.BLU && TankMimikry == null)) && actor.TargetID == module.Enemies(OID.Tail).FirstOrDefault()?.InstanceID)
                    hints.Add("Attack the head! (Attacking the tail will reflect damage onto you)");
                if (actor.Class.GetClassCategory() is ClassCategory.PhysRanged && actor.TargetID == module.PrimaryActor.InstanceID)
                    hints.Add("Attack the tail! (Attacking the head will reflect damage onto you)");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var converter = module.Enemies(OID.Converter).FirstOrDefault();
            var convertertargetable = module.Enemies(OID.Converter).Where(x => x.IsTargetable).FirstOrDefault();
            if (converter != null && convertertargetable != null)
                arena.AddCircle(converter.Position, 1.4f, ArenaColor.Safe);
        }
    }
}
