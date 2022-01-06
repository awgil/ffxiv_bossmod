using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    class DebugUI : IDisposable
    {
        private EventGenerator _gen;
        private DateTime _combatStart;
        private DebugObjects _debugObjects = new();
        private DebugGraphics _debugGraphics = new();

        public DebugUI(EventGenerator gen)
        {
            _gen = gen;
            _gen.PlayerCombatEnter += EnterCombat;
        }

        public void Dispose()
        {
            _gen.PlayerCombatEnter -= EnterCombat;
        }

        public void Draw()
        {
            string combatTime = _gen.PlayerInCombat ? (DateTime.Now - _combatStart).ToString() : "---";
            ImGui.Text($"Current zone: {_gen.CurrentZone}, pos = {Utils.Vec3String(Service.ClientState.LocalPlayer?.Position ?? new Vector3())}");
            ImGui.Text($"Combat time: {combatTime}, target = {_gen.ActorString(Service.ClientState.LocalPlayer?.TargetObjectId ?? 0)}");
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
            if (ImGui.CollapsingHeader("Graphics scene"))
            {
                _debugGraphics.DrawSceneTree();
            }
            if (ImGui.CollapsingHeader("Graphics watch"))
            {
                _debugGraphics.DrawWatchedMods();
            }
        }

        private void DrawStatuses()
        {
            foreach (var elem in _gen.Actors)
            {
                if (ImGui.TreeNode(Utils.ObjectString(elem.Value.Chara)))
                {
                    foreach (var status in elem.Value.Chara.StatusList)
                    {
                        var src = status.SourceObject ? Utils.ObjectString(status.SourceObject!) : "none";
                        ImGui.Text($"{status.StatusId} '{status.GameData.Name}': param={status.Param}, stacks={status.StackCount}, time={status.RemainingTime:f2}, source={src}");
                    }
                    ImGui.TreePop();
                }
            }
        }

        private void DrawCastingEnemiesList()
        {
            ImGui.BeginTable("enemies", 5);
            ImGui.TableSetupColumn("Caster");
            ImGui.TableSetupColumn("Target");
            ImGui.TableSetupColumn("Action");
            ImGui.TableSetupColumn("Time");
            ImGui.TableSetupColumn("Position");
            ImGui.TableHeadersRow();
            foreach (var elem in _gen.Actors)
            {
                if (!elem.Value.IsCasting)
                    continue;
                if (elem.Value.Chara.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc)
                    continue;
                if (elem.Value.Chara.SubKind != (byte)Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind.Enemy)
                    continue;

                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text(Utils.ObjectString(elem.Value.Chara));
                ImGui.TableNextColumn(); ImGui.Text(_gen.ActorString(elem.Value.CastTargetID));
                ImGui.TableNextColumn(); ImGui.Text(Utils.ActionString(elem.Value.CastActionID));
                ImGui.TableNextColumn(); ImGui.Text(Utils.CastTimeString(elem.Value.CastCurrentTime, elem.Value.CastTotalTime));
                ImGui.TableNextColumn(); ImGui.Text(Utils.Vec3String(elem.Value.Chara.Position));
            }
            ImGui.EndTable();
        }

        private void EnterCombat(object? sender, PlayerCharacter? pc)
        {
            _combatStart = DateTime.Now;
        }
    }
}
