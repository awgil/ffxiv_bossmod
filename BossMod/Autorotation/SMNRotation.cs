using System.Text;

namespace BossMod
{
    public static class SMNRotation
    {
        public enum AID : uint
        {
            None = 0,

            // single target GCDs
            Ruin1 = 163,
            Ruin2 = 172,
            Ruin3 = 3579,
            Ruin4 = 7426,
            RubyRuin1 = 25808,
            RubyRuin2 = 25811,
            RubyRuin3 = 25817,
            RubyRite = 25823,
            CrimsonCyclone = 25835,
            CrimsonStrike = 25885,
            TopazRuin1 = 25809,
            TopazRuin2 = 25812,
            TopazRuin3 = 25818,
            TopazRite = 25824,
            MountainBuster = 25836,
            EmeraldRuin1 = 25810,
            EmeraldRuin2 = 25813,
            EmeraldRuin3 = 25819,
            EmeraldRite = 25825,
            Slipstream = 25837,
            AstralImpulse = 25820,
            FountainOfFire = 16514,
            ScarletFlame = 16519,

            // aoe GCDs
            Outburst = 16511,
            TriDisaster = 25826,
            RubyOutburst = 25814,
            RubyDisaster = 25827,
            RubyCatastrophe = 25832,
            TopazOutburst = 25815,
            TopazDisaster = 25828,
            TopazCatastrophe = 25833,
            EmeraldOutburst = 25816,
            EmeraldDisaster = 25829,
            EmeraldCatastrophe = 25834,
            AstralFlare = 25821,
            BrandOfPurgatory = 16515,

            // attunement placeholders
            Gemshine = 25883,
            PreciousBrilliance = 25884,
            AstralFlow = 25822,

            // summons / stances
            SummonCarbuncle = 25798,
            SummonRuby = 25802,
            SummonIfrit1 = 25805,
            SummonIfrit2 = 25838,
            SummonTopaz = 25803,
            SummonTitan1 = 25806,
            SummonTitan2 = 25839,
            SummonEmerald = 25804,
            SummonGaruda1 = 25807,
            SummonGaruda2 = 25840,
            Aethercharge = 25800,
            DreadwyrmTrance = 3581,
            SummonBahamut = 7427,
            SummonPhoenix = 25831,

            // oGCDs
            EnergyDrain = 16508,
            EnergySiphon = 16510,
            Fester = 181,
            Painflare = 3578,
            Deathflare = 3582,
            EnkindleBahamut = 7429,
            EnkindlePhoenix = 16516,

            // single-target heal GCDs
            Physick = 16230,

            // offsensive CDs
            SearingLight = 25801,
            Swiftcast = 7561,
            LucidDreaming = 7562,

            // defensive CDs
            RadiantAegis = 25799,
            Rekindle = 25830,
            Addle = 7560,
            Surecast = 7559,

            // misc
            Resurrection = 173,
            Sleep = 25880,

            // pet abilities
            PetRadiantAegis = 25841,
            PetWyrmwave = 7428,
            PetAkhMorn = 7449,
            PetEverlastingFlight = 16517,
            PetRevelation = 16518,
            PetGlitteringRuby = 25843,
            PetGlitteringTopaz = 25844,
            PetGlitteringEmerald = 25845,
            PetBurningStrike = 25846,
            PetRockBuster = 25847,
            PetAerialSlash = 25848,
            PetInferno1 = 25849,
            PetEarthenFury1 = 25850,
            PetAerialBlast1 = 25851,
            PetInferno2 = 25852,
            PetEarthenFury2 = 25853,
            PetAerialBlast2 = 25854,
        }
        public static ActionID IDStatPotion = new(ActionType.Item, 1000000); // hq grade 6 tincture of ???

        public enum SID : uint
        {
            None = 0,
            Swiftcast = 167,
        }

        public enum Attunement { None, Ifrit, Titan, Garuda }

        // full state needed for determining next action
        public class State : CommonRotation.State
        {
            public bool PetSummoned;
            public bool IfritReady;
            public bool TitanReady;
            public bool GarudaReady;
            public Attunement Attunement;
            public int AttunementStacks;
            public float AttunementLeft;
            public float SummonLockLeft;
            public int AetherflowStacks; // 0-2
            public float SwiftcastLeft; // 0 if buff not up, max 10
            public float AetherchargeCD; // 60 max, 0 if ready
            public float EnergyDrainCD; // 60 max, 0 if ready
            public float RadiantAegisCD;
            public float AddleCD; // 90 max, 0 if ready
            public float SwiftcastCD; // 60 max, 0 if ready
            public float SurecastCD; // 120 max, 0 if ready
            public float LucidDreamingCD; // 60 max, 0 if ready

