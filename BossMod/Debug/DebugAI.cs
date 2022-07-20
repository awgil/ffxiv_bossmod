using ImGuiNET;
using System;

namespace BossMod
{
    class DebugAI : IDisposable
    {
        private Autorotation _autorot;
        private MiniArena _arena = new(new(), new ArenaBoundsCircle(new(), 10));
        private AI.AvoidAOE _avoid;
        private float _arenaRadius = 10;
        private float _desiredRange = 5;
        private CommonActions.Positional _desiredPositional = CommonActions.Positional.Any;

        public DebugAI(Autorotation autorot)
        {
            _autorot = autorot;
            _avoid = new(autorot.Bossmods);
        }

        public void Dispose()
        {
            _avoid.Dispose();
        }

        public void Draw()
        {
            ImGui.SliderFloat("Radius", ref _arenaRadius, 5, 50);
            ImGui.SliderFloat("Max range", ref _desiredRange, 0, 25);
            ImGui.SameLine();
            if (ImGui.RadioButton("Any", _desiredPositional == CommonActions.Positional.Any))
                _desiredPositional = CommonActions.Positional.Any;
            ImGui.SameLine();
            if (ImGui.RadioButton("Flank", _desiredPositional == CommonActions.Positional.Flank))
                _desiredPositional = CommonActions.Positional.Flank;
            ImGui.SameLine();
            if (ImGui.RadioButton("Rear", _desiredPositional == CommonActions.Positional.Rear))
                _desiredPositional = CommonActions.Positional.Rear;

            var player = _autorot.WorldState.Party.Player();
            var playerPos = player?.Position ?? new();
            var target = _autorot.WorldState.Actors.Find(player?.TargetID ?? 0);
            _avoid.SetDesired(target?.Position, target?.Rotation ?? new(), _desiredRange, _desiredPositional);
            var safe = player != null ? _avoid.Update(player) : null;
            ImGui.TextUnformatted($"Safespot: {safe} ({_avoid.SafeZone.ChildCount})");

            _arena.Bounds = new ArenaBoundsCircle(playerPos, _arenaRadius);
            var forbiddenZone = new Clip2D().Difference(SafeZone.DefaultBounds(playerPos), _avoid.SafeZone);
            _arena.Begin(Camera.Instance?.CameraAzimuth ?? 0);
            _arena.Zone(Clip2D.Triangulate(forbiddenZone), ArenaColor.AOE);
            _arena.Zone(Clip2D.Triangulate(_avoid.DesiredZone), ArenaColor.SafeFromAOE);
            _arena.Actor(player, ArenaColor.PC);
            _arena.Actor(target, ArenaColor.Enemy);
            if (safe != null)
                _arena.AddLine(playerPos, safe.Value, ArenaColor.Safe);
            _arena.End();

            ImGui.BeginTable("targets", 6, ImGuiTableFlags.Resizable);
            ImGui.TableSetupColumn("Object");
            ImGui.TableSetupColumn("Rotation");
            ImGui.TableSetupColumn("Dist from self");
            ImGui.TableSetupColumn("Dist from target");
            ImGui.TableSetupColumn("Cast");
            ImGui.TableSetupColumn("Auto AOE");
            ImGui.TableHeadersRow();
            foreach (var actor in _autorot.PotentialTargets)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextUnformatted($"{actor.OID:X} '{actor.Name} ({actor.InstanceID:X})");
                ImGui.TableNextColumn(); ImGui.TextUnformatted(actor.Rotation.ToString());
                ImGui.TableNextColumn(); ImGui.TextUnformatted(player != null ? $"{(actor.Position - player.Position).Length():f3}" : "---");
                ImGui.TableNextColumn(); ImGui.TextUnformatted(target != null ? $"{(actor.Position - target.Position).Length():f3}" : "---");
                ImGui.TableNextColumn(); ImGui.TextUnformatted(actor.CastInfo != null ? actor.CastInfo.Action.ToString() : "");
                ImGui.TableNextColumn(); ImGui.TextUnformatted(DescribeAutoAOE(actor));
            }
            ImGui.EndTable();
        }

        private string DescribeAutoAOE(Actor a)
        {
            if (a.CastInfo == null)
                return "";
            var data = a.CastInfo.IsSpell() ? Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(a.CastInfo.Action.ID) : null;
            if (data == null)
                return $"unknown";
            var origin = _autorot.WorldState.Actors.Find(a.CastInfo.TargetID)?.Position ?? a.CastInfo.LocXZ;
            switch (data.CastType)
            {
                case 0:
                    return $"n/a";
                case 1:
                    return $"n/a (st)";
                case 2:
                case 5:
                case 7:
                    return $"circle-{data.CastType} {origin} {data.EffectRange}";
                case 3:
                case 13:
                    return $"cone-{data.CastType} {(DetectConeAngle(data)?.ToString() ?? "???")} {origin} {a.Rotation} {data.EffectRange}";
                case 4:
                case 12:
                    return $"rect-{data.CastType} {origin} {a.Rotation} {data.EffectRange} {data.XAxisModifier}";
                case 8:
                    return $"charge-{data.CastType} {a.Position} -> {origin} x {data.XAxisModifier}";
                case 10:
                    return $"donut-{data.CastType} {origin} ???-{data.EffectRange}";
                default:
                    return $"unknown-{data.CastType} {origin} {a.Rotation} {data.EffectRange} {data.XAxisModifier}";
            }
        }

        private Angle? DetectConeAngle(Lumina.Excel.GeneratedSheets.Action data)
        {
            var omen = data.Omen.Value;
            if (omen == null)
                return null;
            var path = omen.Path.ToString();
            var pos = path.IndexOf("fan");
            if (pos < 0)
                return null;
            return int.Parse(path.Substring(pos + 3, 3)).Degrees();
        }
    }
}
