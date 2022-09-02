using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P6SHegemone
{
    class Agonies : BossComponent
    {
        public enum PlayerState { None, Circle, Donut, Stack }

        public int NumActiveMechanics { get; private set; }
        private PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];

        private const float _stackRadius = 6;
        private const float _spreadRadius = 15;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_states[slot] == PlayerState.Circle)
            {
                // check only own circle - no one should be inside, this automatically resolves mechanic for us
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _spreadRadius).Any())
                    hints.Add("Spread!");
            }
            else
            {
                // check that we're not clipped by circles and stacked otherwise
                if (PlayersWithState(module, PlayerState.Stack).InRadius(actor.Position, _stackRadius).Count() != 1)
                    hints.Add("Stack!");
                if (PlayersWithState(module, PlayerState.Circle).InRadius(actor.Position, _spreadRadius).Any())
                    hints.Add("GTFO from spread markers!");
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _states[playerSlot] == PlayerState.Circle ? PlayerPriority.Danger : PlayerPriority.Normal;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_states[pcSlot] == PlayerState.Circle)
            {
                // draw only own circle - no one should be inside, this automatically resolves mechanic for us
                arena.AddCircle(pc.Position, _spreadRadius, ArenaColor.Danger);
            }
            else
            {
                // draw spread and stack circles
                foreach (var player in PlayersWithState(module, PlayerState.Stack))
                    arena.AddCircle(player.Position, _stackRadius, ArenaColor.Safe);
                foreach (var player in PlayersWithState(module, PlayerState.Circle))
                    arena.AddCircle(player.Position, _spreadRadius, ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var state = StateForSpell((AID)spell.Action.ID);
            if (state != PlayerState.None)
            {
                var slot = module.Raid.FindSlot(spell.TargetID);
                if (slot >= 0)
                {
                    _states[slot] = state;
                    ++NumActiveMechanics;
                }
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var state = StateForSpell((AID)spell.Action.ID);
            if (state != PlayerState.None)
            {
                var slot = module.Raid.FindSlot(spell.TargetID);
                if (slot >= 0)
                {
                    _states[slot] = PlayerState.None;
                    --NumActiveMechanics;
                }
            }
        }

        private PlayerState StateForSpell(AID id) => id switch
        {
            AID.AgoniesDarkburst1 or AID.AgoniesDarkburst2 or AID.AgoniesDarkburst3 => PlayerState.Circle,
            AID.AgoniesDarkPerimeter1 or AID.AgoniesDarkPerimeter2 => PlayerState.Donut,
            AID.AgoniesUnholyDarkness1 or AID.AgoniesUnholyDarkness2 or AID.AgoniesUnholyDarkness3 => PlayerState.Stack,
            _ => PlayerState.None,
        };

        private IEnumerable<Actor> PlayersWithState(BossModule module, PlayerState state)
        {
            foreach (var (slot, player) in module.Raid.WithSlot(true))
                if (_states[slot] == state)
                    yield return player;
        }
    }
}
