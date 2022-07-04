using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.AI
{
    class AIManager : IDisposable
    {
        private WorldState _ws;
        private AIConfig _config;
        private WindowManager.Window? _ui;
        private List<Behaviour> _behStack = new();
        private int _masterSlot = PartyState.PlayerSlot; // non-zero means corresponding player is master
        private Navigation _navi;
        private UseAction _useAction = new();
        private Autorotation _autorot;

        public AIManager(WorldState ws, InputOverride inputOverride, Autorotation autorot)
        {
            _ws = ws;
            _config = Service.Config.Get<AIConfig>();
            _navi = new(inputOverride);
            _autorot = autorot;
            Service.ChatGui.ChatMessage += OnChatMessage;
        }

        public void Dispose()
        {
            SwitchToIdle();
            Service.ChatGui.ChatMessage -= OnChatMessage;
        }

        public void Update()
        {
            if (_ws.Party.ContentIDs[_masterSlot] == 0)
                _masterSlot = PartyState.PlayerSlot;

            if (_config.Enabled)
            {
                var master = _ws.Party[_masterSlot];
                if (master != null && _behStack.Count > 0)
                {
                    if (!_behStack.Last().Execute(master))
                    {
                        _behStack.RemoveAt(_behStack.Count - 1);
                    }
                }

                _navi.Update();
            }
            else if (_behStack.Count > 0)
            {
                SwitchToIdle();
            }

            bool showUI = _config.Enabled && Service.ClientState.LocalPlayer != null;
            if (showUI && _ui == null)
            {
                _ui = WindowManager.CreateWindow("AI", DrawOverlay, () => { }, () => true);
                _ui.SizeHint = new(100, 100);
                _ui.MinSize = new(100, 100);
                _ui.Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            }
            else if (!showUI && _ui != null)
            {
                WindowManager.CloseWindow(_ui);
                _ui = null;
            }
        }

        private void DrawOverlay()
        {
            ImGui.TextUnformatted($"AI stack size: {_behStack.Count}, master={_ws.Party[_masterSlot]?.Name}");
            ImGui.TextUnformatted($"Current behaviour: {_behStack.LastOrDefault()}");
            if (ImGui.Button("Reset"))
                SwitchToIdle();
            ImGui.SameLine();
            if (ImGui.Button("Follow leader"))
            {
                var leader = Service.PartyList[(int)Service.PartyList.PartyLeaderIndex];
                if (leader != null)
                {
                    int leaderSlot = _ws.Party.ContentIDs.IndexOf((ulong)leader.ContentId);
                    if (leaderSlot >= 0)
                        SwitchToFollow(leaderSlot);
                }
            }
        }

        private void SwitchToIdle()
        {
            _behStack.Clear();
            _masterSlot = PartyState.PlayerSlot;
            _navi.TargetPos = null;
            _navi.TargetRot = null;
        }

        private void SwitchToFollow(int masterSlot)
        {
            SwitchToIdle();
            _masterSlot = masterSlot;
            _behStack.Add(new BehaviourFollow(_ws, _navi, _useAction, _autorot));
        }

        private int FindPartyMemberSlotFromSender(SeString sender)
        {
            var source = sender.Payloads.FirstOrDefault() as PlayerPayload;
            if (source == null)
                return -1;
            var pm = Service.PartyList.FirstOrDefault(pm => pm.Name.TextValue == source.PlayerName && pm.World.Id == source.World.RowId);
            if (pm == null)
                return -1;
            return _ws.Party.ContentIDs.IndexOf((ulong)pm.ContentId);
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!_config.Enabled || type != XivChatType.Party)
                return;

            var messagePrefix = message.Payloads.FirstOrDefault() as TextPayload;
            if (messagePrefix?.Text == null || !messagePrefix.Text.StartsWith("vbmai "))
                return;

            var messageData = messagePrefix.Text.Split(' ');
            if (messageData.Length < 2)
                return;

            switch (messageData[1])
            {
                case "follow":
                    var master = FindPartyMemberSlotFromSender(sender);
                    if (master >= 0)
                        SwitchToFollow(master);
                    break;
                case "cancel":
                    SwitchToIdle();
                    break;
                default:
                    Service.Log($"[AI] Unknown command: {messageData[1]}");
                    break;
            }
        }
    }
}
