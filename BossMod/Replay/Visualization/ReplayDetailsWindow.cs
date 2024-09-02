using BossMod.Autorotation;
using ImGuiNET;

namespace BossMod.ReplayVisualization;

class ReplayDetailsWindow : UIWindow
{
    private readonly ReplayPlayer _player;
    private readonly RotationDatabase _rotationDB;
    private readonly AIHints _hints = new();
    private BossModuleManager _mgr;
    private AIHintsBuilder _hintsBuilder;
    private RotationModuleManager _rmm;
    private readonly DateTime _first;
    private readonly DateTime _last;
    private DateTime _curTime; // note that is could fall between frames
    private DateTime _prevFrame;
    private float _playSpeed;
    private float _azimuth;
    private bool _azimuthOverride = true;
    private int _povSlot = PartyState.PlayerSlot;
    private readonly ConfigUI _config;
    private bool _showConfig;
    private readonly EventList _events;
    private readonly ReplayAnalysis.AnalysisManager _analysis;

    private readonly UITree _pfTree = new();
    private AIHintsVisualizer? _pfVisu;
    private float _pfTargetRadius = 3;
    private Positional _pfPositional = Positional.Any;

    public DateTime CurrentTime
    {
        get => _curTime;
        set => MoveTo(value);
    }

    public ReplayDetailsWindow(Replay data, RotationDatabase rotationDB) : base($"Replay: {data.Path}", false, new(1500, 1000))
    {
        _player = new(data);
        _rotationDB = rotationDB;
        _mgr = new(_player.WorldState);
        _hintsBuilder = new(_player.WorldState, _mgr);
        _rmm = new(rotationDB, _mgr, _hints);
        _curTime = _first = data.Ops[0].Timestamp;
        _last = data.Ops[^1].Timestamp;
        _player.AdvanceTo(_first, _mgr.Update);
        _config = new(Service.Config, _player.WorldState, null, null);
        _events = new(data, MoveTo, rotationDB.Plans, this);
        _analysis = new([data]);
    }

    protected override void Dispose(bool disposing)
    {
        _analysis.Dispose();
        _config.Dispose();
        _rmm.Dispose();
        _hintsBuilder.Dispose();
        _mgr.Dispose();
        base.Dispose(disposing);
    }

