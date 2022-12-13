using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    class FiresteelStrike : Components.StackSpread
    {
        public int NumJumps { get; private set; }
        public int NumCleaves { get; private set; }
        private List<int> _jumpTargets = new();
        private BitMask _interceptMask;

        private static AOEShapeRect _cleaveShape = new(65, 4);

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
                if (_jumpTargets[NumCleaves] == slot)
                    hints.Add("Hide behind someone!", !TargetIntercepted(module));
                else if (_interceptMask[slot])
                    hints.Add("Intercept next cleave!", !TargetIntercepted(module));
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
            if (NumJumps >= 2 && NumCleaves < _jumpTargets.Count)
            {
                var target = module.Raid[_jumpTargets[NumCleaves]];
                if (target != null)
                {
                    _cleaveShape.Draw(arena, module.PrimaryActor.Position, Angle.FromDirection(target.Position - module.PrimaryActor.Position), target == pc || _interceptMask[pcSlot] ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NFiresteelStrikeAOE1:
                case AID.NFiresteelStrikeAOE2:
                case AID.SFiresteelStrikeAOE1:
                case AID.SFiresteelStrikeAOE2:
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
                case AID.NBlessedBeaconAOE1:
                case AID.NBlessedBeaconAOE2:
                case AID.SBlessedBeaconAOE1:
                case AID.SBlessedBeaconAOE2:
                    if (spell.Targets.Count > 0)
                        _interceptMask.Clear(module.Raid.FindSlot(spell.Targets[0].ID));
                    ++NumCleaves;
                    break;
            }
        }

        private bool TargetIntercepted(BossModule module)
        {
            var target = NumCleaves < _jumpTargets.Count ? module.Raid[_jumpTargets[NumCleaves]] : null;
            if (target == null)
                return true;

            var toTarget = target.Position - module.PrimaryActor.Position;
            var angle = Angle.FromDirection(toTarget);
            var distSq = toTarget.LengthSq();
            return module.Raid.WithSlot().IncludedInMask(_interceptMask).InShape(_cleaveShape, module.PrimaryActor.Position, angle).WhereActor(a => (a.Position - module.PrimaryActor.Position).LengthSq() < distSq).Any();
        }
    }
}
