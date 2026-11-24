using Dalamud.Game.Config;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.ActionTweaks.CustomCombos;

public sealed unsafe class ComboHotbarService(IClientState clientState, IGameConfig gameConfig, ConfigRoot configRoot) : IHostedService, IDisposable
{
    public const int NumBars = 18;
    public const int NumSlots = 16;
    public const int NumNormalBars = 10;

    private readonly CustomComboConfig _comboConfig = configRoot.Get<CustomComboConfig>();
    public ComboHotbarsConfig Config => _comboConfig.Hotbars;
    public readonly List<string> Messages = []; // user-visible notices, e.g. pending additions that couldn't be placed

    private bool _wasActive;
    private EventSubscription? _configSubscription;

#nullable disable
    private HookAddress<RaptureHotbarModule.Delegates.WriteSavedSlot> _writeSavedSlotHook;
    private HookAddress<RaptureHotbarModule.Delegates.LoadSavedHotbar> _loadSavedHotbarHook;
    private HookAddress<RaptureHotbarModule.Delegates.ReloadMacroSlots> _reloadMacroSlotsHook;
    private HookAddress<RaptureHotbarModule.Delegates.ReloadAllMacroSlots> _reloadAllMacroSlotsHook;
    private HookAddress<RaptureHotbarModule.Delegates.DeleteMacroSlots> _deleteMacroSlotsHook;
    private HookAddress<RaptureHotbarModule.Delegates.ReloadGearsetSlots> _reloadGearsetSlotsHook;
    private HookAddress<RaptureHotbarModule.Delegates.DeleteGearsetSlots> _deleteGearsetSlotsHook;

    private delegate bool ReadFileDelegate(RaptureHotbarModule* self, bool decrypt, byte* ptr, ushort version, uint length);
    private HookAddress<ReadFileDelegate> _readFileHook;
#nullable restore

    private delegate void ReassignGearsetIdDelegate(RaptureHotbarModule* self, int gearsetA, int gearsetB);
    private HookAddress<ReassignGearsetIdDelegate>? _reassignGearsetIdHook;
    private delegate void CopyHotbarDelegate(RaptureHotbarModule* self, int srcJob, int srcBar, int dstJob, uint dstBar);
    private HookAddress<CopyHotbarDelegate>? _copyHotbarHook;

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _writeSavedSlotHook = new(RaptureHotbarModule.Addresses.WriteSavedSlot, WriteSavedSlotDetour);
        _loadSavedHotbarHook = new(RaptureHotbarModule.Addresses.LoadSavedHotbar, LoadSavedHotbarDetour);
        _reloadMacroSlotsHook = new(RaptureHotbarModule.Addresses.ReloadMacroSlots, ReloadMacroSlotsDetour);
        _reloadAllMacroSlotsHook = new(RaptureHotbarModule.Addresses.ReloadAllMacroSlots, ReloadAllMacroSlotsDetour);
        _deleteMacroSlotsHook = new(RaptureHotbarModule.Addresses.DeleteMacroSlots, DeleteMacroSlotsDetour);
        _reloadGearsetSlotsHook = new(RaptureHotbarModule.Addresses.ReloadGearsetSlots, ReloadGearsetSlotsDetour);
        _deleteGearsetSlotsHook = new(RaptureHotbarModule.Addresses.DeleteGearsetSlots, DeleteGearsetSlotsDetour);
        _readFileHook = new(((nint*)*(nint*)RaptureHotbarModule.Instance())[1], ReadFileDetour);
        _reassignGearsetIdHook = new("48 89 5C 24 10 48 89 6C 24 18 48 89 74 24 20 57 41 54 41 55 41 56 41 57 48 83 EC 20 44 8B EA 45 8B E0 48 8D 91 8C 23 01 00", ReassignGearsetIdDetour);
        _copyHotbarHook = new("40 55 56 41 55 41 56 41 57 48 83 EC 20 44 8B 7C 24 70 45 8B F1", CopyHotbarDetour);

        clientState.ClassJobChanged += OnClassJobChanged;
        clientState.Login += ApplyAll;
        gameConfig.UiConfigChanged += OnUiConfigChanged;
        _configSubscription = _comboConfig.Modified.Subscribe(OnConfigModified);

        _ = Service.Framework.Run(() =>
        {
            _wasActive = IsActive;
            ApplyAll();
        }, cancellationToken);
        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose()
    {
        clientState.ClassJobChanged -= OnClassJobChanged;
        clientState.Login -= ApplyAll;
        gameConfig.UiConfigChanged -= OnUiConfigChanged;
        _configSubscription?.Dispose();

        RestoreAll(); // put normal hotbars back

        _copyHotbarHook?.Dispose();
        _reassignGearsetIdHook?.Dispose();
        _readFileHook?.Dispose();
        _deleteGearsetSlotsHook?.Dispose();
        _reloadGearsetSlotsHook?.Dispose();
        _deleteMacroSlotsHook?.Dispose();
        _reloadAllMacroSlotsHook?.Dispose();
        _reloadMacroSlotsHook?.Dispose();
        _loadSavedHotbarHook?.Dispose();
        _writeSavedSlotHook?.Dispose();
    }

