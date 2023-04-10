using System;
using System.Numerics;

namespace BossMod
{
    public struct ClientActionRequest
    {
        public ActionID Action;
        public ulong TargetID;
        public Vector3 TargetPos;
        public uint SourceSequence;
        public float InitialAnimationLock;
        public float InitialCastTimeElapsed;
        public float InitialCastTimeTotal;
        public float InitialRecastElapsed;
        public float InitialRecastTotal;
    }

    public struct ClientActionReject
    {
        public ActionID Action;
        public uint SourceSequence;
        public float RecastElapsed;
        public float RecastTotal;
        public uint LogMessageID;
    }

    // client-specific state and events (action requests, gauge, etc)
    // this is generally not available for non-player party members, but we can try to guess
    public class ClientState
    {
        // implementation of operations
        public event EventHandler<OpActionRequest>? ActionRequested;
        public class OpActionRequest : WorldState.Operation
        {
            public ClientActionRequest Request;

            protected override void Exec(WorldState ws)
            {
                ws.Client.ActionRequested?.Invoke(ws, this);
            }

            public override string Str(WorldState? ws) => $"CLAR|{Request.Action}|{Request.TargetID:X8}|{StrVec3(Request.TargetPos)}|{Request.SourceSequence}|{Request.InitialAnimationLock:f3}|{Request.InitialCastTimeElapsed:f3}/{Request.InitialCastTimeTotal:f3}|{Request.InitialRecastElapsed:f3}/{Request.InitialRecastTotal:f3}";
        }

        public event EventHandler<OpActionReject>? ActionRejected;
        public class OpActionReject : WorldState.Operation
        {
            public ClientActionReject Value;

            protected override void Exec(WorldState ws)
            {
                ws.Client.ActionRejected?.Invoke(ws, this);
            }

            public override string Str(WorldState? ws) => $"CLRJ|{Value.Action}|{Value.SourceSequence}|{Value.RecastElapsed:f3}/{Value.RecastTotal:f3}|{Value.LogMessageID}";
        }
    }
}