    public override void Draw()
    {
        var curFrame = DateTime.Now;
        if (_playSpeed > 0)
            MoveTo(_curTime + (curFrame - _prevFrame) * _playSpeed);
        _prevFrame = curFrame;

        DrawControlRow();
        DrawTimelineRow();
        ImGui.TextUnformatted($"Num loaded modules: {_mgr.LoadedModules.Count}, num active modules: {_mgr.LoadedModules.Count(m => m.StateMachine.ActiveState != null)}, active module: {_mgr.ActiveModule?.GetType()}");
        if (!_azimuthOverride)
            _azimuth = _mgr.WorldState.Client.CameraAzimuth.Deg;
        ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, 180);
        ImGui.SameLine();
        ImGui.Checkbox("Override", ref _azimuthOverride);
        if (_mgr.ActiveModule != null)
        {
            _hintsBuilder.Update(_hints, _povSlot);
            _rmm.Update(0, float.MaxValue, false);

            var drawTimerPre = DateTime.Now;
            _mgr.ActiveModule.Draw(_azimuthOverride ? _azimuth.Degrees() : _mgr.WorldState.Client.CameraAzimuth, _povSlot, true, true);
            var drawTimerPost = DateTime.Now;

            var compList = string.Join(", ", _mgr.ActiveModule.Components.Select(c => c.GetType().Name));
            var pov = _mgr.WorldState.Party[_povSlot];
            var povOffsetString = "";
            if (pov != null)
            {
                var povOffset = pov.Position - _mgr.ActiveModule.Center;
                povOffsetString = $"{povOffset} [R={povOffset.Length():f3}, dir={Angle.FromDirection(povOffset)}]";
            }
            ImGui.TextUnformatted($"Current state: {_mgr.ActiveModule.StateMachine.ActiveState?.ID:X}, Time since pull: {_mgr.ActiveModule.StateMachine.TimeSinceActivation:f3}, Draw time: {(drawTimerPost - drawTimerPre).TotalMilliseconds:f3}ms, Components: {compList}, Player offset: {povOffsetString}, Draw cache: {_mgr.ActiveModule.Arena.DrawCacheStats()}");

            if (ImGui.CollapsingHeader("Plan execution"))
            {
                if (ImGui.Button("Timeline"))
                {
                    _ = new StateMachineWindow(_mgr.ActiveModule);
                }

                if (_mgr.ActiveModule.Info?.PlanLevel > 0)
                {
                    ImGui.SameLine();
                    var plans = _rotationDB.Plans.GetPlans(_mgr.ActiveModule.GetType(), _mgr.WorldState.Party.Player()?.Class ?? Class.None);
                    var newSel = UIPlanDatabaseEditor.DrawPlanCombo(plans, plans.SelectedIndex, "Plan");
                    if (newSel != plans.SelectedIndex)
                    {
                        plans.SelectedIndex = newSel;
                        _rotationDB.Plans.ModifyManifest(_mgr.ActiveModule.GetType(), _mgr.WorldState.Party.Player()?.Class ?? Class.None);
                    }

                    ImGui.SameLine();
                    if (ImGui.Button(plans.SelectedIndex >= 0 ? "Edit" : "New"))
                    {
                        if (plans.SelectedIndex < 0)
                        {
                            var plan = new Plan($"New {plans.Plans.Count + 1}", _mgr.ActiveModule.GetType()) { Guid = Guid.NewGuid().ToString(), Class = _mgr.WorldState.Party.Player()?.Class ?? Class.None, Level = _mgr.ActiveModule.Info.PlanLevel };
                            plans.SelectedIndex = plans.Plans.Count;
                            _rotationDB.Plans.ModifyPlan(null, plan);
                        }

                        var enc = _player.Replay.Encounters.FirstOrDefault(e => e.InstanceID == _mgr.ActiveModule.PrimaryActor.InstanceID);
                        if (enc != null)
                        {
                            _ = new ReplayTimelineWindow(_player.Replay, enc, new(1), _rotationDB.Plans, this);
                        }
                    }
                }

                // TODO: more fancy action history/queue...
                ImGui.TextUnformatted($"Modules: {_rmm}");
                ImGui.TextUnformatted($"GCD={_mgr.WorldState.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining:f3}, AnimLock={_mgr.WorldState.Client.AnimationLock:f3}, Combo={_mgr.WorldState.Client.ComboState.Remaining:f3}, RBIn={_mgr.RaidCooldowns.NextDamageBuffIn():f3}");
                var player = _mgr.WorldState.Party.Player();
                if (player != null)
                {
                    var best = _hints.ActionsToExecute.FindBest(_mgr.WorldState, player, _mgr.WorldState.Client.Cooldowns, _mgr.WorldState.Client.AnimationLock, _hints, 0.02f);
                    ImGui.TextUnformatted($"! {best.Action} ({best.Priority:f2}) in {best.Delay:f3}");
                }
                foreach (var a in _hints.ActionsToExecute.Entries)
                {
                    ImGui.TextUnformatted($"> {a.Action} ({a.Priority:f2}) in {a.Delay:f3}");
                }
            }
        }

        DrawPartyTable();
        DrawEnemyTables();
        DrawAllActorsTable();
        DrawAI();