    public bool IsActive => Config.Enabled && !Config.ShowVanilla;

    // note: pvp has its own hotbars
    // todo: pvp hotbars
    public bool CanApply(RaptureHotbarModule* m) => IsActive && m != null && m->ModuleReady && (m->ActiveHotbarClassJobId & 0x80) == 0 && !GameMain.IsInPvPArea();

    // saved hotbar index the game uses for a bar of the current job
    public static uint EffectiveJob(RaptureHotbarModule* m, uint bar) => m->IsHotbarShared(bar) ? 0u : m->ActiveHotbarClassJobId;

    public bool IsLocked(uint bar) => gameConfig.TryGet(bar < NumNormalBars ? UiConfigOption.HotbarLock : UiConfigOption.HotbarCrossLock, out uint v) && v != 0;

    // start covering a slot with its current content
    public void CoverSlot(uint bar, uint slot)
    {
        var m = RaptureHotbarModule.Instance();
        if (m == null || !m->ModuleReady)
            return;
        var job = EffectiveJob(m, bar);
        if (Config.Find(job, bar, slot) != null)
            return;
        var s = m->Hotbars[(int)bar].GetHotbarSlot(slot);
        Config.Slots.Add(new() { Job = job, Bar = bar, Slot = slot, Type = s->CommandType, Id = s->CommandId });
        _comboConfig.Modified.Fire();
    }

    public void UncoverSlot(uint bar, uint slot)
    {
        var m = RaptureHotbarModule.Instance();
        if (m == null || !m->ModuleReady)
            return;
        var job = EffectiveJob(m, bar);
        if (Config.Slots.RemoveAll(s => s.Job == job && s.Bar == bar && s.Slot == slot) > 0)
        {
            _comboConfig.Modified.Fire();
            RefreshBar(m, bar);
        }
    }

    public void CoverBar(uint bar)
    {
        for (var slot = 0u; slot < NumSlots; ++slot)
            CoverSlot(bar, slot);
    }

    public void UncoverBar(uint bar)
    {
        var m = RaptureHotbarModule.Instance();
        if (m == null || !m->ModuleReady)
            return;
        var job = EffectiveJob(m, bar);
        if (Config.Slots.RemoveAll(s => s.Job == job && s.Bar == bar) > 0)
        {
            _comboConfig.Modified.Fire();
            RefreshBar(m, bar);
        }
    }

    public void ApplyAll()
    {
        var m = RaptureHotbarModule.Instance();
        if (!CanApply(m))
            return;
        for (var bar = 0u; bar < NumBars; ++bar)
            ApplyBar(m, bar);
    }

    private void ApplyBar(RaptureHotbarModule* m, uint bar)
    {
        var job = EffectiveJob(m, bar);
        foreach (var s in Config.Slots)
            if (s.Job == job && s.Bar == bar)
                ApplySlot(m, s);
    }

    private static void ApplySlot(RaptureHotbarModule* m, ComboHotbarSlot s)
    {
        var slot = m->Hotbars[(int)s.Bar].GetHotbarSlot(s.Slot);
        if (slot == null || slot->CommandType == s.Type && slot->CommandId == s.Id)
            return;
        // same as the game does when loading a saved hotbar
        slot->Clear();
        slot->Set(s.Type, s.Id);
    }

    // reload normal hotbars for every bar the layout covers for the current job
    private void RestoreAll()
    {
        var m = RaptureHotbarModule.Instance();
        if (m == null || !m->ModuleReady || (m->ActiveHotbarClassJobId & 0x80) != 0 || GameMain.IsInPvPArea())
            return;
        for (var bar = 0u; bar < NumBars; ++bar)
            if (Config.HasBar(EffectiveJob(m, bar), bar))
                _loadSavedHotbarHook.Original(m, m->ActiveHotbarClassJobId, bar);
    }

    // reload normal hotbar, then reapply whatever is still covered
    private void RefreshBar(RaptureHotbarModule* m, uint bar)
    {
        if (GameMain.IsInPvPArea() || (m->ActiveHotbarClassJobId & 0x80) != 0)
            return;
        _loadSavedHotbarHook.Original(m, m->ActiveHotbarClassJobId, bar);
        if (CanApply(m))
            ApplyBar(m, bar);
    }

