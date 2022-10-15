using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Raid.T05Twintania
{
    // P4 mechanics
    class P4Twisters : BossComponent
    {
        private List<Actor> _twisters = new();
        private List<WPos> _predictedPositions = new();
        private IEnumerable<Actor> ActiveTwisters => _twisters.Where(t => t.EventState != 7);

        public override void Init(BossModule module)
        {
            _twisters = module.Enemies(OID.Twister);
        }

        public override void Update(BossModule module)
        {
            if (_predictedPositions.Count == 0 && (module.PrimaryActor.CastInfo?.IsSpell(AID.Twister) ?? false) && module.PrimaryActor.CastInfo.FinishAt <= module.WorldState.CurrentTime)
                _predictedPositions.AddRange(module.Raid.WithoutSlot().Select(a => a.Position));
            if (_twisters.Count > 0)
                _predictedPositions.Clear();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.Twister) ?? false)
                hints.Add("Move!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var p in _predictedPositions)
                hints.AddForbiddenZone(ShapeDistance.Circle(p, 5), module.PrimaryActor.CastInfo?.FinishAt ?? new());
            foreach (var t in ActiveTwisters)
                hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, t.HitboxRadius));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var twister in ActiveTwisters)
                arena.AddCircle(twister.Position, twister.HitboxRadius, ArenaColor.Danger);
        }
    }

    // TODO: do we need anything specific here? maybe AI to stun/slow adds?
    //class P4Dreadknights : BossComponent
    //{
    //}

    class P4AI : BossComponent
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var e in hints.PotentialTargets)
            {
                switch ((OID)e.Actor.OID)
                {
                    case OID.Boss:
                        e.Priority = 1;
                        e.DesiredPosition = new(-7, -15);
                        e.DesiredRotation = 180.Degrees();
                        break;
                    case OID.Dreadknight:
                        e.Priority = 2;
                        break;
                }
            }
        }
    }
}
