using System.Linq;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to ashplumes (normal or parts of gloryplume)
    // normal ashplume is boss cast (with different IDs depending on stack/spread) + instant aoe some time later
    // gloryplume is one instant cast with animation only soon after boss cast + instant aoe some time later
    class Ashplume : Component
    {
        public enum State { UnknownGlory, Stack, Spread, Done }

        public State CurState { get; private set; }
        private P3S _module;

        private static float _stackRadius = 8;
        private static float _spreadRadius = 6;

        public Ashplume(P3S module, State initial)
        {
            CurState = initial;
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CurState == State.Stack)
            {
                // note: it seems to always target 1 tank & 1 healer, so correct stacks are always tanks+dd and healers+dd
                int numStacked = 0;
                bool haveTanks = actor.Role == Role.Tank;
                bool haveHealers = actor.Role == Role.Healer;
                foreach (var pair in _module.Raid.WithoutSlot().InRadiusExcluding(actor, _stackRadius))
                {
                    ++numStacked;
                    haveTanks |= pair.Role == Role.Tank;
                    haveHealers |= pair.Role == Role.Healer;
                }
                if (numStacked != 3)
                {
                    hints.Add("Stack in fours!");
                }
                else if (haveTanks && haveHealers)
                {
                    hints.Add("Incorrect stack!");
                }
            }
            else if (CurState == State.Spread)
            {
                if (_module.Raid.WithoutSlot().InRadiusExcluding(actor, _spreadRadius).Any())
                {
                    hints.Add("Spread!");
                }
            }
        }

        public override void AddGlobalHints(GlobalHints hints)
        {
            if (CurState == State.Stack)
                hints.Add("Stack!");
            else if (CurState == State.Spread)
                hints.Add("Spread!");
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (CurState == State.UnknownGlory || CurState == State.Done)
                return;

            // draw all raid members, to simplify positioning
            float aoeRadius = CurState == State.Stack ? _stackRadius : _spreadRadius;
            foreach (var player in _module.Raid.WithoutSlot().Exclude(pc))
            {
                bool inRange = GeometryUtils.PointInCircle(player.Position - pc.Position, aoeRadius);
                arena.Actor(player, inRange ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }

            // draw circle around pc
            arena.AddCircle(pc.Position, aoeRadius, arena.ColorDanger);
        }

        public override void OnEventCast(CastEvent info)
        {
            if (!info.IsSpell())
                return;
            switch ((AID)info.Action.ID)
            {
                case AID.ExperimentalGloryplumeSpread:
                    CurState = State.Spread;
                    break;
                case AID.ExperimentalGloryplumeStack:
                    CurState = State.Stack;
                    break;
                case AID.ExperimentalGloryplumeSpreadAOE:
                case AID.ExperimentalGloryplumeStackAOE:
                case AID.ExperimentalAshplumeSpreadAOE:
                case AID.ExperimentalAshplumeStackAOE:
                    CurState = State.Done;
                    break;
            }
        }
    }
}
