﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace BossMod.BLM
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private BLMConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;
        private DateTime _lastManaTick;
        private uint _prevMP;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<BLMConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();
            _prevMP = player.CurMP;

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override CommonRotation.PlayerState GetState() => _state;

        public override CommonRotation.Strategy GetStrategy() => _strategy;

        public override Targeting SelectBetterTarget(AIHints.Enemy initial)
        {
            // TODO: multidot?..
            var bestTarget = initial;
            if (_state.Unlocked(AID.Blizzard2))
            {
                var bestAOECount = NumTargetsHitByAOE(initial.Actor);
                foreach (
                    var candidate in Autorot.Hints.PriorityTargets.Where(
                        e => e != initial && e.Actor.Position.InCircle(Player.Position, 25)
                    )
                )
                {
                    var candidateAOECount = NumTargetsHitByAOE(candidate.Actor);
                    if (
                        candidateAOECount > bestAOECount
                        || candidateAOECount == bestAOECount
                            && candidate.Actor.HP.Cur > bestTarget.Actor.HP.Cur
                    )
                    {
                        bestTarget = candidate;
                        bestAOECount = candidateAOECount;
                    }
                }
            }
            return new(bestTarget, bestTarget.StayAtLongRange ? 25 : 15);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);
            _strategy.UseAOERotation =
                Autorot.PrimaryTarget != null
                && autoAction != AutoActionST
                && NumTargetsHitByAOE(Autorot.PrimaryTarget) >= 3;
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(AID.Transpose))
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.Transpose),
                    Player,
                    !Player.InCombat && _state.ElementalLevel > 0 && _state.CurMP < 10000
                );
            if (_state.Unlocked(AID.Manaward))
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.Manaward),
                    Player,
                    Player.HP.Cur < Player.HP.Max * 0.8f
                );
            if (_state.Unlocked(AID.UmbralSoul))
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.UmbralSoul),
                    Player,
                    !Player.InCombat
                        && _state.ElementalLevel < 0
                        && (_state.UmbralHearts < 3 || _state.ElementalLeft < 3)
                );
            if (_state.Unlocked(AID.Sharpcast))
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.Sharpcast),
                    Player,
                    !Player.InCombat && _state.SharpcastLeft == 0
                );
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
                return new();
            var aid = Rotation.GetNextBestGCD(_state, _strategy);
            return MakeResult(aid, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
                return new();

            ActionID res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
            if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

            if (res.ID == (uint)AID.LeyLines)
                return new NextAction(res, null, Player.PosRot.XYZ(), ActionSource.Automatic);

            return MakeResult(res, Autorot.PrimaryTarget);
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            var gauge = Service.JobGauges.Get<BLMGauge>();

            // track mana ticks
            if (_prevMP < Player.CurMP && !gauge.InAstralFire)
            {
                var expectedTick = Rotation.MPTick(-gauge.UmbralIceStacks);
                if (Player.CurMP - _prevMP == expectedTick)
                {
                    _lastManaTick = Autorot.WorldState.CurrentTime;
                }
            }
            _prevMP = Player.CurMP;
            _state.TimeToManaTick =
                3
                - (
                    _lastManaTick != default
                        ? (float)(Autorot.WorldState.CurrentTime - _lastManaTick).TotalSeconds % 3
                        : 0
                );

            _state.ElementalLevel = gauge.InAstralFire
                ? gauge.AstralFireStacks
                : -gauge.UmbralIceStacks;
            _state.ElementalLeft = gauge.ElementTimeRemaining * 0.001f;
            _state.EnochianTimer = gauge.EnochianTimer * 0.001f;
            _state.UmbralHearts = gauge.UmbralHearts;
            _state.Polyglot = gauge.PolyglotStacks;
            _state.Paradox = gauge.IsParadoxActive;

            _state.TriplecastLeft = StatusDetails(Player, SID.Triplecast, Player.InstanceID).Left;
            _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
            _state.SharpcastLeft = StatusDetails(Player, SID.Sharpcast, Player.InstanceID).Left;
            _state.ThundercloudLeft = StatusDetails(
                Player,
                SID.Thundercloud,
                Player.InstanceID
            ).Left;
            _state.FirestarterLeft = StatusDetails(Player, SID.Firestarter, Player.InstanceID).Left;

            _state.TargetThunderLeft = Math.Max(
                StatusDetails(
                    Autorot.PrimaryTarget,
                    _state.ExpectedThunder1,
                    Player.InstanceID
                ).Left,
                StatusDetails(
                    Autorot.PrimaryTarget,
                    _state.ExpectedThunder2,
                    Player.InstanceID
                ).Left
            );

            _state.LeyLinesLeft = StatusDetails(Player, SID.LeyLines, Player.InstanceID).Left;
            _state.InLeyLines =
                StatusDetails(Player, SID.CircleOfPower, Player.InstanceID).Left > 0;
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.Fire1).PlaceholderForAuto = SupportedSpell(
                AID.Fire4
            ).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.Fire2).PlaceholderForAuto = _config.FullRotation
                ? AutoActionAOE
                : AutoActionNone;

            // smart targets
            SupportedSpell(AID.AetherialManipulation).TransformTarget = _config.MouseoverFriendly
                ? SmartTargetFriendly
                : null;
        }

        private int NumTargetsHitByAOE(Actor primary) =>
            Autorot.Hints.NumPriorityTargetsInAOECircle(primary.Position, 5);
    }
}
