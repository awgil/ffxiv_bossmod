using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    class DebugUI : IDisposable
    {
        private WorldState _ws;
        private Autorotation _autorot;
        private DebugObjects _debugObjects = new();
        private DebugParty _debugParty = new();
        private DebugGraphics _debugGraphics = new();
        private DebugAction _debugAction = new();
        private DebugHate _debugHate = new();
        private DebugInput _debugInput = new();

        public DebugUI(WorldState ws, Autorotation autorot)
        {
            _ws = ws;
            _autorot = autorot;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            ImGui.TextUnformatted($"Current zone: {_ws.CurrentZone}, pos = {Utils.Vec3String(Service.ClientState.LocalPlayer?.Position ?? new Vector3())}");
            if (ImGui.Button("Perform full dump"))
            {
                DebugObjects.DumpObjectTable();
                DebugGraphics.DumpScene();
            }

            if (ImGui.CollapsingHeader("Full object list"))
            {
                _debugObjects.DrawObjectTable();
            }
            if (ImGui.CollapsingHeader("Statuses"))
            {
                DrawStatuses();
            }
            if (ImGui.CollapsingHeader("Casting enemies"))
            {
                DrawCastingEnemiesList();
            }
            if (ImGui.CollapsingHeader("Party"))
            {
                _debugParty.DrawParty();
            }
            if (ImGui.CollapsingHeader("Graphics scene"))
            {
                _debugGraphics.DrawSceneTree();
            }
            if (ImGui.CollapsingHeader("Graphics watch"))
            {
                _debugGraphics.DrawWatchedMods();
            }
            if (Camera.Instance != null && ImGui.CollapsingHeader("Matrices"))
            {
                _debugGraphics.DrawMatrices();
            }
            if (ImGui.CollapsingHeader("Actions"))
            {
                _debugAction.DrawActionData();
            }
            if (ImGui.CollapsingHeader("Hate"))
            {
                _debugHate.Draw();
            }
            if (ImGui.CollapsingHeader("Targets"))
            {
                DrawTargets();
            }
            if (ImGui.CollapsingHeader("Input"))
            {
                _debugInput.Draw();
            }
        }

        private void DrawStatuses()
        {
            foreach (var elem in _ws.Actors)
            {
                var obj = Service.ObjectTable.SearchById(elem.InstanceID);
                if (ImGui.TreeNode(Utils.ObjectString(obj!)))
                {
                    var chara = obj as BattleChara;
                    if (chara != null)
                    {
                        foreach (var status in chara.StatusList)
                        {
                            var src = status.SourceObject ? Utils.ObjectString(status.SourceObject!) : "none";
                            ImGui.TextUnformatted($"{status.StatusId} '{status.GameData.Name}': param={status.Param}, stacks={status.StackCount}, time={status.RemainingTime:f2}, source={src}");
                        }
                    }
                    ImGui.TreePop();
                }
            }
        }

        private void DrawCastingEnemiesList()
        {
            ImGui.BeginTable("enemies", 6);
            ImGui.TableSetupColumn("Caster");
            ImGui.TableSetupColumn("Target");
            ImGui.TableSetupColumn("Action");
            ImGui.TableSetupColumn("Time");
            ImGui.TableSetupColumn("Location");
            ImGui.TableSetupColumn("Position");
            ImGui.TableHeadersRow();
            foreach (var elem in _ws.Actors)
            {
                if (elem.CastInfo == null || elem.Type != ActorType.Enemy)
                    continue;

                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.ObjectString(elem.InstanceID));
                ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.ObjectString(elem.CastInfo.TargetID));
                ImGui.TableNextColumn(); ImGui.TextUnformatted(elem.CastInfo.Action.ToString());
                ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.CastTimeString(elem.CastInfo, _ws.CurrentTime));
                ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.Vec3String(elem.CastInfo.Location));
                ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.Vec3String(elem.Position));
            }
            ImGui.EndTable();
        }

        private unsafe void DrawTargets()
        {
            var selfPos = Service.ClientState.LocalPlayer?.Position ?? new();
            var targPos = Service.ClientState.LocalPlayer?.TargetObject?.Position ?? new();
            var angle = GeometryUtils.DirectionFromVec3(targPos - selfPos);
            var ts = FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance();
            DrawTarget("Target", ts->Target, selfPos, angle);
            DrawTarget("Soft target", ts->SoftTarget, selfPos, angle);
            DrawTarget("GPose target", ts->GPoseTarget, selfPos, angle);
            DrawTarget("Mouseover", ts->MouseOverTarget, selfPos, angle);
            DrawTarget("Focus", ts->FocusTarget, selfPos, angle);
            ImGui.TextUnformatted($"UI Mouseover: {(Mouseover.Instance?.Object != null ? Utils.ObjectString(Mouseover.Instance.Object) : "<null>")}");
        }

        private unsafe void DrawTarget(string kind, FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* obj, Vector3 selfPos, float refAngle)
        {
            if (obj == null)
                return;

            var dist = (obj->Position - selfPos).Length();
            var angle = GeometryUtils.DirectionFromVec3(obj->Position - selfPos) - refAngle;
            var visHalf = MathF.Asin(obj->HitboxRadius / dist);
            ImGui.TextUnformatted($"{kind}: {Utils.ObjectString(obj->ObjectID)}, hb={obj->HitboxRadius} ({Utils.RadianString(visHalf)}), dist={dist}, angle={Utils.RadianString(angle)} ({Utils.RadianString(Math.Max(0, Math.Abs(angle) - visHalf))})");
        }
    }
}