    private void OnConfigModified()
    {
        var active = IsActive;
        if (active == _wasActive)
            return;
        _wasActive = active;
        if (active)
            ApplyAll();
        else
            RestoreAll();
    }

    private void OnClassJobChanged(uint classJobId) => ApplyAll();

    private void OnUiConfigChanged(object? sender, ConfigChangeEvent e)
    {
        if (e.Option is UiConfigOption.HotbarLock or UiConfigOption.HotbarCrossLock && Config.Pending.Count > 0)
            ApplyPending();
    }

    // place additions the game made while hotbars were locked into free covered slots
    private void ApplyPending()
    {
        var modified = false;
        for (var i = 0; i < Config.Pending.Count; ++i)
        {
            var p = Config.Pending[i];
            if (IsLocked(p.Bar))
                continue;

            Config.Pending.RemoveAt(i--);
            modified = true;
            if (Config.Slots.Exists(s => s.Job == p.Job && s.Type == p.Type && s.Id == p.Id))
                continue; // already in the layout

            var target = Config.Find(p.Job, p.Bar, p.Slot) is { Type: RaptureHotbarModule.HotbarSlotType.Empty } original ? original
                : Config.Slots.Find(s => s.Job == p.Job && s.Bar == p.Bar && s.Type == RaptureHotbarModule.HotbarSlotType.Empty)
                ?? Config.Slots.Find(s => s.Job == p.Job && s.Type == RaptureHotbarModule.HotbarSlotType.Empty);
            if (target != null)
            {
                target.Type = p.Type;
                target.Id = p.Id;
            }
            else
            {
                Messages.Add($"No free combo slot for {SlotName(p.Type, p.Id)}; it was not added to your combo hotbars.");
            }
        }
        if (modified)
        {
            _comboConfig.Modified.Fire();
            ApplyAll();
        }
    }

    private void WriteSavedSlotDetour(RaptureHotbarModule* self, uint classJobId, uint hotbarId, uint slotId, RaptureHotbarModule.HotbarSlot* slotSource, bool ignoreSharedHotbars, bool isPvpSlot)
    {
        if (!isPvpSlot && hotbarId < NumBars && slotId < NumSlots && slotSource != null && CanApply(self))
        {
            // same shared-hotbar resolution as the game
            var job = !ignoreSharedHotbars && self->IsHotbarShared(hotbarId) ? 0u : classJobId;
            if (Config.Find(job, hotbarId, slotId) is { } covered)
            {
                if (!IsLocked(hotbarId))
                {
                    // user edit: goes into the combo layout instead of the normal hotbars
                    covered.Type = slotSource->CommandType;
                    covered.Id = slotSource->CommandId;
                }
                else
                {
                    // game-initiated write while locked (like when you just learn an action): defer until unlocked, keep showing the layout
                    if (slotSource->CommandType != RaptureHotbarModule.HotbarSlotType.Empty)
                        Config.Pending.Add(new() { Job = job, Bar = hotbarId, Slot = slotId, Type = slotSource->CommandType, Id = slotSource->CommandId });
                    ApplySlot(self, covered);
                }
                _comboConfig.Modified.Fire();
                return;
            }
        }
        _writeSavedSlotHook.Original(self, classJobId, hotbarId, slotId, slotSource, ignoreSharedHotbars, isPvpSlot);
    }

    private void LoadSavedHotbarDetour(RaptureHotbarModule* self, uint classJobId, uint hotbarId)
    {
        _loadSavedHotbarHook.Original(self, classJobId, hotbarId);
        if (hotbarId < NumBars && classJobId == self->ActiveHotbarClassJobId && CanApply(self))
            ApplyBar(self, hotbarId);
    }

    private void ReloadMacroSlotsDetour(RaptureHotbarModule* self, byte macroSet, byte macroIndex)
    {
        _reloadMacroSlotsHook.Original(self, macroSet, macroIndex);
        ApplyAll();
    }

    private void ReloadAllMacroSlotsDetour(RaptureHotbarModule* self)
    {
        _reloadAllMacroSlotsHook.Original(self);
        ApplyAll();
    }

    private void DeleteMacroSlotsDetour(RaptureHotbarModule* self, byte macroSet, byte macroIndex)
    {
        _deleteMacroSlotsHook.Original(self, macroSet, macroIndex);
        var id = (uint)(macroIndex | (macroSet << 8));
        ClearLayoutSlots(RaptureHotbarModule.HotbarSlotType.Macro, id);
        ApplyAll();
    }

    private void ReloadGearsetSlotsDetour(RaptureHotbarModule* self, int gearsetId)
    {
        _reloadGearsetSlotsHook.Original(self, gearsetId);
        ApplyAll();
    }

