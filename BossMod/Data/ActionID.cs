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
}

public enum Positional { Any, Flank, Rear, Front }

public struct ActionID
{
    public uint Raw; // high byte is type, low 3 bytes is ID

    public ActionType Type => (ActionType)(Raw >> 24);
    public uint ID => Raw & 0x00FFFFFFu;

    public ActionID(uint raw = 0) { Raw = raw; }
    public ActionID(ActionType type, uint id) { Raw = ((uint)type << 24) | id; }

    public static implicit operator bool(ActionID x) => x.Raw != 0;
    public static bool operator ==(ActionID l, ActionID r) => l.Raw == r.Raw;
    public static bool operator !=(ActionID l, ActionID r) => l.Raw != r.Raw;
    public override bool Equals(object? obj) => obj is ActionID && this == (ActionID)obj;
    public override int GetHashCode() => Raw.GetHashCode();
    public override string ToString() => $"{Type} {ID} '{Name()}'";

    public AID As<AID>() where AID : Enum => (AID)(object)ID;

    public string Name() => Type switch
    {
        ActionType.Spell => Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(ID)?.Name ?? "<not found>",
        ActionType.Item => $"{Service.LuminaRow<Lumina.Excel.GeneratedSheets.Item>(ID % 1000000)?.Name ?? "<not found>"}{(ID > 1000000 ? " (HQ)" : "")}", // see Dalamud.Game.Text.SeStringHandling.Payloads.GetAdjustedId; TODO: id > 500000 is "collectible", >2000000 is "event" ??
        ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1 => $"{(BozjaHolsterID)ID}",
        _ => ""
    };

    public float CastTime() => Type switch
    {
        ActionType.Spell => (Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(ID)?.Cast100ms ?? 0) * 0.1f,
        _ => 0
    };

    public bool IsCasted() => CastTime() > 0;

    public bool IsGroundTargeted() => Type switch
    {
        ActionType.Spell => Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(ID)?.TargetArea ?? false,
        _ => false
    };

    public static ActionID MakeSpell<AID>(AID id) where AID : Enum
    {
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