        if (ImGui.CollapsingHeader($"Events (version: {_player.Replay.GameVersion})"))
            _events.Draw();
        if (ImGui.CollapsingHeader("Analysis"))
            _analysis.Draw();
    }

    private void DrawControlRow()
    {
        ImGui.TextUnformatted($"{_curTime:O}");
        ImGui.SameLine();
        if (ImGui.Button("<<<"))
            Rewind(20);
        ImGui.SameLine();
        if (ImGui.Button("<<"))
            Rewind(5);
        ImGui.SameLine();
        if (ImGui.Button("<"))
            Rewind(1);
        ImGui.SameLine();
        if (ImGui.Button("|<"))
            RewindPrevFrame();
        ImGui.SameLine();
        if (ImGui.Button("||"))
            _playSpeed = _playSpeed == 0 ? 1 : 0;
        ImGui.SameLine();
        if (ImGui.Button(">|"))
            AdvanceNextFrame();
        ImGui.SameLine();
        if (ImGui.Button(">"))
            _playSpeed = 0.2f;
        ImGui.SameLine();
        if (ImGui.Button(">>"))
            _playSpeed = 1;
        ImGui.SameLine();
        if (ImGui.Button(">>>"))
            _playSpeed = 10;

        ImGui.SameLine();
        ImGui.Checkbox("Show config", ref _showConfig);
        if (_showConfig)
        {
            _config.Draw();
        }
    }

    private void DrawTimelineRow()
    {
        var dl = ImGui.GetWindowDrawList();
        var cursor = ImGui.GetCursorScreenPos();
        var w = ImGui.GetWindowWidth() - 2 * ImGui.GetCursorPosX() - 15;
        cursor.Y += 4;
        dl.AddLine(cursor, cursor + new Vector2(w, 0), 0xff00ffff);

        var curp = cursor + new Vector2(w * (float)((_curTime - _first) / (_last - _first)), 0);
        dl.AddTriangleFilled(curp, curp + new Vector2(3, 5), curp + new Vector2(-3, 5), 0xff00ffff);
        foreach (var e in _player.Replay.Encounters)
        {
            DrawCheckpoint(e.Time.Start, 0xff00ff00, cursor, w);
            DrawCheckpoint(e.Time.End, 0xff0000ff, cursor, w);
        }
        foreach (var m in _player.Replay.UserMarkers)
        {
            DrawCheckpoint(m.Key, 0xffff0000, cursor, w);
        }
        if (ImGui.IsWindowFocused() && (ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)))
        {
            var pos = ImGui.GetMousePos();
            if (Math.Abs(pos.Y - cursor.Y) <= 3 && pos.X >= cursor.X && pos.X <= cursor.X + w)
            {
                var t = _first + (pos.X - cursor.X) / w * (_last - _first);
                var margin = (_last - _first).TotalSeconds * 3 / w;
                var enc = _player.Replay.Encounters.Find(e => Math.Abs((t - e.Time.Start).TotalSeconds) <= margin);
                MoveTo(enc?.Time.Start ?? t);
            }
        }
        ImGui.Dummy(new(w, 8));
    }

    private void DrawCheckpoint(DateTime timestamp, uint color, Vector2 cursor, float w)
    {
        var off = (float)((timestamp - _first) / (_last - _first));
        var center = cursor + new Vector2(w * off, 0);
        ImGui.GetWindowDrawList().AddCircleFilled(center, 3, color);
    }

    // x, z, rot, hp, name, target, cast, statuses
    private void DrawCommonColumns(Actor actor)
    {
        var posX = actor.Position.X;
        var posZ = actor.Position.Z;
        var rot = actor.Rotation.Deg;
        bool modified = false;
        ImGui.TableNextColumn();
        modified |= ImGui.DragFloat("###X", ref posX, 0.25f, 80, 120);
        ImGui.TableNextColumn();
        modified |= ImGui.DragFloat("###Z", ref posZ, 0.25f, 80, 120);
        ImGui.TableNextColumn();
        modified |= ImGui.DragFloat("###Rot", ref rot, 1, -180, 180);
        if (modified)
            actor.PosRot = new(posX, actor.PosRot.Y, posZ, rot.Degrees().Rad);

        ImGui.TableNextColumn();
        if (actor.HPMP.MaxHP > 0)
        {
            float frac = Math.Min((float)(actor.HPMP.CurHP + actor.HPMP.Shield) / actor.HPMP.MaxHP, 1);
            ImGui.ProgressBar(frac, new(ImGui.GetColumnWidth(), 0), $"{frac * 100:f1}% ({actor.HPMP.CurHP} + {actor.HPMP.Shield} / {actor.HPMP.MaxHP})");
        }

        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{(actor.IsDead ? "(Dead) " : "")}{actor} (r={actor.HitboxRadius:f2})");

        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{_player.WorldState.Actors.Find(actor.TargetID)}");

        ImGui.TableNextColumn();
        if (actor.CastInfo != null)
            ImGui.TextUnformatted($"{actor.CastInfo.Action}: {Utils.CastTimeString(actor.CastInfo, _player.WorldState.CurrentTime)}");

        ImGui.TableNextColumn();
        if (actor.MountId > 0)
        {
            ImGui.TextUnformatted($"'Mounted' ({actor.MountId})");
            ImGui.SameLine();
        }
        foreach (var s in actor.Statuses.Where(s => s.ID != 0))
        {
            var src = _player.WorldState.Actors.Find(s.SourceID);
            if (src?.Type is ActorType.Player or ActorType.Pet)
                continue;
            if (s.ID is 360 or 362 or 364 or 365 or 413 or 902)
                continue; // skip FC buff
            ImGui.TextUnformatted($"{Utils.StatusString(s.ID)} ({s.Extra}): {Utils.StatusTimeString(s.ExpireAt, _player.WorldState.CurrentTime)}");
            ImGui.SameLine();
        }
    }

    private void DrawPartyTable()
    {
        if (!ImGui.CollapsingHeader("Party"))
            return;

        ImGui.BeginTable("party", 11, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("POV", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 25);
        ImGui.TableSetupColumn("Class", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 30);
        ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("Rot", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("HP", ImGuiTableColumnFlags.WidthFixed, 200);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 100);
        ImGui.TableSetupColumn("Target", ImGuiTableColumnFlags.None, 100);
        ImGui.TableSetupColumn("Cast", ImGuiTableColumnFlags.None, 100);
        ImGui.TableSetupColumn("Statuses", ImGuiTableColumnFlags.None, 100);
        ImGui.TableSetupColumn("Hints", ImGuiTableColumnFlags.None, 250);
        ImGui.TableHeadersRow();
        foreach ((int slot, var player) in _player.WorldState.Party.WithSlot(true))
        {
            ImGui.PushID((int)player.InstanceID);
            ImGui.TableNextRow();

            bool isPOV = _povSlot == slot;
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("###POV", ref isPOV) && isPOV)
            {
                _povSlot = slot;
                ResetPF();
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(player.Class.ToString());

            DrawCommonColumns(player);

            ImGui.TableNextColumn();
            if (_mgr.ActiveModule != null)
            {
                var hints = _mgr.ActiveModule.CalculateHintsForRaidMember(slot, player);
                foreach ((var hint, bool risk) in hints)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, risk ? 0xff00ffff : 0xff00ff00);
                    ImGui.TextUnformatted(hint);
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                }
            }

            ImGui.PopID();
        }
        ImGui.EndTable();
    }

    private void DrawEnemyTables()
    {
        if (_mgr.ActiveModule == null)
            return;

        DrawEnemyTable(_mgr.ActiveModule.PrimaryActor.OID, _mgr.ActiveModule.RelevantEnemies.GetValueOrDefault(_mgr.ActiveModule.PrimaryActor.OID) ?? [_mgr.ActiveModule.PrimaryActor]);
        foreach ((var oid, var list) in _mgr.ActiveModule.RelevantEnemies)
        {
            if (oid != _mgr.ActiveModule.PrimaryActor.OID)
            {
                DrawEnemyTable(oid, list);
            }
        }
    }

    private void DrawEnemyTable(uint oid, ICollection<Actor> actors)
    {
        var moduleInfo = _mgr.ActiveModule != null ? ModuleRegistry.FindByOID(_mgr.ActiveModule.PrimaryActor.OID) : null;
        var oidName = moduleInfo?.ObjectIDType?.GetEnumName(oid);
        if (!ImGui.CollapsingHeader($"Enemy {oid:X} {oidName ?? ""}") || actors.Count == 0)
            return;

        ImGui.BeginTable($"enemy_{oid}", 8, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("Rot", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("HP", ImGuiTableColumnFlags.WidthFixed, 200);
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Target");
        ImGui.TableSetupColumn("Cast");
        ImGui.TableSetupColumn("Statuses");
        ImGui.TableHeadersRow();
        foreach (var enemy in actors)
        {
            ImGui.PushID((int)enemy.InstanceID);
            ImGui.TableNextRow();
            DrawCommonColumns(enemy);
            ImGui.PopID();
        }
        ImGui.EndTable();
    }

    private void DrawAllActorsTable()
    {
        if (!ImGui.CollapsingHeader("All actors"))
            return;

        ImGui.BeginTable($"actors", 8, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("Rot", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 90);
        ImGui.TableSetupColumn("HP", ImGuiTableColumnFlags.WidthFixed, 200);
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Target");
        ImGui.TableSetupColumn("Cast");
        ImGui.TableSetupColumn("Statuses");
        ImGui.TableHeadersRow();
        foreach (var actor in _player.WorldState.Actors)
        {
            ImGui.PushID((int)actor.InstanceID);
            ImGui.TableNextRow();
            DrawCommonColumns(actor);
            ImGui.PopID();
        }
        ImGui.EndTable();
    }

    private void DrawAI()
    {
        if (!ImGui.CollapsingHeader("AI hints"))
            return;
        if (_mgr.ActiveModule == null)
            return;
        var player = _mgr.ActiveModule.Raid[_povSlot];
        if (player == null)
            return;

        if (_pfVisu == null)
        {
            var playerAssignment = Service.Config.Get<PartyRolesConfig>()[_mgr.WorldState.Party.Members[_povSlot].ContentId];
            var pfTank = playerAssignment == PartyRolesConfig.Assignment.MT || playerAssignment == PartyRolesConfig.Assignment.OT && !_mgr.WorldState.Party.WithoutSlot().Any(p => p != player && p.Role == Role.Tank);
            _pfVisu = new(_hints, _mgr.WorldState, player, player.TargetID, e => (e, _pfTargetRadius, _pfPositional, pfTank));
        }
        _pfVisu.Draw(_pfTree);

        bool rebuild = false;
        //rebuild |= ImGui.SliderFloat("Zone cushion", ref _pfCushion, 0.1f, 5);
        rebuild |= ImGui.SliderFloat("Ability range", ref _pfTargetRadius, 3, 25);
        rebuild |= UICombo.Enum("Ability positional", ref _pfPositional);
        if (rebuild)
            ResetPF();
    }

    //private string FlagTransitionString((bool active, float transIn) arg) => $"{(arg.active ? "end" : "start")} in {arg.transIn:f2}s";

    private void MoveTo(DateTime t)
    {
        if (t < _player.WorldState.CurrentTime)
        {
            _rmm.Dispose();
            _hintsBuilder.Dispose();
            _mgr.Dispose();
            _player.Reset();
            _mgr = new(_player.WorldState);
            _hintsBuilder = new(_player.WorldState, _mgr);
            _rmm = new(_rotationDB, _mgr, _hints);
        }
        _player.AdvanceTo(t, _mgr.Update);
        _curTime = t;
        ResetPF();
    }

    private void Rewind(float seconds)
    {
        _playSpeed = 0;
        MoveTo(_curTime.AddSeconds(-seconds));
    }

    private void AdvanceNextFrame()
    {
        _playSpeed = 0;
        var ts = _player.NextTimestamp();
        if (ts != default)
            MoveTo(ts);
    }

    private void RewindPrevFrame()
    {
        _playSpeed = 0;
        MoveTo(_player.PrevTimestamp());
    }

    private void ResetPF()
    {
        _pfVisu = null;
    }
}
