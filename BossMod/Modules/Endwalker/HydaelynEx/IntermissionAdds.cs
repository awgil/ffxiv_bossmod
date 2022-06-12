using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    // component for intermission adds (crystals & echoes)
    class IntermissionAdds : Component
    {
        private HashSet<ulong> _activeCrystals = new();

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var echo in module.Enemies(OID.Echo).Where(e => e.IsTargetable && !e.IsDead))
                arena.Actor(echo, ArenaColor.Enemy);

            // note that there are two crystals per position, one targetable and one not - untargetable one can be tethered to second echo
            foreach (var crystal in module.Enemies(OID.CrystalOfLight))
            {
                if (crystal.IsTargetable && !crystal.IsDead)
                {
                    bool isActive = _activeCrystals.Contains(crystal.InstanceID);
                    arena.Actor(crystal, isActive ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
                }

                var tether = module.WorldState.Actors.Find(crystal.Tether.Target);
                if (tether != null)
                    arena.AddLine(crystal.Position, tether.Position, ArenaColor.Danger);
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.IncreaseConviction))
                _activeCrystals.Add(info.CasterID);
        }
    }
}
