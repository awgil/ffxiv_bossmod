using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    class InfernalFetters : BossComponent
    {
        public BitMask Fetters;
        private int _fettersStrength;

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (Fetters[slot] && actor.Role != Role.Tank)
            {
                var partner = module.Raid.WithSlot().Exclude(slot).IncludedInMask(Fetters).FirstOrDefault().Item2;
                if (partner != null)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(partner.Position, 10)); // TODO: tweak range...
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return Fetters[playerSlot] ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (Fetters.NumSetBits() > 1)
            {
                var from = module.Raid[Fetters.LowestSetBit()];
                var to = module.Raid[Fetters.HighestSetBit()];
                if (from != null && to != null)
                    arena.AddLine(from.Position, to.Position, _fettersStrength > 1 ? ArenaColor.Danger : ArenaColor.Safe);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.InfernalFetters)
            {
                Fetters.Set(module.Raid.FindSlot(actor.InstanceID));
                _fettersStrength = status.Extra;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.InfernalFetters)
            {
                Fetters.Clear(module.Raid.FindSlot(actor.InstanceID));
                _fettersStrength = 0;
            }
        }
    }
}
