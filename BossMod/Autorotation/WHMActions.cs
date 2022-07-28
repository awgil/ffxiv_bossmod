using Dalamud.Game.ClientState.JobGauge.Types;
using ImGuiNET;
using System.Linq;

namespace BossMod
{
    class WHMActions : CommonActions
    {
        private WHMConfig _config;
        private WHMRotation.State _state;
        private WHMRotation.Strategy _strategy;
        private ActionID _nextBestSTDamageAction = ActionID.MakeSpell(WHMRotation.AID.Stone1);
        private ActionID _nextBestAOEDamageAction = ActionID.MakeSpell(WHMRotation.AID.Holy1);
        private ActionID _nextBestSTHealAction = ActionID.MakeSpell(WHMRotation.AID.Cure1);
        private ActionID _nextBestAOEHealAction = ActionID.MakeSpell(WHMRotation.AID.Medica1);

        public WHMActions(Autorotation autorot, Actor player)
            : base(autorot, player)
        {
            _config = Service.Config.Get<WHMConfig>();
            _state = BuildState(autorot.WorldState.Actors.Find(player.TargetID));
            _strategy = new();

            SmartQueueRegisterSpell(WHMRotation.AID.Asylum);
            SmartQueueRegisterSpell(WHMRotation.AID.DivineBenison);
            SmartQueueRegisterSpell(WHMRotation.AID.Tetragrammaton);
            SmartQueueRegisterSpell(WHMRotation.AID.Benediction);
            SmartQueueRegisterSpell(WHMRotation.AID.LiturgyOfTheBell);
            SmartQueueRegisterSpell(WHMRotation.AID.Swiftcast);
            SmartQueueRegisterSpell(WHMRotation.AID.Temperance);
            SmartQueueRegisterSpell(WHMRotation.AID.Aquaveil);
            SmartQueueRegisterSpell(WHMRotation.AID.Surecast);
            SmartQueueRegister(CommonRotation.IDSprint);
            SmartQueueRegister(WHMRotation.IDStatPotion);
        }

        protected override void OnCastSucceeded(ActorCastEvent ev)
        {
            Log($"Cast {ev.Action} @ {ev.MainTargetID:X}, next-best={_nextBestSTDamageAction}/{_nextBestAOEDamageAction}/{_nextBestSTHealAction}/{_nextBestAOEHealAction} [{_state}]");
        }

        protected override CommonRotation.PlayerState OnUpdate(Actor? target, bool moving)
        {
            var currState = BuildState(target);
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, WHMRotation.IDStatPotion);
            _strategy.NumAssizeMedica1Targets = CountAOEHealTargets(15, Player.Position);
            _strategy.NumRaptureMedica2Targets = CountAOEHealTargets(20, Player.Position);
            _strategy.NumCure3Targets = SmartCure3Target().Item2;
            _strategy.EnableAssize = AllowAssize(); // note: should be plannable...
            _strategy.AllowReplacingHealWithMisery = _config.NeverOvercapBloodLilies && target?.Type == ActorType.Enemy;

            // cooldown execution
            _strategy.ExecuteAsylum = SmartQueueActiveSpell(WHMRotation.AID.Asylum);
            _strategy.ExecuteDivineBenison = SmartQueueActiveSpell(WHMRotation.AID.DivineBenison) && AllowDivineBenison(target);
            _strategy.ExecuteTetragrammaton = SmartQueueActiveSpell(WHMRotation.AID.Tetragrammaton);
            _strategy.ExecuteBenediction = SmartQueueActiveSpell(WHMRotation.AID.Benediction);
            _strategy.ExecuteLiturgyOfTheBell = SmartQueueActiveSpell(WHMRotation.AID.LiturgyOfTheBell);
            _strategy.ExecuteSwiftcast = SmartQueueActiveSpell(WHMRotation.AID.Swiftcast);
            _strategy.ExecuteTemperance = SmartQueueActiveSpell(WHMRotation.AID.Temperance);
            _strategy.ExecuteAquaveil = SmartQueueActiveSpell(WHMRotation.AID.Aquaveil);
            _strategy.ExecuteSurecast = SmartQueueActiveSpell(WHMRotation.AID.Surecast);

