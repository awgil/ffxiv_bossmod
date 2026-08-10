namespace BossMod;

public readonly struct ClientActionRequest(ActionID action, ulong targetID, Vector3 targetPos, uint sourceSequence,
    float initialAnimationLock, float initialCastTimeElapsed, float initialCastTimeTotal, float initialRecastElapsed, float initialRecastTotal)
{
    public readonly ActionID Action = action;
    public readonly ulong TargetID = targetID;
    public readonly Vector3 TargetPos = targetPos;
    public readonly uint SourceSequence = sourceSequence;
    public readonly float InitialAnimationLock = initialAnimationLock;
    public readonly float InitialCastTimeElapsed = initialCastTimeElapsed;
    public readonly float InitialCastTimeTotal = initialCastTimeTotal;
    public readonly float InitialRecastElapsed = initialRecastElapsed;
    public readonly float InitialRecastTotal = initialRecastTotal;

    public override readonly string ToString() => $"#{SourceSequence} {Action} @ {TargetID:X8} {TargetPos:f3}";
}

public readonly struct ClientActionReject(ActionID action, uint sourceSequence, float recastElapsed, float recastTotal, uint logMessageID)
{
    public readonly ActionID Action = action;
    public readonly uint SourceSequence = sourceSequence;
    public readonly float RecastElapsed = recastElapsed;
    public readonly float RecastTotal = recastTotal;
    public readonly uint LogMessageID = logMessageID;

    public override readonly string ToString() => $"#{SourceSequence} {Action} ({LogMessageID})";
}

public struct Cooldown(float elapsed, float total) : IEquatable<Cooldown>
{
    public float Elapsed = elapsed;
    public float Total = total;

    public readonly float Remaining => Total - Elapsed;

    public static bool operator ==(Cooldown left, Cooldown right) => left.Elapsed == right.Elapsed && left.Total == right.Total;
    public static bool operator !=(Cooldown left, Cooldown right) => left.Elapsed != right.Elapsed || left.Total != right.Total;

    public readonly bool Equals(Cooldown other) => this == other;
    public override readonly bool Equals(object? obj) => obj is Cooldown other && Equals(other);
    public override readonly int GetHashCode() => (Elapsed, Total).GetHashCode();
    public override readonly string ToString() => $"{Elapsed:f3}/{Total:f3}";
}

// client-specific state and events (action requests, gauge, etc)
// this is generally not available for non-player party members, but we can try to guess
public sealed class ClientState
{
    public readonly struct Fate(uint id, Vector3 center, float radius, byte progress, byte handInCount, uint objectiveNpc)
    {
        public readonly uint ID = id;
        public readonly Vector3 Center = center;
        public readonly float Radius = radius;
        public readonly byte Progress = progress;
        public readonly byte HandInCount = handInCount;
        public readonly uint ObjectiveNpc = objectiveNpc;

        public static bool operator ==(Fate left, Fate right) => left.ID == right.ID;
        public static bool operator !=(Fate left, Fate right) => left.ID != right.ID;

        public readonly bool Equals(Fate other) => this == other;
        public override readonly bool Equals(object? obj) => obj is Fate other && Equals(other);
        public override readonly int GetHashCode() => ID.GetHashCode();
    }

    public struct Combo(uint action, float remaining)
    {
        public readonly uint Action = action;
        public float Remaining = remaining;

        public static bool operator ==(Combo left, Combo right) => left.Action == right.Action && left.Remaining == right.Remaining;
        public static bool operator !=(Combo left, Combo right) => left.Action != right.Action || left.Remaining != right.Remaining;

        public readonly bool Equals(Combo other) => this == other;
        public override readonly bool Equals(object? obj) => obj is Combo other && Equals(other);
        public override readonly int GetHashCode() => (Action, Remaining).GetHashCode();
    }

    public readonly struct Gauge(ulong low, ulong high)
    {
        public readonly ulong Low = low;
        public readonly ulong High = high;
    }

