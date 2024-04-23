namespace BossMod;

public static class CommonRotation
{
    public static int SpellCDGroup<AID>(AID spell) where AID : Enum
    {
        var cg = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>((uint)(object)spell)?.CooldownGroup ?? 0;
        return cg is > 0 and <= 80 ? cg - 1 : -1;
    }

    public class PlayerState(WorldState ws)
    {
        public int Level;
        public int UnlockProgress;
        public uint CurMP; // 10000 max
        public bool TargetingEnemy;
        public bool HaveTankStance;
        public float RangeToTarget; // minus both hitboxes; <= 0 means inside hitbox, <= 3 means in melee range, maxvalue if there is no target
        public float AnimationLock; // typical actions have 0.6 delay, but some (notably primal rend and potion) are >1
        public float AnimationLockDelay; // average time between action request and confirmation; this is added to effective animation lock for actions
        public float ComboTimeLeft; // 0 if not in combo, max 30
        public uint ComboLastAction;
        public float RaidBuffsLeft; // 0 if no damage-up status is up, otherwise it is time left on longest
        public int LimitBreakLevel;

        // these simply point to client state
        public readonly Cooldown[] Cooldowns = ws.Client.Cooldowns;
        public readonly ActionID[] DutyActions = ws.Client.DutyActions;
        public readonly byte[] BozjaHolster = ws.Client.BozjaHolster;

        // both 2.5 max (unless slowed), reduced by gear attributes and certain status effects
        public float AttackGCDTime;
        public float SpellGCDTime;

        // find a slot containing specified duty action; returns -1 if not found
        public int FindDutyActionSlot(ActionID action) => Array.IndexOf(DutyActions, action);
        // find a slot containing specified duty action, if other duty action is the specified one; returns -1 if not found, or other action is different
        public int FindDutyActionSlot(ActionID action, ActionID other)
        {
            var slot = FindDutyActionSlot(action);
            return slot >= 0 && DutyActions[1 - slot] == other ? slot : -1;
        }

        public float GCD => Cooldowns[CommonDefinitions.GCDGroup].Remaining; // 2.5 max (decreased by SkS), 0 if not on gcd
        public float SprintCD => Cooldowns[CommonDefinitions.SprintCDGroup].Remaining; // 60.0 max
        public float PotionCD => Cooldowns[CommonDefinitions.PotionCDGroup].Remaining; // variable max
        public float CD<CDGroup>(CDGroup group) where CDGroup : Enum => Cooldowns[(int)(object)group].Remaining;

        public float DutyActionCD(int slot) => slot is >= 0 and < 2 ? Cooldowns[CommonDefinitions.DutyAction0CDGroup + slot].Remaining : float.MaxValue;
        public float DutyActionCD(ActionID action) => DutyActionCD(FindDutyActionSlot(action));

        // check whether weaving typical ogcd off cooldown would end its animation lock by the specified deadline
        public float OGCDSlotLength => 0.6f + AnimationLockDelay; // most actions have 0.6 anim lock delay, which allows double-weaving oGCDs between GCDs
        public bool CanWeave(float deadline) => AnimationLock + OGCDSlotLength <= deadline; // is it still possible to weave typical oGCD without missing deadline?
        // check whether weaving ogcd with specified remaining cooldown and lock time would end its animation lock by the specified deadline
        // deadline is typically either infinity (if we don't care about GCDs) or GCD (for second/only ogcd slot) or GCD-OGCDSlotLength (for first ogcd slot)
        public bool CanWeave(float cooldown, float actionLock, float deadline) => deadline < 10000 ? MathF.Max(cooldown, AnimationLock) + actionLock + AnimationLockDelay <= deadline : cooldown <= AnimationLock;
        public bool CanWeave<CDGroup>(CDGroup group, float actionLock, float deadline) where CDGroup : Enum => CanWeave(CD(group), actionLock, deadline);
    }

    public class Strategy
    {
        public enum OffensiveAbilityUse : uint
        {
            Automatic = 0, // use standard logic for ability

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1, // delay until window end

            [PropertyDisplay("Force", 0x8000ff00)]
            Force = 2, // force use ASAP
        }

        public float CombatTimer; // MinValue if not in combat, negative during countdown, zero or positive during combat
        public bool ForbidDOTs;
        public float ForceMovementIn;
        public float FightEndIn; // how long fight will last (we try to spend all resources before this happens)
        public float RaidBuffsIn; // estimate time when new raidbuff window starts (if it is smaller than FightEndIn, we try to conserve resources)
        public float PositionLockIn; // time left to use moving abilities (Primal Rend and Onslaught) - we won't use them if it is ==0; setting this to 2.5f will make us use PR asap
        public Positional NextPositional;
        public bool NextPositionalImminent; // true if next positional will happen on next gcd
        public bool NextPositionalCorrect; // true if correctly positioned for next positional
    }
}
