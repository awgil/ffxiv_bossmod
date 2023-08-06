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

            public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLAR")
                .Emit(Request.Action)
                .EmitActor(Request.TargetID)
                .Emit(Request.TargetPos)
                .Emit(Request.SourceSequence)
                .Emit(Request.InitialAnimationLock, "f3")
                .EmitFloatPair(Request.InitialCastTimeElapsed, Request.InitialCastTimeTotal)
                .EmitFloatPair(Request.InitialRecastElapsed, Request.InitialRecastTotal);
        }

        public event EventHandler<OpActionReject>? ActionRejected;
        public class OpActionReject : WorldState.Operation
        {
            public ClientActionReject Value;

            protected override void Exec(WorldState ws)
            {
                ws.Client.ActionRejected?.Invoke(ws, this);
            }

            public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLRJ")
                .Emit(Value.Action)
                .Emit(Value.SourceSequence)
                .EmitFloatPair(Value.RecastElapsed, Value.RecastTotal)
                .Emit(Value.LogMessageID);
        }
    }
}
