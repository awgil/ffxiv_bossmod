namespace BossMod;

public record struct QuestLockEntry(int Level, uint QuestID);

public class ActionDefinition(float range, float castTime, int cooldownGroup, float cooldown, int maxChargesAtCap, float animationLock)
{
    public float Range = range; // 0 is for self-targeted abilities
    public float CastTime = castTime; // 0 for instant-cast
    public int CooldownGroup = cooldownGroup;
    public float Cooldown = cooldown; // for multi-charge abilities - for single charge
    public int MaxChargesAtCap = maxChargesAtCap;
    public float AnimationLock = animationLock;
    public float EffectDuration; // used by planner UI

    public float CooldownAtFirstCharge => (MaxChargesAtCap - 1) * Cooldown;
}

public static class CommonDefinitions
{
    public static readonly ActionID IDAutoAttack = new(ActionType.Spell, 7);
    public static readonly ActionID IDAutoShot = new(ActionType.Spell, 8);
    public static readonly ActionID IDSprint = new(ActionType.Spell, 3);
    public static readonly ActionID IDPotionStr = new(ActionType.Item, 1039727); // hq grade 8 tincture of strength
    public static readonly ActionID IDPotionDex = new(ActionType.Item, 1039728); // hq grade 8 tincture of dexterity
    public static readonly ActionID IDPotionVit = new(ActionType.Item, 1039729); // hq grade 8 tincture of vitality
    public static readonly ActionID IDPotionInt = new(ActionType.Item, 1039730); // hq grade 8 tincture of intelligence
    public static readonly ActionID IDPotionMnd = new(ActionType.Item, 1039731); // hq grade 8 tincture of mind

    public const int SprintCDGroup = 55;
    public const int GCDGroup = 57;
    public const int PotionCDGroup = 58;
    public const int DutyAction0CDGroup = 80;
    public const int DutyAction1CDGroup = 81;

    public static Dictionary<ActionID, ActionDefinition> CommonActionData(ActionID statPotion)
    {
        var res = new Dictionary<ActionID, ActionDefinition>();
        (res[IDSprint] = new(0, 0, SprintCDGroup, 60, 1, 0.6f)).EffectDuration = 10;
        (res[statPotion] = new(0, 0, PotionCDGroup, 270, 1, 1.1f)).EffectDuration = 30;

        // bozja actions
        for (var i = BozjaHolsterID.None + 1; i < BozjaHolsterID.Count; ++i)
        {
            var normalAction = BozjaActionID.GetNormal(i);
            var normalData = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(normalAction.ID);
            if (normalData == null)
                continue;

            bool isItem = normalAction == BozjaActionID.GetHolster(i);
            var animLock = isItem ? 1.1f : 0.6f;
            res[normalAction] = new(normalData.Range, normalData.Cast100ms * 0.1f, normalData.CooldownGroup - 1, normalData.Recast100ms * 0.1f, Math.Max((int)normalData.MaxCharges, 1), animLock);
            if (!isItem)
            {
                // TODO: remove fake cdgroup
                res[ActionID.MakeBozjaHolster(i, 0)] = res[ActionID.MakeBozjaHolster(i, 1)] = new(0, 0, 71, 0, 1, 2.1f);
            }
        }

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
    public static ActionDefinition GCDWithCharges<AID, CDGroup>(this Dictionary<ActionID, ActionDefinition> res, AID aid, float range, CDGroup cdGroup, float cooldown, int maxChargesAtCap, float animationLock = 0.6f) where AID : Enum where CDGroup : Enum
        => res[ActionID.MakeSpell(aid)] = new(range, 0, (int)(object)cdGroup, cooldown, maxChargesAtCap, animationLock);

    // check whether given actor has tank stance
    public static bool HasTankStance(Actor a)
    {
        var stanceSID = a.Class switch
        {
            Class.WAR => (uint)WAR.SID.Defiance,
            Class.PLD => (uint)PLD.SID.IronWill,
            Class.GNB => (uint)GNB.SID.RoyalGuard,
            _ => 0u
        };
        return stanceSID != 0 && a.FindStatus(stanceSID) != null;
    }
}
