//using System;
//using System.Collections.Generic;

//namespace BossMod
//{
//    // data-driven definition of a boss module
//    // such an approach allows automatically building initial versions of sequences during pulls, live editing, etc. - useful for prog, to avoid tedious manual log parsing
//    public class BossModuleDesc
//    {
//        public ushort ZoneID; // boss module is automatically activated when entering this zone
//        public Type? BossModuleType; // concrete class that is instantiated; implements boss module interface and is constructed with this desc structure
//        public Guid InitialState;

//        public class Npc
//        {
//            public string Name = "";
//            public string Description = "";
//            public int NumSpawnedBeforeFight;
//            public bool IsDynamicallySpawned;
//        }
//        public Dictionary<uint, Npc> Npcs = new();

//        [Flags]
//        public enum ActionTargets
//        {
//            None = 0,
//            Self = 1 << 0,
//            MainTarget = 1 << 1,
//            OtherPlayer = 1 << 2,
//            OtherEnemy = 1 << 3,
//        }

//        public class Action
//        {
//            public string Name = "";
//            public Dictionary<uint, ActionTargets> Casters = new();
//        }
//        public Dictionary<uint, Action> Actions = new();

//        public enum StateType
//        {
//            Custom, // custom state; all three methods are searched in boss module type and assigned as state actions
//            Timeout, // CommonStates.Timeout: custom update method is ignored
//            SpellCastStart, // CommonStates.CastStart: custom update method should return WorldState.Actor?, for which condition is checked; enter/exit are customizable
//            SpellCastEnd, // CommonStates.CastEnd: custom update method should return WorldState.Actor?, for which condition is checked; enter/exit are customizable
//        };

//        public class Transition
//        {
//            public uint Argument; // for states with predefined update logic (e.g. SpellCast*), this is event argument (e.g. action ID) corresponding to transition; for custom logic, this is arbitrary; 0 means "default transition"
//            public Guid Target; // if non-null and this reaction passes, transition to this state instead of default
//            public string Method = ""; // either name of the method to execute (should return bool - whether transition is to happen), or one of predefined methods "accept", "warn", "ignore"
//        }

//        public class StateDesc
//        {
//            public string Name = "";
//            public float Duration;
//            public StateMachine.StateHint Flags;
//            public StateType type;
//            public string MainMethod = ""; // note that meaning and expected signature differs depending on type; this is either custom update method or get-actor method or whatever
//            public string EnterMethod = "";
//            public string ExitMethod = "";
//            public List<Transition> Transitions = new();
//        }
//        public Dictionary<Guid, StateDesc> States = new();
//    }
//}
