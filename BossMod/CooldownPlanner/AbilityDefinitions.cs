using System;
using System.Collections.Generic;

namespace BossMod
{
    // TODO: this should be reworked/removed during planner refactoring
    public static class AbilityDefinitions
    {
        public class Track
        {
            public enum Category { SharedCooldown }

            public Category TrackCategory;
            public int CooldownGroup;
            public string Name = "";
        }

        public class Ability
        {
            public int CooldownTrack = 0;
            public ActionDefinition Definition;

            public bool IsGCD => Definition.CooldownGroup == CommonDefinitions.GCDGroup;
            public bool IsPlannable => Definition.Category != ActionCategory.None;

            public Ability(int track, ActionDefinition definition)
            {
                CooldownTrack = track;
                Definition = definition;
            }
        }

        public class Class
        {
            public List<Track> Tracks = new(); // track 0 is always normal GCD abilities
            public Dictionary<ActionID, Ability> Abilities = new();
            private Dictionary<int, int> _cooldownGroupToTrackIndex = new();

            public Class(Dictionary<ActionID, ActionDefinition> supportedActions, Type cdgType)
            {
                AddTrack(Track.Category.SharedCooldown, CommonDefinitions.GCDGroup, "GCD");
                foreach (var (action, data) in supportedActions)
                {
                    if (!_cooldownGroupToTrackIndex.ContainsKey(data.CooldownGroup))
                        AddTrack(Track.Category.SharedCooldown, data.CooldownGroup, cdgType.GetEnumName(data.CooldownGroup) ?? $"Group {data.CooldownGroup}");

                    var track = _cooldownGroupToTrackIndex[data.CooldownGroup];
                    Abilities[action] = new(track, data);
                }
            }

            public int AddTrack(Track.Category category, int cooldownGroup, string name)
            {
                int index = Tracks.Count;
                Tracks.Add(new() { TrackCategory = category, CooldownGroup = cooldownGroup, Name = name });
                _cooldownGroupToTrackIndex[cooldownGroup] = index;
                return index;
            }
        }

        public static Dictionary<BossMod.Class, Class> Classes = new();

        static AbilityDefinitions()
        {
            Classes[BossMod.Class.WAR] = new(WAR.Definitions.SupportedActions, typeof(WAR.CDGroup));
            Classes[BossMod.Class.PLD] = new(PLD.Definitions.SupportedActions, typeof(PLD.CDGroup));
            Classes[BossMod.Class.WHM] = new(WHM.Definitions.SupportedActions, typeof(WHM.CDGroup));
            Classes[BossMod.Class.SCH] = new(SCH.Definitions.SupportedActions, typeof(SCH.CDGroup));
            Classes[BossMod.Class.DRG] = new(DRG.Definitions.SupportedActions, typeof(DRG.CDGroup));
            Classes[BossMod.Class.MNK] = new(MNK.Definitions.SupportedActions, typeof(MNK.CDGroup));
            Classes[BossMod.Class.BRD] = new(BRD.Definitions.SupportedActions, typeof(BRD.CDGroup));
            Classes[BossMod.Class.BLM] = new(BLM.Definitions.SupportedActions, typeof(BLM.CDGroup));
        }
    }
}
