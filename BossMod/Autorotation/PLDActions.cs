using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using ImGuiNET;
using System.Linq;

namespace BossMod
{
    class PLDActions : CommonActions
    {
        private PLDConfig _config;
        private PLDRotation.State _state;
        private PLDRotation.Strategy _strategy;
        private ActionID _nextBestSTAction = ActionID.MakeSpell(PLDRotation.AID.FastBlade);
        private ActionID _nextBestAOEAction = ActionID.MakeSpell(PLDRotation.AID.TotalEclipse);

        public PLDActions(Autorotation autorot)
            : base(autorot, ActionID.MakeSpell(PLDRotation.AID.FastBlade))
        {
            _config = Service.Config.Get<PLDConfig>();
            _state = BuildState();
            _strategy = new();

            SmartQueueRegisterSpell(PLDRotation.AID.Rampart);
            SmartQueueRegisterSpell(PLDRotation.AID.Reprisal);
            SmartQueueRegisterSpell(PLDRotation.AID.ArmsLength);
            SmartQueueRegisterSpell(PLDRotation.AID.Provoke);
            SmartQueueRegisterSpell(PLDRotation.AID.Shirk);
            SmartQueueRegister(CommonRotation.IDSprint);
            SmartQueueRegister(PLDRotation.IDStatPotion);
        }

        protected override void OnCastSucceeded(ActionID actionID)
        {
            Log($"Cast {actionID}, next-best={_nextBestSTAction}/{_nextBestAOEAction} [{_state}]");
        }

        protected override CommonRotation.State OnUpdate()
        {
            var currState = BuildState();
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, PLDRotation.IDStatPotion);

            // cooldown execution
            _strategy.ExecuteRampart = SmartQueueActiveSpell(PLDRotation.AID.Rampart);
            _strategy.ExecuteReprisal = SmartQueueActiveSpell(PLDRotation.AID.Reprisal) && AllowReprisal();
            _strategy.ExecuteArmsLength = SmartQueueActiveSpell(PLDRotation.AID.ArmsLength);
            _strategy.ExecuteProvoke = SmartQueueActiveSpell(PLDRotation.AID.Provoke); // TODO: check that not MT already
            _strategy.ExecuteShirk = SmartQueueActiveSpell(PLDRotation.AID.Shirk); // TODO: check that hate is close to MT...

            var nextBestST = _config.FullRotation ? PLDRotation.GetNextBestAction(_state, _strategy, false) : ActionID.MakeSpell(PLDRotation.AID.FastBlade);
            var nextBestAOE = _config.FullRotation ? PLDRotation.GetNextBestAction(_state, _strategy, true) : ActionID.MakeSpell(PLDRotation.AID.TotalEclipse);
            if (_nextBestSTAction != nextBestST || _nextBestAOEAction != nextBestAOE)
            {
                Log($"Next-best changed: ST={_nextBestSTAction}->{nextBestST}, AOE={_nextBestAOEAction}->{nextBestAOE} [{_state}]");
                _nextBestSTAction = nextBestST;
                _nextBestAOEAction = nextBestAOE;
            }
            return _state;
        }

        protected override (ActionID, ulong) DoReplaceActionAndTarget(ActionID actionID, Targets targets)
        {
            if (actionID.Type == ActionType.Spell)
            {
                actionID = (PLDRotation.AID)actionID.ID switch
                {
                    PLDRotation.AID.FastBlade => _config.FullRotation ? _nextBestSTAction : actionID,
                    PLDRotation.AID.TotalEclipse => _config.FullRotation ? _nextBestAOEAction : actionID,
                    PLDRotation.AID.RiotBlade => _config.STCombos ? ActionID.MakeSpell(PLDRotation.GetNextRiotBladeComboAction(_state.ComboLastMove)) : actionID,
                    _ => actionID
                };
            }
            ulong targetID = actionID.Type == ActionType.Spell ? (PLDRotation.AID)actionID.ID switch
            {
                PLDRotation.AID.Shirk => SmartTargetShirk(actionID, targets),
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override void DrawOverlay()
        {
            ImGui.TextUnformatted($"Next: {PLDRotation.ActionShortString(_nextBestSTAction)} / {PLDRotation.ActionShortString(_nextBestAOEAction)}");
            ImGui.TextUnformatted(_strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private PLDRotation.State BuildState()
        {
            PLDRotation.State s = new();
            FillCommonState(s, PLDRotation.IDStatPotion);
            if (Service.ClientState.LocalPlayer != null)
            {
                //s.Gauge = Service.JobGauges.Get<PLDGauge>().OathGauge;

                //foreach (var status in Service.ClientState.LocalPlayer.StatusList)
                //{
                //    switch ((PLDRotation.SID)status.StatusId)
                //    {
                //        case PLDRotation.SID.FightOrFlight:
                //            s.FightOrFlightLeft = StatusDuration(status.RemainingTime);
                //            break;
                //    }
                //}

                s.FightOrFlightCD = SpellCooldown(PLDRotation.AID.FightOrFlight);
                s.RampartCD = SpellCooldown(PLDRotation.AID.Rampart);
                s.ReprisalCD = SpellCooldown(PLDRotation.AID.Reprisal);
                s.ArmsLengthCD = SpellCooldown(PLDRotation.AID.ArmsLength);
                s.ProvokeCD = SpellCooldown(PLDRotation.AID.Provoke);
                s.ShirkCD = SpellCooldown(PLDRotation.AID.Shirk);
            }
            return s;
        }

        private void LogStateChange(PLDRotation.State prev, PLDRotation.State curr)
        {
            // do nothing if not in combat
            if (Service.ClientState.LocalPlayer == null || !Service.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat))
                return;

            // detect expired buffs
            if (curr.ComboTimeLeft == 0 && prev.ComboTimeLeft != 0 && prev.ComboTimeLeft < 1)
                Log($"Expired combo [{curr}]");
        }

        // shirk smart targeting: target if friendly > mouseover if friendly > other tank
        private ulong SmartTargetShirk(ActionID action, Targets targets)
        {
            targets = SmartQueueTarget(action, targets);
            var target = SmartTargetFriendly(targets, _config.SmartShirkTarget);
            if (target != null)
                return target.InstanceID;

            if (_config.SmartShirkTarget)
            {
                target = Autorot.Bossmods.WorldState.Party.WithoutSlot().FirstOrDefault(a => a.InstanceID != Service.ClientState.LocalPlayer?.ObjectId && a.Role == Role.Tank);
                if (target != null)
                    return target.InstanceID;
            }

            // can't find good target, deactivate smart-queue entry to prevent silly spam
            Log($"Smart-target failed, removing from queue");
            SmartQueueDeactivate(action);
            return targets.MainTarget;
        }

        // check whether any targetable enemies are in reprisal range (TODO: consider checking only target?..)
        private bool AllowReprisal()
        {
            var playerPos = Service.ClientState.LocalPlayer?.Position ?? new();
            return Service.ObjectTable.Any(o => o.ObjectKind == ObjectKind.BattleNpc && (BattleNpcSubKind)o.SubKind == BattleNpcSubKind.Enemy && Utils.GameObjectIsTargetable(o) && (o.Position - playerPos).LengthSquared() <= (5 + o.HitboxRadius) * (5 + o.HitboxRadius));
        }
    }
}