    public readonly struct Stats(int skillSpeed, int spellSpeed, int haste)
    {
        public readonly int SkillSpeed = skillSpeed;
        public readonly int SpellSpeed = spellSpeed;
        public readonly int Haste = haste;

        public static bool operator ==(Stats left, Stats right) => left.SkillSpeed == right.SkillSpeed && left.SpellSpeed == right.SpellSpeed && left.Haste == right.Haste;
        public static bool operator !=(Stats left, Stats right) => left.SkillSpeed != right.SkillSpeed || left.SpellSpeed != right.SpellSpeed || left.Haste != right.Haste;

        public readonly bool Equals(Stats other) => this == other;
        public override readonly bool Equals(object? obj) => obj is Combo other && Equals(other);
        public override readonly int GetHashCode() => (SkillSpeed, SpellSpeed, Haste).GetHashCode();
    }

    public readonly struct Pet(ulong instanceID, byte order, byte stance)
    {
        public readonly ulong InstanceID = instanceID;
        public readonly byte Order = order;
        public readonly byte Stance = stance;

        public static bool operator ==(Pet left, Pet right) => left.InstanceID == right.InstanceID && left.Order == right.Order && left.Stance == right.Stance;
        public static bool operator !=(Pet left, Pet right) => left.InstanceID != right.InstanceID || left.Order != right.Order || left.Stance != right.Stance;

        public readonly bool Equals(Pet other) => this == other;
        public override readonly bool Equals(object? obj) => obj is Pet other && Equals(other);
        public override readonly int GetHashCode() => InstanceID.GetHashCode();
    }

    public readonly struct Companion(ulong instanceID, byte stance, float timeLeft, bool stabled)
    {
        public readonly ulong InstanceID = instanceID;
        public readonly byte Stance = stance;
        public readonly float TimeLeft = timeLeft;
        public readonly bool Stabled = stabled;

        public static bool operator ==(Companion left, Companion right) => left.InstanceID == right.InstanceID && left.Stance == right.Stance && left.TimeLeft == right.TimeLeft;
        public static bool operator !=(Companion left, Companion right) => left.InstanceID != right.InstanceID || left.Stance != right.Stance || left.TimeLeft != right.TimeLeft;

        public readonly bool Equals(Companion other) => this == other;
        public override readonly bool Equals(object? obj) => obj is Companion other && Equals(other);
        public override readonly int GetHashCode() => (InstanceID, Stance, TimeLeft).GetHashCode();
        public override readonly string ToString() => $"ID: {InstanceID}, Stance: {Stance}";
    }

    public readonly struct DutyAction(ActionID action, byte curCharges, byte maxCharges)
    {
        public readonly ActionID Action = action;
        public readonly byte CurCharges = curCharges;
        public readonly byte MaxCharges = maxCharges;

        public static bool operator ==(DutyAction left, DutyAction right) => left.Action == right.Action && left.CurCharges == right.CurCharges && left.MaxCharges == right.MaxCharges;
        public static bool operator !=(DutyAction left, DutyAction right) => left.Action != right.Action || left.CurCharges != right.CurCharges || left.MaxCharges != right.MaxCharges;

        public readonly bool Equals(DutyAction other) => this == other;
        public override readonly bool Equals(object? obj) => obj is DutyAction other && Equals(other);
        public override readonly int GetHashCode() => (Action, CurCharges, MaxCharges).GetHashCode();
        public override string ToString() => $"ID: {Action.ID}, Charges: {CurCharges}/{MaxCharges}";
    }

    public readonly struct HateInfo(ulong instanceID, Hate[] targets)
    {
        public readonly ulong InstanceID = instanceID;
        // targets are sorted by enmity order, except that player is always first
        // nonzero entries are always at the front of the array - there is space for 32 entries, but a maximum of 8 are currently used
        public readonly Hate[] Targets = targets;
    }

