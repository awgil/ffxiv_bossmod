using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using System;

namespace BossMod
{
    class Aspho1S : IBossModule
    {
        private enum OID
        {
            Boss = 0x3522,
            Helper = 0x233C,
            FlailLR = 0x3523, // "anchor" weapon, purely visual
            FlailI = 0x3524, // "ball" weapon, also used for knockbacks
            FlailO = 0x3525, // "chakram" weapon
        };

        private enum AID
        {
            GaolerFlailRL = 26102, // Boss->Boss
            GaolerFlailLR = 26103, // Boss->Boss
            GaolerFlailIO1 = 26104, // Boss->Boss
            GaolerFlailIO2 = 26105, // Boss->Boss
            GaolerFlailOI1 = 26106, // Boss->Boss
            GaolerFlailOI2 = 26107, // Boss->Boss
            Aetherflail1 = 26114, // Boss->Boss -- seen BlueRI & RedRO
            Aetherflail2 = 26115, // Boss->Boss -- seen BlueLO, RedLI, RedLO - maybe it's *L*?
            Aetherflail3 = 26118, // Boss->Boss -- seen BlueOL
            Aetherflail4 = 26119, // Boss->Boss -- seen RedOR
            KnockbackGrace = 26126, // Boss->MT
            KnockbackPurge = 26127, // Boss->MT
            ShiningCells = 26134, // Boss->Boss, raidwide aoe
            SlamShut = 26135, // Boss->Boss, raidwide aoe
            Aetherchain = 26137, // Boss->Boss
            ShacklesOfTime = 26140, // Boss->Boss
            Intemperance = 26142, // Boss->Boss
            IntemperateTormentUp = 26143, // Boss->Boss (bottom->top)
            IntemperateTormentDown = 26144, // Boss->Boss (bottom->top)
            AetherialShackles = 26149, // Boss->Boss
            FourShackles = 26150, // Boss->Boss
            HeavyHand = 26153, // Boss->MT, generic tankbuster
            WarderWrath = 26154, // Boss->Boss, generic raidwide
            GaolerFlailR1 = 28070, // Helper->Helper, first hit, right-hand cone
            GaolerFlailL1 = 28071, // Helper->Helper, first hit, left-hand cone
            GaolerFlailI1 = 28072, // Helper->Helper, first hit, point-blank
            GaolerFlailO1 = 28073, // Helper->Helper, first hit, donut
            GaolerFlailR2 = 28074, // Helper->Helper, second hit, right-hand cone
            GaolerFlailL2 = 28075, // Helper->Helper, second hit, left-hand cone
            GaolerFlailI2 = 28076, // Helper->Helper, second hit, point-blank
            GaolerFlailO2 = 28077, // Helper->Helper, second hit, donut
        };

        private EventGenerator _gen;
        private Aspho1SStages _stages = new();

        public Aspho1S(EventGenerator gen)
        {
            _gen = gen;
            _gen.PlayerCombatEnter += EnterCombat;
            _gen.PlayerCombatExit += ExitCombat;
            _gen.ActorCastStarted += ActorCastStarted;
            _gen.ActorCastFinished += ActorCastFinished;
        }

        public void Dispose()
        {
            _gen.PlayerCombatEnter += ExitCombat;
            _gen.PlayerCombatExit -= ExitCombat;
            _gen.ActorCastStarted -= ActorCastStarted;
            _gen.ActorCastFinished -= ActorCastFinished;
        }

        public void Draw()
        {
            _stages.Draw();
            _stages.DrawDebugButtons();
        }

        private void EnterCombat(object? sender, PlayerCharacter? pc)
        {
            _stages.Start();
        }

        private void ExitCombat(object? sender, PlayerCharacter? pc)
        {
            _stages.Reset();
        }

