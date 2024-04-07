namespace BossMod;

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

public struct Cooldown : IEquatable<Cooldown>
{
    public float Elapsed;
    public float Total;

    public float Remaining => Total - Elapsed;

    public static bool operator ==(Cooldown l, Cooldown r) => l.Elapsed == r.Elapsed && l.Total == r.Total;
    public static bool operator !=(Cooldown l, Cooldown r) => l.Elapsed != r.Elapsed || l.Total != r.Total;
    public override bool Equals(object? obj) => obj is Cooldown && this == (Cooldown)obj;
    public override int GetHashCode() => Elapsed.GetHashCode() ^ Total.GetHashCode();
    public override string ToString() => $"{Elapsed:f3}/{Total:f3}";
    public bool Equals(Cooldown other) => this == other;
}

// client-specific state and events (action requests, gauge, etc)
// this is generally not available for non-player party members, but we can try to guess
public class ClientState
{
    public const int NumCooldownGroups = 82;

    public float? CountdownRemaining;
    public Cooldown[] Cooldowns = new Cooldown[NumCooldownGroups];
    public ActionID[] DutyActions = new ActionID[2];
    public byte[] BozjaHolster = new byte[(int)BozjaHolsterID.Count]; // number of copies in holster per item

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        if (CountdownRemaining != null)
            yield return new OpCountdownChange() { Value = CountdownRemaining };

        var cooldowns = Cooldowns.Select((v, i) => (i, v)).Where(iv => iv.Item2.Total > 0).ToList();
        if (cooldowns.Count > 0)
            yield return new OpCooldown() { Cooldowns = cooldowns };

        if (DutyActions.Any(a => a))
            yield return new OpDutyActionsChange() { Slot0 = DutyActions[0], Slot1 = DutyActions[1] };

        var bozjaHolster = BozjaHolster.Select((v, i) => ((BozjaHolsterID)i, v)).Where(iv => iv.Item2 > 0).ToList();
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
    public event Action<OpActionRequest>? ActionRequested;
    public class OpActionRequest : WorldState.Operation
    {
        public ClientActionRequest Request;

        protected override void Exec(WorldState ws)
        {
            ws.Client.ActionRequested?.Invoke(this);
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

    public event Action<OpActionReject>? ActionRejected;
    public class OpActionReject : WorldState.Operation
    {
        public ClientActionReject Value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.ActionRejected?.Invoke(this);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLRJ")
            .Emit(Value.Action)
            .Emit(Value.SourceSequence)
            .EmitFloatPair(Value.RecastElapsed, Value.RecastTotal)
            .Emit(Value.LogMessageID);
    }

    public event Action<OpCountdownChange>? CountdownChanged;
    public class OpCountdownChange : WorldState.Operation
    {
        public float? Value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.CountdownRemaining = Value;
            ws.Client.CountdownChanged?.Invoke(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            if (Value != null)
                WriteTag(output, "CDN+").Emit(Value.Value);
            else
                WriteTag(output, "CDN-");
        }
    }

    public event Action<OpCooldown>? CooldownsChanged;
    public class OpCooldown : WorldState.Operation
    {
        public bool Reset;
        public List<(int group, Cooldown value)> Cooldowns = new();

        protected override void Exec(WorldState ws)
        {
            if (Reset)
                Array.Fill(ws.Client.Cooldowns, default);
            foreach (var cd in Cooldowns)
                ws.Client.Cooldowns[cd.group] = cd.value;
            ws.Client.CooldownsChanged?.Invoke(this);
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

    public event Action<OpDutyActionsChange>? DutyActionsChanged;
    public class OpDutyActionsChange : WorldState.Operation
    {
        public ActionID Slot0;
        public ActionID Slot1;

        protected override void Exec(WorldState ws)
        {
            ws.Client.DutyActions[0] = Slot0;
            ws.Client.DutyActions[1] = Slot1;
            ws.Client.DutyActionsChanged?.Invoke(this);
        }

        public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLDA").Emit(Slot0).Emit(Slot1);
    }

    public event Action<OpBozjaHolsterChange>? BozjaHolsterChanged;
    public class OpBozjaHolsterChange : WorldState.Operation
    {
        public List<(BozjaHolsterID entry, byte count)> Contents = new();

        protected override void Exec(WorldState ws)
        {
            Array.Fill(ws.Client.BozjaHolster, (byte)0);
            foreach (var e in Contents)
                ws.Client.BozjaHolster[(int)e.entry] = e.count;
            ws.Client.BozjaHolsterChanged?.Invoke(this);
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