    public readonly struct Hate(ulong instanceID, int enmity)
    {
        public readonly ulong InstanceID = instanceID;
        public readonly int Enmity = enmity;

        public static bool operator ==(Hate left, Hate right) => left.InstanceID == right.InstanceID && left.Enmity == right.Enmity;
        public static bool operator !=(Hate left, Hate right) => left.InstanceID != right.InstanceID || left.Enmity != right.Enmity;

        public readonly bool Equals(Hate other) => this == other;
        public override readonly bool Equals(object? obj) => obj is Hate other && Equals(other);
        public override readonly int GetHashCode() => (InstanceID, Enmity).GetHashCode();
    }

    public const int NumCooldownGroups = 87;
    public const int NumClassLevels = 35; // see ClassJob.ExpArrayIndex
    public const int NumBlueMageSpells = 24;
    public const int NumDutyActions = 5;
    public const int NumHateTargets = 32;
    public float? CountdownRemaining;
    public Angle CameraAzimuth; // updated every frame by the frame-start event
    public Gauge GaugePayload; // updated every frame by the frame-start event
    public float AnimationLock;
    public Combo ComboState;
    public Stats PlayerStats;
    public float MoveSpeed = 6f;
    public readonly Cooldown[] Cooldowns = new Cooldown[NumCooldownGroups];
    public readonly DutyAction[] DutyActions = new DutyAction[NumDutyActions];
    public readonly byte[] BozjaHolster = new byte[(int)BozjaHolsterID.Count]; // number of copies in holster per item
    public readonly uint[] BlueMageSpells = new uint[NumBlueMageSpells];
    public readonly short[] ClassJobLevels = new short[NumClassLevels];
    public Fate ActiveFate;
    public Pet ActivePet;
    public Companion ActiveCompanion;
    public ulong FocusTargetId;
    public Angle ForcedMovementDirection; // used for temporary misdirection and spinning states
    public uint[] ContentKeyValueData = new uint[6]; // used for content-specific persistent player attributes, like bozja resistance rank
    public HateInfo CurrentTargetHate = new(default, new Hate[NumHateTargets]);

    public readonly Dictionary<uint, uint> Inventory; // tracks supported regular items and all key items

    public uint GetInventoryItemQuantity(uint itemId) => Inventory.TryGetValue(itemId, out var q) ? q : 0;

    public ClientState()
    {
        Inventory = [];
        foreach (var it in ActionDefinitions.Instance.SupportedItems)
        {
            Inventory[it] = default;
        }
    }

    // if an action has SecondaryCostType between 1 and 4, it's considered usable as long as the corresponding timer in this array is >0; the timer is set to 5 when certain ActionEffects are received and ticks down each frame
    // 1: unknown - referenced in ActionManager code but not present in sheets, included for completeness
    // 2: block
    // 3: parry
    // 4: dodge
    public float[] ProcTimers = new float[4];

    public uint GetContentValue(uint key) => ContentKeyValueData[0] == key
        ? ContentKeyValueData[1]
        : ContentKeyValueData[2] == key
            ? ContentKeyValueData[3]
            : ContentKeyValueData[4] == key
                ? ContentKeyValueData[5]
                : default;

    public uint ElementalLevel => GetContentValue(4);
    public uint ElementalLevelSynced => GetContentValue(2);
    public uint ResistanceRank => GetContentValue(5);

    public int ClassJobLevel(Class c)
    {
        var index = Service.LuminaRow<Lumina.Excel.Sheets.ClassJob>((uint)c)?.ExpArrayIndex ?? -1;
        return ClassJobLevels.BoundSafeAt(index, (short)-1);
    }

    // TODO: think about how to improve it...
    public static unsafe T GetGauge<T>(in Gauge gauge) where T : unmanaged
    {
        T res = default;
        ((ulong*)&res)[1] = gauge.Low;
        if (sizeof(T) > 16)
        {
            ((ulong*)&res)[2] = gauge.High;
        }

        return res;
    }
    public T GetGauge<T>() where T : unmanaged => GetGauge<T>(GaugePayload);

