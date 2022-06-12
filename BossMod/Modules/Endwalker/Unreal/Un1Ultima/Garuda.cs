using System;

namespace BossMod.Endwalker.Unreal.Un1Ultima
{
    class Garuda : BossModule.Component
    {
        private bool _vulcanBurstImminent;
        private Actor? _mistralSong;
        private Actor? _eots;
        private Actor? _geocrush;

        private static AOEShapeCone _aoeMistralSong = new(20, 75.Degrees());
        private static AOEShapeDonut _aoeEOTS = new(13, 25); // TODO: check inner range
        private static AOEShapeCircle _aoeGeocrush = new(18); // TODO: check falloff

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_aoeMistralSong != null)
            {
                var adjPos = _vulcanBurstImminent ? module.Arena.ClampToBounds(BossModule.AdjustPositionForKnockback(actor.Position, _mistralSong, 30)) : actor.Position;
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
            var adjPos = _vulcanBurstImminent ? arena.ClampToBounds(BossModule.AdjustPositionForKnockback(pc.Position, _mistralSong, 30)) : pc.Position;
            if (adjPos != pc.Position)
            {
                arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                arena.Actor(adjPos, new(), arena.ColorDanger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.MistralSong:
                    _mistralSong = actor;
                    _vulcanBurstImminent = true;
                    break;
                case AID.EyeOfTheStorm:
                    _eots = actor;
                    break;
                case AID.Geocrush:
                    _geocrush = actor;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
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

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.VulcanBurst))
                _vulcanBurstImminent = false;
        }
    }
}