            var nextBestSTDamage = _config.FullRotation ? WHMRotation.GetNextBestAction(_state, _strategy, false, false, moving) : ActionID.MakeSpell(WHMRotation.AID.Stone1);
            var nextBestAOEDamage = _config.FullRotation ? WHMRotation.GetNextBestAction(_state, _strategy, true, false, moving) : ActionID.MakeSpell(WHMRotation.AID.Holy1);
            var nextBestSTHeal = _config.FullRotation ? WHMRotation.GetNextBestAction(_state, _strategy, false, true, moving) : ActionID.MakeSpell(WHMRotation.AID.Cure1);
            var nextBestAOEHeal = _config.FullRotation ? WHMRotation.GetNextBestAction(_state, _strategy, true, true, moving) : ActionID.MakeSpell(WHMRotation.AID.Medica1);
            if (_nextBestSTDamageAction != nextBestSTDamage || _nextBestAOEDamageAction != nextBestAOEDamage || _nextBestSTHealAction != nextBestSTHeal || _nextBestAOEHealAction != nextBestAOEHeal)
            {
                Log($"Next-best changed: STd={_nextBestSTDamageAction}->{nextBestSTDamage}, AOEd={_nextBestAOEDamageAction}->{nextBestAOEDamage}, STh={_nextBestSTHealAction}->{nextBestSTHeal}, AOEh={_nextBestAOEHealAction}->{nextBestAOEHeal} [{_state}]");
                _nextBestSTDamageAction = nextBestSTDamage;
                _nextBestAOEDamageAction = nextBestAOEDamage;
                _nextBestSTHealAction = nextBestSTHeal;
                _nextBestAOEHealAction = nextBestAOEHeal;
            }
            return _state;
        }

        protected override (ActionID, ulong) DoReplaceActionAndTarget(ActionID actionID, Targets targets)
        {
            if (actionID.Type == ActionType.Spell)
            {
                actionID = (WHMRotation.AID)actionID.ID switch
                {
                    WHMRotation.AID.Stone1 or WHMRotation.AID.Stone2 or WHMRotation.AID.Stone3 or WHMRotation.AID.Stone4 or WHMRotation.AID.Glare1 or WHMRotation.AID.Glare3 => _config.FullRotation ? _nextBestSTDamageAction : actionID,
                    WHMRotation.AID.Holy1 or WHMRotation.AID.Holy3 => _config.FullRotation ? _nextBestAOEDamageAction : actionID,
                    WHMRotation.AID.Cure1 => _config.FullRotation ? _nextBestSTHealAction : actionID,
                    WHMRotation.AID.Medica1 => _config.FullRotation ? _nextBestAOEHealAction : actionID,
                    WHMRotation.AID.Raise => _config.SwiftFreeRaise ? ActionID.MakeSpell(SmartRaiseAction()) : actionID,
                    _ => actionID
                };
            }
            ulong targetID = actionID.Type == ActionType.Spell ? (WHMRotation.AID)actionID.ID switch
            {
                WHMRotation.AID.Cure1 or WHMRotation.AID.Cure2 or WHMRotation.AID.Regen or WHMRotation.AID.AfflatusSolace or WHMRotation.AID.Raise or WHMRotation.AID.Esuna or WHMRotation.AID.Rescue => SmartTargetSTFriendly(actionID, targets, false),
                WHMRotation.AID.DivineBenison or WHMRotation.AID.Tetragrammaton or WHMRotation.AID.Benediction or WHMRotation.AID.Aquaveil => SmartTargetSTFriendly(actionID, targets, true),
                WHMRotation.AID.Cure3 => SmartTargetCure3(targets),
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override AIResult CalculateBestAction(Actor player, Actor? primaryTarget, bool moving)
        {
            if (primaryTarget == null)
                return new();

            if (primaryTarget.Type == ActorType.Enemy)
            {
                // TODO: this kinda works until L45...
                return new() { Action = WHMRotation.GetNextBestAction(_state, _strategy, false, false, moving), Target = primaryTarget };
            }
            else if (!moving && primaryTarget.IsDead)
            {
                return new() { Action = ActionID.MakeSpell(SmartRaiseAction()), Target = primaryTarget };
            }
            else if (primaryTarget.InCombat && primaryTarget.HP.Cur < 0.9f * primaryTarget.HP.Max)
            {
                // TODO: this aoe/st heal selection is not very good...
                var action = _strategy.NumAssizeMedica1Targets > 2 || _strategy.NumRaptureMedica2Targets > 2 || _strategy.NumCure3Targets > 2 ? _nextBestAOEHealAction : _nextBestSTHealAction;
                return new() { Action = action, Target = primaryTarget };
            }
            else if (!moving && primaryTarget.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)))
            {
                return new() { Action = ActionID.MakeSpell(WHMRotation.AID.Esuna), Target = primaryTarget };
            }
            else
            {
                return new();
            }
        }

