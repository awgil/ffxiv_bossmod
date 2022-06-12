using System;
using System.Collections.Generic;

namespace BossMod
{
    public static class AbilityDefinitions
    {
        public class Track
        {
            public enum Category { SharedCooldown }

            public Category TrackCategory;
            public string Name = "";
        }

        public class Ability
        {
            public enum Category { None, SelfMitigation, RaidMitigation }

            public int CooldownTrack = 0;
            public float CastTime = 0;
            public float AnimLock = 0.6f;
            public int Charges = 1;
            public float Cooldown = 0; // cooldown for a single charge, for multi-charge abilities; 0 for gcd abilities
            public float EffectDuration;
            public Category AbilityCategory; // category for planning, none if ability is non-plannable

            public bool IsGCD => Cooldown == 0;
            public bool IsPlannable => AbilityCategory != Category.None;
        }

        public class Class
        {
            public List<Track> Tracks = new(); // track 0 is always normal GCD abilities
            public Dictionary<ActionID, Ability> Abilities = new();

            public Class()
            {
                AddTrack(Track.Category.SharedCooldown, "GCD");
            }

            public int AddTrack(Track.Category category, string name)
            {
                Tracks.Add(new() { TrackCategory = category, Name = name });
                return Tracks.Count - 1;
            }

            public Ability AddSpell<AID>(AID aid, int track, float cooldown, float effectDuration = 0, Ability.Category category = Ability.Category.None) where AID : Enum
            {
                return Abilities[ActionID.MakeSpell(aid)] = new() { CooldownTrack = track, Cooldown = cooldown, EffectDuration = effectDuration, AbilityCategory = category };
            }

            public Ability AddGCDSpell<AID>(AID aid) where AID : Enum
            {
                return AddSpell(aid, 0, 2.5f);
            }

            public Ability AddCooldownTrackAndSpell<AID>(AID aid, float cooldown, float effectDuration = 0, Ability.Category category = Ability.Category.None) where AID : Enum
            {
                var track = AddTrack(Track.Category.SharedCooldown, aid.ToString());
                return AddSpell(aid, track, cooldown, effectDuration, category);
            }

            public void AddSharedCooldownSpells<AID>(IEnumerable<AID> aids, string name, float cooldown, float effectDuration = 0, Ability.Category category = Ability.Category.None) where AID : Enum
            {
                int track = AddTrack(Track.Category.SharedCooldown, name);
                foreach (var aid in aids)
                    AddSpell(aid, track, cooldown, effectDuration, category);
            }
        }

        public static Dictionary<BossMod.Class, Class> Classes = new();

        static AbilityDefinitions()
        {
            Classes[BossMod.Class.WAR] = WARRotation.BuildDefinitions();
        }
    }
}
