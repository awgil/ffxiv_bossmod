using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            switch (Type)
            {
                case ActionType.Spell:
                    {
                        var actionData = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(ID);
                        string name = actionData?.Name ?? "<not found>";
                        return $"{Type} {ID} '{name}'";
                    }
                case ActionType.Item:
                    {
                        // see Dalamud.Game.Text.SeStringHandling.Payloads.GetAdjustedId
                        // TODO: id > 500000 is "collectible", >2000000 is "event" ??
                        bool isHQ = ID > 1000000;
                        var itemData = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Item>(ID % 1000000);
                        string name = itemData?.Name ?? "<not found>";
                        return $"{Type} {ID} '{name}'{(isHQ ? " (HQ)" : "")}";
                    }
                default:
                    return $"{Type} {ID}";
            }
        }

        public bool IsCasted()
        {
            return Type switch
            {
                ActionType.Spell => (Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(ID)?.Cast100ms ?? 0) > 0,
                _ => false
            };
        }

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
            return new(ActionType.Spell, (uint)(object)id);
        }
    }
}
