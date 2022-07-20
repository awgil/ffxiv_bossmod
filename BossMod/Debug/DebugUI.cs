using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

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
        private DebugInput _debugInput;
        private DebugAI _debugAI;

        public DebugUI(WorldState ws, Autorotation autorot, InputOverride inputOverride)
        {
            _ws = ws;
            _autorot = autorot;
            _debugInput = new(inputOverride, autorot);
            _debugAI = new(autorot);
        }

        public void Dispose()
        {
            _debugAI.Dispose();
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
            if (ImGui.CollapsingHeader("UI object list"))
            {
                _debugObjects.DrawUIObjects();
            }
            if (ImGui.CollapsingHeader("Statuses"))
            {
                DrawStatuses();
            }
            if (ImGui.CollapsingHeader("Casting enemies"))
            {
                DrawCastingEnemiesList();
            }
            if (ImGui.CollapsingHeader("Party (dalamud)"))
            {
                _debugParty.DrawPartyDalamud();
            }
            if (ImGui.CollapsingHeader("Party (custom)"))
            {
                _debugParty.DrawPartyCustom();
            }
            if (ImGui.CollapsingHeader("AI"))
            {
                _debugAI.Draw();
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
                var obj = (elem.InstanceID >> 32) == 0 ? Service.ObjectTable.SearchById((uint)elem.InstanceID) : null;
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
                ImGui.TableNextColumn(); ImGui.TextUnformatted(elem.Position.ToString());
            }
            ImGui.EndTable();
        }

        private unsafe void DrawTargets()
        {
            var selfPos = Service.ClientState.LocalPlayer?.Position ?? new();
            var targPos = Service.ClientState.LocalPlayer?.TargetObject?.Position ?? new();
            var angle = Angle.FromDirection(new((targPos - selfPos).XZ()));
            var ts = FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance();
            DrawTarget("Target", ts->Target, selfPos, angle);
            DrawTarget("Soft target", ts->SoftTarget, selfPos, angle);
            DrawTarget("GPose target", ts->GPoseTarget, selfPos, angle);
            DrawTarget("Mouseover", ts->MouseOverTarget, selfPos, angle);
            DrawTarget("Focus", ts->FocusTarget, selfPos, angle);
            ImGui.TextUnformatted($"UI Mouseover: {(Mouseover.Instance?.Object != null ? Utils.ObjectString(Mouseover.Instance.Object) : "<null>")}");

            if (ImGui.Button("Target closest enemy"))
            {
                var closest = Service.ObjectTable.Where(o => o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc && o.SubKind == 5).MinBy(o => (o.Position - selfPos).LengthSquared());
                if (closest != null)
                    Service.TargetManager.SetTarget(closest);
            }
        }

        private unsafe void DrawTarget(string kind, FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* obj, Vector3 selfPos, Angle refAngle)
        {
            if (obj == null)
                return;

            var dist = (obj->Position - selfPos).Length();
            var angle = Angle.FromDirection(new((obj->Position - selfPos).XZ())) - refAngle;
            var visHalf = Angle.Asin(obj->HitboxRadius / dist);
            ImGui.TextUnformatted($"{kind}: {Utils.ObjectString(obj->ObjectID)}, hb={obj->HitboxRadius} ({visHalf}), dist={dist}, angle={angle} ({Math.Max(0, angle.Abs().Rad - visHalf.Rad).Radians()})");
        }
    }
}
