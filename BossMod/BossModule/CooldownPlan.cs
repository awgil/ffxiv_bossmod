using System.Collections.Generic;

namespace BossMod
{
    public class CooldownPlan
    {
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

            public AbilityUse Clone()
            {
                return (AbilityUse)MemberwiseClone();
            }
        }

        public Class Class;
        public string Name;
        public Dictionary<uint, List<AbilityUse>> PlanAbilities = new();

        public CooldownPlan(Class @class, string name)
        {
            Class = @class;
            Name = name;
            foreach (var (aid, info) in AbilityDefinitions.Classes[@class].Abilities)
                if (info.IsPlannable)
                    PlanAbilities[aid.Raw] = new();
        }

        public CooldownPlan Clone()
        {
            var res = new CooldownPlan(Class, Name);
            foreach (var (k, vRes) in res.PlanAbilities)
                foreach (var vSrc in PlanAbilities[k])
                    vRes.Add(vSrc.Clone());
            return res;
        }
    }
}