    private void DeleteGearsetSlotsDetour(RaptureHotbarModule* self, int gearsetId)
    {
        _deleteGearsetSlotsHook.Original(self, gearsetId);
        ClearLayoutSlots(RaptureHotbarModule.HotbarSlotType.GearSet, (uint)gearsetId);
        ApplyAll();
    }

    private void ReassignGearsetIdDetour(RaptureHotbarModule* self, int gearsetA, int gearsetB)
    {
        _reassignGearsetIdHook!.Original(self, gearsetA, gearsetB);
        // the game swaps the two gearset numbers in all hotbars; do the same for the layout
        var modified = false;
        foreach (var s in Config.Slots.Where(s => s.Type == RaptureHotbarModule.HotbarSlotType.GearSet))
        {
            if (s.Id == (uint)gearsetA)
                (s.Id, modified) = ((uint)gearsetB, true);
            else if (s.Id == (uint)gearsetB)
                (s.Id, modified) = ((uint)gearsetA, true);
        }
        if (modified)
            _comboConfig.Modified.Fire();
        ApplyAll();
    }

    // copies a whole bar from one saved hotbar to another; copies into covered slots are edits of the layout (regardless of lock)
    private void CopyHotbarDetour(RaptureHotbarModule* self, int srcJob, int srcBar, int dstJob, uint dstBar)
    {
        var covered = CanApply(self) && dstJob is >= 0 and < 0x46 && srcJob is >= 0 and < 0x46 && dstBar < NumBars && srcBar is >= 0 and < NumBars ? Config.BarSlots((uint)dstJob, dstBar).ToList() : [];
        if (covered.Count == 0)
        {
            _copyHotbarHook!.Original(self, srcJob, srcBar, dstJob, dstBar);
            return;
        }

        // keep normal hotbar contents of covered slots as they were, the copy goes into the layout instead
        var saved = covered.Select(c => *self->SavedHotbars[dstJob].Hotbars[(int)dstBar].GetSavedHotbarSlot(c.Slot)).ToList();
        _copyHotbarHook!.Original(self, srcJob, srcBar, dstJob, dstBar);
        for (var i = 0; i < covered.Count; ++i)
        {
            var c = covered[i];
            *self->SavedHotbars[dstJob].Hotbars[(int)dstBar].GetSavedHotbarSlot(c.Slot) = saved[i];

            // source is the source job's combo layout where it has one, its normal hotbar otherwise
            if (Config.Find((uint)srcJob, (uint)srcBar, c.Slot) is { } src)
            {
                (c.Type, c.Id) = (src.Type, src.Id);
            }
            else
            {
                var s = self->SavedHotbars[srcJob].Hotbars[srcBar].GetSavedHotbarSlot(c.Slot);
                (c.Type, c.Id) = (s->CommandType, s->CommandId);
            }
        }
        _comboConfig.Modified.Fire();
        if ((uint)dstJob == EffectiveJob(self, dstBar))
            ApplyBar(self, dstBar);
    }

    private bool ReadFileDetour(RaptureHotbarModule* self, bool decrypt, byte* ptr, ushort version, uint length)
    {
        // on login, the game defers the job switch until hotbar data is loaded, and performs it at the end of this function
        var res = _readFileHook.Original(self, decrypt, ptr, version, length);
        if (res && CanApply(self))
            ApplyAll();
        return res;
    }

    private void ClearLayoutSlots(RaptureHotbarModule.HotbarSlotType type, uint id)
    {
        var modified = false;
        foreach (var s in Config.Slots.Where(s => s.Type == type && s.Id == id))
            (s.Type, s.Id, modified) = (RaptureHotbarModule.HotbarSlotType.Empty, 0, true);
        if (modified)
            _comboConfig.Modified.Fire();
    }

    public static string SlotName(RaptureHotbarModule.HotbarSlotType type, uint id) => type switch
    {
        RaptureHotbarModule.HotbarSlotType.Empty => "<empty>",
        RaptureHotbarModule.HotbarSlotType.Action => Service.LuminaRow<Lumina.Excel.Sheets.Action>(id)?.Name.ToString() ?? $"action {id}",
        RaptureHotbarModule.HotbarSlotType.Item => Service.LuminaRow<Lumina.Excel.Sheets.Item>(id % 1000000)?.Name.ToString() ?? $"item {id}",
        RaptureHotbarModule.HotbarSlotType.GeneralAction => Service.LuminaRow<Lumina.Excel.Sheets.GeneralAction>(id)?.Name.ToString() ?? $"general action {id}",
        RaptureHotbarModule.HotbarSlotType.Macro => $"macro {(id >> 8 == 0 ? "individual" : "shared")} #{id & 0xFF}",
        RaptureHotbarModule.HotbarSlotType.GearSet => $"gear set {id + 1}",
        _ => $"{type} {id}"
    };
}
