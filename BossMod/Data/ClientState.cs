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
public sealed class ClientState
{
    public readonly record struct Fate(uint ID, Vector3 Center, float Radius);

    public const int NumCooldownGroups = 82;

    public float? CountdownRemaining;
    public readonly Cooldown[] Cooldowns = new Cooldown[NumCooldownGroups];
    public readonly ActionID[] DutyActions = new ActionID[2];
    public readonly byte[] BozjaHolster = new byte[(int)BozjaHolsterID.Count]; // number of copies in holster per item
    public Fate ActiveFate;

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        if (CountdownRemaining != null)
            yield return new OpCountdownChange(CountdownRemaining);

        var cooldowns = Cooldowns.Select((v, i) => (i, v)).Where(iv => iv.v.Total > 0).ToList();
        if (cooldowns.Count > 0)
            yield return new OpCooldown(false, cooldowns);

        if (DutyActions.Any(a => a))
            yield return new OpDutyActionsChange(DutyActions[0], DutyActions[1]);

        var bozjaHolster = BozjaHolster.Select((v, i) => ((BozjaHolsterID)i, v)).Where(iv => iv.v > 0).ToList();
        if (BozjaHolster.Any(count => count != 0))
            yield return new OpBozjaHolsterChange(bozjaHolster);
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
    public sealed record class OpActionRequest(ClientActionRequest Request) : WorldState.Operation
    {
        protected override void Exec(WorldState ws) => ws.Client.ActionRequested.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLAR"u8)
            .Emit(Request.Action)
            .EmitActor(Request.TargetID)
            .Emit(Request.TargetPos)
            .Emit(Request.SourceSequence)
            .Emit(Request.InitialAnimationLock, "f3")
            .EmitFloatPair(Request.InitialCastTimeElapsed, Request.InitialCastTimeTotal)
            .EmitFloatPair(Request.InitialRecastElapsed, Request.InitialRecastTotal);
    }

    public Event<OpActionReject> ActionRejected = new();
    public sealed record class OpActionReject(ClientActionReject Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws) => ws.Client.ActionRejected.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLRJ"u8)
            .Emit(Value.Action)
            .Emit(Value.SourceSequence)
            .EmitFloatPair(Value.RecastElapsed, Value.RecastTotal)
            .Emit(Value.LogMessageID);
    }

    public Event<OpCountdownChange> CountdownChanged = new();
    public sealed record class OpCountdownChange(float? Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Client.CountdownRemaining = Value;
            ws.Client.CountdownChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            if (Value != null)
                output.EmitFourCC("CDN+"u8).Emit(Value.Value);
            else
                output.EmitFourCC("CDN-"u8);
        }
    }

    public Event<OpCooldown> CooldownsChanged = new();
    public sealed record class OpCooldown(bool Reset, List<(int group, Cooldown value)> Cooldowns) : WorldState.Operation
    {
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
            output.EmitFourCC("CLCD"u8);
            output.Emit(Reset);
            output.Emit((byte)Cooldowns.Count);
            foreach (var e in Cooldowns)
                output.Emit((byte)e.group).Emit(e.value.Remaining).Emit(e.value.Total);
        }
    }

    public Event<OpDutyActionsChange> DutyActionsChanged = new();
    public sealed record class OpDutyActionsChange(ActionID Slot0, ActionID Slot1) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Client.DutyActions[0] = Slot0;
            ws.Client.DutyActions[1] = Slot1;
            ws.Client.DutyActionsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLDA"u8).Emit(Slot0).Emit(Slot1);
    }

    public Event<OpBozjaHolsterChange> BozjaHolsterChanged = new();
    public sealed record class OpBozjaHolsterChange(List<(BozjaHolsterID entry, byte count)> Contents) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            Array.Fill(ws.Client.BozjaHolster, (byte)0);
            foreach (var e in Contents)
                ws.Client.BozjaHolster[(int)e.entry] = e.count;
            ws.Client.BozjaHolsterChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("CLBH"u8);
            output.Emit((byte)Contents.Count);
            foreach (var e in Contents)
                output.Emit((byte)e.entry).Emit(e.count);
        }
    }

    public Event<OpActiveFateChange> ActiveFateChanged = new();
    public sealed record class OpActiveFateChange(Fate Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Client.ActiveFate = Value;
            ws.Client.ActiveFateChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLAF"u8).Emit(Value.ID).Emit(Value.Center).Emit(Value.Radius, "f3");
    }
}
