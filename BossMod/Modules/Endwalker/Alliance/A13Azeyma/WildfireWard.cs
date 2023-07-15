using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A13Azeyma
{
    class WildfireWard : BossComponent
    {
        private bool _active;
        private List<Actor> _glimpse = new();

        private static WPos[] _tri = { new(-750, -762), new(-760.392f, -744), new(-739.608f, -744) };
        private static float _knockbackDistance = 15;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!_active)
                return;
            if (!actor.Position.InTri(_tri[0], _tri[1], _tri[2]))
                hints.Add("Go to safe zone!");
            var adjPos = AdjustedPosition(actor);
            if (adjPos != actor.Position && !adjPos.InTri(_tri[0], _tri[1], _tri[2]))
                hints.Add("About to be knocked into fire!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_active)
                arena.ZoneTri(_tri[0], _tri[1], _tri[2], ArenaColor.SafeFromAOE);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var adjPos = AdjustedPosition(pc);
            if (adjPos != pc.Position)
            {
                arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
                arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.WildfireWard:
                    _active = true;
                    break;
                case AID.IlluminatingGlimpse:
                    _glimpse.Add(caster);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.IlluminatingGlimpse)
            {
                _glimpse.Remove(caster);
                if (_glimpse.Count == 0)
                    _active = false;
            }
        }

        private WPos AdjustedPosition(Actor actor)
        {
            var glimpse = _glimpse.FirstOrDefault();
            if (glimpse == null)
                return actor.Position;

            var dir = glimpse.Rotation.ToDirection();
            var normal = dir.OrthoL();
            return actor.Position + normal * _knockbackDistance;
        }
    }
}