            // per-level ability unlocks (TODO: consider abilities unlocked by quests - they could be unavailable despite level being high enough)
            public bool UnlockedSummonCarbuncle => Level >= 2; // includes radiant aegis
            public bool UnlockedPhysick => Level >= 4;
            public bool UnlockedSummonRuby => Level >= 6; // includes aethercharge, gemshine and ruby ruin
            public bool UnlockedAddle => Level >= 8;
            public bool UnlockedSleep => Level >= 10;
            public bool UnlockedEnergyDrain => Level >= 10; // includes fester
            public bool UnlockedResurrection => Level >= 12;
            public bool UnlockedLucidDreaming => Level >= 14;
            public bool UnlockedSummonTopaz => Level >= 15; // quest-locked, includes topaz ruin
            public bool UnlockedSwiftcast => Level >= 18;
            public bool UnlockedSummonEmerald => Level >= 22; // includes emerald ruin
            public bool UnlockedOutburst => Level >= 26; // includes precious brilliance + elemental outbursts
            public bool UnlockedRuin2 => Level >= 30; // quest-locked, includes elemental variants
            public bool UnlockedSummonIfrit => Level >= 30;
            public bool UnlockedSummonTitan => Level >= 35; // quest-locked
            public bool UnlockedPainflare => Level >= 40; // quest-locked
            public bool UnlockedSurecast => Level >= 44;
            public bool UnlockedSummonGaruda => Level >= 45; // quest-locked
            public bool UnlockedEnergySiphon => Level >= 52; // quest-locked
            public bool UnlockedRuin3 => Level >= 54; // quest-locked, includes elemental variants
            public bool UnlockedDreadwyrmTrance => Level >= 58; // quest-locked, includes astral impulse and astral flare
            public bool UnlockedAstralFlow => Level >= 60; // quest-locked, includes deathflare
            // TODO: L62+

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, Att={Attunement}/{AttunementStacks}/{AttunementLeft:f1}, SummLock={SummonLockLeft:f1}, IfritR={IfritReady}, TitanR={TitanReady}, GarudaR={GarudaReady}, Aetherflow={AetherflowStacks}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}, moving={Moving}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            // cooldowns
            public bool ExecuteRadiantAegis;
            public bool ExecuteSwiftcast;
            public bool ExecuteSurecast;
            public bool ExecuteAddle;

            public override string ToString()
            {
                var sb = new StringBuilder("SmartQueue:");
                if (ExecuteRadiantAegis)
                    sb.Append(" RadiantAegis");
                if (ExecuteSurecast)
                    sb.Append(" Surecast");
                if (ExecuteAddle)
                    sb.Append(" Addle");
                if (ExecuteSwiftcast)
                    sb.Append(" Swiftcast");
                if (ExecuteSprint)
                    sb.Append(" Sprint");
                return sb.ToString();
            }
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float windowEnd, bool aoe)
        {
            // 1. use cooldowns if requested in rough priority order
            if (strategy.ExecuteRadiantAegis && state.UnlockedSummonCarbuncle && state.CanWeave(state.RadiantAegisCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.RadiantAegis);
            if (strategy.ExecuteSurecast && state.UnlockedSurecast && state.CanWeave(state.SurecastCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Surecast);
            if (strategy.ExecuteAddle && state.UnlockedAddle && state.CanWeave(state.AddleCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Addle);
            if (strategy.ExecuteSwiftcast && state.UnlockedSwiftcast && state.CanWeave(state.SwiftcastCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Swiftcast);
            if (strategy.ExecuteSprint && state.CanWeave(state.SprintCD, 0.6f, windowEnd))
                return CommonRotation.IDSprint;

            // TODO: reconsider priorities, this kinda works at low level
            if (state.UnlockedEnergyDrain && state.AetherflowStacks == 0 && state.CanWeave(state.EnergyDrainCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.EnergyDrain);

            if (state.UnlockedEnergyDrain && state.AetherflowStacks > 0)
                return ActionID.MakeSpell(AID.Fester);

            return new();
        }

        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
        {
            // make sure pet is summoned
            if (!state.PetSummoned && state.UnlockedSummonCarbuncle)
                return AID.SummonCarbuncle;

            if (state.AttunementLeft > state.GCD)
            {
                AID action;
                if (aoe && state.UnlockedOutburst)
                {
                    action = state.Attunement switch
                    {
                        Attunement.Ifrit => AID.RubyOutburst,
                        Attunement.Titan => AID.TopazOutburst,
                        Attunement.Garuda => AID.EmeraldOutburst,
                        _ => AID.None
                    };
                }
                else
                {
                    action = state.Attunement switch
                    {
                        Attunement.Ifrit => state.UnlockedRuin2 ? AID.RubyRuin2 : AID.RubyRuin1,
                        Attunement.Titan => state.UnlockedRuin2 ? AID.TopazRuin2 : AID.TopazRuin1,
                        Attunement.Garuda => state.UnlockedRuin2 ? AID.EmeraldRuin2 : AID.EmeraldRuin1,
                        _ => AID.None
                    };
                }
                if (action != AID.None)
                    return action;
            }

            if (state.UnlockedSummonRuby && state.Attunement == Attunement.None && !state.IfritReady && !state.TitanReady && !state.GarudaReady && state.AetherchargeCD <= state.GCD)
                return AID.Aethercharge;

            if (state.SummonLockLeft <= state.GCD)
            {
                if (state.TitanReady && state.UnlockedSummonTopaz)
                    return AID.SummonTopaz;
                if (state.IfritReady && state.UnlockedSummonRuby)
                    return AID.SummonRuby;
                if (state.GarudaReady && state.UnlockedSummonEmerald)
                    return AID.SummonEmerald;
            }

            return state.UnlockedRuin2 ? AID.Ruin2 : AID.Ruin1;
        }

        public static ActionID GetNextBestAction(State state, Strategy strategy, bool aoe)
        {
            ActionID res = new();
            if (state.CanDoubleWeave) // first ogcd slot
                res = GetNextBestOGCD(state, strategy, state.DoubleWeaveWindowEnd, aoe);
            if (!res && state.CanSingleWeave) // second/only ogcd slot
                res = GetNextBestOGCD(state, strategy, state.GCD, aoe);
            if (!res) // gcd
                res = ActionID.MakeSpell(GetNextBestGCD(state, strategy, aoe));
            return res;
        }

        // short string for supported action
        public static string ActionShortString(ActionID action)
        {
            return action == CommonRotation.IDSprint ? "Sprint" : action == IDStatPotion ? "StatPotion" : ((AID)action.ID).ToString();
        }
    }
}
