using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public struct QuestLockEntry
    {
        public int Level;
        public uint QuestID;

        public QuestLockEntry(int level, uint questID)
        {
            Level = level;
            QuestID = questID;
        }
    }

    public enum ActionCategory { None, SelfMitigation, RaidMitigation }

    public class ActionDefinition
    {
        public float Range; // 0 is for self-targeted abilities
        public float CastTime; // 0 for instant-cast
        public int CooldownGroup;
        public float Cooldown; // for multi-charge abilities - for single charge
        public int MaxChargesAtCap;
        public float AnimationLock;
        public float EffectDuration; // used by planner UI
        public ActionCategory Category; // used by planner UI

        public float CooldownAtFirstCharge => (MaxChargesAtCap - 1) * Cooldown;

        public ActionDefinition(float range, float castTime, int cooldownGroup, float cooldown, int maxChargesAtCap, float animationLock)
        {
            Range = range;
            CastTime = castTime;
            CooldownGroup = cooldownGroup;
            Cooldown = cooldown;
            MaxChargesAtCap = maxChargesAtCap;
            AnimationLock = animationLock;
        }

        // TODO improve...
        public void SetPlanningProperties(float effectDuration, ActionCategory category = ActionCategory.None)
        {
            EffectDuration = effectDuration;
            Category = category;
        }
    }

    public static class CommonDefinitions
    {
        public static ActionID IDAutoAttack = new(ActionType.Spell, 7);
        public static ActionID IDSprint = new(ActionType.General, 4);
        public static ActionID IDPotionStr = new(ActionType.Item, 1036109); // hq grade 6 tincture of strength
        public static ActionID IDPotionDex = new(ActionType.Item, 1036110); // hq grade 6 tincture of dexterity
        public static ActionID IDPotionVit = new(ActionType.Item, 1036111); // hq grade 6 tincture of vitality
        public static ActionID IDPotionInt = new(ActionType.Item, 1036112); // hq grade 6 tincture of intelligence
        public static ActionID IDPotionMnd = new(ActionType.Item, 1036113); // hq grade 6 tincture of mind

        public static int SprintCDGroup = 55;
        public static int GCDGroup = 57;
        public static int PotionCDGroup = 58;

        public static Dictionary<ActionID, ActionDefinition> CommonActionData(ActionID statPotion)
        {
            var res = new Dictionary<ActionID, ActionDefinition>();
            (res[IDSprint] = new(0, 0, SprintCDGroup, 60, 1, 0.6f)).EffectDuration = 10;
            (res[statPotion] = new(0, 0, PotionCDGroup, 270, 1, 1.1f)).EffectDuration = 30;
            return res;
        }

        public static ActionDefinition GCD<AID>(this Dictionary<ActionID, ActionDefinition> res, AID aid, float range, float animationLock = 0.6f) where AID : Enum
            => res[ActionID.MakeSpell(aid)] = new(range, 0, GCDGroup, 2.5f, 1, animationLock);
        public static ActionDefinition GCDCast<AID>(this Dictionary<ActionID, ActionDefinition> res, AID aid, float range, float castTime, float animationLock = 0.1f) where AID : Enum
            => res[ActionID.MakeSpell(aid)] = new(range, castTime, GCDGroup, 2.5f, 1, animationLock);
        public static ActionDefinition OGCD<AID, CDGroup>(this Dictionary<ActionID, ActionDefinition> res, AID aid, float range, CDGroup cdGroup, float cooldown, float animationLock = 0.6f) where AID : Enum where CDGroup : Enum
            => res[ActionID.MakeSpell(aid)] = new(range, 0, (int)(object)cdGroup, cooldown, 1, animationLock);
        public static ActionDefinition OGCDWithCharges<AID, CDGroup>(this Dictionary<ActionID, ActionDefinition> res, AID aid, float range, CDGroup cdGroup, float cooldown, int maxChargesAtCap, float animationLock = 0.6f) where AID : Enum where CDGroup : Enum
            => res[ActionID.MakeSpell(aid)] = new(range, 0, (int)(object)cdGroup, cooldown, maxChargesAtCap, animationLock);
    }
}
