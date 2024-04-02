using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ImGuiNET;
using System.Text;

namespace BossMod;

public class DebugObjects
{
    private UITree _tree = new();
    private bool _showCrap = false;
    private uint _selectedID = 0;

    public unsafe void DrawObjectTable()
    {
        ImGui.Checkbox("Show players, minions and mounts", ref _showCrap);

        GameObject? selected = null;
        for (int i = 0; i < Service.ObjectTable.Length; ++i)
        {
            var obj = Service.ObjectTable[i];
            if (obj == null)
                continue;
            if (!_showCrap && obj.ObjectKind is Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player or Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Companion or Dalamud.Game.ClientState.Objects.Enums.ObjectKind.MountType)
                continue;

            var internalObj = Utils.GameObjectInternal(obj);
            var localID = Utils.ReadField<uint>(internalObj, 0x78);
            var uniqueID = obj.ObjectId != 0xE0000000 ? obj.ObjectId : localID;

            var posRot = new Vector4(obj.Position.X, obj.Position.Y, obj.Position.Z, obj.Rotation);
            foreach (var n in _tree.Node($"#{i} {Utils.ObjectString(obj)} ({localID:X}) ({Utils.ObjectKindString(obj)}) {Utils.PosRotString(posRot)}###{uniqueID:X}", contextMenu: () => ObjectContextMenu(obj), select: () => _selectedID = uniqueID))
            {
                var character = obj as Character;
                var battleChara = obj as BattleChara;
                var internalChara = Utils.BattleCharaInternal(battleChara);

                _tree.LeafNode($"Gimmick ID: {Utils.ReadField<uint>(internalObj, 0x7C):X}");
                _tree.LeafNode($"Radius: {obj.HitboxRadius:f3}");
                _tree.LeafNode($"Owner: {Utils.ObjectString(obj.OwnerId)}");
                _tree.LeafNode($"BNpcBase/Name: {obj.DataId}/{Utils.GameObjectInternal(obj)->GetNpcID()}");
                _tree.LeafNode($"Targetable: {obj.IsTargetable}");
                _tree.LeafNode($"Friendly: {Utils.GameObjectIsFriendly(obj)}");
                _tree.LeafNode($"Is character: {internalObj->IsCharacter()}");
                _tree.LeafNode($"Event state: {Utils.GameObjectEventState(obj)}");
                if (character != null)
                {
                    _tree.LeafNode($"Class: {(Class)character.ClassJob.Id} ({character.ClassJob.Id})");
                    _tree.LeafNode($"HP: {character.CurrentHp}/{character.MaxHp} ({Utils.CharacterShieldValue(character)})");
                    _tree.LeafNode($"Status flags: {character.StatusFlags}");
                }
                if (battleChara != null)
                {
                    _tree.LeafNode($"Cast: {Utils.CastTimeString(battleChara.CurrentCastTime, battleChara.TotalCastTime)} {new ActionID((ActionType)battleChara.CastActionType, battleChara.CastActionId)}");
                    foreach (var nn in _tree.Node("Statuses"))
                    {
                        for (int j = 0; j < battleChara.StatusList.Length; ++j)
                        {
                            var s = battleChara.StatusList[j];
                            if (s == null || s.StatusId == 0)
                                continue;
                            _tree.LeafNode($"#{j}: {Utils.StatusString(s.StatusId)} ({s.Param:X}) from {Utils.ObjectString(s.SourceId)}, {s.RemainingTime:f3}s left");
                        }
                    }
                }
            }

            if (uniqueID == _selectedID)
                selected = obj;
        }

        if (selected != null)
        {
            var h = new Vector3(0, Utils.GameObjectInternal(selected)->Height, 0);
            Camera.Instance?.DrawWorldLine(Service.ClientState.LocalPlayer?.Position ?? default, selected.Position, 0xff0000ff);
            Camera.Instance?.DrawWorldCircle(selected.Position, selected.HitboxRadius, 0xff00ff00);
            Camera.Instance?.DrawWorldCircle(selected.Position + h, selected.HitboxRadius, 0xff00ff00);
            Camera.Instance?.DrawWorldCircle(selected.Position - h, selected.HitboxRadius, 0xff00ff00);
            int numSegments = CurveApprox.CalculateCircleSegments(selected.HitboxRadius, 360.Degrees(), 1);
            for (int i = 0; i < numSegments; ++i)
            {
                var p = selected.Position + selected.HitboxRadius * (i * 360.0f / numSegments).Degrees().ToDirection().ToVec3();
                Camera.Instance?.DrawWorldLine(p - h, p + h, 0xff00ff00);
            }
        }
    }

    public unsafe void DrawUIObjects()
    {
        var module = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->UIModule->GetUI3DModule();
        var objs = (FFXIVClientStructs.FFXIV.Client.UI.UI3DModule.ObjectInfo*)module->ObjectInfoArray;
        ImGui.BeginTable("uiobj", 3, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("Index");
        ImGui.TableSetupColumn("GameObj");
        ImGui.TableSetupColumn("NamePlateKind");
        ImGui.TableHeadersRow();
        for (int i = 0; i < 426; ++i)
        {
            var o = objs[i].GameObject;
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{i}: {(ulong)o:X}");
            ImGui.TableNextColumn(); if (o != null) ImGui.TextUnformatted($"{o->DataID:X} '{MemoryHelper.ReadSeString((IntPtr)o->Name, 64)}' <{o->ObjectID:X}>");
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{objs[i].NamePlateObjectKind}");
        }
        ImGui.EndTable();
    }

    public static unsafe void DumpObjectTable()
    {
        var res = new StringBuilder("--- object table dump ---");
        foreach (var obj in Service.ObjectTable)
        {
            var internalObj = Utils.GameObjectInternal(obj);
            res.Append($"\nobj {Utils.ObjectString(obj)}, kind={Utils.ObjectKindString(obj)}, pos={Utils.Vec3String(obj.Position)}, rot={obj.Rotation.Radians()}, renderFlags={internalObj->RenderFlags}, vfxScale={internalObj->VfxScale}, targetable={internalObj->GetIsTargetable()}, drawPtr=0x{(IntPtr)internalObj->DrawObject:X}");
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
                    res.Append($", castAction={new ActionID((ActionType)chara.CastActionType, chara.CastActionId)}, castTarget={targetString}, castLoc={Utils.Vec3String(Utils.BattleCharaCastLocation(chara))}, castTime={Utils.CastTimeString(chara.CurrentCastTime, chara.TotalCastTime)}");
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

    private unsafe void ObjectContextMenu(GameObject obj)
    {
        if (ImGui.MenuItem("Target"))
        {
            Service.TargetManager.Target = obj;
        }

        if (ImGui.MenuItem("Interact"))
        {
            TargetSystem.Instance()->InteractWithObject(Utils.GameObjectInternal(obj));
        }
    }
}
