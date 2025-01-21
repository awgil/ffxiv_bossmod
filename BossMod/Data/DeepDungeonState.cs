using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod;

public sealed class DeepDungeonState
{
    public DungeonProgress Progress;
    public byte DungeonId;
    public RoomFlags[] MapData = new RoomFlags[25];
    public PartyMember[] Party = new PartyMember[4];
    public Item[] Items = new Item[16];
    public Chest[] Chests = new Chest[16];
    public byte[] Magicite = new byte[3];

    public enum DungeonType : byte
    {
        None = 0,
        POTD = 1,
        HOH = 2,
        EO = 3
    }

    public DungeonType Type => (DungeonType)DungeonId;

    public record struct DungeonProgress(byte Floor, byte Tileset, byte WeaponLevel, byte ArmorLevel, byte SyncedGearLevel, byte HoardCount, byte ReturnProgress, byte PassageProgress)
    {
        public readonly bool IsBossFloor => Floor % 10 == 0;
    }
    public record struct PartyMember(ulong EntityId, byte Room);
    public record struct Item(byte Count, byte Flags)
    {
        public readonly bool Usable => (Flags & (1 << 0)) != 0;
        public readonly bool Active => (Flags & (1 << 1)) != 0;
    }
    public record struct Chest(byte Type, byte Room);

    public Item GetItem(PomanderID pid) => GetSlotForPomander(pid) is var s && s >= 0 ? Items[s] : default;

    public int GetSlotForPomander(PomanderID pid) => Service.LuminaRow<Lumina.Excel.Sheets.DeepDungeon>(DungeonId)!.Value.PomanderSlot.ToList().FindIndex(p => p.RowId == (uint)pid);
    public PomanderID GetPomanderForSlot(int slot)
    {
        var slots = Service.LuminaRow<Lumina.Excel.Sheets.DeepDungeon>(DungeonId)!.Value.PomanderSlot;
        return slot >= 0 && slot < slots.Count ? (PomanderID)slots[slot].RowId : PomanderID.None;
    }

    public bool ReturnActive => Progress.ReturnProgress >= 11;
    public bool PassageActive => Progress.PassageProgress >= 11;
    public byte Floor => Progress.Floor;

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        if (Progress != default || DungeonId != 0)
            yield return new OpProgressChange(DungeonId, Progress);

        if (MapData.Any(m => m > 0))
            yield return new OpMapDataChange(MapData);

        if (Party.Any(p => p != default))
            yield return new OpPartyStateChange(Party);

        if (Items.Any(i => i != default))
            yield return new OpItemsChange(Items);

        if (Chests.Any(c => c != default))
            yield return new OpChestsChange(Chests);

        if (Magicite.Any(c => c > 0))
            yield return new OpMagiciteChange(Magicite);
    }

    public Event<OpProgressChange> ProgressChanged = new();
    public sealed record class OpProgressChange(byte DungeonId, DungeonProgress Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.DeepDungeon.DungeonId = DungeonId;
            ws.DeepDungeon.Progress = Value;
            ws.DeepDungeon.ProgressChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDPG"u8)
                .Emit(DungeonId)
                .Emit(Value.Floor)
                .Emit(Value.Tileset)
                .Emit(Value.WeaponLevel)
                .Emit(Value.ArmorLevel)
                .Emit(Value.SyncedGearLevel)
                .Emit(Value.HoardCount)
                .Emit(Value.ReturnProgress)
                .Emit(Value.PassageProgress);
        }
    }

    public Event<OpMapDataChange> MapDataChanged = new();
    public sealed record class OpMapDataChange(RoomFlags[] Value) : WorldState.Operation
    {
        public readonly RoomFlags[] Value = Value;

        protected override void Exec(WorldState ws)
        {
            ws.DeepDungeon.MapData = Value;
            ws.DeepDungeon.MapDataChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDMP"u8).Emit(Array.ConvertAll(Value, r => (byte)r));
        }
    }

    public Event<OpPartyStateChange> PartyStateChanged = new();
    public sealed record class OpPartyStateChange(PartyMember[] Value) : WorldState.Operation
    {
        public readonly PartyMember[] Value = Value;

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
        public readonly Item[] Value = Value;

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
        public readonly Chest[] Value = Value;

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

    public Event<OpMagiciteChange> MagiciteChanged = new();
    public sealed record class OpMagiciteChange(byte[] Value) : WorldState.Operation
    {
        public readonly byte[] Value = Value;

        protected override void Exec(WorldState ws)
        {
            ws.DeepDungeon.Magicite = Value;
            ws.DeepDungeon.MagiciteChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDMG"u8).Emit(Value);
        }
    }
}
