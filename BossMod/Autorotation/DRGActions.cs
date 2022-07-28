using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    class DRGActions : CommonActions
    {
        private DRGConfig _config;
        private DRGRotation.State _state;
        private DRGRotation.Strategy _strategy;
        private ActionID _nextBestSTAction = ActionID.MakeSpell(DRGRotation.AID.TrueThrust);
        private ActionID _nextBestAOEAction = ActionID.MakeSpell(DRGRotation.AID.DoomSpike);

        public DRGActions(Autorotation autorot, Actor player)
            : base(autorot, player, ActionID.MakeSpell(DRGRotation.AID.TrueThrust))
        {
            _config = Service.Config.Get<DRGConfig>();
            _state = BuildState(autorot.WorldState.Actors.Find(player.TargetID));
            _strategy = new();

            SmartQueueRegisterSpell(DRGRotation.AID.ArmsLength);
            SmartQueueRegisterSpell(DRGRotation.AID.SecondWind);
            SmartQueueRegisterSpell(DRGRotation.AID.Bloodbath);
            SmartQueueRegisterSpell(DRGRotation.AID.LegSweep);
            SmartQueueRegister(CommonRotation.IDSprint);
            SmartQueueRegister(DRGRotation.IDStatPotion);
        }

        protected override void OnCastSucceeded(ActorCastEvent ev)
        {
            Log($"Cast {ev.Action} @ {ev.MainTargetID:X}, next-best={_nextBestSTAction}/{_nextBestAOEAction} [{_state}]");
        }

        protected override CommonRotation.State OnUpdate(Actor? target, bool moving)
        {
            var currState = BuildState(target);
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, DRGRotation.IDStatPotion);

            // cooldown execution
            _strategy.ExecuteArmsLength = SmartQueueActiveSpell(DRGRotation.AID.ArmsLength);
            _strategy.ExecuteSecondWind = SmartQueueActiveSpell(DRGRotation.AID.SecondWind) && Player.HP.Cur < Player.HP.Max;
            _strategy.ExecuteBloodbath = SmartQueueActiveSpell(DRGRotation.AID.Bloodbath);
            _strategy.ExecuteLegSweep = SmartQueueActiveSpell(DRGRotation.AID.LegSweep);

            var nextBestST = _config.FullRotation ? DRGRotation.GetNextBestAction(_state, _strategy, false) : ActionID.MakeSpell(DRGRotation.AID.TrueThrust);
            var nextBestAOE = _config.FullRotation ? DRGRotation.GetNextBestAction(_state, _strategy, true) : ActionID.MakeSpell(DRGRotation.AID.DoomSpike);
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
                actionID = (DRGRotation.AID)actionID.ID switch
                {
                    DRGRotation.AID.TrueThrust => _config.FullRotation ? _nextBestSTAction : actionID,
                    DRGRotation.AID.DoomSpike => _config.FullRotation ? _nextBestAOEAction : actionID,
                    _ => actionID
                };
            }
            ulong targetID = actionID.Type == ActionType.Spell ? (DRGRotation.AID)actionID.ID switch
            {
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override AIResult CalculateBestAction(Actor player, Actor? primaryTarget, bool moving)
        {
            if (primaryTarget?.Type != ActorType.Enemy)
                return new();

            // TODO: aoe...
            //bool useAOE = _state.UnlockedArmOfTheDestroyer && Autorot.PotentialTargetsInRangeFromPlayer(5).Count() > 2;
            var action = /*useAOE ? _nextBestAOEAction : */_nextBestSTAction;
            return new() { Action = action, Target = primaryTarget/*, Positional = Positional.Flank*/ };
        }

        public override void DrawOverlay()
        {
            ImGui.TextUnformatted($"Next: {DRGRotation.ActionShortString(_nextBestSTAction)} / {DRGRotation.ActionShortString(_nextBestAOEAction)}");
            ImGui.TextUnformatted(_strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private DRGRotation.State BuildState(Actor? target)
        {
            DRGRotation.State s = new();
            FillCommonState(s, target, DRGRotation.IDStatPotion);

            //s.Chakra = Service.JobGauges.Get<DRGGauge>().Chakra;

            foreach (var status in Player.Statuses)
            {
                switch ((DRGRotation.SID)status.ID)
                {
                    case DRGRotation.SID.PowerSurge:
                        s.PowerSurgeLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }

            s.LifeSurgeCD = SpellCooldown(DRGRotation.AID.LifeSurge);
            s.ArmsLengthCD = SpellCooldown(DRGRotation.AID.ArmsLength);
            s.SecondWindCD = SpellCooldown(DRGRotation.AID.SecondWind);
            s.BloodbathCD = SpellCooldown(DRGRotation.AID.Bloodbath);
            s.LegSweepCD = SpellCooldown(DRGRotation.AID.LegSweep);
            return s;
        }

        private void LogStateChange(DRGRotation.State prev, DRGRotation.State curr)
        {
            // do nothing if not in combat
            if (!Player.InCombat)
                return;

            // detect expired buffs
            //if (curr.Form == DRGRotation.Form.None && prev.Form != DRGRotation.Form.None)
            //    Log($"Dropped form [{curr}]");
        }
    }
}
