using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    // TODO: implement hints for second part of the mechanic (intercepted cleaves)
    class FiresteelStrike : Components.StackSpread
    {
        public int NumJumps { get; private set; }
        public int NumCleaves { get; private set; }
        private List<int> _jumpTargets = new();
        private BitMask _interceptMask;

        public FiresteelStrike() : base(0, 10, alwaysShowSpreads: true) { }

        public override void Init(BossModule module)
        {
            SpreadMask = module.Raid.WithSlot().Mask();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (NumJumps < 2)
            {
                base.AddHints(module, slot, actor, hints, movementHints);
            }
            else if (NumCleaves < _jumpTargets.Count)
            {
                // TODO: add actual checks...
                if (_interceptMask[slot])
                    hints.Add("Intercept next cleave!", false);
                else if (_jumpTargets[NumCleaves] == slot)
                    hints.Add("Hide behind someone!", false);
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            if (NumJumps < 2)
                return base.CalcPriority(module, pcSlot, pc, playerSlot, player, ref customColor);
            else if (NumCleaves < _jumpTargets.Count)
                return playerSlot == _jumpTargets[NumCleaves] ? PlayerPriority.Danger : PlayerPriority.Normal;
            else
                return PlayerPriority.Irrelevant;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            // TODO: implement..
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FiresteelStrikeAOE1:
                case AID.FiresteelStrikeAOE2:
                    if (spell.Targets.Count > 0)
                    {
                        var slot = module.Raid.FindSlot(spell.Targets[0].ID);
                        if (slot >= 0)
                        {
                            _jumpTargets.Add(slot);
                            SpreadMask.Clear(slot);
                        }
                    }
                    if (++NumJumps == 2)
                    {
                        _interceptMask = SpreadMask;
                        SpreadMask.Reset();
                    }
                    break;
                case AID.BlessedBeaconAOE1:
                case AID.BlessedBeaconAOE2:
                    if (spell.Targets.Count > 0)
                        _interceptMask.Clear(module.Raid.FindSlot(spell.Targets[0].ID));
                    ++NumCleaves;
                    break;
            }
        }
    }
}
