using BossMod;
using System;
using System.Collections.Generic;

namespace UIDev
{
    public class Replay
    {
        public class Cast
        {
            public ActionID ID;
            public DateTime Start;
            public DateTime End;
            public Participant? Target;
        }

        public class ActionTarget
        {
            public Participant? Target;
        }

        public class Action
        {
            public ActionID ID;
            public DateTime Time;
            public Participant? Source;
            public Participant? MainTarget;
            public List<ActionTarget> Targets = new();
        }

        public class Participant
        {
            public uint InstanceID;
            public uint OID;
            public ActorType Type;
            public string Name = "";
            public DateTime Spawn;
            public DateTime Despawn;
            public List<Cast> Casts = new();
            public List<Action> Actions = new();
        }

        public class Status
        {
            public uint ID;
            public Participant? Target;
            public Participant? Source;
            public DateTime Apply;
            public DateTime Expire; // initial expire timer
            public DateTime Fade; // status could be removed before or after official expire time
            public ushort StartingExtra;
        }

        public class Tether
        {
            public uint ID;
            public Participant? Source;
            public Participant? Target;
            public DateTime Appear;
            public DateTime Disappear;
        }

        public class Icon
        {
            public uint ID;
            public Participant? Target;
            public DateTime Timestamp;
        }

        public class EnvControl
        {
            public uint Feature;
            public byte Index;
            public uint State;
            public DateTime Timestamp;
        }

        public class Encounter
        {
            public uint InstanceID;
            public uint OID;
            public DateTime Start;
            public DateTime End;
            public ushort Zone;
            public List<Participant> Players = new();
            public Dictionary<uint, List<Participant>> Enemies = new(); // key = oid
            public List<Action> Actions = new();
            public List<Status> Statuses = new();
            public List<Tether> Tethers = new();
            public List<Icon> Icons = new();
            public List<EnvControl> EnvControls = new();
        }

        public List<ReplayOps.Operation> Ops = new();
        public List<Encounter> Encounters = new();
    }
}
