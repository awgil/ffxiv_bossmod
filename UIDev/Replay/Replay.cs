using System;
using System.Collections.Generic;

namespace UIDev
{
    public class Replay
    {
        public class Encounter
        {
            public uint OID;
            public DateTime Start;
            public DateTime End;
            public ushort Zone;
        }

        public List<ReplayOps.Operation> Ops = new();
        public List<Encounter> Encounters = new();
    }
}
