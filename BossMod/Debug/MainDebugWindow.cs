using BossMod.Autorotation;
using BossMod.Autorotation.xan;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace BossMod;

class MainDebugWindow(WorldState ws, RotationModuleManager autorot, ZoneModuleManager zmm, ActionManagerEx amex, MovementOverride move, AIHintsBuilder hintBuilder, IDalamudPluginInterface dalamud) : UIWindow("Boss mod debug UI", false, new(300, 200))
{
    private readonly DebugObstacles _debugObstacles = new(hintBuilder.Obstacles, dalamud);
    private readonly DebugObjects _debugObjects = new();
    private readonly DebugParty _debugParty = new();
    private readonly DebugMapEffect _debugMapEffect = new(ws);
    private readonly DebugGraphics _debugGraphics = new();
    private readonly DebugAction _debugAction = new(ws, amex);
    private readonly DebugHate _debugHate = new(ws);
    private readonly DebugInput _debugInput = new(autorot, move);
    private readonly DebugAutorotation _debugAutorot = new(autorot);
    private readonly DebugAddon _debugAddon = new();
    private readonly DebugTiming _debugTiming = new();
    private readonly DebugQuests _debugQuests = new();
    private readonly DebugVfx _debugVfx = new();

    protected override void Dispose(bool disposing)
    {
        _debugAction.Dispose();
        _debugInput.Dispose();
        _debugAddon.Dispose();
        _debugMapEffect.Dispose();
        _debugVfx.Dispose();
        base.Dispose(disposing);
    }

