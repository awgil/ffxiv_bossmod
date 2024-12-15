namespace BossMod;

public sealed class DeepDungeonState
{
    public DungeonProgress Progress;
    public byte[] MapData = new byte[25];
    public PartyMember[] Party = new PartyMember[4];
    public Item[] Items = new Item[16];
    public Chest[] Chests = new Chest[16];

    public record struct DungeonProgress(byte Floor, byte WeaponLevel, byte ArmorLevel, byte SyncedGearLevel, byte HoardCount, byte ReturnProgress, byte PassageProgress);
    public record struct PartyMember(ulong EntityId, byte Room);
    public record struct Item(byte Count, byte Flags)
    {
        public readonly bool Usable => (Flags & (1 << 0)) != 0;
        public readonly bool Active => (Flags & (1 << 1)) != 0;
    }
    public record struct Chest(byte Type, byte Room);

    public Item GetItem(PomanderID pid) => pid == PomanderID.None ? default : Items[(uint)pid - 1];

    public bool ReturnActive => Progress.ReturnProgress >= 11;
    public bool PassageActive => Progress.PassageProgress >= 11;

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        if (Progress != default)
            yield return new OpProgressChange(Progress);

        if (MapData.Any(m => m > 0))
            yield return new OpMapDataChange(MapData);

        if (Party.Any(p => p != default))
            yield return new OpPartyStateChange(Party);

        if (Items.Any(i => i != default))
            yield return new OpItemsChange(Items);

        if (Chests.Any(c => c != default))
            yield return new OpChestsChange(Chests);
    }

    public Event<OpProgressChange> ProgressChanged = new();
    public sealed record class OpProgressChange(DungeonProgress Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.DeepDungeon.Progress = Value;
            ws.DeepDungeon.ProgressChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDPG"u8)
                .Emit(Value.Floor)
                .Emit(Value.WeaponLevel)
                .Emit(Value.ArmorLevel)
                .Emit(Value.SyncedGearLevel)
                .Emit(Value.HoardCount)
                .Emit(Value.ReturnProgress)
                .Emit(Value.PassageProgress);
        }
    }

    public Event<OpMapDataChange> MapDataChanged = new();
    public sealed record class OpMapDataChange(byte[] Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.DeepDungeon.MapData = Value;
            ws.DeepDungeon.MapDataChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDMP"u8).Emit(Value);
        }
    }

    public Event<OpPartyStateChange> PartyStateChanged = new();
    public sealed record class OpPartyStateChange(PartyMember[] Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.DeepDungeon.Party = Value;
            ws.DeepDungeon.PartyStateChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDPT"u8);
            foreach (var member in Value)
                output.EmitActor(member.EntityId).Emit(member.Room);
        }
    }

    public Event<OpItemsChange> ItemsChanged = new();
    public sealed record class OpItemsChange(Item[] Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.DeepDungeon.Items = Value;
            ws.DeepDungeon.ItemsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDIT"u8);
            foreach (var item in Value)
                output.Emit(item.Count).Emit(item.Flags, "X");
        }
    }

    public Event<OpChestsChange> ChestsChanged = new();
    public sealed record class OpChestsChange(Chest[] Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.DeepDungeon.Chests = Value;
            ws.DeepDungeon.ChestsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDCT"u8);
            foreach (var chest in Value)
                output.Emit(chest.Type).Emit(chest.Room);
        }
    }
}
