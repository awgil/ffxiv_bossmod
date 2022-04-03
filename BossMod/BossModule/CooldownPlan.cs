using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public class CooldownPlan
    {
        public enum SupportedClass { WAR, Count }
        public enum AbilityCategory { SelfMitigation, RaidMitigation }

        public class SupportedAbility
        {
            public AbilityCategory Category;
            public ActionID Action;
            public float Duration;
            public float Cooldown;

            public SupportedAbility(AbilityCategory category, ActionID action, float duration, float cooldown)
            {
                Category = category;
                Action = action;
                Duration = duration;
                Cooldown = cooldown;
            }
        }

        public class SupportedClassData
        {
            public Class ClassID;
            public List<SupportedAbility> Abilities = new();
        }

        public static SupportedClassData[] SupportedClasses;

        static CooldownPlan()
        {
            SupportedClasses = new SupportedClassData[(int)SupportedClass.Count + 1];
            SupportedClasses[(int)SupportedClass.Count] = new();

            var war = SupportedClasses[(int)SupportedClass.WAR] = new() { ClassID = Class.WAR };
            war.Abilities.Add(new(AbilityCategory.SelfMitigation, ActionID.MakeSpell(WARRotation.AID.Rampart), 20, 90));
            war.Abilities.Add(new(AbilityCategory.SelfMitigation, ActionID.MakeSpell(WARRotation.AID.Vengeance), 15, 120));
            war.Abilities.Add(new(AbilityCategory.SelfMitigation, ActionID.MakeSpell(WARRotation.AID.ThrillOfBattle), 10, 90));
            war.Abilities.Add(new(AbilityCategory.SelfMitigation, ActionID.MakeSpell(WARRotation.AID.Equilibrium), 0, 60));
            war.Abilities.Add(new(AbilityCategory.SelfMitigation, ActionID.MakeSpell(WARRotation.AID.Bloodwhetting), 4, 25));
            war.Abilities.Add(new(AbilityCategory.SelfMitigation, ActionID.MakeSpell(WARRotation.AID.ArmsLength), 6, 120));
            war.Abilities.Add(new(AbilityCategory.RaidMitigation, ActionID.MakeSpell(WARRotation.AID.Reprisal), 10, 60));
            war.Abilities.Add(new(AbilityCategory.RaidMitigation, ActionID.MakeSpell(WARRotation.AID.ShakeItOff), 15, 90));
        }

        public class AbilityUse
        {
            public uint StateID;
            public float TimeSinceActivation;
            public float WindowLength;

            public AbilityUse(uint stateID, float timeSinceActivation, float windowLength)
            {
                StateID = stateID;
                TimeSinceActivation = timeSinceActivation;
                WindowLength = windowLength;
            }
        }

        public SupportedClass PlanClass;
        public List<AbilityUse>[] PlanAbilities; // [i] - list of uses for ability #i

        public CooldownPlan(SupportedClass planClass)
        {
            PlanClass = planClass;

            var suppAbilities = SupportedClasses[(int)planClass].Abilities;
            PlanAbilities = new List<AbilityUse>[suppAbilities.Count];
            for (int i = 0; i < suppAbilities.Count; ++i)
                PlanAbilities[i] = new();
        }
    }
}
