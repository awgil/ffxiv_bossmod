using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P10SPandaemonium
{
    class EntanglingWebAOE : Components.LocationTargetedAOEs
    {
        public EntanglingWebAOE() : base(ActionID.MakeSpell(AID.EntanglingWebAOE), 5) { }
    }

    class EntanglingWebHints : BossComponent
    {
        private IReadOnlyList<Actor> _pillars = ActorEnumeration.EmptyList;
        private List<Actor> _targets = new();

        private static float _radius = 5;

        public override void Init(BossModule module)
        {
            _pillars = module.Enemies(OID.Pillar);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_targets.Contains(actor))
            {
                var overlapCount = _targets.InRadiusExcluding(actor, _radius * 2).Count();
                if (overlapCount < 2 && !_pillars.InRadius(actor.Position, _radius).Any())
                    hints.Add("Stand near pillar!");
                if (overlapCount == 0)
                    hints.Add("Overlap with other web!");
            }
            else if (_targets.InRadius(actor.Position, _radius).Any())
            {
                hints.Add("GTFO from webs!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actors(_pillars, ArenaColor.Object, true);
            foreach (var t in _targets)
                arena.AddCircle(t.Position, _radius, ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.EntanglingWebAOE && _targets.Count > 0)
                _targets.RemoveAt(0);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.EntanglingWeb)
                _targets.Add(actor);
        }
    }
}
