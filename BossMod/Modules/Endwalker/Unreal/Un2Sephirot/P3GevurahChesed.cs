namespace BossMod.Endwalker.Unreal.Un2Sephirot
{
    class P3GevurahChesed : Components.CastCounter
    {
        private BitMask _physResistMask;
        private int _physSide; // 0 if not active, -1 if left, +1 if right

        private static AOEShape _shape = new AOEShapeRect(40, 10);

        public P3GevurahChesed() : base(ActionID.MakeSpell(AID.LifeForce)) { } // doesn't matter which spell to track

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var side = ForbiddenSide(slot);
            if (side != 0 && _shape.Check(actor.Position, Origin(side), 0.Degrees()))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var side = ForbiddenSide(pcSlot);
            if (side != 0)
                _shape.Draw(arena, Origin(side), 0.Degrees());
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.ForceAgainstMight)
                _physResistMask.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.GevurahChesed or AID.ChesedGevurah)
                _physSide = (AID)spell.Action.ID == AID.GevurahChesed ? -1 : +1;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.GevurahChesed or AID.ChesedGevurah)
                _physSide = 0;
        }

        private int ForbiddenSide(int slot)
        {
            if (_physSide == 0 || _physResistMask.None())
                return 0;
            return _physResistMask[slot] ? -_physSide : +_physSide;
        }

        private WPos Origin(int side) => new(side * 10, -20);
    }
}
