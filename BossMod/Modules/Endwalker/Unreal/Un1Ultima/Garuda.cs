namespace BossMod.Endwalker.Unreal.Un1Ultima
{
    class Garuda : BossComponent
    {
        private bool _vulcanBurstImminent;
        private Actor? _mistralSong;
        private Actor? _eots;
        private Actor? _geocrush;

        private static AOEShapeCone _aoeMistralSong = new(20, 75.Degrees());
        private static AOEShapeDonut _aoeEOTS = new(13, 25); // TODO: check inner range
        private static AOEShapeCircle _aoeGeocrush = new(18); // TODO: check falloff

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_aoeMistralSong != null)
            {
                var adjPos = _vulcanBurstImminent ? module.Bounds.ClampToBounds(Components.Knockback.AwayFromSource(actor.Position, _mistralSong, 30)) : actor.Position;
                if (_aoeMistralSong.Check(adjPos, _mistralSong))
                    hints.Add("GTFO from aoe!");
            }

            if (_eots != null)
                hints.Add("Stand near inner edge", _aoeEOTS.Check(actor.Position, _eots));
            else if (_aoeGeocrush.Check(actor.Position, _geocrush))
                hints.Add("Go to edge!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _aoeMistralSong.Draw(arena, _mistralSong);
            _aoeEOTS.Draw(arena, _eots);
            if (_eots == null)
                _aoeGeocrush.Draw(arena, _geocrush);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var adjPos = _vulcanBurstImminent ? arena.Bounds.ClampToBounds(Components.Knockback.AwayFromSource(pc.Position, _mistralSong, 30)) : pc.Position;
            if (adjPos != pc.Position)
            {
                arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
                arena.Actor(adjPos, 0.Degrees(), ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.MistralSong:
                    _mistralSong = caster;
                    _vulcanBurstImminent = true;
                    break;
                case AID.EyeOfTheStorm:
                    _eots = caster;
                    break;
                case AID.Geocrush:
                    _geocrush = caster;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.MistralSong:
                    _mistralSong = null;
                    _vulcanBurstImminent = false;
                    break;
                case AID.EyeOfTheStorm:
                    _eots = null;
                    break;
                case AID.Geocrush:
                    _geocrush = null;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.VulcanBurst)
                _vulcanBurstImminent = false;
        }
    }
}
