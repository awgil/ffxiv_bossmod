namespace BossMod;

public record struct ClientActionRequest
(
    ActionID Action,
    ulong TargetID,
    Vector3 TargetPos,
    uint SourceSequence,
    float InitialAnimationLock,
    float InitialCastTimeElapsed,
    float InitialCastTimeTotal,
    float InitialRecastElapsed,
    float InitialRecastTotal
)
{
    public override readonly string ToString() => $"#{SourceSequence} {Action} @ {TargetID:X8} {TargetPos:f3}";
}

public record struct ClientActionReject(ActionID Action, uint SourceSequence, float RecastElapsed, float RecastTotal, uint LogMessageID)
{
    public override readonly string ToString() => $"#{SourceSequence} {Action} ({LogMessageID})";
}

public record struct Cooldown(float Elapsed, float Total)
{
    public readonly float Remaining => Total - Elapsed;

    public override readonly string ToString() => $"{Elapsed:f3}/{Total:f3}";
}

// client-specific state and events (action requests, gauge, etc)
// this is generally not available for non-player party members, but we can try to guess
public class ClientState
{
    public const int NumCooldownGroups = 82;

    public float? CountdownRemaining;
    public readonly Cooldown[] Cooldowns = new Cooldown[NumCooldownGroups];
    public readonly ActionID[] DutyActions = new ActionID[2];
    public readonly byte[] BozjaHolster = new byte[(int)BozjaHolsterID.Count]; // number of copies in holster per item

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        if (CountdownRemaining != null)
            yield return new OpCountdownChange() { Value = CountdownRemaining };

        var cooldowns = Cooldowns.Select((v, i) => (i, v)).Where(iv => iv.v.Total > 0).ToList();
        if (cooldowns.Count > 0)
            yield return new OpCooldown() { Cooldowns = cooldowns };

        if (DutyActions.Any(a => a))
            yield return new OpDutyActionsChange() { Slot0 = DutyActions[0], Slot1 = DutyActions[1] };

        var bozjaHolster = BozjaHolster.Select((v, i) => ((BozjaHolsterID)i, v)).Where(iv => iv.v > 0).ToList();
        if (BozjaHolster.Any(count => count != 0))
            yield return new OpBozjaHolsterChange() { Contents = bozjaHolster };
    }

    public void Tick(float dt)
    {
        if (CountdownRemaining != null)
            CountdownRemaining = CountdownRemaining.Value - dt;

        // TODO: update cooldowns only if 'timestop' status is not active...
        foreach (ref var cd in Cooldowns.AsSpan())
        {
            cd.Elapsed += dt;
            if (cd.Elapsed >= cd.Total)
                cd.Elapsed = cd.Total = 0;
        }
    }

    // implementation of operations
    public Event<OpActionRequest> ActionRequested = new();
    public class OpActionRequest : WorldState.Operation
    {
        public ClientActionRequest Request;

        protected override void Exec(WorldState ws) => ws.Client.ActionRequested.Fire(this);

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLAR")
            .Emit(Request.Action)
            .EmitActor(Request.TargetID)
            .Emit(Request.TargetPos)
            .Emit(Request.SourceSequence)
            .Emit(Request.InitialAnimationLock, "f3")
            .EmitFloatPair(Request.InitialCastTimeElapsed, Request.InitialCastTimeTotal)
            .EmitFloatPair(Request.InitialRecastElapsed, Request.InitialRecastTotal);
    }

    public Event<OpActionReject> ActionRejected = new();
    public class OpActionReject : WorldState.Operation
    {
        public ClientActionReject Value;

        protected override void Exec(WorldState ws) => ws.Client.ActionRejected.Fire(this);

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLRJ")
            .Emit(Value.Action)
            .Emit(Value.SourceSequence)
            .EmitFloatPair(Value.RecastElapsed, Value.RecastTotal)
            .Emit(Value.LogMessageID);
    }

    public Event<OpCountdownChange> CountdownChanged = new();
    public class OpCountdownChange : WorldState.Operation
    {
        public float? Value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.CountdownRemaining = Value;
            ws.Client.CountdownChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            if (Value != null)
                WriteTag(output, "CDN+").Emit(Value.Value);
            else
                WriteTag(output, "CDN-");
        }
    }

    public Event<OpCooldown> CooldownsChanged = new();
    public class OpCooldown : WorldState.Operation
    {
        public bool Reset;
        public List<(int group, Cooldown value)> Cooldowns = [];

        protected override void Exec(WorldState ws)
        {
            if (Reset)
                Array.Fill(ws.Client.Cooldowns, default);
            foreach (var cd in Cooldowns)
                ws.Client.Cooldowns[cd.group] = cd.value;
            ws.Client.CooldownsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            WriteTag(output, "CLCD");
            output.Emit(Reset);
            output.Emit((byte)Cooldowns.Count);
            foreach (var e in Cooldowns)
                output.Emit((byte)e.group).Emit(e.value.Remaining).Emit(e.value.Total);
        }
    }

    public Event<OpDutyActionsChange> DutyActionsChanged = new();
    public class OpDutyActionsChange : WorldState.Operation
    {
        public ActionID Slot0;
        public ActionID Slot1;

        protected override void Exec(WorldState ws)
        {
            ws.Client.DutyActions[0] = Slot0;
            ws.Client.DutyActions[1] = Slot1;
            ws.Client.DutyActionsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLDA").Emit(Slot0).Emit(Slot1);
    }

    public Event<OpBozjaHolsterChange> BozjaHolsterChanged = new();
    public class OpBozjaHolsterChange : WorldState.Operation
    {
        public List<(BozjaHolsterID entry, byte count)> Contents = [];

        protected override void Exec(WorldState ws)
        {
            Array.Fill(ws.Client.BozjaHolster, (byte)0);
            foreach (var e in Contents)
                ws.Client.BozjaHolster[(int)e.entry] = e.count;
            ws.Client.BozjaHolsterChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            WriteTag(output, "CLBH");
            output.Emit((byte)Contents.Count);
            foreach (var e in Contents)
                output.Emit((byte)e.entry).Emit(e.count);
        }
    }
}
