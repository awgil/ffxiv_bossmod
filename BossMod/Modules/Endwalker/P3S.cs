using ImGuiNET;
using System;
using System.Collections.Generic;

namespace BossMod
{
    public class P3S : BossModule
    {
        public enum OID : uint
        {
            Boss = 0x353F,
            Helper = 0x233C, // x45
        };

        public enum AID : uint
        {
            ExperimentalFireplume = 26304, // Boss->Boss, this is 'multiorb' variant at least
            HeatOfCondemnation = 26368, // Boss->Boss
            ScorchedExaltation = 26374, // Boss->Boss
        };

        private WorldState.Actor? _boss;

        public P3S(WorldState ws)
            : base(ws, 8)
        {
            StateMachine.State? s;
            s = BuildScorchedExaltationState(ref InitialState, 8);
            s = BuildHeatOfCondemnationState(ref s.Next, 3);
            s = BuildExperimentalFireplumeState(ref s.Next, 6);
        }

        protected override void DrawArena()
        {
            Arena.Border();
        }

        protected override void NonPlayerCreated(WorldState.Actor actor)
        {
            if ((OID)actor.OID == OID.Boss)
            {
                _boss = actor;
            }
        }

        protected override void NonPlayerDestroyed(WorldState.Actor actor)
        {
            if (_boss == actor)
            {
                _boss = null;
            }
        }

        protected override void Reset()
        {
        }

        private StateMachine.State BuildScorchedExaltationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.ScorchedExaltation, delay, 5, "ScorchedExaltation");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State BuildHeatOfCondemnationState(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, () => _boss, AID.HeatOfCondemnation, delay, 6, "Tether");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State BuildExperimentalFireplumeState(ref StateMachine.State? link, float delay)
        {
            // TODO: detect AOE type and direction?..
            // multi-orb version: immediately after cast end, 8 helpers teleport to positions and start casting 
            return CommonStates.Cast(ref link, () => _boss, AID.ExperimentalFireplume, delay, 5, "Fireplume");
        }
    }
}
