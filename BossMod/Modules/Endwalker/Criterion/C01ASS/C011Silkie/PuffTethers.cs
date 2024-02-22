using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie
{
    class PuffTethers : BossComponent
    {
        private bool _originAtBoss;
        private PuffTracker? _tracker;
        private SlipperySoap.Color _bossColor;

        private static ComponentType _hintType = ComponentType.Hint;

        public PuffTethers(bool originAtBoss)
        {
            _originAtBoss = originAtBoss;
        }

        public override void Init(BossModule module)
        {
            _tracker = module.FindComponent<PuffTracker>();
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_tracker == null)
                return;
            DrawTetherHints(module, pc, _tracker.ChillingPuffs, false);
            DrawTetherHints(module, pc, _tracker.FizzlingPuffs, true);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_tracker == null)
                return;
            DrawTether(module, pc, _tracker.ChillingPuffs);
            DrawTether(module, pc, _tracker.FizzlingPuffs);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if (actor != module.PrimaryActor)
                return;
            var color = SlipperySoap.ColorForStatus(status.ID);
            if (color != SlipperySoap.Color.None)
                _bossColor = color;
        }

        private void DrawTetherHints(BossModule module, Actor player, List<Actor> puffs, bool yellow)
        {
            var source = puffs.Find(p => p.Tether.Target == player.InstanceID);
            if (source == null)
                return;

            var moveDir = (player.Position - source.Position).Normalized();
            var movePos = source.Position + 10 * moveDir;
            var moveAngle = Angle.FromDirection(moveDir);
            if (yellow)
            {
                C011Silkie.ShapeYellow.Draw(module.Arena, movePos, moveAngle + 45.Degrees(), _hintType);
                C011Silkie.ShapeYellow.Draw(module.Arena, movePos, moveAngle + 135.Degrees(), _hintType);
                C011Silkie.ShapeYellow.Draw(module.Arena, movePos, moveAngle - 135.Degrees(), _hintType);
                C011Silkie.ShapeYellow.Draw(module.Arena, movePos, moveAngle - 45.Degrees(), _hintType);
            }
            else
            {
                C011Silkie.ShapeBlue.Draw(module.Arena, movePos, moveAngle, _hintType);
            }

            var bossOrigin = _originAtBoss ? module.PrimaryActor.Position : module.Bounds.Center;
            switch (_bossColor)
            {
                case SlipperySoap.Color.Green:
                    C011Silkie.ShapeGreen.Draw(module.Arena, bossOrigin, new(), _hintType);
                    break;
                case SlipperySoap.Color.Blue:
                    C011Silkie.ShapeBlue.Draw(module.Arena, bossOrigin, new(), _hintType);
                    break;
            }
        }

        private void DrawTether(BossModule module, Actor player, List<Actor> puffs)
        {
            var source = puffs.Find(p => p.Tether.Target == player.InstanceID);
            if (source != null)
            {
                module.Arena.AddLine(source.Position, player.Position, ComponentType.Danger);
            }
        }
    }

    class PuffTethers1 : PuffTethers { public PuffTethers1() : base(false) { } }
    class PuffTethers2 : PuffTethers { public PuffTethers2() : base(true) { } }
}
