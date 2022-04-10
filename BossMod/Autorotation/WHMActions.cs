using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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

        public WHMActions(Autorotation autorot)
            : base(autorot)
        {
            _config = autorot.Config.Get<WHMConfig>();
            _state = BuildState();
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
            //SmartQueueRegister(WHMRotation.IDStatPotion);
        }

        protected override void OnCastSucceeded(ActionID actionID)
        {
            Log($"Cast {actionID}, next-best={_nextBestSTDamageAction}/{_nextBestAOEDamageAction}/{_nextBestSTHealAction}/{_nextBestAOEHealAction} [{_state}]");
        }

        protected override CommonRotation.State OnUpdate()
        {
            var currState = BuildState();
            LogStateChange(_state, currState);
            _state = currState;

            FillCommonStrategy(_strategy, WHMRotation.IDStatPotion);

            var player = Service.ClientState.LocalPlayer;
            if (player != null)
            {
                _strategy.NumAssizeMedica1Targets = CountAOEHealTargets(15, player.Position);
                _strategy.NumRaptureMedica2Targets = CountAOEHealTargets(20, player.Position);
                _strategy.NumCure3Targets = SmartCure3Target().Item2;
            }
            else
            {
                _strategy.NumAssizeMedica1Targets = _strategy.NumRaptureMedica2Targets = _strategy.NumCure3Targets = 0;
            }
            _strategy.EnableAssize = AllowAssize(); // note: should be plannable...
            _strategy.AllowReplacingHealWithMisery = _config.NeverOvercapBloodLilies && TargetIsEnemy();

            // cooldown execution
            _strategy.ExecuteAsylum = SmartQueueActiveSpell(WHMRotation.AID.Asylum);
            _strategy.ExecuteDivineBenison = SmartQueueActiveSpell(WHMRotation.AID.DivineBenison) && AllowDivineBenison();
            _strategy.ExecuteTetragrammaton = SmartQueueActiveSpell(WHMRotation.AID.Tetragrammaton);
            _strategy.ExecuteBenediction = SmartQueueActiveSpell(WHMRotation.AID.Benediction);
            _strategy.ExecuteLiturgyOfTheBell = SmartQueueActiveSpell(WHMRotation.AID.LiturgyOfTheBell);
            _strategy.ExecuteSwiftcast = SmartQueueActiveSpell(WHMRotation.AID.Swiftcast);
            _strategy.ExecuteTemperance = SmartQueueActiveSpell(WHMRotation.AID.Temperance);
            _strategy.ExecuteAquaveil = SmartQueueActiveSpell(WHMRotation.AID.Aquaveil);
            _strategy.ExecuteSurecast = SmartQueueActiveSpell(WHMRotation.AID.Surecast);

            var nextBestSTDamage = _config.FullRotation ? WHMRotation.GetNextBestAction(_state, _strategy, false, false) : ActionID.MakeSpell(WHMRotation.AID.Stone1);
            var nextBestAOEDamage = _config.FullRotation ? WHMRotation.GetNextBestAction(_state, _strategy, true, false) : ActionID.MakeSpell(WHMRotation.AID.Holy1);
            var nextBestSTHeal = _config.FullRotation ? WHMRotation.GetNextBestAction(_state, _strategy, false, true) : ActionID.MakeSpell(WHMRotation.AID.Cure1);
            var nextBestAOEHeal = _config.FullRotation ? WHMRotation.GetNextBestAction(_state, _strategy, true, true) : ActionID.MakeSpell(WHMRotation.AID.Medica1);
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

        protected override (ActionID, uint) DoReplaceActionAndTarget(ActionID actionID, Targets targets)
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
            uint targetID = actionID.Type == ActionType.Spell ? (WHMRotation.AID)actionID.ID switch
            {
                WHMRotation.AID.Cure1 or WHMRotation.AID.Cure2 or WHMRotation.AID.Regen or WHMRotation.AID.AfflatusSolace or WHMRotation.AID.Raise or WHMRotation.AID.Esuna or WHMRotation.AID.Rescue => SmartTargetSTFriendly(actionID, targets, false),
                WHMRotation.AID.DivineBenison or WHMRotation.AID.Tetragrammaton or WHMRotation.AID.Benediction or WHMRotation.AID.Aquaveil => SmartTargetSTFriendly(actionID, targets, true),
                WHMRotation.AID.Cure3 => SmartTargetCure3(targets),
                _ => targets.MainTarget
            } : targets.MainTarget;
            return (actionID, targetID);
        }

        public override void DrawOverlay()
        {
            ImGui.Text($"Next: {WHMRotation.ActionShortString(_nextBestSTDamageAction)} / {WHMRotation.ActionShortString(_nextBestAOEDamageAction)} / {WHMRotation.ActionShortString(_nextBestSTHealAction)} / {WHMRotation.ActionShortString(_nextBestAOEHealAction)}");
            ImGui.Text(_strategy.ToString());
            ImGui.Text($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            ImGui.Text($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.Text($"GCD={_state.GCD:f3}, AnimLock={_state.AnimationLock:f3}+{_state.AnimationLockDelay:f3}");
        }

        private WHMRotation.State BuildState()
        {
            WHMRotation.State s = new();
            FillCommonState(s, WHMRotation.AID.Stone1, WHMRotation.IDStatPotion);

            var player = Service.ClientState.LocalPlayer;
            if (player != null)
            {
                var gauge = Service.JobGauges.Get<WHMGauge>();
                s.NormalLilies = gauge.Lily;
                s.BloodLilies = gauge.BloodLily;
                s.NextLilyIn = 30 - gauge.LilyTimer * 0.001f;

                foreach (var status in player.StatusList)
                {
                    switch ((WHMRotation.SID)status.StatusId)
                    {
                        case WHMRotation.SID.Swiftcast:
                            s.SwiftcastLeft = StatusDuration(status.RemainingTime);
                            break;
                        case WHMRotation.SID.ThinAir:
                            s.ThinAirLeft = StatusDuration(status.RemainingTime);
                            break;
                        case WHMRotation.SID.Freecure:
                            s.FreecureLeft = StatusDuration(status.RemainingTime);
                            break;
                        case WHMRotation.SID.Medica2:
                            if (status.SourceID == player.ObjectId)
                                s.MedicaLeft = StatusDuration(status.RemainingTime);
                            break;
                    }
                }

                var target = player.TargetObject as BattleChara;
                if (target != null)
                {
                    foreach (var status in target.StatusList)
                    {
                        switch ((WHMRotation.SID)status.StatusId)
                        {
                            case WHMRotation.SID.Aero1:
                            case WHMRotation.SID.Aero2:
                            case WHMRotation.SID.Dia:
                                if (status.SourceID == player.ObjectId)
                                    s.TargetDiaLeft = StatusDuration(status.RemainingTime);
                                break;
                        }
                    }
                }

                s.AssizeCD = SpellCooldown(WHMRotation.AID.Assize);
                s.AsylumCD = SpellCooldown(WHMRotation.AID.Asylum);
                s.DivineBenisonCD = SpellCooldown(WHMRotation.AID.DivineBenison);
                s.TetragrammatonCD = SpellCooldown(WHMRotation.AID.Tetragrammaton);
                s.BenedictionCD = SpellCooldown(WHMRotation.AID.Benediction);
                s.LiturgyOfTheBellCD = SpellCooldown(WHMRotation.AID.LiturgyOfTheBell);
                s.SwiftcastCD = SpellCooldown(WHMRotation.AID.Swiftcast);
                s.LucidDreamingCD = SpellCooldown(WHMRotation.AID.LucidDreaming);
                s.PresenceOfMindCD = SpellCooldown(WHMRotation.AID.PresenceOfMind);
                s.ThinAirCD = SpellCooldown(WHMRotation.AID.ThinAir);
                s.PlenaryIndulgenceCD = SpellCooldown(WHMRotation.AID.PlenaryIndulgence);
                s.TemperanceCD = SpellCooldown(WHMRotation.AID.Temperance);
                s.AquaveilCD = SpellCooldown(WHMRotation.AID.Aquaveil);
                s.SurecastCD = SpellCooldown(WHMRotation.AID.Surecast);
            }
            return s;
        }

        private void LogStateChange(WHMRotation.State prev, WHMRotation.State curr)
        {
            // do nothing if not in combat
            if (Service.ClientState.LocalPlayer == null || !Service.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat))
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

        private uint SmartTargetSTFriendly(ActionID action, Targets targets, bool smartQueued)
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

        private uint SmartTargetCure3(Targets targets)
        {
            var target = SmartTargetFriendly(targets, _config.MouseoverFriendly);
            if (target != null)
                return target.InstanceID;

            return _config.SmartCure3Target ? SmartCure3Target().Item1 : targets.MainTarget;
        }

        private int CountAOEHealTargets(float radius, Vector3 center)
        {
            return LivePartyMembers().Count(o => o.CurrentHp < o.MaxHp && GeometryUtils.PointInCircle(o.Position - center, radius));
        }

        // select best target for cure3, such that most people are hit
        private (uint, int) SmartCure3Target()
        {
            var playerPos = Service.ClientState.LocalPlayer?.Position ?? new();
            return LivePartyMembers().Select(o => (o.ObjectId, GeometryUtils.PointInCircle(o.Position - playerPos, 30) ? CountAOEHealTargets(6, o.Position) : -1)).MaxBy(oc => oc.Item2);
        }

        // TODO: we could use worldstate here if it had HP...
        private IEnumerable<Character> LivePartyMembers()
        {
            if (Service.PartyList.Length > 0)
            {
                foreach (var p in Service.PartyList)
                {
                    var o = p.GameObject as Character;
                    if (o != null && !Utils.GameObjectIsDead(o))
                        yield return o;
                }
            }
            else
            {
                var o = Service.ClientState.LocalPlayer as Character;
                if (o != null && !Utils.GameObjectIsDead(o))
                    yield return o;
            }
        }

        // check whether player's target is hostile
        private bool TargetIsEnemy()
        {
            var target = Service.ClientState.LocalPlayer?.TargetObject;
            return target != null && target.ObjectKind == ObjectKind.BattleNpc && (BattleNpcSubKind)target.SubKind == BattleNpcSubKind.Enemy;
        }

        // check whether any targetable enemies are in assize range
        private bool AllowAssize()
        {
            var playerPos = Service.ClientState.LocalPlayer?.Position ?? new();
            return Service.ObjectTable.Any(o => o.ObjectKind == ObjectKind.BattleNpc && (BattleNpcSubKind)o.SubKind == BattleNpcSubKind.Enemy && Utils.GameObjectIsTargetable(o) && GeometryUtils.PointInCircle(o.Position - playerPos, 15 + o.HitboxRadius));
        }

        // check whether potential divine benison target doesn't already have it applied
        private bool AllowDivineBenison()
        {
            var targets = SmartQueueTargetSpell(WHMRotation.AID.DivineBenison, new());
            var target = SmartTargetFriendly(targets, _config.MouseoverFriendly);
            return target != null && target.FindStatus(WHMRotation.SID.DivineBenison, Service.ClientState.LocalPlayer?.ObjectId ?? 0) == null;
        }
    }
}
