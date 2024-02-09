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
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var tail = module.Enemies(OID.Tail).Where(x => x.IsTargetable && x.FindStatus(775) == null && x.FindStatus(477) != null).FirstOrDefault();
            if (tail != null)
            {
                if (actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer)
                    hints.Add("Attack the head! (Attacking the tail will reflect damage onto you)",false);
                if (actor.Class.GetClassCategory() is ClassCategory.PhysRanged)
                    hints.Add("Attack the tail! (Attacking the head will reflect damage onto you)",false);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var converter1 = module.Enemies(OID.Converter).FirstOrDefault();
            var convertertargetable = module.Enemies(OID.Converter).Where(x => x.IsTargetable).FirstOrDefault();
            if (converter1 != null && convertertargetable != null)
                arena.AddCircle(converter1.Position, 1.4f, ArenaColor.Safe);
        }
    }
}
