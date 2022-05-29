using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P2SanctityOfTheWard2 : BossModule.Component
    {
        private List<Actor> _towers = new();
        private ulong _preyTargets;

        private static float _towerRadius = 3;

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.AddLine(module.Arena.WorldNW, module.Arena.WorldSE, arena.ColorBorder);
            arena.AddLine(module.Arena.WorldNE, module.Arena.WorldSW, arena.ColorBorder);
            foreach (var (_, prey) in module.Raid.WithSlot().IncludedInMask(_preyTargets))
                arena.Actor(prey, arena.ColorDanger);
            foreach (var tower in _towers)
                arena.AddCircle(tower.CastInfo!.Location, _towerRadius, arena.ColorSafe);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.Conviction2AOE))
                _towers.Add(actor);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.Conviction2AOE))
                _towers.Remove(actor);
        }

        public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            if (iconID == (uint)IconID.Prey)
            {
                int slot = module.Raid.FindSlot(actorID);
                if (slot >= 0)
                    BitVector.SetVector64Bit(ref _preyTargets, slot);
            }
        }
    }
}
