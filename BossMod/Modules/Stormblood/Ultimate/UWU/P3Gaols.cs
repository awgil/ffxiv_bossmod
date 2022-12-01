using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P3Gaols : BossComponent
    {
        private BitMask _targets;

        public bool Active => _targets.Any();

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Active)
            {
                var hint = string.Join(" > ", Service.Config.Get<UWUConfig>().P3GaolPriorities.Resolve(module.Raid).Where(i => _targets[i.slot]).OrderBy(i => i.group).Select(i => module.Raid[i.slot]?.Name));
                hints.Add($"Gaols: {hint}");
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.RockThrowBoss:
                case AID.RockThrowHelper:
                    _targets.Set(module.Raid.FindSlot(spell.MainTargetID));
                    break;
            }
        }
    }
}