    public List<WorldState.Operation> CompareToInitial()
    {
        List<WorldState.Operation> ops = [with(15)];
        if (CountdownRemaining != null)
        {
            ops.Add(new OpCountdownChange(CountdownRemaining));
        }

        if (AnimationLock != default)
        {
            ops.Add(new OpAnimationLockChange(AnimationLock));
        }

        if (ComboState.Remaining != default)
        {
            ops.Add(new OpComboChange(ComboState));
        }

        if (PlayerStats != default)
        {
            ops.Add(new OpPlayerStatsChange(PlayerStats));
        }

        if (MoveSpeed != 6f)
        {
            ops.Add(new OpMoveSpeedChange(MoveSpeed));
        }

        var cooldowns = new List<(int, Cooldown)>(NumCooldownGroups);

        for (var i = 0; i < NumCooldownGroups; ++i)
        {
            var cds = Cooldowns[i];
            if (cds.Total != default)
            {
                cooldowns.Add((i, cds));
            }
        }
        if (cooldowns.Count != 0)
        {
            ops.Add(new OpCooldown(false, cooldowns));
        }

        for (var i = 0; i < NumDutyActions; ++i)
        {
            if (DutyActions[i] != default)
            {
                ops.Add(new OpDutyActionsChange(DutyActions));
                break;
            }
        }

        for (var i = 0; i < (int)BozjaHolsterID.Count; ++i)
        {
            if (BozjaHolster[i] != default)
            {
                var bozjaHolster = new List<(BozjaHolsterID, byte)>((int)BozjaHolsterID.Count);
                for (var j = 0; j < (int)BozjaHolsterID.Count; ++j)
                {
                    var holster = BozjaHolster[j];
                    if (holster != default)
                    {
                        bozjaHolster.Add(((BozjaHolsterID)i, holster));
                    }
                }
                ops.Add(new OpBozjaHolsterChange(bozjaHolster));
                break;
            }
        }

        for (var i = 0; i < NumBlueMageSpells; ++i)
        {
            if (BlueMageSpells[i] != default)
            {
                ops.Add(new OpBlueMageSpellsChange(BlueMageSpells));
                break;
            }
        }

        for (var i = 0; i < NumClassLevels; ++i)
        {
            if (ClassJobLevels[i] != default)
            {
                ops.Add(new OpClassJobLevelsChange(ClassJobLevels));
                break;
            }
        }

        if (ActiveFate.ID != default)
        {
            ops.Add(new OpActiveFateChange(ActiveFate));
        }

        if (ActivePet.InstanceID != default)
        {
            ops.Add(new OpActivePetChange(ActivePet));
        }

        if (FocusTargetId != default)
        {
            ops.Add(new OpFocusTargetChange(FocusTargetId));
        }

        if (ForcedMovementDirection != default)
        {
            ops.Add(new OpForcedMovementDirectionChange(ForcedMovementDirection));
        }

        ref readonly var hate = ref CurrentTargetHate;
        if (hate.InstanceID != default)
        {
            ops.Add(new OpHateChange(hate.InstanceID, hate.Targets));
        }
        else
        {
            for (var i = 0; i < NumHateTargets; ++i)
            {
                ref readonly var h = ref hate.Targets[i];
                if (h != default)
                {
                    ops.Add(new OpHateChange(hate.InstanceID, hate.Targets));
                    break;
                }
            }
        }
        return ops;
    }

    public void Tick(float dt)
    {
        if (CountdownRemaining != null)
        {
            CountdownRemaining = CountdownRemaining.Value - dt;
        }

        if (AnimationLock > 0f)
        {
            AnimationLock = Math.Max(0, AnimationLock - dt);
        }

        if (ComboState.Remaining > 0f)
        {
            ComboState.Remaining -= dt;
            if (ComboState.Remaining <= 0f)
            {
                ComboState = default;
            }
        }

        // TODO: update cooldowns only if 'timestop' status is not active...
        foreach (ref var cd in Cooldowns.AsSpan())
        {
            cd.Elapsed += dt;
            if (cd.Elapsed >= cd.Total)
            {
                cd.Elapsed = cd.Total = default;
            }
        }
    }