        private void ActorCastStarted(object? sender, EventGenerator.Actor actor)
        {
            if ((OID)actor.Chara.DataId == OID.Boss)
            {
                _stages.NotifyCastStart();

                bool hintSuccess = false;
                switch ((AID)actor.CastActionID)
                {
                    case AID.GaolerFlailRL:
                        hintSuccess = SetSafeZoneHint("Flails", "left->right");
                        break;
                    case AID.GaolerFlailLR:
                        hintSuccess = SetSafeZoneHint("Flails", "right->left");
                        break;
                    case AID.GaolerFlailIO1:
                    case AID.GaolerFlailIO2:
                        hintSuccess = SetSafeZoneHint("Flails", "out->in");
                        break;
                    case AID.GaolerFlailOI1:
                    case AID.GaolerFlailOI2:
                        hintSuccess = SetSafeZoneHint("Flails", "in->out");
                        break;
                    case AID.Aetherflail1:
                    case AID.Aetherflail2:
                    case AID.Aetherflail3:
                    case AID.Aetherflail4:
                        hintSuccess = _stages.NextStage.Name == "Aetherflail";
                        break;
                    case AID.Aetherchain:
                        hintSuccess = _stages.NextStage.Name == "Aetherchain";
                        break;
                    case AID.KnockbackGrace:
                        hintSuccess = SetKnockbackHint("stack!");
                        break;
                    case AID.KnockbackPurge:
                        hintSuccess = SetKnockbackHint("gtfo!");
                        break;
                    case AID.ShiningCells:
                        hintSuccess = _stages.NextStage.Name == "Cells";
                        break;
                    case AID.SlamShut:
                        hintSuccess = _stages.NextStage.Name == "SlamShut";
                        break;
                    case AID.ShacklesOfTime:
                        hintSuccess = _stages.NextStage.Name == "ShackleOfTime";
                        break;
                    case AID.Intemperance:
                        hintSuccess = _stages.NextStage.Name == "Intemperance";
                        break;
                    case AID.IntemperateTormentUp:
                        hintSuccess = SetExplosionOrderHint("bottom->top");
                        break;
                    case AID.IntemperateTormentDown:
                        hintSuccess = SetExplosionOrderHint("top->bottom");
                        break;
                    case AID.AetherialShackles:
                        // TODO: shackle helper...
                        hintSuccess = _stages.NextStage.Name == "Shackles";
                        break;
                    case AID.FourShackles:
                        // TODO: shackle helper...
                        hintSuccess = _stages.NextStage.Name == "FourShackles";
                        break;
                    case AID.HeavyHand:
                        hintSuccess = _stages.NextStage.Name == "Tankbuster";
                        break;
                    case AID.WarderWrath:
                        hintSuccess = _stages.NextStage.Name == "Raidwide";
                        break;
                }

                if (!hintSuccess)
                {
                    PluginLog.Log($"[aspho1s] Unexpected boss cast start {Utils.ActionString(actor.CastActionID)} in stage {_stages.NextStage.Name}");
                }
            }
        }

        private void ActorCastFinished(object? sender, EventGenerator.Actor actor)
        {
            if ((OID)actor.Chara.DataId == OID.Boss)
            {
                _stages.NotifyCastEnd();

                // TODO: debug for statuses...
                if ((AID)actor.CastActionID == AID.FourShackles)
                {
                    DebugObjects.DumpObjectTable();
                }
            }
        }

        private bool SetSafeZoneHint(string expectedStage, string hint)
        {
            if (_stages.NextStage.Name != expectedStage)
                return false;
            _stages.Hint = $"safe zones: {hint}";
            return true;
        }

        private bool SetKnockbackHint(string hint)
        {
            if (_stages.NextStage.Name != "Knockback")
                return false;
            _stages.Hint = $"type: {hint}";
            return true;
        }

        private bool SetExplosionOrderHint(string hint)
        {
            if (_stages.NextStage.Name != "CubeExplode")
                return false;
            _stages.Hint = $"order: {hint}";
            return true;
        }
    }
}
