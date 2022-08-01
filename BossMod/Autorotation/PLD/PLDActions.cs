//using ImGuiNET;
//using System;
//using System.Linq;

//namespace BossMod
//{
//    class PLDActions : CommonActions
//    {
//        private PLDConfig _config;
//        private PLDRotation.State _state;
//        private PLDRotation.Strategy _strategy;
//        private ActionID _nextBestSTAction = ActionID.MakeSpell(PLDRotation.AID.FastBlade);
//        private ActionID _nextBestAOEAction = ActionID.MakeSpell(PLDRotation.AID.TotalEclipse);

//        public PLDActions(Autorotation autorot, Actor player)
//            : base(autorot, player)
//        {
//            _config = Service.Config.Get<PLDConfig>();
//            _state = BuildState(autorot.WorldState.Actors.Find(player.TargetID));
//            _strategy = new();

//            SmartQueueRegisterSpell(PLDRotation.AID.Rampart);
//            SmartQueueRegisterSpell(PLDRotation.AID.Reprisal);
//            SmartQueueRegisterSpell(PLDRotation.AID.ArmsLength);
//            SmartQueueRegisterSpell(PLDRotation.AID.Provoke);
//            SmartQueueRegisterSpell(PLDRotation.AID.Shirk);
//            SmartQueueRegisterSpell(PLDRotation.AID.LowBlow);
//            SmartQueueRegisterSpell(PLDRotation.AID.Interject);
//            SmartQueueRegister(CommonDefinitions.IDSprint);
//            SmartQueueRegister(PLDRotation.IDStatPotion);
//        }

//        protected override void OnCastSucceeded(ActorCastEvent ev)
//        {
//            Log($"Cast {ev.Action} @ {ev.MainTargetID:X}, next-best={_nextBestSTAction}/{_nextBestAOEAction} [{_state}]");
//        }

//        protected override CommonRotation.PlayerState OnUpdate(Actor? target, bool moving)
//        {
//            var currState = BuildState(target);
//            LogStateChange(_state, currState);
//            _state = currState;

//            FillCommonStrategy(_strategy, PLDRotation.IDStatPotion);

//            // cooldown execution
//            _strategy.ExecuteRampart = SmartQueueActiveSpell(PLDRotation.AID.Rampart);
//            _strategy.ExecuteReprisal = SmartQueueActiveSpell(PLDRotation.AID.Reprisal) && AllowReprisal();
//            _strategy.ExecuteArmsLength = SmartQueueActiveSpell(PLDRotation.AID.ArmsLength);
//            _strategy.ExecuteProvoke = SmartQueueActiveSpell(PLDRotation.AID.Provoke); // TODO: check that not MT already
//            _strategy.ExecuteShirk = SmartQueueActiveSpell(PLDRotation.AID.Shirk); // TODO: check that hate is close to MT...
//            _strategy.ExecuteLowBlow = SmartQueueActiveSpell(PLDRotation.AID.LowBlow);
//            _strategy.ExecuteInterject = SmartQueueActiveSpell(PLDRotation.AID.Interject) && AllowInterject(target);

//            var nextBestST = _config.FullRotation ? PLDRotation.GetNextBestAction(_state, _strategy, false) : ActionID.MakeSpell(PLDRotation.AID.FastBlade);
//            var nextBestAOE = _config.FullRotation ? PLDRotation.GetNextBestAction(_state, _strategy, true) : ActionID.MakeSpell(PLDRotation.AID.TotalEclipse);
//            if (_nextBestSTAction != nextBestST || _nextBestAOEAction != nextBestAOE)
//            {
//                Log($"Next-best changed: ST={_nextBestSTAction}->{nextBestST}, AOE={_nextBestAOEAction}->{nextBestAOE} [{_state}]");
//                _nextBestSTAction = nextBestST;
//                _nextBestAOEAction = nextBestAOE;
//            }
//            return _state;
//        }

//        protected override (ActionID, ulong) DoReplaceActionAndTarget(ActionID actionID, Targets targets)
//        {
//            if (actionID.Type == ActionType.Spell)
//            {
//                actionID = (PLDRotation.AID)actionID.ID switch
//                {
//                    PLDRotation.AID.FastBlade => _config.FullRotation ? _nextBestSTAction : actionID,
//                    PLDRotation.AID.TotalEclipse => _config.FullRotation ? _nextBestAOEAction : actionID,
//                    PLDRotation.AID.RiotBlade => _config.STCombos ? ActionID.MakeSpell(PLDRotation.GetNextRiotBladeComboAction(_state.ComboLastMove)) : actionID,
//                    _ => actionID
//                };
//            }
//            ulong targetID = actionID.Type == ActionType.Spell ? (PLDRotation.AID)actionID.ID switch
//            {
//                PLDRotation.AID.Shirk => SmartTargetCoTank(actionID, targets, _config.SmartShirkTarget)?.InstanceID ?? targets.MainTarget,
//                PLDRotation.AID.Provoke => SmartTargetHostile(actionID, targets, _config.ProvokeMouseover)?.InstanceID ?? targets.MainTarget,
//                _ => targets.MainTarget
//            } : targets.MainTarget;
//            return (actionID, targetID);
//        }

//        public override AIResult CalculateBestAction(Actor player, Actor? primaryTarget, bool moving)
//        {
//            if (primaryTarget?.Type != ActorType.Enemy)
//                return new();
//            // TODO: not all our aoe moves are radius 5 point-blank...
//            bool useAOE = _state.UnlockedTotalEclipse && Autorot.PotentialTargetsInRangeFromPlayer(5).Count() > 2;
//            var action = useAOE ? _nextBestAOEAction : _nextBestSTAction;
//            return new() { Action = action, Target = primaryTarget };
//        }

//        public override void DrawOverlay()
//        {
//            ImGui.TextUnformatted($"Next: {PLDRotation.ActionShortString(_nextBestSTAction)} / {PLDRotation.ActionShortString(_nextBestAOEAction)}");
//            ImGui.TextUnformatted(_strategy.ToString());
//            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
//            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
//            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
//        }

//        private PLDRotation.State BuildState(Actor? target)
//        {
//            PLDRotation.State s = new();
//            FillCommonPlayerState(s, target, PLDRotation.IDStatPotion);

//            //s.Gauge = Service.JobGauges.Get<PLDGauge>().OathGauge;

//            foreach (var status in Player.Statuses)
//            {
//                switch ((PLDRotation.SID)status.ID)
//                {
//                    case PLDRotation.SID.FightOrFlight:
//                        s.FightOrFlightLeft = StatusDuration(status.ExpireAt);
//                        break;
//                }
//            }

//            return s;
//        }

//        private void LogStateChange(PLDRotation.State prev, PLDRotation.State curr)
//        {
//            // do nothing if not in combat
//            if (!Player.InCombat)
//                return;

//            // detect expired buffs
//            if (curr.ComboTimeLeft == 0 && prev.ComboTimeLeft != 0 && prev.ComboTimeLeft < 1)
//                Log($"Expired combo [{curr}]");
//        }

//        // check whether any targetable enemies are in reprisal range (TODO: consider checking only target?..)
//        private bool AllowReprisal()
//        {
//            return Autorot.PotentialTargetsInRangeFromPlayer(5).Any();
//        }

//        private bool AllowInterject(Actor? target)
//        {
//            return target?.CastInfo?.Interruptible ?? false;
//        }
//    }
//}