    // implementation of operations
    public Event<OpActionRequest> ActionRequested = new();
    public sealed class OpActionRequest(ClientActionRequest request) : WorldState.Operation
    {
        public readonly ClientActionRequest Request = request;

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
    public sealed class OpActionReject(ClientActionReject value) : WorldState.Operation
    {
        public readonly ClientActionReject Value = value;

        protected override void Exec(WorldState ws) => ws.Client.ActionRejected.Fire(this);

        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLRJ"u8)
            .Emit(Value.Action)
            .Emit(Value.SourceSequence)
            .EmitFloatPair(Value.RecastElapsed, Value.RecastTotal)
            .Emit(Value.LogMessageID);
    }

    public Event<OpCountdownChange> CountdownChanged = new();
    public sealed class OpCountdownChange(float? value) : WorldState.Operation
    {
        public readonly float? Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.CountdownRemaining = Value;
            ws.Client.CountdownChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            if (Value != null)
            {
                output.EmitFourCC("CDN+"u8).Emit(Value.Value);
            }
            else
            {
                output.EmitFourCC("CDN-"u8);
            }
        }
    }

    public Event<OpAnimationLockChange> AnimationLockChanged = new();
    public sealed class OpAnimationLockChange(float value) : WorldState.Operation
    {
        public readonly float Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.AnimationLock = Value;
            ws.Client.AnimationLockChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLAL"u8).Emit(Value);
    }

    public Event<OpComboChange> ComboChanged = new();
    public sealed class OpComboChange(Combo value) : WorldState.Operation
    {
        public readonly Combo Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.ComboState = Value;
            ws.Client.ComboChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLCB"u8).Emit(Value.Action).Emit(Value.Remaining);
    }

    public Event<OpPlayerStatsChange> PlayerStatsChanged = new();
    public sealed class OpPlayerStatsChange(Stats value) : WorldState.Operation
    {
        public readonly Stats Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.PlayerStats = Value;
            ws.Client.PlayerStatsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLST"u8).Emit(Value.SkillSpeed).Emit(Value.SpellSpeed).Emit(Value.Haste);
    }

    public Event<OpMoveSpeedChange> MoveSpeedChanged = new();
    public sealed class OpMoveSpeedChange(float speed) : WorldState.Operation
    {
        public readonly float Speed = speed;

        protected override void Exec(WorldState ws)
        {
            ws.Client.MoveSpeed = Speed;
            ws.Client.MoveSpeedChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLMV"u8).Emit(Speed);
    }

    public Event<OpCooldown> CooldownsChanged = new();
    public sealed class OpCooldown(bool reset, List<(int, Cooldown)> cooldowns) : WorldState.Operation
    {
        public readonly bool Reset = reset;
        public readonly List<(int group, Cooldown value)> Cooldowns = cooldowns;

        protected override void Exec(WorldState ws)
        {
            if (Reset)
            {
                Array.Fill(ws.Client.Cooldowns, default);
            }

            var count = Cooldowns.Count;
            for (var i = 0; i < count; ++i)
            {
                var cd = Cooldowns[i];
                ws.Client.Cooldowns[cd.group] = cd.value;
            }
            ws.Client.CooldownsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            var count = Cooldowns.Count;
            output.EmitFourCC("CLCD"u8);
            output.Emit(Reset);
            output.Emit((byte)count);
            for (var i = 0; i < count; ++i)
            {
                var cd = Cooldowns[i];
                output.Emit((byte)cd.group).Emit(cd.value.Elapsed).Emit(cd.value.Total);
            }
        }
    }

