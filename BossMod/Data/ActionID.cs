using System;

namespace BossMod
{
    // matches FFXIVClientStructs.FFXIV.Client.Game.ActionType
    public enum ActionType : byte
    {
        None = 0,
        Spell = 1,
        Item = 2,
        KeyItem = 3,
        Ability = 4,
        General = 5,
        Companion = 6,
        CraftAction = 9,
        MainCommand = 10,
        PetAction = 11,
        Mount = 13,
        PvPAction = 14,
        Waymark = 15,
        ChocoboRaceAbility = 16,
        ChocoboRaceItem = 17,
        SquadronAction = 19,
        Accessory = 20
    }

    public enum Positional { Any, Flank, Rear, Front }

    public struct ActionID
    {
        public uint Raw; // high byte is type, low 3 bytes is ID

        public ActionType Type => (ActionType)(Raw >> 24);
        public uint ID => Raw & 0x00FFFFFFu;

        public ActionID(uint raw = 0) { Raw = raw; }
        public ActionID(ActionType type, uint id) { Raw = ((uint)type << 24) | id; }

        public static bool operator ==(ActionID l, ActionID r) => l.Raw == r.Raw;
        public static bool operator !=(ActionID l, ActionID r) => l.Raw != r.Raw;
        public static implicit operator bool(ActionID x) => x.Raw != 0;

        public string Name()
        {
            switch (Type)
            {
                case ActionType.Spell:
                        return Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(ID)?.Name ?? "<not found>";
                case ActionType.Item:
                    {
                        // see Dalamud.Game.Text.SeStringHandling.Payloads.GetAdjustedId
                        // TODO: id > 500000 is "collectible", >2000000 is "event" ??
                        bool isHQ = ID > 1000000;
                        string name = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Item>(ID % 1000000)?.Name ?? "<not found>";
                        return $"{name}{(isHQ ? " (HQ)" : "")}";
                    }
                default:
                    return "";
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is ActionID && this == (ActionID)obj;
        }

        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Type} {ID} '{Name()}'";
        }

        public float CastTime()
        {
            return Type switch
            {
                ActionType.Spell => (Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(ID)?.Cast100ms ?? 0) * 0.1f,
                _ => 0
            };
        }

        public bool IsCasted() => CastTime() > 0;

        public bool IsGroundTargeted()
        {
            return Type switch
            {
                ActionType.Spell => Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(ID)?.TargetArea ?? false,
                _ => false
            };
        }

        public static ActionID MakeSpell<AID>(AID id) where AID : Enum
        {
            var castID = (uint)(object)id;
            return castID != 0 ? new(ActionType.Spell, castID) : new();
        }
    }
}
