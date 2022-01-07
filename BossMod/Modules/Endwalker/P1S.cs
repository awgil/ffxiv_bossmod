using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    public class P1S : IBossModule
    {
        public enum OID : uint
        {
            Boss = 0x3522,
            Helper = 0x233C,
            FlailLR = 0x3523, // "anchor" weapon, purely visual
            FlailI = 0x3524, // "ball" weapon, also used for knockbacks
            FlailO = 0x3525, // "chakram" weapon
        };

        public enum AID : uint
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

        class SpellWithHintState : SpellCastState
        {
            private string _hint = "";
            private Func<AID, string> _getHint;

            public SpellWithHintState(int nextState, string name, WorldState ws, double timeBeforeCast, double castDuration, double resolveTime, Func<AID, string> getHint)
                : base(nextState, name, ws, (uint)OID.Boss, timeBeforeCast, castDuration, resolveTime)
            {
                _getHint = getHint;
            }

            public override bool DrawHint()
            {
                if (_hint.Length > 0)
                    ImGui.Text(_hint);
                return true;
            }

            protected override void CastStarted(WorldState.Actor actor)
            {
                _hint = _getHint((AID)actor.CastInfo!.ActionID);
            }

            protected override void Clear()
            {
                _hint = "";
            }
        }

        private WorldState _ws;
        private WorldState.Actor? _boss;
        private StateMachine _sm;
        private MiniArena _arena = new();

        public P1S(WorldState ws)
        {
            _ws = ws;
            _ws.PlayerInCombatChanged += EnterExitCombat;
            _ws.ActorCreated += ActorCreated;
            _ws.ActorDestroyed += ActorDestroyed;
            foreach (var v in _ws.Actors)
                ActorCreated(null, v.Value);

            StateMachine.Desc shacklesAndAOE = new();
            shacklesAndAOE.Add(new ExpectedSpellCastState(shacklesAndAOE.NextTransitionID, "Shackles", ws, (uint)OID.Boss, (uint)AID.AetherialShackles, 6, 3));
            shacklesAndAOE.Add(new ExpectedSpellCastState(shacklesAndAOE.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 4, 5));
            shacklesAndAOE.Add(new TimeoutState(-1, "Shackles resolve", 10));

            StateMachine.Desc intemp1 = new();
            intemp1.Add(new ExpectedSpellCastState(intemp1.NextTransitionID, "Intemperance", ws, (uint)OID.Boss, (uint)AID.Intemperance, 11, 2));
            intemp1.Add(new SpellWithHintState(intemp1.NextTransitionID, "Cube1", ws, 6, 10, 1, IntemperanceHint));
            intemp1.Add(new ExpectedSpellCastState(intemp1.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 1, 5));
            intemp1.Add(new TimeoutState(intemp1.NextTransitionID, "Cube2", 5));
            intemp1.Add(new ExpectedSpellCastState(intemp1.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 0, 5));
            intemp1.Add(new TimeoutState(-1, "Cube3", 6));

            StateMachine.Desc sotAndTB = new();
            sotAndTB.Add(new ExpectedSpellCastState(sotAndTB.NextTransitionID, "ShacklesOfTime", ws, (uint)OID.Boss, (uint)AID.ShacklesOfTime, 4, 4));
            sotAndTB.Add(new ExpectedSpellCastState(sotAndTB.NextTransitionID, "Tankbuster", ws, (uint)OID.Boss, (uint)AID.HeavyHand, 5, 5));
            sotAndTB.Add(new TimeoutState(-1, "Shackles resolve", 5));

            StateMachine.Desc fourShackles = new();
            fourShackles.Add(new ExpectedSpellCastState(fourShackles.NextTransitionID, "FourShackles", ws, (uint)OID.Boss, (uint)AID.FourShackles, 13, 3));
            fourShackles.Add(new TimeoutState(fourShackles.NextTransitionID, "Hit1", 10));
            fourShackles.Add(new TimeoutState(fourShackles.NextTransitionID, "Hit2", 5));
            fourShackles.Add(new TimeoutState(fourShackles.NextTransitionID, "Hit3", 5));
            fourShackles.Add(new TimeoutState(-1, "Hit4", 5));

            StateMachine.Desc intemp2 = new();
            intemp2.Add(new ExpectedSpellCastState(intemp2.NextTransitionID, "Intemperance", ws, (uint)OID.Boss, (uint)AID.Intemperance, 11, 2));
            intemp2.Add(new SpellWithHintState(intemp2.NextTransitionID, "Cube1", ws, 6, 10, 1, IntemperanceHint));
            intemp2.Add(new TimeoutState(intemp2.NextTransitionID, "Cube2", 11));
            intemp2.Add(new TimeoutState(intemp2.NextTransitionID, "Flail", 7)); // TODO: cast-start happens 8 sec before cube2, cast-end happens 3 sec after cube2
            intemp2.Add(new TimeoutState(-1, "Cube3", 4));

            StateMachine.Desc shacklesAndAetherchainFirst = new();
            shacklesAndAetherchainFirst.Add(new ExpectedSpellCastState(shacklesAndAetherchainFirst.NextTransitionID, "Aetherchain", ws, (uint)OID.Boss, (uint)AID.Aetherchain, 6, 5));
            shacklesAndAetherchainFirst.Add(new ExpectedSpellCastState(shacklesAndAetherchainFirst.NextTransitionID, "Aetherchain", ws, (uint)OID.Boss, (uint)AID.Aetherchain, 3, 5));
            shacklesAndAetherchainFirst.Add(new TimeoutState(-1, "Shackles resolve", 0));

            StateMachine.Desc shacklesAndAetherchainSecond = new();
            shacklesAndAetherchainSecond.Add(new ExpectedSpellCastState(shacklesAndAetherchainSecond.NextTransitionID, "Shackles", ws, (uint)OID.Boss, (uint)AID.AetherialShackles, 9, 3));
            shacklesAndAetherchainSecond.Add(new ExpectedSpellCastState(shacklesAndAetherchainSecond.NextTransitionID, "Aetherchain", ws, (uint)OID.Boss, (uint)AID.Aetherchain, 6, 5));
            shacklesAndAetherchainSecond.Add(new ExpectedSpellCastState(shacklesAndAetherchainSecond.NextTransitionID, "Aetherchain", ws, (uint)OID.Boss, (uint)AID.Aetherchain, 3, 5));
            shacklesAndAetherchainSecond.Add(new TimeoutState(-1, "Shackles resolve", 0));

            StateMachine.Desc sotAndKnockbackFirst = new();
            sotAndKnockbackFirst.Add(new SpellWithHintState(sotAndKnockbackFirst.NextTransitionID, "Knockback", ws, 2, 5, 5, KnockbackHint));
            sotAndKnockbackFirst.Add(new TimeoutState(-1, "Shackles resolve", 3));

            StateMachine.Desc sotAndKnockbackSecond = new();
            sotAndKnockbackSecond.Add(new ExpectedSpellCastState(sotAndKnockbackSecond.NextTransitionID, "ShacklesOfTime", ws, (uint)OID.Boss, (uint)AID.ShacklesOfTime, 6, 4));
            sotAndKnockbackSecond.Add(new SpellWithHintState(sotAndKnockbackSecond.NextTransitionID, "Knockback", ws, 2, 5, 5, KnockbackHint));
            sotAndKnockbackSecond.Add(new TimeoutState(-1, "Shackles resolve", 3));

            StateMachine.Desc desc = new();
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "Tankbuster", ws, (uint)OID.Boss, (uint)AID.HeavyHand, 8, 5));
            desc.Add(new ComplexState(desc.NextTransitionID, shacklesAndAOE));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Flail", ws, 4, 12, 4, FlailHint));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Knockback", ws, 5, 5, 5, KnockbackHint));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Flail", ws, 3, 12, 4, FlailHint));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 5, 5));
            desc.Add(new ComplexState(desc.NextTransitionID, intemp1));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Knockback", ws, 5, 5, 5, KnockbackHint));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "Cells", ws, (uint)OID.Boss, (uint)AID.ShiningCells, 8, 7));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Aetherflail", ws, 8, 12, 4, AetherflailHint));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Knockback", ws, 7, 5, 5, KnockbackHint));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Aetherflail", ws, 2, 12, 4, AetherflailHint));
            desc.Add(new ComplexState(desc.NextTransitionID, sotAndTB));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "SlamShut", ws, (uint)OID.Boss, (uint)AID.SlamShut, 1, 6));
            desc.Add(new ComplexState(desc.NextTransitionID, fourShackles));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 5, 5));
            desc.Add(new ComplexState(desc.NextTransitionID, intemp2));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 3, 5));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "Cells", ws, (uint)OID.Boss, (uint)AID.ShiningCells, 11, 7));
            desc.Add(new VariantSpellCastState(ws, (uint)OID.Boss, 6, new VariantSpellCastState.Variant[] {
                new VariantSpellCastState.Variant(50, "Shackles+Aetherchain", (uint)AID.AetherialShackles, 3),
                new VariantSpellCastState.Variant(60, "ShacklesOfTime+Knockback", (uint)AID.ShacklesOfTime, 4)
            }));
            desc.NextID = 50;
            desc.Add(new ComplexState(desc.NextTransitionID, shacklesAndAetherchainFirst));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 7, 5));
            desc.Add(new ComplexState(desc.NextTransitionID, sotAndKnockbackSecond));
            desc.Add(new ExpectedSpellCastState(70, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 3, 5));
            desc.NextID = 60;
            desc.Add(new ComplexState(desc.NextTransitionID, sotAndKnockbackFirst));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 3, 5));
            desc.Add(new ComplexState(desc.NextTransitionID, shacklesAndAetherchainFirst));
            desc.Add(new ExpectedSpellCastState(70, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 7, 5));
            desc.NextID = 70;
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Aetherflail", ws, 9, 12, 4, AetherflailHint));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Aetherflail", ws, 6, 12, 4, AetherflailHint));
            desc.Add(new SpellWithHintState(desc.NextTransitionID, "Aetherflail", ws, 6, 12, 4, AetherflailHint));
            desc.Add(new ExpectedSpellCastState(desc.NextTransitionID, "AOE", ws, (uint)OID.Boss, (uint)AID.WarderWrath, 13, 5));
            desc.Add(new TimeoutState(-1, "???", 1000));
            _sm = new(desc);
        }

        public void Dispose()
        {
            _ws.PlayerInCombatChanged -= EnterExitCombat;
            _ws.ActorCreated -= ActorCreated;
            _ws.ActorDestroyed -= ActorDestroyed;
        }

        public void Draw(float cameraAzimuth)
        {
            _sm.Update();
            _sm.Draw();

            _arena.Begin(cameraAzimuth);
            _arena.BorderSquare();
            if (_boss != null)
                _arena.Actor(_boss.Position, 0xff0000ff);
            _arena.End();
        }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            if (inCombat)
                _sm.Start();
            else
                _sm.Reset();
        }

        private void ActorCreated(object? sender, WorldState.Actor actor)
        {
            if ((OID)actor.OID == OID.Boss)
                _boss = actor;
        }

        private void ActorDestroyed(object? sender, WorldState.Actor actor)
        {
            if (_boss == actor)
                _boss = null;
        }

        private static string FlailHint(AID actionID)
        {
            switch (actionID)
            {
                case AID.GaolerFlailRL:
                    return "Order: right->left";
                case AID.GaolerFlailLR:
                    return "Order: left->right";
                case AID.GaolerFlailIO1:
                case AID.GaolerFlailIO2:
                    return "Order: in->out";
                case AID.GaolerFlailOI1:
                case AID.GaolerFlailOI2:
                    return "Order: out->in";
                default:
                    Service.Log($"Unexpected flails action: {actionID}");
                    return "Order: ???";
            }
        }

        private static string KnockbackHint(AID actionID)
        {
            switch (actionID)
            {
                case AID.KnockbackGrace:
                    return "What to do: stack!";
                case AID.KnockbackPurge:
                    return "What to do: gtfo!";
                default:
                    Service.Log($"Unexpected knockback cast: {actionID}");
                    return "What to do: ???";
            }
        }

        private static string IntemperanceHint(AID actionID)
        {
            switch (actionID)
            {
                case AID.IntemperateTormentUp:
                    return "Explosion order: bottom->top";
                case AID.IntemperateTormentDown:
                    return "Explosion order: top->bottom";
                default:
                    Service.Log($"Unexpected intemperance explosion cast: {actionID}");
                    return "Explosion order: ???";
            }
        }

        private static string AetherflailHint(AID actionID)
        {
            // TODO
            switch (actionID)
            {
                case AID.Aetherflail1:
                case AID.Aetherflail2:
                case AID.Aetherflail3:
                case AID.Aetherflail4:
                    return "Order: unknown";
                default:
                    Service.Log($"Unexpected aetherflail cast: {actionID}");
                    return "Order: ???";
            }
        }
    }
}
