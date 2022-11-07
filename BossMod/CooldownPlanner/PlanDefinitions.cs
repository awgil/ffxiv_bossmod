using System;
using System.Collections.Generic;

namespace BossMod
{
    public static class PlanDefinitions
    {
        public class CooldownTrack
        {
            public string Name;
            public (ActionID aid, int minLevel)[] Actions;

            public CooldownTrack(string name, (ActionID aid, int minLevel)[] actions)
            {
                Name = name;
                Actions = actions;
            }

            public CooldownTrack(string name, ActionID aid, int minLevel)
            {
                Name = name;
                Actions = new[] { (aid, minLevel) };
            }
        }

        public class ClassData
        {
            public Type AIDType;
            public Dictionary<ActionID, ActionDefinition> Abilities;
            public List<CooldownTrack> CooldownTracks = new();

            public ClassData(Type aidType, Dictionary<ActionID, ActionDefinition> supportedActions)
            {
                AIDType = aidType;
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
            var c = new ClassData(typeof(WAR.AID), WAR.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("Vengeance", ActionID.MakeSpell(WAR.AID.Vengeance), 38));
            c.CooldownTracks.Add(new("Rampart", ActionID.MakeSpell(WAR.AID.Rampart), 8));
            c.CooldownTracks.Add(new("Thrill", ActionID.MakeSpell(WAR.AID.ThrillOfBattle), 30));
            c.CooldownTracks.Add(new("Holmgang", ActionID.MakeSpell(WAR.AID.Holmgang), 42));
            c.CooldownTracks.Add(new("Bloodwhetting", new[] { (ActionID.MakeSpell(WAR.AID.Bloodwhetting), 82), (ActionID.MakeSpell(WAR.AID.RawIntuition), 56), (ActionID.MakeSpell(WAR.AID.NascentFlash), 76) }));
            c.CooldownTracks.Add(new("Equilibrium", ActionID.MakeSpell(WAR.AID.Equilibrium), 58));
            c.CooldownTracks.Add(new("ArmsLength", ActionID.MakeSpell(WAR.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("Reprisal", ActionID.MakeSpell(WAR.AID.Reprisal), 22));
            c.CooldownTracks.Add(new("ShakeItOff", ActionID.MakeSpell(WAR.AID.ShakeItOff), 68));
            return c;
        }

        private static ClassData DefinePLD()
        {
            var c = new ClassData(typeof(PLD.AID), PLD.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("Sentinel", ActionID.MakeSpell(PLD.AID.Sentinel), 38));
            c.CooldownTracks.Add(new("Rampart", ActionID.MakeSpell(PLD.AID.Rampart), 8));
            c.CooldownTracks.Add(new("HallowedGround", ActionID.MakeSpell(PLD.AID.HallowedGround), 30));
            c.CooldownTracks.Add(new("Sheltron", ActionID.MakeSpell(PLD.AID.Sheltron), 35));
            c.CooldownTracks.Add(new("ArmsLength", ActionID.MakeSpell(PLD.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("Reprisal", ActionID.MakeSpell(PLD.AID.Reprisal), 22));
            return c;
        }

        private static ClassData DefineWHM()
        {
            var c = new ClassData(typeof(WHM.AID), WHM.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineSCH()
        {
            var c = new ClassData(typeof(SCH.AID), SCH.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineDRG()
        {
            var c = new ClassData(typeof(DRG.AID), DRG.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineMNK()
        {
            var c = new ClassData(typeof(MNK.AID), MNK.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineBRD()
        {
            var c = new ClassData(typeof(BRD.AID), BRD.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineBLM()
        {
            var c = new ClassData(typeof(BLM.AID), BLM.Definitions.SupportedActions);
            return c;
        }
    }
}
