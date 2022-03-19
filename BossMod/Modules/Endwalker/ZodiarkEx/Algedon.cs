namespace BossMod.Endwalker.ZodiarkEx
{
    using static BossModule;

    // state related to algedon mechanic
    class Algedon : Component
    {
        private Actor? _caster;

        private static AOEShapeRect _shape = new(60, 15);

        public bool Done => _caster == null;

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_shape.Check(actor.Position, _caster))
                hints.Add("GTFO from diagonal aoe!");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            _shape.Draw(arena, _caster);
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AlgedonAOE))
                _caster = actor;
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AlgedonAOE))
                _caster = null;
        }
    }
}
