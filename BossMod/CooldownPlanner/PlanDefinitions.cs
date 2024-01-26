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

        public class StrategyTrack
        {
            public string Name;
            public Type? Values;
            public float Cooldown;

            public StrategyTrack(string name, Type? values = null, float cooldown = 0)
            {
                Name = name;
                Values = values;
                Cooldown = cooldown;
            }
        }

        public class ClassData
        {
            public Type AIDType;
            public Dictionary<ActionID, ActionDefinition> Abilities;
            public List<CooldownTrack> CooldownTracks = new();
            public List<StrategyTrack> StrategyTracks = new();

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
            Classes[Class.DNC] = DefineDNC();
            Classes[Class.BLM] = DefineBLM();
            Classes[Class.RPR] = DefineRPR();
            Classes[Class.GNB] = DefineGNB();
            Classes[Class.SAM] = DefineSAM();
        }

        private static ClassData DefineWAR()
        {
            var c = new ClassData(typeof(WAR.AID), WAR.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("Veng", ActionID.MakeSpell(WAR.AID.Vengeance), 38));
            c.CooldownTracks.Add(new("Rampart", ActionID.MakeSpell(WAR.AID.Rampart), 8));
            c.CooldownTracks.Add(new("Thrill", ActionID.MakeSpell(WAR.AID.ThrillOfBattle), 30));
            c.CooldownTracks.Add(new("Holmgang", ActionID.MakeSpell(WAR.AID.Holmgang), 42));
            c.CooldownTracks.Add(new("BW", new[] { (ActionID.MakeSpell(WAR.AID.Bloodwhetting), 82), (ActionID.MakeSpell(WAR.AID.RawIntuition), 56), (ActionID.MakeSpell(WAR.AID.NascentFlash), 76) }));
            c.CooldownTracks.Add(new("Equi", ActionID.MakeSpell(WAR.AID.Equilibrium), 58));
            c.CooldownTracks.Add(new("ArmsL", ActionID.MakeSpell(WAR.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("Reprisal", ActionID.MakeSpell(WAR.AID.Reprisal), 22));
            c.CooldownTracks.Add(new("SIO", ActionID.MakeSpell(WAR.AID.ShakeItOff), 68));
            c.CooldownTracks.Add(new("Taunt", ActionID.MakeSpell(WAR.AID.Provoke), 15));
            c.CooldownTracks.Add(new("Shirk", ActionID.MakeSpell(WAR.AID.Shirk), 48));
            c.CooldownTracks.Add(new("Sprint", CommonDefinitions.IDSprint, 1));
            c.StrategyTracks.Add(new("Gauge", typeof(WAR.Rotation.Strategy.GaugeUse)));
            c.StrategyTracks.Add(new("Infuriate", typeof(WAR.Rotation.Strategy.InfuriateUse)));
            c.StrategyTracks.Add(new("Potion", typeof(WAR.Rotation.Strategy.PotionUse), 270));
            c.StrategyTracks.Add(new("IR", typeof(WAR.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Upheaval", typeof(WAR.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("PR", typeof(WAR.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Onslaught", typeof(WAR.Rotation.Strategy.OnslaughtUse)));
            c.StrategyTracks.Add(new("Special", typeof(WAR.Rotation.Strategy.SpecialAction)));
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
            c.CooldownTracks.Add(new("Feint", ActionID.MakeSpell(DRG.AID.Feint), 22));
            c.CooldownTracks.Add(new("ArmsL", ActionID.MakeSpell(DRG.AID.ArmsLength), 32));
            c.StrategyTracks.Add(new("TrueN", typeof(DRG.Rotation.Strategy.TrueNorthUse)));
            c.StrategyTracks.Add(new("SpineShatter", typeof(DRG.Rotation.Strategy.SpineShatteruse)));
            return c;
        }

        private static ClassData DefineMNK()
        {
            var c = new ClassData(typeof(MNK.AID), MNK.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("Feint", ActionID.MakeSpell(MNK.AID.Feint), 22));
            c.CooldownTracks.Add(new("ArmsL", ActionID.MakeSpell(MNK.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("RoE", ActionID.MakeSpell(MNK.AID.RiddleOfEarth), 64));
            c.CooldownTracks.Add(new("Mantra", ActionID.MakeSpell(MNK.AID.Mantra), 42));
            c.StrategyTracks.Add(new("Dash", typeof(MNK.Rotation.Strategy.DashStrategy)));
            c.StrategyTracks.Add(new("TrueN", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Nadi", typeof(MNK.Rotation.Strategy.NadiChoice)));
            c.StrategyTracks.Add(new("RoF", typeof(MNK.Rotation.Strategy.FireStrategy)));
            c.StrategyTracks.Add(new("RoW", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("BHood", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(
                new("PerfBal", typeof(CommonRotation.Strategy.OffensiveAbilityUse))
            );
            c.StrategyTracks.Add(new("SSS", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            return c;
        }

        private static ClassData DefineBRD()
        {
            var c = new ClassData(typeof(BRD.AID), BRD.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("Troub", ActionID.MakeSpell(BRD.AID.Troubadour), 62));
            c.CooldownTracks.Add(new("Minne", ActionID.MakeSpell(BRD.AID.NaturesMinne), 66));
            c.CooldownTracks.Add(new("ArmsL", ActionID.MakeSpell(BRD.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("Sprint", CommonDefinitions.IDSprint, 1));
            c.StrategyTracks.Add(new("Songs", typeof(BRD.Rotation.Strategy.SongUse)));
            c.StrategyTracks.Add(new("Potion", typeof(BRD.Rotation.Strategy.PotionUse), 270));
            c.StrategyTracks.Add(new("DOTs", typeof(BRD.Rotation.Strategy.DotUse)));
            c.StrategyTracks.Add(new("Apex", typeof(BRD.Rotation.Strategy.ApexArrowUse)));
            c.StrategyTracks.Add(new("Blast", typeof(BRD.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("RS", typeof(BRD.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("BL", typeof(BRD.Rotation.Strategy.BloodletterUse)));
            c.StrategyTracks.Add(new("EA", typeof(BRD.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Barrage", typeof(BRD.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("SW", typeof(BRD.Rotation.Strategy.OffensiveAbilityUse)));
            return c;
        }

        private static ClassData DefineDNC()
        {
            var c = new ClassData(typeof(DNC.AID), DNC.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("Samba", ActionID.MakeSpell(DNC.AID.ShieldSamba), 56));
            c.CooldownTracks.Add(new("Waltz", ActionID.MakeSpell(DNC.AID.CuringWaltz), 52));
            c.CooldownTracks.Add(new("Improv", ActionID.MakeSpell(DNC.AID.Improvisation), 80));
            c.CooldownTracks.Add(new("ArmsL", ActionID.MakeSpell(BRD.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("Sprint", CommonDefinitions.IDSprint, 1));
            c.StrategyTracks.Add(new("Gauge", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Feather", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("TechStep", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("StdStep", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            return c;
        }

        private static ClassData DefineBLM()
        {
            var c = new ClassData(typeof(BLM.AID), BLM.Definitions.SupportedActions);
            return c;
        }

        private static ClassData DefineRPR()
        {
            var c = new ClassData(typeof(RPR.AID), RPR.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("ACrest", ActionID.MakeSpell(RPR.AID.ArcaneCrest), 40));
            c.CooldownTracks.Add(new("Feint", ActionID.MakeSpell(RPR.AID.Feint), 22));
            c.CooldownTracks.Add(new("ArmsL", ActionID.MakeSpell(RPR.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("Sprint", CommonDefinitions.IDSprint, 1));
            c.StrategyTracks.Add(new("Gauge", typeof(RPR.Rotation.Strategy.GaugeUse)));
            c.StrategyTracks.Add(new("SOUL", typeof(RPR.Rotation.Strategy.BloodstalkUse)));
            c.StrategyTracks.Add(new("SS", typeof(RPR.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("TrN", typeof(RPR.Rotation.Strategy.TrueNorthUse)));
            c.StrategyTracks.Add(new("ENSH", typeof(RPR.Rotation.Strategy.EnshroudUse)));
            c.StrategyTracks.Add(new("ARC", typeof(RPR.Rotation.Strategy.ArcaneCircleUse)));
            c.StrategyTracks.Add(new("Glut", typeof(RPR.Rotation.Strategy.GluttonyUse), 60));
            c.StrategyTracks.Add(new("Potion", typeof(RPR.Rotation.Strategy.PotionUse), 270));
            c.StrategyTracks.Add(new("spec", typeof(RPR.Rotation.Strategy.SpecialAction)));
            return c;
        }

        private static ClassData DefineSAM()
        {
            var c = new ClassData(typeof(SAM.AID), SAM.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("ThirdEye", ActionID.MakeSpell(SAM.AID.ThirdEye), 6));
            c.CooldownTracks.Add(new("Feint", ActionID.MakeSpell(SAM.AID.Feint), 22));
            c.CooldownTracks.Add(new("ArmsL", ActionID.MakeSpell(SAM.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("Sprint", CommonDefinitions.IDSprint, 1));
            c.StrategyTracks.Add(new("TrueN", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Cast", typeof(CommonRotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Higanbana", typeof(SAM.Rotation.Strategy.HiganbanaUse)));
            c.StrategyTracks.Add(new("Meikyo", typeof(SAM.Rotation.Strategy.MeikyoUse)));
            c.StrategyTracks.Add(new("Dash", typeof(SAM.Rotation.Strategy.DashUse)));
            c.StrategyTracks.Add(new("Enpi", typeof(SAM.Rotation.Strategy.EnpiUse)));
            c.StrategyTracks.Add(new("Kenki", typeof(SAM.Rotation.Strategy.KenkiUse)));
            return c;
        }

        private static ClassData DefineGNB()
        {
            var c = new ClassData(typeof(GNB.AID), GNB.Definitions.SupportedActions);
            c.CooldownTracks.Add(new("Nebula", ActionID.MakeSpell(GNB.AID.Nebula), 38));
            c.CooldownTracks.Add(new("Rampart", ActionID.MakeSpell(GNB.AID.Rampart), 8));
            c.CooldownTracks.Add(new("Camoufl", ActionID.MakeSpell(GNB.AID.Camouflage), 6));
            c.CooldownTracks.Add(new("Bolide", ActionID.MakeSpell(GNB.AID.Superbolide), 50));
            c.CooldownTracks.Add(new("HOC", new[] { (ActionID.MakeSpell(GNB.AID.HeartOfCorundum), 82), (ActionID.MakeSpell(GNB.AID.HeartOfStone), 68) }));
            c.CooldownTracks.Add(new("Aurora", ActionID.MakeSpell(GNB.AID.Aurora), 45));
            c.CooldownTracks.Add(new("ArmsL", ActionID.MakeSpell(GNB.AID.ArmsLength), 32));
            c.CooldownTracks.Add(new("Reprisal", ActionID.MakeSpell(GNB.AID.Reprisal), 22));
            c.CooldownTracks.Add(new("HoL", ActionID.MakeSpell(GNB.AID.HeartOfLight), 64));
            c.CooldownTracks.Add(new("Taunt", ActionID.MakeSpell(GNB.AID.Provoke), 15));
            c.CooldownTracks.Add(new("Shirk", ActionID.MakeSpell(GNB.AID.Shirk), 48));
            c.CooldownTracks.Add(new("Sprint", CommonDefinitions.IDSprint, 1));
            c.StrategyTracks.Add(new("Gauge", typeof(GNB.Rotation.Strategy.GaugeUse)));
            c.StrategyTracks.Add(new("Potion", typeof(GNB.Rotation.Strategy.PotionUse), 270));
            c.StrategyTracks.Add(new("NoM", typeof(GNB.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Fest", typeof(GNB.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Gnash", typeof(GNB.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("Zone", typeof(GNB.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("BowS", typeof(GNB.Rotation.Strategy.OffensiveAbilityUse)));
            c.StrategyTracks.Add(new("RD", typeof(GNB.Rotation.Strategy.RoughDivideUse)));
            c.StrategyTracks.Add(new("Special", typeof(GNB.Rotation.Strategy.SpecialAction)));
            return c;
        }
    }
}