    public Event<OpDutyActionsChange> DutyActionsChanged = new();
    public sealed class OpDutyActionsChange(DutyAction[] slots) : WorldState.Operation
    {
        public readonly DutyAction[] Slots = slots;

        protected override void Exec(WorldState ws)
        {
            Array.Fill(ws.Client.DutyActions, default);
            var len = Slots.Length;
            for (var i = 0; i < len; ++i)
            {
                ws.Client.DutyActions[i] = Slots[i];
            }
            ws.Client.DutyActionsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("CLDA"u8);
            var len = Slots.Length;
            output.Emit((byte)len);
            for (var i = 0; i < len; ++i)
            {
                ref readonly var s = ref Slots[i];
                output.Emit(s.Action).Emit(s.CurCharges).Emit(s.MaxCharges);
            }
        }
    }

    public Event<OpBozjaHolsterChange> BozjaHolsterChanged = new();
    public sealed class OpBozjaHolsterChange(List<(BozjaHolsterID, byte)> contents) : WorldState.Operation
    {
        public readonly List<(BozjaHolsterID entry, byte count)> Contents = contents;

        protected override void Exec(WorldState ws)
        {
            Array.Fill(ws.Client.BozjaHolster, (byte)0);
            for (var i = 0; i < Contents.Count; ++i)
            {
                var e = Contents[i];
                ws.Client.BozjaHolster[(int)e.entry] = e.count;
            }
            ws.Client.BozjaHolsterChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            var count = Contents.Count;
            output.EmitFourCC("CLBH"u8);
            output.Emit((byte)count);
            for (var i = 0; i < count; ++i)
            {
                var e = Contents[i];
                output.Emit((byte)e.entry).Emit(e.count);
            }
        }
    }

    public Event<OpBlueMageSpellsChange> BlueMageSpellsChanged = new();
    public sealed class OpBlueMageSpellsChange(uint[] values) : WorldState.Operation
    {
        public readonly uint[] Values = values;

        protected override void Exec(WorldState ws)
        {
            Array.Copy(Values, ws.Client.BlueMageSpells, NumBlueMageSpells);
            ws.Client.BlueMageSpellsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            var len = Values.Length;
            output.EmitFourCC("CBLU"u8);
            output.Emit((byte)Values.Length);
            for (var i = 0; i < len; ++i)
            {
                output.Emit(Values[i]);
            }
        }
    }

    public Event<OpClassJobLevelsChange> ClassJobLevelsChanged = new();
    public sealed class OpClassJobLevelsChange(short[] values) : WorldState.Operation
    {
        public readonly short[] Values = values;

        protected override void Exec(WorldState ws)
        {
            Array.Fill(ws.Client.ClassJobLevels, default);
            var len = Values.Length;
            for (var i = 0; i < len; ++i)
            {
                ws.Client.ClassJobLevels[i] = Values[i];
            }

            ws.Client.ClassJobLevelsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            var len = Values.Length;
            output.EmitFourCC("CLVL"u8);
            output.Emit((byte)len);
            for (var i = 0; i < len; ++i)
            {
                output.Emit(Values[i]);
            }
        }
    }

    public Event<OpActiveFateChange> ActiveFateChanged = new();
    public sealed class OpActiveFateChange(Fate value) : WorldState.Operation
    {
        public readonly Fate Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.ActiveFate = Value;
            ws.Client.ActiveFateChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLAF"u8).Emit(Value.ID).Emit(Value.Center).Emit(Value.Radius, "f3").Emit(Value.Progress).Emit(Value.HandInCount).Emit(Value.ObjectiveNpc);
    }

    public Event<OpActivePetChange> ActivePetChanged = new();
    public sealed class OpActivePetChange(Pet value) : WorldState.Operation
    {
        public readonly Pet Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.ActivePet = Value;
            ws.Client.ActivePetChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CPET"u8).Emit(Value.InstanceID, "X8").Emit(Value.Order).Emit(Value.Stance);
    }

