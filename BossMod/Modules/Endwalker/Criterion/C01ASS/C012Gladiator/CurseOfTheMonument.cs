using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator
{
    class SunderedRemains : Components.SelfTargetedAOEs
    {
        public SunderedRemains(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCircle(10)) { } // TODO: max-casts...
    }
    class NSunderedRemains : SunderedRemains { public NSunderedRemains() : base(AID.NSunderedRemains) { } }
    class SSunderedRemains : SunderedRemains { public SSunderedRemains() : base(AID.SSunderedRemains) { } }

    class ScreamOfTheFallen : Components.StackSpread
    {
        public int NumCasts { get; private set; }
        private BitMask _second;
        private List<Actor> _towers = new();

        private static float _towerRadius = 3;

        public ScreamOfTheFallen() : base(0, 15, alwaysShowSpreads: true) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (!IsSpreadTarget(actor))
                hints.Add("Soak the tower!", !ActiveTowers(_second[slot]).Any(t => t.Position.InCircle(actor.Position, _towerRadius)));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            if (!IsSpreadTarget(pc))
                foreach (var t in ActiveTowers(_second[pcSlot]))
                    arena.AddCircle(t.Position, _towerRadius, ArenaColor.Safe, 2);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.FirstInLine:
                    SpreadTargets.Add(actor);
                    break;
                case SID.SecondInLine:
                    _second.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NExplosion or AID.SExplosion)
                _towers.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NExplosion or AID.SExplosion)
            {
                switch (++NumCasts)
                {
                    case 2:
                        SpreadTargets.Clear();
                        SpreadTargets.AddRange(module.Raid.WithSlot().IncludedInMask(_second).Actors());
                        break;
                    case 4:
                        SpreadTargets.Clear();
                        break;
                }
            }
        }

        private IEnumerable<Actor> ActiveTowers(bool second) => second ? _towers.Take(2) : _towers.Skip(2);
    }
}
