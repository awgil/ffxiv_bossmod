using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace BossMod.ActionTweaks.CustomCombos;

public sealed class ComboHotbarSlot
{
    public uint Job; // saved hotbar index (0 for shared hotbars, class/job id otherwise)
    public uint Bar; // 0-9 normal hotbars, 10-17 cross hotbars
    public uint Slot; // 0-15
    public RaptureHotbarModule.HotbarSlotType Type;
    public uint Id;
}

public sealed class ComboHotbarsConfig
{
    public bool Enabled = false;
    public bool ShowVanilla = false; // temporarily show normal hotbars
    public List<ComboHotbarSlot> Slots = []; // only these slots are covered, the rest show normal hotbars
    public List<ComboHotbarSlot> Pending = []; // additions made by the game while hotbars were locked, applied on unlock

    public ComboHotbarSlot? Find(uint job, uint bar, uint slot) => Slots.Find(s => s.Job == job && s.Bar == bar && s.Slot == slot);
    public bool HasBar(uint job, uint bar) => Slots.Exists(s => s.Job == job && s.Bar == bar);
    public IEnumerable<ComboHotbarSlot> BarSlots(uint job, uint bar) => Slots.Where(s => s.Job == job && s.Bar == bar);
}
