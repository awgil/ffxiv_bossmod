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
        private DateTime _combatStart;
        private DebugObjects _debugObjects = new();
        private DebugParty _debugParty = new();
        private DebugGraphics _debugGraphics = new();
        private DebugAction _debugAction = new();
        private DebugHate _debugHate = new();

        public DebugUI(WorldState ws, Autorotation autorot)
        {
            _ws = ws;
            _autorot = autorot;

            _ws.PlayerInCombatChanged += EnterExitCombat;
        }

        public void Dispose()
        {
            _ws.PlayerInCombatChanged -= EnterExitCombat;
        }

        public void Draw()
        {
            string combatTime = _ws.PlayerInCombat ? (_ws.CurrentTime - _combatStart).ToString() : "---";
            ImGui.Text($"Current zone: {_ws.CurrentZone}, pos = {Utils.Vec3String(Service.ClientState.LocalPlayer?.Position ?? new Vector3())}");
            ImGui.Text($"Combat time: {combatTime}, target = {Utils.ObjectString(Service.ClientState.LocalPlayer?.TargetObjectId ?? 0)}");
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
            if (ImGui.CollapsingHeader("WAR"))
            {
                _autorot.WarActions.DrawDebug();
            }
            if (ImGui.CollapsingHeader("Hate"))
            {
                _debugHate.Draw();
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
                            ImGui.Text($"{status.StatusId} '{status.GameData.Name}': param={status.Param}, stacks={status.StackCount}, time={status.RemainingTime:f2}, source={src}");
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
                ImGui.TableNextColumn(); ImGui.Text(Utils.ObjectString(elem.InstanceID));
                ImGui.TableNextColumn(); ImGui.Text(Utils.ObjectString(elem.CastInfo.TargetID));
                ImGui.TableNextColumn(); ImGui.Text(elem.CastInfo.Action.ToString());
                ImGui.TableNextColumn(); ImGui.Text(Utils.CastTimeString(elem.CastInfo, _ws.CurrentTime));
                ImGui.TableNextColumn(); ImGui.Text(Utils.Vec3String(elem.CastInfo.Location));
                ImGui.TableNextColumn(); ImGui.Text(Utils.Vec3String(elem.Position));
            }
            ImGui.EndTable();
        }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            _combatStart = DateTime.Now;
        }
    }
}
