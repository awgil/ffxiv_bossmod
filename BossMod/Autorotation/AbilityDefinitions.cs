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
            public float CastTime = 0;
            public float AnimLock = 0.6f;
            public int Charges = 1;
            public float Cooldown = 0; // cooldown for a single charge, for multi-charge abilities; 0 for gcd abilities
            public float EffectDuration;
            public ActionCategory Category; // category for planning, none if ability is non-plannable

            public bool IsGCD => Cooldown == 0;
            public bool IsPlannable => Category != ActionCategory.None;
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
                    Abilities[action] = new() { CooldownTrack = track, AnimLock = data.AnimationLock, Charges = data.MaxChargesAtCap, Cooldown = data.Cooldown, EffectDuration = data.EffectDuration, Category = data.Category };
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
        }
    }
}
