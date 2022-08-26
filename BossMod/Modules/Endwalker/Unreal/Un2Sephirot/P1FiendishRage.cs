using System.Linq;

namespace BossMod.Endwalker.Unreal.Un2Sephirot
{
    class P1FiendishRage : Components.CastCounter
    {
        private BitMask _targets;

        private static float _range = 6;

        public P1FiendishRage() : base(ActionID.MakeSpell(AID.FiendishRage)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_targets.Any())
            {
                int numClips = module.Raid.WithSlot(true).IncludedInMask(_targets).InRadius(actor.Position, _range).Count();
                if (module.PrimaryActor.TargetID == actor.InstanceID)
                {
                    if (numClips > 0)
                    {
                        hints.Add("GTFO from marked players!");
                    }
                }
                else if (numClips != 1)
                {
                    hints.Add("Stack with single group!");
                }
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var target in module.Raid.WithSlot(true).IncludedInMask(_targets))
                arena.AddCircle(target.Item2.Position, _range, ArenaColor.Danger);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if ((IconID)iconID == IconID.FiendishRage)
                _targets.Set(module.Raid.FindSlot(actor.InstanceID));
        }
    }
}