        public override void DrawOverlay()
        {
            ImGui.TextUnformatted($"Next: {WHMRotation.ActionShortString(_nextBestSTDamageAction)} / {WHMRotation.ActionShortString(_nextBestAOEDamageAction)} / {WHMRotation.ActionShortString(_nextBestSTHealAction)} / {WHMRotation.ActionShortString(_nextBestAOEHealAction)}");
            ImGui.TextUnformatted(_strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private WHMRotation.State BuildState(Actor? target)
        {
            WHMRotation.State s = new();
            FillCommonPlayerState(s, target, WHMRotation.IDStatPotion);

            var gauge = Service.JobGauges.Get<WHMGauge>();
            s.NormalLilies = gauge.Lily;
            s.BloodLilies = gauge.BloodLily;
            s.NextLilyIn = 30 - gauge.LilyTimer * 0.001f;

            foreach (var status in Player.Statuses)
            {
                switch ((WHMRotation.SID)status.ID)
                {
                    case WHMRotation.SID.Swiftcast:
                        s.SwiftcastLeft = StatusDuration(status.ExpireAt);
                        break;
                    case WHMRotation.SID.ThinAir:
                        s.ThinAirLeft = StatusDuration(status.ExpireAt);
                        break;
                    case WHMRotation.SID.Freecure:
                        s.FreecureLeft = StatusDuration(status.ExpireAt);
                        break;
                    case WHMRotation.SID.Medica2:
                        if (status.SourceID == Player.InstanceID)
                            s.MedicaLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }

            if (target != null)
            {
                foreach (var status in target.Statuses)
                {
                    switch ((WHMRotation.SID)status.ID)
                    {
                        case WHMRotation.SID.Aero1:
                        case WHMRotation.SID.Aero2:
                        case WHMRotation.SID.Dia:
                            if (status.SourceID == Player.InstanceID)
                                s.TargetDiaLeft = StatusDuration(status.ExpireAt);
                            break;
                    }
                }
            }

            return s;
        }

        private void LogStateChange(WHMRotation.State prev, WHMRotation.State curr)
        {
            // do nothing if not in combat
            if (!Player.InCombat)
                return;

            // detect expired buffs
            if (curr.SwiftcastLeft == 0 && prev.SwiftcastLeft != 0 && prev.SwiftcastLeft < 1)
                Log($"Expired Swiftcast [{curr}]");
            if (curr.ThinAirLeft == 0 && prev.ThinAirLeft != 0 && prev.ThinAirLeft < 1)
                Log($"Expired ThinAir [{curr}]");
            if (curr.FreecureLeft == 0 && prev.FreecureLeft != 0 && prev.FreecureLeft < 1)
                Log($"Expired Freecure [{curr}]");
        }

        private WHMRotation.AID SmartRaiseAction()
        {
            // 1. swiftcast, if ready and not up yet
            if (_state.UnlockedSwiftcast && _state.SwiftcastLeft <= 0 && _state.SwiftcastCD <= 0)
                return WHMRotation.AID.Swiftcast;

            // 2. thin air, if ready and not up yet
            if (_state.UnlockedThinAir && _state.ThinAirLeft <= 0 && _state.ThinAirCD <= 60)
                return WHMRotation.AID.ThinAir;

            return WHMRotation.AID.Raise;
        }

        private ulong SmartTargetSTFriendly(ActionID action, Targets targets, bool smartQueued)
        {
            if (smartQueued)
                targets = SmartQueueTarget(action, targets);

            var target = SmartTargetFriendly(targets, _config.MouseoverFriendly);
            if (target != null)
                return target.InstanceID;

            // TODO: consider even smarter targeting here: select person with most HP deficit for heals, any with removable debuffs for esuna, ???

            // can't find good target, deactivate smart-queue entry to prevent silly spam
            if (smartQueued)
            {
                Log($"Smart-target failed, removing from queue");
                SmartQueueDeactivate(action);
            }
            return targets.MainTarget; // TODO: self-target is probably not what we want...
        }

        private ulong SmartTargetCure3(Targets targets)
        {
            var target = SmartTargetFriendly(targets, _config.MouseoverFriendly);
            if (target != null)
                return target.InstanceID;

            return _config.SmartCure3Target ? SmartCure3Target().Item1 : targets.MainTarget;
        }

        private int CountAOEHealTargets(float radius, WPos center)
        {
            var rsq = radius * radius;
            return Autorot.WorldState.Party.WithoutSlot().Count(o => o.HP.Cur < o.HP.Max && (o.Position - center).LengthSq() <= rsq);
        }

        // select best target for cure3, such that most people are hit
        private (ulong, int) SmartCure3Target()
        {
            var rsq = 30 * 30;
            return Autorot.WorldState.Party.WithoutSlot().Select(o => (o.InstanceID, (o.Position - Player.Position).LengthSq() <= rsq ? CountAOEHealTargets(6, o.Position) : -1)).MaxBy(oc => oc.Item2);
        }

        // check whether any targetable enemies are in assize range
        private bool AllowAssize()
        {
            return Autorot.PotentialTargetsInRangeFromPlayer(15).Any();
        }

        // check whether potential divine benison target doesn't already have it applied
        private bool AllowDivineBenison(Actor? target)
        {
            var targets = SmartQueueTargetSpell(WHMRotation.AID.DivineBenison, new() { MainTarget = target?.InstanceID ?? 0 });
            var adjTarget = SmartTargetFriendly(targets, _config.MouseoverFriendly);
            return adjTarget != null && adjTarget.FindStatus(WHMRotation.SID.DivineBenison, Player.InstanceID) == null;
        }
    }
}
