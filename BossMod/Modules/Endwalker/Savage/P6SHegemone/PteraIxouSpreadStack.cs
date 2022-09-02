using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P6SHegemone
{
    // TODO: very similar to Agonies, generalize to spread/stack...
    class PteraIxouSpreadStack : BossComponent
    {
        public enum PlayerState { None, Spread, Stack }

        private PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];

        private const float _stackRadius = 6;
        private const float _spreadRadius = 10;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_states[slot] == PlayerState.Spread)
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
                if (PlayersWithState(module, PlayerState.Spread).InRadius(actor.Position, _spreadRadius).Any())
                    hints.Add("GTFO from spread markers!");
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _states[playerSlot] == PlayerState.Spread ? PlayerPriority.Danger : PlayerPriority.Normal;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_states[pcSlot] == PlayerState.Spread)
            {
                // draw only own circle - no one should be inside, this automatically resolves mechanic for us
                arena.AddCircle(pc.Position, _spreadRadius, ArenaColor.Danger);
            }
            else
            {
                // draw spread and stack circles
                foreach (var player in PlayersWithState(module, PlayerState.Stack))
                    arena.AddCircle(player.Position, _stackRadius, ArenaColor.Safe);
                foreach (var player in PlayersWithState(module, PlayerState.Spread))
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
                }
            }
        }

        private PlayerState StateForSpell(AID id) => id switch
        {
            AID.PteraIxouDarkSphere => PlayerState.Spread,
            AID.PteraIxouUnholyDarkness => PlayerState.Stack,
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
