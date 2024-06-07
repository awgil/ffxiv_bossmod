using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;

namespace BossMod;

class MainDebugWindow(WorldState ws, Autorotation autorot) : UIWindow("Boss mod debug UI", false, new(300, 200))
{
    private readonly DebugObjects _debugObjects = new();
    private readonly DebugParty _debugParty = new();
    private readonly DebugGraphics _debugGraphics = new();
    private readonly DebugAction _debugAction = new(ws);
    private readonly DebugHate _debugHate = new();
    private readonly DebugInput _debugInput = new(autorot);
    private readonly DebugAutorotation _debugAutorot = new(autorot);
    private readonly DebugClassDefinitions _debugClassDefinitions = new(ws);
    private readonly DebugAddon _debugAddon = new();
    private readonly DebugTiming _debugTiming = new();
    private readonly DebugVfx _debugVfx = new();

    protected override void Dispose(bool disposing)
    {
        _debugAction.Dispose();
        _debugInput.Dispose();
        _debugClassDefinitions.Dispose();
        _debugAddon.Dispose();
        _debugVfx.Dispose();
        base.Dispose(disposing);
    }

    public unsafe override void Draw()
    {
        var playerCID = UIState.Instance()->PlayerState.ContentId;
        var player = Service.ClientState.LocalPlayer;
        ImGui.TextUnformatted($"Current zone: {ws.CurrentZone}, player=0x{(ulong)Utils.GameObjectInternal(player):X}, playerCID={playerCID:X}, pos = {Utils.Vec3String(player?.Position ?? new Vector3())}");
        ImGui.TextUnformatted($"ID scramble: {Network.IDScramble.Delta} = {*Network.IDScramble.OffsetAdjusted} - {*Network.IDScramble.OffsetBaseFixed} - {*Network.IDScramble.OffsetBaseChanging}");
        ImGui.TextUnformatted($"Player mode: {Utils.CharacterInternal(player)->Mode}");

        var eventFwk = FFXIVClientStructs.FFXIV.Client.Game.Event.EventFramework.Instance();
        var instanceDirector = eventFwk != null ? eventFwk->GetInstanceContentDirector() : null;
        ImGui.TextUnformatted($"Content time left: {(instanceDirector != null ? $"{instanceDirector->ContentDirector.ContentTimeLeft:f1}" : "n/a")}");

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
            _debugParty.DrawPartyCustom(false);
        }
        if (ImGui.CollapsingHeader("Party (duty recorder)"))
        {
            _debugParty.DrawPartyCustom(true);
        }
        if (ImGui.CollapsingHeader("Autorotation"))
        {
            _debugAutorot.Draw();
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
        if (Camera.Instance != null && ImGui.CollapsingHeader("In-game overlay"))
        {
            _debugGraphics.DrawOverlay();
        }
        if (ImGui.CollapsingHeader("Action manager ex"))
        {
            _debugAction.DrawActionManagerExtensions();
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
        if (ImGui.CollapsingHeader("Class definitions"))
        {
            _debugClassDefinitions.Draw();
        }
        if (ImGui.CollapsingHeader("Player attributes"))
        {
            DrawPlayerAttributes();
        }
        if (ImGui.CollapsingHeader("Countdown"))
        {
            DrawCountdown();
        }
        if (ImGui.CollapsingHeader("Addon / agent"))
        {
            _debugAddon.Draw();
        }
        if (ImGui.CollapsingHeader("Timing"))
        {
            _debugTiming.Draw();
        }
        if (ImGui.CollapsingHeader("Window system"))
        {
            DrawWindowSystem();
        }
        if (ImGui.CollapsingHeader("VFX"))
        {
            _debugVfx.Draw();
        }
        if (ImGui.CollapsingHeader("Limit break"))
        {
            DrawLimitBreak();
        }
    }

    private void DrawStatuses()
    {
        foreach (var elem in ws.Actors)
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
        ImGui.BeginTable("enemies", 7, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("Caster");
        ImGui.TableSetupColumn("Target");
        ImGui.TableSetupColumn("Action");
        ImGui.TableSetupColumn("Time");
        ImGui.TableSetupColumn("Location");
        ImGui.TableSetupColumn("Position");
        ImGui.TableSetupColumn("Rotation");
        ImGui.TableHeadersRow();
        foreach (var elem in ws.Actors)
        {
            if (elem.CastInfo == null || elem.Type != ActorType.Enemy)
                continue;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(Utils.ObjectString(elem.InstanceID));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(Utils.ObjectString(elem.CastInfo.TargetID));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(elem.CastInfo.Action.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(Utils.CastTimeString(elem.CastInfo, ws.CurrentTime));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(Utils.Vec3String(elem.CastInfo.Location));
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(elem.Position.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(elem.CastInfo.Rotation.ToString());
        }
        ImGui.EndTable();
    }

    private unsafe void DrawTargets()
    {
        var cursorPos = ActionManagerEx.Instance?.GetWorldPosUnderCursor();
        ImGui.TextUnformatted($"World pos under cursor: {(cursorPos == null ? "n/a" : Utils.Vec3String(cursorPos.Value))}");

        var selfPos = Service.ClientState.LocalPlayer?.Position ?? new();
        var targPos = Service.ClientState.LocalPlayer?.TargetObject?.Position ?? new();
        var angle = Angle.FromDirection(new((targPos - selfPos).XZ()));
        var ts = FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance();
        DrawTarget("Target", ts->Target, selfPos, angle);
        DrawTarget("Soft target", ts->SoftTarget, selfPos, angle);
        DrawTarget("GPose target", ts->GPoseTarget, selfPos, angle);
        DrawTarget("Mouseover", ts->MouseOverTarget, selfPos, angle);
        DrawTarget("Focus", ts->FocusTarget, selfPos, angle);
        ImGui.TextUnformatted($"UI Mouseover: {Utils.ObjectString(Utils.MouseoverID())}");

        if (ImGui.Button("Target closest enemy"))
        {
            var closest = Service.ObjectTable.Where(o => o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc && o.SubKind == 5).MinBy(o => (o.Position - selfPos).LengthSquared());
            if (closest != null)
                Service.TargetManager.Target = closest;
        }
    }

    private unsafe void DrawTarget(string kind, FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* obj, Vector3 selfPos, Angle refAngle)
    {
        if (obj == null)
            return;

        var selfToObj = (Vector3)obj->Position - selfPos;
        var dist = selfToObj.Length();
        var angle = Angle.FromDirection(new(selfToObj.XZ())) - refAngle;
        var visHalf = Angle.Asin(obj->HitboxRadius / dist);
        ImGui.TextUnformatted($"{kind}: #{obj->ObjectIndex} {Utils.ObjectString(obj->EntityId)} {obj->BaseId}:{obj->GetNameId()}, hb={obj->HitboxRadius} ({visHalf}), dist={dist}, angle={angle} ({Math.Max(0, angle.Abs().Rad - visHalf.Rad).Radians()})");
    }

    private unsafe void DrawPlayerAttributes()
    {
        if (ImGui.Button("Clear trial"))
        {
            Utils.WriteField((void*)Service.Condition.Address, (int)Dalamud.Game.ClientState.Conditions.ConditionFlag.OnFreeTrial, false);
        }

        var uiState = UIState.Instance();
        ImGui.BeginTable("attrs", 2);
        ImGui.TableSetupColumn("Index");
        ImGui.TableSetupColumn("Value");
        ImGui.TableHeadersRow();
        for (int i = 0; i < 74; ++i)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(i.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(uiState->PlayerState.Attributes[i].ToString());
        }
        ImGui.EndTable();
    }

    private unsafe void DrawCountdown()
    {
        var agent = AgentCountDownSettingDialog.Instance();
        ImGui.TextUnformatted($"Active: {agent->Active} (showing cd={agent->ShowingCountdown})");
        ImGui.TextUnformatted($"Initiator: {Utils.ObjectString(agent->InitiatorId)}");
        ImGui.TextUnformatted($"Time left: {agent->TimeRemaining:f3}");
    }

    private void DrawWindowSystem()
    {
        ImGui.TextUnformatted($"Any focus: {Service.WindowSystem!.HasAnyFocus}");
        foreach (var w in Service.WindowSystem.Windows)
        {
            ImGui.TextUnformatted($"{w.WindowName}: focus={w.IsFocused}");
        }
    }

    private unsafe void DrawLimitBreak()
    {
        var lb = LimitBreakController.Instance();
        ImGui.TextUnformatted($"Value: {lb->CurrentUnits}/{lb->BarUnits} ({lb->BarCount} bars)");
        ImGui.TextUnformatted($"PVP: {lb->IsPvP}");
    }
}
