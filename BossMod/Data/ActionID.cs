namespace BossMod;

// matches FFXIVClientStructs.FFXIV.Client.Game.ActionType, with some custom additions
public enum ActionType : byte
{
    None = 0,
    Spell = 1, // CS renamed that to Action
    Item = 2,
    KeyItem = 3,
    Ability = 4,
    General = 5, // CS renamed that to GeneralAction
    Buddy = 6,
    MainCommand = 7,
    Companion = 8,
    CraftAction = 9,
    PetAction = 11,
    Mount = 13,
    PvPAction = 14,
    Waymark = 15, // CS renamed that to FieldMarker
    ChocoboRaceAbility = 16,
    ChocoboRaceItem = 17,
    SquadronAction = 19, // CS renamed that to BgcArmyAction
    Ornament = 20,

    // below are custom additions, these aren't proper actions from game's point of view, but it makes sense for us to treat them as such
    BozjaHolsterSlot0 = 0xE0, // id = BozjaHolsterID, use from holster to replace duty action 0
    BozjaHolsterSlot1 = 0xE1, // id = BozjaHolsterID, use from holster to replace duty action 1
    Pomander = 0xE2, // id = PomanderID
    Magicite = 0xE3, // id = slot (1-3)
}

public enum Positional { Any, Flank, Rear, Front }

// high byte is type, low 3 bytes is ID
public readonly record struct ActionID(uint Raw)
{
    public readonly ActionType Type => (ActionType)(Raw >> 24);
    public readonly uint ID => Raw & 0x00FFFFFFu;

    public ActionID(ActionType type, uint id) : this(((uint)type << 24) | id) { }

    public static implicit operator bool(ActionID x) => x.Raw != 0;
    public override readonly string ToString() => $"{Type} {ID} '{Name()}'";

    public readonly AID As<AID>() where AID : Enum => (AID)(object)ID;

    public readonly string Name() => Type switch
    {
        ActionType.Spell => Service.LuminaRow<Lumina.Excel.Sheets.Action>(ID)?.Name.ToString() ?? "<not found>",
        ActionType.Item => $"{Service.LuminaRow<Lumina.Excel.Sheets.Item>(ID % 1000000)?.Name ?? "<not found>"}{(ID > 1000000 ? " (HQ)" : "")}", // see Dalamud.Game.Text.SeStringHandling.Payloads.GetAdjustedId; TODO: id > 500000 is "collectible", >2000000 is "event" ??
        ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1 => $"{(BozjaHolsterID)ID}",
        ActionType.PetAction => Service.LuminaRow<Lumina.Excel.Sheets.PetAction>(ID)?.Name.ToString() ?? "<not found>",
        _ => ""
    };

    // see ActionManager.GetSpellIdForAction
    public readonly uint SpellId() => Type switch
    {
        ActionType.Spell => ID,
        ActionType.Item => Service.LuminaRow<Lumina.Excel.Sheets.Item>(ID % 500000)?.ItemAction.ValueNullable?.Action.RowId ?? 0,
        ActionType.KeyItem => Service.LuminaRow<Lumina.Excel.Sheets.EventItem>(ID)?.Action.RowId ?? 0,
        ActionType.Ability => 2, // 'interaction'
        ActionType.General => Service.LuminaRow<Lumina.Excel.Sheets.GeneralAction>(ID)?.Action.RowId ?? 0, // note: duty action 1/2 (26/27) use special code
        ActionType.Mount => 4, // 'mount'
        ActionType.Ornament => 20061, // 'accessorize'
        _ => 0
    };

    public readonly float CastTime() => Type switch
    {
        ActionType.Spell => (Service.LuminaRow<Lumina.Excel.Sheets.Action>(ID)?.Cast100ms ?? 0) * 0.1f,
        _ => 0
    };

    public readonly float CastTimeExtra() => Service.LuminaRow<Lumina.Excel.Sheets.Action>(SpellId())?.ExtraCastTime100ms * 0.1f ?? 0;

    public readonly bool IsCasted() => CastTime() > 0;

    public static ActionID MakeSpell<AID>(AID? id) where AID : Enum
    {
        if (id == null)
            return new();

        var castID = (uint)(object)id;
        return castID != 0 ? new(ActionType.Spell, castID) : new();
    }

    public static ActionID MakeBozjaHolster(BozjaHolsterID id, int slot) => slot switch
    {
        0 => new(ActionType.BozjaHolsterSlot0, (uint)id),
        1 => new(ActionType.BozjaHolsterSlot1, (uint)id),
        _ => default
    };
}
