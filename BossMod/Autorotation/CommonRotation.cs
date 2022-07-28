using System;

namespace BossMod
{
    public static class CommonRotation
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
        public static int SpellCDGroup<AID>(AID spell) where AID : Enum
        {
            var cg = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>((uint)(object)spell)?.CooldownGroup ?? 0;
            return cg is > 0 and <= 80 ? cg - 1 : -1;
        }

        public class PlayerState
        {
            public int Level;
            public uint CurMP; // 10000 max
            public float AnimationLock; // typical actions have 0.6 delay, but some (notably primal rend and potion) are >1
            public float AnimationLockDelay; // average time between action request and confirmation; this is added to effective animation lock for actions
            public float ComboTimeLeft; // 0 if not in combo, max 30
            public uint ComboLastAction;
            public float RaidBuffsLeft; // 0 if no damage-up status is up, otherwise it is time left on longest
            public float[] Cooldowns = new float[80];

            public float GCD => Cooldowns[GCDGroup]; // 2.5 max (decreased by SkS), 0 if not on gcd
            public float SprintCD => Cooldowns[SprintCDGroup]; // 60.0 max
            public float PotionCD => Cooldowns[PotionCDGroup]; // variable max
            public float CD<CDGroup>(CDGroup group) where CDGroup : Enum => Cooldowns[(int)(object)group];

            public float OGCDDelay => 0.1f; // TODO: consider returning AnimationLockDelay instead...
            public float OGCDSlotLength => 0.6f + OGCDDelay; // most actions have 0.6 anim lock delay, which allows double-weaving oGCDs between GCDs
            public float DoubleWeaveWindowEnd => GCD - OGCDSlotLength; // amount of time left until last possible moment to weave second oGCD
            public bool CanDoubleWeave => AnimationLock + OGCDSlotLength <= DoubleWeaveWindowEnd; // is it still possible to double-weave without delaying GCD?
            public bool CanSingleWeave => AnimationLock + OGCDSlotLength <= GCD; // is it still possible to single-weave without delaying GCD?

            // check whether weaving ogcd with specified remaining cooldown and lock time, so that we won't be locked by specific window-end
            // window-end is typically either GCD (for second/only ogcd slot) or DoubleWeaveWindowEnd (for first ogcd slot)
            public bool CanWeave(float cooldown, float actionLock, float windowEnd) => MathF.Max(cooldown, 0) + actionLock + OGCDDelay <= windowEnd;
            public bool CanWeave<CDGroup>(CDGroup group, float actionLock, float windowEnd) where CDGroup : Enum => CanWeave(CD(group), actionLock, windowEnd);
        }

        public class Strategy
        {
            public enum PotionUse { Manual, DelayUntilRaidBuffs, DelayUntilPersonalBuffs, Immediate }

            public bool Prepull; // true if neither self nor target are in combat; TODO consider replacing with countdown timer
            public float FightEndIn; // how long fight will last (we try to spend all resources before this happens)
            public float RaidBuffsIn; // estimate time when new raidbuff window starts (if it is smaller than FightEndIn, we try to conserve resources)
            public float PositionLockIn; // time left to use moving abilities (Primal Rend and Onslaught) - we won't use them if it is ==0; setting this to 2.5f will make us use PR asap
            public PotionUse Potion; // strategy for automatic potion use
            public bool ExecuteSprint;
        }

        public static AbilityDefinitions.Class BuildCommonDefinitions()
        {
            AbilityDefinitions.Class res = new();
            int sprintTrack = res.AddTrack(AbilityDefinitions.Track.Category.SharedCooldown, "Sprint");
            res.Abilities[IDSprint] = new() { CooldownTrack = sprintTrack, Cooldown = 60, EffectDuration = 10 };
            return res;
        }
    }
}
