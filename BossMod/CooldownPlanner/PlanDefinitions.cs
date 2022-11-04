using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public static class PlanDefinitions
    {
        public class CooldownTrack
        {
            public string Name;
            public ActionID[] AIDs;

            public CooldownTrack(string name, ActionID[] aids)
            {
                Name = name;
                AIDs = aids;
            }
        }

        public class ClassData
        {
            public Dictionary<ActionID, ActionDefinition> Abilities;
            public List<CooldownTrack> CooldownTracks = new();

            public ClassData(Dictionary<ActionID, ActionDefinition> supportedActions)
            {
                Abilities = supportedActions;
            }
        }

        public static Dictionary<Class, ClassData> Classes = new();

        static PlanDefinitions()
        {
            Classes[Class.WAR] = DefineWAR();
            Classes[Class.PLD] = DefinePLD();
            Classes[Class.WHM] = DefineWHM();
            Classes[Class.SCH] = DefineSCH();
            Classes[Class.DRG] = DefineDRG();
            Classes[Class.MNK] = DefineMNK();
            Classes[Class.BRD] = DefineBRD();
            Classes[Class.BLM] = DefineBLM();
        }

        private static ClassData DefineWAR()
        {
            var c = new ClassData(WAR.Definitions.SupportedActions);
            c.CooldownTracks.Add(DefineTrack(c.Abilities, WAR.CDGroup.Vengeance));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, WAR.CDGroup.Rampart));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, WAR.CDGroup.ThrillOfBattle));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, WAR.CDGroup.Holmgang));
            c.CooldownTracks.Add(new("Bloodwhetting", new[] { ActionID.MakeSpell(WAR.AID.RawIntuition) })); // TODO: others...
            c.CooldownTracks.Add(DefineTrack(c.Abilities, WAR.CDGroup.Equilibrium));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, WAR.CDGroup.ArmsLength));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, WAR.CDGroup.Reprisal));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, WAR.CDGroup.ShakeItOff));
            return c;
        }

        private static ClassData DefinePLD()
        {
            var c = new ClassData(PLD.Definitions.SupportedActions);
            c.CooldownTracks.Add(DefineTrack(c.Abilities, PLD.CDGroup.Sentinel));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, PLD.CDGroup.Rampart));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, PLD.CDGroup.HallowedGround));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, PLD.CDGroup.Sheltron));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, PLD.CDGroup.ArmsLength));
            c.CooldownTracks.Add(DefineTrack(c.Abilities, PLD.CDGroup.Reprisal));
            return c;
        }

        private static ClassData DefineWHM()
        {
            var c = new ClassData(WHM.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineSCH()
        {
            var c = new ClassData(SCH.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineDRG()
        {
            var c = new ClassData(DRG.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineMNK()
        {
            var c = new ClassData(MNK.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineBRD()
        {
            var c = new ClassData(BRD.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineBLM()
        {
            var c = new ClassData(BLM.Definitions.SupportedActions);
            return c;
        }

        private static CooldownTrack DefineTrack<CDG>(Dictionary<ActionID, ActionDefinition> actions, CDG group) where CDG : Enum
        {
            var igroup = (int)(object)group;
            return new(typeof(CDG).GetEnumName(group)!, actions.Where(kv => kv.Value.CooldownGroup == igroup).Select(kv => kv.Key).ToArray());
        }
    }
}