    public Event<OpActiveCompanionChange> ActiveCompanionChanged = new();
    public sealed class OpActiveCompanionChange(Companion Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Client.ActiveCompanion = Value;
            ws.Client.ActiveCompanionChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CHOC"u8).Emit(Value.InstanceID, "X8").Emit(Value.Stance).Emit(Value.TimeLeft).Emit(Value.Stabled);
    }

    public Event<OpFocusTargetChange> FocusTargetChanged = new();
    public sealed class OpFocusTargetChange(ulong value) : WorldState.Operation
    {
        public readonly ulong Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.FocusTargetId = Value;
            ws.Client.FocusTargetChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLFT"u8).Emit(Value, "X8");
    }

    public Event<OpForcedMovementDirectionChange> ForcedMovementDirectionChanged = new();
    public sealed class OpForcedMovementDirectionChange(Angle value) : WorldState.Operation
    {
        public readonly Angle Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.ForcedMovementDirection = Value;
            ws.Client.ForcedMovementDirectionChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLFD"u8).Emit(Value);
    }

    public Event<OpContentKVDataChange> ContentKVDataChanged = new();
    public sealed class OpContentKVDataChange(uint[] value) : WorldState.Operation
    {
        public readonly uint[] Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.ContentKeyValueData = Value;
            ws.Client.ContentKVDataChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("CLKV"u8);
            var len = Value.Length;
            for (var i = 0; i < len; ++i)
            {
                output.Emit(Value[i]);
            }
        }
    }

    public Event<OpFateInfo> FateInfo = new();
    public sealed class OpFateInfo(uint fateID, DateTime startTime) : WorldState.Operation
    {
        public readonly uint FateID = fateID;
        public readonly DateTime StartTime = startTime;

        protected override void Exec(WorldState ws) => ws.Client.FateInfo.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("FATE"u8).Emit(FateID).Emit(StartTime.Ticks);
    }

    public Event<OpHateChange> HateChanged = new();
    public sealed class OpHateChange(ulong instanceID, Hate[] targets) : WorldState.Operation
    {
        public readonly ulong InstanceID = instanceID;
        public readonly Hate[] Targets = targets;

        protected override void Exec(WorldState ws)
        {
            ws.Client.CurrentTargetHate = new(InstanceID, Targets);
            ws.Client.HateChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("HATE"u8).EmitActor(InstanceID);
            var countNonEmpty = Array.IndexOf(Targets, default);
            output.Emit(countNonEmpty);
            for (var i = 0; i < countNonEmpty; ++i)
            {
                output.EmitActor(Targets[i].InstanceID).Emit(Targets[i].Enmity);
            }
        }
    }

    public Event<OpProcTimersChange> ProcTimersChanged = new();
    public sealed class OpProcTimersChange(float[] value) : WorldState.Operation
    {
        public readonly float[] Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.Client.ProcTimers = Value;
            ws.Client.ProcTimersChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLPR"u8).Emit(Value[0]).Emit(Value[1]).Emit(Value[2]).Emit(Value[3]);
    }

    public Event<OpInventoryChange> InventoryChanged = new();
    public sealed class OpInventoryChange(uint itemId, uint quantity) : WorldState.Operation
    {
        public readonly uint ItemId = itemId;
        public readonly uint Quantity = quantity;

        protected override void Exec(WorldState ws)
        {
            ws.Client.Inventory[ItemId] = Quantity;
            ws.Client.InventoryChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("INVT"u8).Emit(ItemId).Emit(Quantity);
    }

    public Event<OpActionFailedLoS> ActionFailedLoS = new();
    public sealed class OpActionFailedLoS(uint actionId, ulong targetId) : WorldState.Operation
    {
        public readonly uint ActionId = actionId;
        public readonly ulong TargetId = targetId;

        protected override void Exec(WorldState ws) => ws.Client.ActionFailedLoS.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("FLOS"u8).Emit(ActionId).EmitActor(TargetId);
    }
}
