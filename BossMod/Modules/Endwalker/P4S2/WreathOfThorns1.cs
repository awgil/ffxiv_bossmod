using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.P4S2
{
    using static BossModule;

    // state related to act 1 wreath of thorns
    // note: there should be two tethered helpers for aoes on activation
    class WreathOfThorns1 : Component
    {
        public enum State { FirstAOEs, Towers, LastAOEs, Done }

        public State CurState { get; private set; } = State.FirstAOEs;
        private List<Actor> _relevantHelpers = new(); // 2 aoes -> 8 towers -> 2 aoes

        private IEnumerable<Actor> _firstAOEs => _relevantHelpers.Take(2);
        private IEnumerable<Actor> _towers => _relevantHelpers.Skip(2).Take(8);
        private IEnumerable<Actor> _lastAOEs => _relevantHelpers.Skip(10);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            switch (CurState)
            {
                case State.FirstAOEs:
                    if (_firstAOEs.InRadius(actor.Position, P4S2.WreathAOERadius).Any())
                    {
                        hints.Add("GTFO from AOE!");
                    }
                    break;
                case State.Towers:
                    {
                        var soakedTower = _towers.InRadius(actor.Position, P4S2.WreathTowerRadius).FirstOrDefault();
                        if (soakedTower == null)
                        {
                            hints.Add("Soak the tower!");
                        }
                        else if (module.Raid.WithoutSlot().Exclude(actor).InRadius(soakedTower.Position, P4S2.WreathTowerRadius).Any())
                        {
                            hints.Add("Multiple soakers for the tower!");
                        }
                    }
                    break;
                case State.LastAOEs:
                    if (_lastAOEs.InRadius(actor.Position, P4S2.WreathAOERadius).Any())
                    {
                        hints.Add("GTFO from AOE!");
                    }
                    break;
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (CurState == State.FirstAOEs || CurState == State.LastAOEs)
                foreach (var aoe in CurState == State.FirstAOEs ? _firstAOEs : _lastAOEs)
                    arena.ZoneCircle(aoe.Position, P4S2.WreathAOERadius, arena.ColorAOE);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (CurState == State.Towers)
            {
                foreach (var tower in _towers)
                    arena.AddCircle(tower.Position, P4S2.WreathTowerRadius, arena.ColorSafe);
                foreach (var player in module.Raid.WithoutSlot())
                    arena.Actor(player, arena.ColorPlayerGeneric);
            }
        }

        public override void OnTethered(BossModule module, Actor actor)
        {
            if (actor.OID == (uint)OID.Helper && actor.Tether.ID == (uint)TetherID.WreathOfThorns)
                _relevantHelpers.Add(actor);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (CurState == State.FirstAOEs && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE))
                CurState = State.Towers;
            else if (CurState == State.Towers && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeTower))
                CurState = State.LastAOEs;
            else if (CurState == State.LastAOEs && actor.CastInfo!.IsSpell(AID.AkanthaiExplodeAOE))
                CurState = State.Done;
        }
    }
}
