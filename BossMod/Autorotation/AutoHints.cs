using System;
using System.Collections.Generic;

namespace BossMod
{
    // utility that determines ai hints automatically based on actor casts
    // this is used e.g. in outdoor or on trash, where we have no active bossmodules
    public class AutoHints : IDisposable
    {
        private WorldState _ws;
        private Dictionary<ulong, (Actor Caster, Actor? Target, AOEShape Shape, bool IsCharge)> _activeAOEs = new();

        public AutoHints(WorldState ws)
        {
            _ws = ws;
            _ws.Actors.CastStarted += OnCastStarted;
            _ws.Actors.CastFinished += OnCastFinished;
        }

        public void Dispose()
        {
            _ws.Actors.CastStarted -= OnCastStarted;
            _ws.Actors.CastFinished -= OnCastFinished;
        }

        public void CalculateAIHints(AIHints hints, WPos playerPos)
        {
            hints.Bounds = new ArenaBoundsSquare(playerPos, 30);
            foreach (var aoe in _activeAOEs.Values)
            {
                var target = aoe.Target?.Position ?? aoe.Caster.CastInfo!.LocXZ;
                var rot = aoe.Caster.CastInfo!.Rotation;
                if (aoe.IsCharge)
                {
                    hints.AddForbiddenZone(ShapeDistance.Rect(aoe.Caster.Position, target, ((AOEShapeRect)aoe.Shape).HalfWidth));
                }
                else
                {
                    hints.AddForbiddenZone(aoe.Shape, target, rot, aoe.Caster.CastInfo.FinishAt);
                }
            }
        }

        private void OnCastStarted(object? sender, Actor actor)
        {
            if (actor.Type != ActorType.Enemy || actor.IsAlly)
                return;
            var data = actor.CastInfo!.IsSpell() ? Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(actor.CastInfo.Action.ID) : null;
            if (data == null || data.CastType == 1)
                return;
            AOEShape? shape = data.CastType switch
            {
                2 => new AOEShapeCircle(data.EffectRange), // used for some point-blank aoes and enemy location-targeted - does not add caster hitbox
                3 => new AOEShapeCone(data.EffectRange + actor.HitboxRadius, DetermineConeAngle(data) * 0.5f),
                4 => new AOEShapeRect(data.EffectRange + actor.HitboxRadius, data.XAxisModifier * 0.5f),
                5 => new AOEShapeCircle(data.EffectRange + actor.HitboxRadius),
                //6 => ???
                //7 => new AOEShapeCircle(data.EffectRange), - used for player ground-targeted circles a-la asylum
                //8 => charge rect
                //10 => new AOEShapeDonut(actor.HitboxRadius, data.EffectRange), // TODO: find a way to determine inner radius (omen examples: 28762 - 4/40 - gl_sircle_4004bp1)
                //11 => cross == 12 + another 12 rotated 90 degrees
                12 => new AOEShapeRect(data.EffectRange, data.XAxisModifier * 0.5f),
                13 => new AOEShapeCone(data.EffectRange, DetermineConeAngle(data) * 0.5f),
                _ => null
            };
            if (shape == null)
            {
                Service.Log($"[AutoHints] Unknown cast type {data.CastType} for {actor.CastInfo.Action}");
                return;
            }
            var target = _ws.Actors.Find(actor.CastInfo.TargetID);
            _activeAOEs[actor.InstanceID] = (actor, target, shape, data.CastType == 8);
        }

        private void OnCastFinished(object? sender, Actor actor)
        {
            _activeAOEs.Remove(actor.InstanceID);
        }

        private Angle DetermineConeAngle(Lumina.Excel.GeneratedSheets.Action data)
        {
            var omen = data.Omen.Value;
            if (omen == null)
            {
                Service.Log($"[AutoHints] No omen data for {data.RowId} '{data.Name}'...");
                return 180.Degrees();
            }
            var path = omen.Path.ToString();
            var pos = path.IndexOf("fan");
            if (pos < 0 || pos + 6 > path.Length)
            {
                Service.Log($"[AutoHints] Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
                return 180.Degrees();
            }

            int angle;
            if (!int.TryParse(path.Substring(pos + 3, 3), out angle))
            {
                Service.Log($"[AutoHints] Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
                return 180.Degrees();
            }

            return angle.Degrees();
        }
    }
}