    public override unsafe void Draw()
    {
        var playerCID = UIState.Instance()->PlayerState.ContentId;
        var player = Service.ObjectTable.LocalPlayer;
        ImGui.TextUnformatted($"Current zone: {ws.CurrentZone}, player=0x{(ulong)Utils.GameObjectInternal(player):X}, playerCID={playerCID:X}, pos = {Utils.Vec3String(player?.Position ?? new Vector3())}");
        // ImGui.TextUnformatted($"ID scramble: {Network.IDScramble.Delta} = {*Network.IDScramble.OffsetAdjusted} - {*Network.IDScramble.OffsetBaseFixed} - {*Network.IDScramble.OffsetBaseChanging}");
        ImGui.TextUnformatted($"Player mode: {(player is null ? "No player found" : Utils.CharacterInternal(player)->Mode.ToString())}");

        var eventFwk = FFXIVClientStructs.FFXIV.Client.Game.Event.EventFramework.Instance();
        var instanceDirector = eventFwk != null ? eventFwk->GetInstanceContentDirector() : null;
        ImGui.TextUnformatted($"Content time left: {(instanceDirector != null ? $"{instanceDirector->ContentDirector.ContentTimeLeft:f1}" : "n/a")}");
        if (instanceDirector != null)
            ImGui.TextUnformatted($"Director address: 0x{(nint)instanceDirector:X}");

        if (ImGui.Button("Perform full dump"))
        {
            DebugObjects.DumpObjectTable();
            DebugGraphics.DumpScene();
        }

        if (ImGui.CollapsingHeader("Obstacles"))
        {
            _debugObstacles.Draw();
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
        if (ImGui.CollapsingHeader("Party"))
        {
            _debugParty.Draw(false);
        }
        if (ImGui.CollapsingHeader("Inventory"))
        {
            DrawInventory();
        }
        if (ImGui.CollapsingHeader("Party (duty recorder)"))
        {
            _debugParty.Draw(true);
        }
        if (ImGui.CollapsingHeader("Action effects"))
        {
            DrawEffects();
        }
        if (ImGui.CollapsingHeader("Map effects"))
        {
            _debugMapEffect.Draw();
        }
        if (ImGui.CollapsingHeader("Autorotation"))
        {
            _debugAutorot.Draw();
        }
        if (ImGui.CollapsingHeader("Party health"))
        {
            DrawPartyHealth();
        }
        if (ImGui.CollapsingHeader("Solo duty module"))
        {
            if (zmm.ActiveModule is QuestBattle.QuestBattle qb)
                qb.DrawDebugInfo();
        }
        if (ImGui.CollapsingHeader("Graphics scene"))
        {
            _debugGraphics.DrawSceneTree();
        }
        if (ImGui.CollapsingHeader("Graphics watch"))
        {
            _debugGraphics.DrawWatchedMods();
        }
        if (ImGui.CollapsingHeader("Matrices"))
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
        if (ImGui.CollapsingHeader("Duty actions"))
        {
            _debugAction.DrawDutyActions();
        }
        if (ImGui.CollapsingHeader("Auto attacks"))
        {
            _debugAction.DrawAutoAttack();
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
        if (ImGui.CollapsingHeader("Markers"))
        {
            DrawMarkers();
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
        if (ImGui.CollapsingHeader("Quests"))
        {
            _debugQuests.Draw();
        }
    }

    private unsafe void DrawStatuses()
    {
        var player = (Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (player == null)
            return;

        ImGui.TextUnformatted($"Forced movement direction: {MovementOverride.ForcedMovementDirection->Radians()}");
        ImGui.SameLine();
        if (ImGui.Button("Add misdirection"))
            player->GetStatusManager()->SetStatus(20, 3909, 20.0f, 100, (GameObjectId)0xE0000000, true);
        ImGui.SameLine();
        if (ImGui.Button("Add thin ice"))
            player->GetStatusManager()->SetStatus(20, 911, 20.0f, 50, (GameObjectId)0xE0000000, true); // param = distance * 10
        ImGui.SameLine();
        if (ImGui.Button("Add spinning"))
            player->GetStatusManager()->SetStatus(20, 2973, 20.0f, 7, (GameObjectId)0xE0000000, true);

        if (ImGui.Button("Clear temp status"))
            player->GetStatusManager()->RemoveStatus(20);

        ImGui.SameLine();
        ImGui.TextUnformatted($"Forced movement direction: {ws.Client.ForcedMovementDirection}");

        ImGui.TextUnformatted($"Player move speed: {ws.Client.MoveSpeed:f2}");

        foreach (var elem in ws.Actors)
        {
            var obj = (elem.InstanceID >> 32) == 0 ? Service.ObjectTable.SearchById((uint)elem.InstanceID) : null;
            if (ImGui.TreeNode(Utils.ObjectString(obj!)))
            {
                if (obj is IBattleChara chara)
                {
                    foreach (var status in chara.StatusList)
                    {
                        var src = status.SourceObject != null ? Utils.ObjectString(status.SourceObject) : "none";
                        ImGui.TextUnformatted($"{status.StatusId} '{status.GameData.Value.Name}': param={status.Param}, stacks={status.Param}, time={status.RemainingTime:f2}, source={src} ({status.SourceId:X8})");
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
            if (elem.CastInfo == null || elem.IsAlly)
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

    private void DrawInventory()
    {
        var player = Service.ObjectTable.LocalPlayer;
        if (player == null)
            return;

        ImGui.BeginTable("items", 3, ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
        ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 30);
        ImGui.TableSetupColumn("Quant", ImGuiTableColumnFlags.WidthFixed, 30);
        ImGui.TableSetupColumn("Name");
        ImGui.TableHeadersRow();
        foreach (var (k, i) in ws.Client.Inventory)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(k.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(i.ToString());
            ImGui.TableNextColumn();
            var namebase = Service.LuminaRow<Lumina.Excel.Sheets.Item>(k % 500000)?.Name.ToString() ?? "<unknown>";
            var namefull = k > 500000 ? $"{namebase} (HQ)" : namebase;
            ImGui.TextUnformatted(namefull);
        }
        ImGui.EndTable();
    }

    private unsafe void DrawEffects()
    {
        var player = Service.ObjectTable.LocalPlayer;
        if (player == null)
            return;

        var aeh = ((BattleChara*)player.Address)->GetActionEffectHandler();
        if (aeh == null)
            return;

        ImGui.TextUnformatted($"Effecthandler address: {(nint)aeh:X}");

        ImGui.BeginTable("effects", 6, ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
        ImGui.TableSetupColumn("Seq", ImGuiTableColumnFlags.WidthFixed, 30);
        ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed, 30);
        ImGui.TableSetupColumn("Action");
        ImGui.TableSetupColumn("Source");
        ImGui.TableSetupColumn("Target");
        ImGui.TableSetupColumn("Effects");
        ImGui.TableHeadersRow();
        foreach (var entry in aeh->IncomingEffects)
        {
            if (entry.ActionId == 0)
                continue;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(entry.GlobalSequence.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(entry.TargetIndex.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{new ActionID((ActionType)entry.ActionType, entry.ActionId)} ({entry.SpellId})");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{entry.Source.Id:X} confirmed={entry.SourceConfirmed}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{entry.Target.Id:X} confirmed={entry.TargetConfirmed}");
            ImGui.TableNextColumn();
            foreach (var eff in entry.Effects.Effects)
            {
                if (eff.Type > 0)
                    ImGui.TextUnformatted($"{(ActionEffectType)eff.Type} {eff.Param0:X2} {eff.Param1:X2} {eff.Param2:X2} {eff.Param3:X2} {eff.Param4:X2} {eff.Value}");
            }
        }
        ImGui.EndTable();
    }

    private readonly TrackPartyHealth _partyHealth = new(ws);

    private void DrawPartyHealth()
    {
        _partyHealth.Update(autorot.Hints);

        var overall = _partyHealth.PartyHealth;

        ImGui.TextUnformatted($"Avg: {overall.AvgCurrent * 100:f1} (current) / {overall.AvgPredicted * 100:f1} (predicted)");
        ImGui.TextUnformatted($"StD: {overall.StdDevCurrent:f3} (current) / {overall.StdDevCurrent:f3} (predicted)");

        ImGui.BeginTable("partyhealth", 4, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("HP");
        ImGui.TableSetupColumn("Type");
        ImGui.TableHeadersRow();
        foreach (var (_, actor) in _partyHealth.TrackedMembers)
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(actor.Name);
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{actor.HPMP.CurHP} ({actor.PendingHPDifference:+#;-#;+0}) / {actor.HPMP.MaxHP} ({actor.HPRatio:#0.#%} / {actor.PendingHPRatio:#0.#%})");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{actor.Type}");
            ImGui.TableNextRow();
        }
        ImGui.EndTable();
    }

    private unsafe void DrawTargets()
    {
        var cursorPos = amex.GetWorldPosUnderCursor();
        ImGui.TextUnformatted($"World pos under cursor: {(cursorPos == null ? "n/a" : Utils.Vec3String(cursorPos.Value))}");

        var player = Service.ObjectTable.LocalPlayer;
        var selfPos = player?.Position ?? new();
        var targPos = Service.ObjectTable.LocalPlayer?.TargetObject?.Position ?? new();
        var angle = player?.Rotation.Radians() ?? default; //Angle.FromDirection(new((targPos - selfPos).XZ()));
        var ts = FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance();
        DrawTarget("Target", ts->Target, selfPos, angle);
        DrawTarget("Soft target", ts->SoftTarget, selfPos, angle);
        DrawTarget("GPose target", ts->GPoseTarget, selfPos, angle);
        DrawTarget("Mouseover", ts->MouseOverTarget, selfPos, angle);
        DrawTarget("Focus", ts->FocusTarget, selfPos, angle);
        var mouseover = FFXIVClientStructs.FFXIV.Client.UI.Misc.PronounModule.Instance()->UiMouseOverTarget;
        ImGui.TextUnformatted($"UI Mouseover: {Utils.ObjectString(mouseover != null ? mouseover->EntityId : 0)}");

        if (ImGui.Button("Target closest enemy"))
        {
            var closest = Service.ObjectTable.Where(o => o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc && o.SubKind == 5).MinBy(o => (o.Position - selfPos).LengthSquared());
            if (closest != null)
                Service.TargetManager.Target = closest;
        }
    }

    private unsafe void DrawTarget(string kind, GameObject* obj, Vector3 selfPos, Angle refAngle)
    {
        if (obj == null)
            return;

        var selfToObj = (Vector3)obj->Position - selfPos;
        var dist = selfToObj.Length();
        var angle = Angle.FromDirection(new(selfToObj.XZ())) - refAngle;
        var visHalf = Angle.Asin(obj->HitboxRadius / dist);
        ImGui.TextUnformatted($"{kind}: #{obj->ObjectIndex} {Utils.ObjectString(obj->EntityId)} {obj->BaseId}:{obj->GetNameId()}, hb={obj->HitboxRadius} ({visHalf}), dist={dist}, angle={angle} ({Math.Max(0, angle.Abs().Rad - visHalf.Rad).Radians()})");
    }

    private static readonly string[] FieldMarkers = ["A", "B", "C", "D", "1", "2", "3", "4"];

    private unsafe void DrawMarkers()
    {
        var markers = MarkingController.Instance();
        using (ImRaii.Table("Field", 2))
        {
            for (var i = 0; i < markers->FieldMarkers.Length; i++)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(FieldMarkers[i]);
                ImGui.TableNextColumn();
                ImGui.Text(markers->FieldMarkers[i].Active ? markers->FieldMarkers[i].Position.ToString() : "-");
            }
        }

        using (ImRaii.Table("Target", 2))
        {
            for (var i = 0; i < markers->Markers.Length; i++)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text($"{i} ???");
                ImGui.TableNextColumn();
                ImGui.Text(markers->Markers[i].ObjectId.ToString("X8"));
            }
        }
    }

    private unsafe void DrawPlayerAttributes()
    {
        if (ImGui.Button("Clear trial"))
        {
            Utils.WriteField((void*)Service.Condition.Address, (int)Dalamud.Game.ClientState.Conditions.ConditionFlag.OnFreeTrial, false);
        }

        var uiState = UIState.Instance();
        var level = (uint)uiState->PlayerState.CurrentLevel;
        var paramGrow = Service.LuminaRow<Lumina.Excel.Sheets.ParamGrow>(level);
        if (paramGrow != null)
        {
            ImGui.TextUnformatted($"Level: {level}, baseSpeed={paramGrow.Value.BaseSpeed}, levelMod={paramGrow.Value.LevelModifier}");
            var sksValue = uiState->PlayerState.Attributes[45];
            var spsValue = uiState->PlayerState.Attributes[46];
            var sksMod = 130 * (paramGrow.Value.BaseSpeed - sksValue) / paramGrow.Value.LevelModifier + 1000;
            var spsMod = 130 * (paramGrow.Value.BaseSpeed - spsValue) / paramGrow.Value.LevelModifier + 1000;
            var hasteValue = uiState->PlayerState.Attributes[47];
            ImGui.TextUnformatted($"SKS: value={sksValue}, mod={sksMod}, gcd={2500 * sksMod / 1000}");
            ImGui.TextUnformatted($"SPS: value={spsValue}, mod={spsMod}, gcd={2500 * spsMod / 1000}");
            ImGui.TextUnformatted($"Haste: value={hasteValue}, gcd-sks={2500 * sksMod / 1000 * hasteValue / 100}, gcd-sps={2500 * spsMod / 1000 * hasteValue / 100}");
        }

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
