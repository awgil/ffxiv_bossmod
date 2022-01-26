using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Text;

namespace BossMod
{
    public class DebugObjects
    {
        private bool _showCrap = false;

        public unsafe void DrawObjectTable()
        {
            ImGui.Checkbox("Show players, minions and mounts", ref _showCrap);

            int i = 0;
            ImGui.BeginTable("objects", 9);
            ImGui.TableSetupColumn("Index");
            ImGui.TableSetupColumn("Actor");
            ImGui.TableSetupColumn("Kind/Subkind");
            ImGui.TableSetupColumn("Class");
            ImGui.TableSetupColumn("OwnerID");
            ImGui.TableSetupColumn("Position");
            ImGui.TableSetupColumn("Rotation");
            ImGui.TableSetupColumn("Render flags");
            ImGui.TableSetupColumn("Draw data");
            ImGui.TableHeadersRow();
            foreach (var obj in Service.ObjectTable)
            {
                int idx = i++;
                bool isCrap = obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player
                    || obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Companion
                    || obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.MountType;
                if (isCrap && !_showCrap)
                    continue;

                var character = obj as Character;
                var internalObj = Utils.GameObjectInternal(obj);
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text($"{idx}");
                ImGui.TableNextColumn(); ImGui.Text(Utils.ObjectString(obj));
                ImGui.TableNextColumn(); ImGui.Text(Utils.ObjectKindString(obj));
                ImGui.TableNextColumn(); ImGui.Text(character != null ? Utils.CharacterClassString(character.ClassJob.Id) : "---");
                ImGui.TableNextColumn(); ImGui.Text($"{obj.OwnerId:X}");
                ImGui.TableNextColumn(); ImGui.Text(Utils.Vec3String(obj.Position));
                ImGui.TableNextColumn(); ImGui.Text(Utils.RadianString(obj.Rotation));
                ImGui.TableNextColumn(); ImGui.Text($"{internalObj->RenderFlags:X}");
                ImGui.TableNextColumn(); ImGui.Text($"0x{(IntPtr)internalObj->DrawObject:X}");
            }
            ImGui.EndTable();
        }

        public static unsafe void DumpObjectTable()
        {
            var res = new StringBuilder("--- object table dump ---");
            foreach (var obj in Service.ObjectTable)
            {
                var internalObj = Utils.GameObjectInternal(obj);
                res.Append($"\nobj {Utils.ObjectString(obj)}, kind={Utils.ObjectKindString(obj)}, pos={Utils.Vec3String(obj.Position)}, rot={Utils.RadianString(obj.Rotation)}, renderFlags={internalObj->RenderFlags}, vfxScale={internalObj->VfxScale}, targetable={internalObj->GetIsTargetable()}, drawPtr=0x{(IntPtr)internalObj->DrawObject:X}");
                if (internalObj->DrawObject != null)
                {
                    res.Append($", drawPos={Utils.Vec3String(internalObj->DrawObject->Object.Position)}, drawScale={Utils.Vec3String(internalObj->DrawObject->Object.Scale)}");
                }

                var chara = obj as BattleChara;
                if (chara)
                {
                    res.Append($", vfxObj=0x{Utils.ReadField<ulong>(internalObj, 0x1840):X}/0x{Utils.ReadField<ulong>(internalObj, 0x1848):X}");
                    if (chara!.IsCasting)
                    {
                        var target = Service.ObjectTable.SearchById(chara.CastTargetObjectId);
                        var targetString = target ? Utils.ObjectString(target!) : "unknown";
                        res.Append($", castAction={Utils.ActionString(chara.CastActionId, (WorldState.ActionType)chara.CastActionType)}, castTarget={targetString}, castLoc={Utils.Vec3String(Utils.BattleCharaCastLocation(chara))}, castTime={Utils.CastTimeString(chara.CurrentCastTime, chara.TotalCastTime)}");
                    }
                    foreach (var status in chara!.StatusList)
                    {
                        var src = status.SourceObject ? Utils.ObjectString(status.SourceObject!) : "none";
                        res.Append($"\n  status {status.StatusId} '{status.GameData.Name}': param={status.Param}, stacks={status.StackCount}, time={status.RemainingTime:f2}, source={src}");
                    }
                }
            }
            Service.Log(res.ToString());
        }
    }
}
