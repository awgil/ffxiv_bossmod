using System;

namespace BossMod.RealmReborn.Extreme.Ex1Ultima
{
    // note that it could be a GenericAOEs, but we customize everything anyway...
    class EyeOfTheStormGeocrush : BossComponent
    {
        private Actor? _eotsCaster;
        private Actor? _geocrushCaster;
        public bool Active => _eotsCaster != null || _geocrushCaster != null;

        private static AOEShapeDonut _aoeEOTS = new(12, 25);
        private static AOEShapeCircle _aoeGeocrush = new(18); // TODO: check falloff

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_eotsCaster != null)
                hints.Add("Stand near inner edge", _aoeEOTS.Check(actor.Position, _eotsCaster));
            else if (_aoeGeocrush.Check(actor.Position, _geocrushCaster))
                hints.Add("Go to edge!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (_eotsCaster != null)
            {
                // we want to stand in a small ring near inner edge of aoe
                var inner = ShapeDistance.Circle(_eotsCaster.Position, _aoeEOTS.InnerRadius - 2);
                var outer = ShapeDistance.InvertedCircle(_eotsCaster.Position, _aoeEOTS.InnerRadius);
                hints.AddForbiddenZone(p => Math.Min(inner(p), outer(p)), _eotsCaster.CastInfo!.NPCFinishAt);
            }
            else if (_geocrushCaster != null)
            {
                hints.AddForbiddenZone(_aoeGeocrush, _geocrushCaster.Position, new(), _geocrushCaster.CastInfo!.NPCFinishAt);
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_eotsCaster != null)
                _aoeEOTS.Draw(arena, _eotsCaster);
            else if (_geocrushCaster != null)
                _aoeGeocrush.Draw(arena, _geocrushCaster);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.EyeOfTheStorm:
                    _eotsCaster = caster;
                    break;
                case AID.Geocrush:
                    _geocrushCaster = caster;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.EyeOfTheStorm:
                    _eotsCaster = null;
                    break;
                case AID.Geocrush:
                    _geocrushCaster = null;
                    break;
            }
        }
    }
}
