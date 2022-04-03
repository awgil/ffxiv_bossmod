using System.Collections.Generic;

namespace BossMod
{
    public class CooldownPlan
    {
        public enum AbilityCategory { SelfMitigation, RaidMitigation }

        public struct SupportedAbility
        {
            public AbilityCategory Category;
            public float Duration;
            public float Cooldown;

            public SupportedAbility(AbilityCategory category, float duration, float cooldown)
            {
                Category = category;
                Duration = duration;
                Cooldown = cooldown;
            }
        }

        public class SupportedClassData
        {
            public Dictionary<ActionID, SupportedAbility> Abilities = new();
        }

        public static Dictionary<Class, SupportedClassData> SupportedClasses = new();

        static CooldownPlan()
        {
            var war = SupportedClasses[Class.WAR] = new();
            war.Abilities[ActionID.MakeSpell(WARRotation.AID.Rampart)] = new(AbilityCategory.SelfMitigation, 20, 90);
            war.Abilities[ActionID.MakeSpell(WARRotation.AID.Vengeance)] = new(AbilityCategory.SelfMitigation, 15, 120);
            war.Abilities[ActionID.MakeSpell(WARRotation.AID.ThrillOfBattle)] = new(AbilityCategory.SelfMitigation, 10, 90);
            war.Abilities[ActionID.MakeSpell(WARRotation.AID.Equilibrium)] = new(AbilityCategory.SelfMitigation, 0, 60);
            war.Abilities[ActionID.MakeSpell(WARRotation.AID.Bloodwhetting)] = new(AbilityCategory.SelfMitigation, 4, 25);
            war.Abilities[ActionID.MakeSpell(WARRotation.AID.ArmsLength)] = new(AbilityCategory.SelfMitigation, 6, 120);
            war.Abilities[ActionID.MakeSpell(WARRotation.AID.Reprisal)] = new(AbilityCategory.RaidMitigation, 10, 60);
            war.Abilities[ActionID.MakeSpell(WARRotation.AID.ShakeItOff)] = new(AbilityCategory.RaidMitigation, 15, 90);
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

        public Class Class;
        public string Name;
        public Dictionary<ActionID, List<AbilityUse>> PlanAbilities = new();

        public CooldownPlan(Class @class, string name)
        {
            Class = @class;
            Name = name;
            foreach (var k in SupportedClasses[@class].Abilities.Keys)
                PlanAbilities[k] = new();
        }
    }
}
