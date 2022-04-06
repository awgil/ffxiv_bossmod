using System;
using System.Text;

namespace BossMod
{
    public class WHMRotation
    {
        public enum AID : uint
        {
            None = 0,

            // single-target damage GCDs
            Stone1 = 119,
            Stone2 = 127,
            Stone3 = 3568,
            Stone4 = 7431,
            Glare1 = 16533,
            Glare3 = 25859,
            Aero1 = 121,
            Aero2 = 132,
            Dia = 16532,
            AfflatusMisery = 16535,

            // aoe damage GCDs
            Holy1 = 139,
            Holy3 = 25860,

            // single-target heal GCDs
            Cure1 = 120,
            Cure2 = 135,
            Regen = 137,
            AfflatusSolace = 16531,

            // aoe heal GCDs
            Medica1 = 124,
            Medica2 = 133,
            Cure3 = 131,
            AfflatusRapture = 16534,

            // oGCDs,
            Assize = 3571,
            Asylum = 3569,
            DivineBenison = 7432,
            Tetragrammaton = 3570,
            Benediction = 140,
            LiturgyOfTheBell = 25862,

            // buff CDs
            Swiftcast = 7561,
            LucidDreaming = 7562,
            PresenceOfMind = 136,
            ThinAir = 7430,
            PlenaryIndulgence = 7433,
            Temperance = 16536,
            Aquaveil = 25861,
            Surecast = 7559,

            // misc
            Raise = 125,
            Repose = 16560,
            Esuna = 7568,
            Rescue = 7571,
        }
        public static ActionID IDStatPotion = new(ActionType.Item, 1036113); // hq grade 6 tincture of mind

        public enum SID : uint
        {
            None = 0,
            Medica2 = 150,
            Freecure = 155,
            Swiftcast = 167,
            ThinAir = 1217,
            Dia = 1871,
        }

        // full state needed for determining next action
        public class State : CommonRotation.State
        {
            public int NormalLilies;
            public int BloodLilies;
            public float NextLilyIn; // max 30
            public float SwiftcastLeft; // 0 if buff not up, max 10
            public float ThinAirLeft; // 0 if buff not up, max 12
            public float FreecureLeft; // 0 if buff not up, max 15
            public float MedicaLeft; // 0 if hot not up, max 15
            public float TargetDiaLeft; // 0 if debuff not up, max 30
            public float AssizeCD; // 45 max, 0 if ready
            public float AsylumCD; // 90 max, 0 if ready
            public float DivineBenisonCD; // 60 max, >30 if 0 stacks ready, >0 if 1 stack ready, ==0 if 2 stacks ready
            public float TetragrammatonCD; // 60 max, 0 if ready
            public float BenedictionCD; // 180 max, 0 if ready
            public float LiturgyOfTheBellCD; // 180 max, 0 if ready
            public float SwiftcastCD; // 60 max, 0 if ready
            public float LucidDreamingCD; // 60 max, 0 if ready
            public float PresenceOfMindCD; // 150 max, 0 if ready
            public float ThinAirCD; // 120 max, >60 if 0 stacks ready, >0 if 1 stack ready, ==0 if 2 stacks ready
            public float PlenaryIndulgenceCD; // 60 max, 0 if ready
            public float TemperanceCD; // 120 max, 0 if ready
            public float AquaveilCD; // 60 max, 0 if ready
            public float SurecastCD; // 120 max, 0 if ready

            // per-level ability unlocks (TODO: consider abilities unlocked by quests - they could be unavailable despite level being high enough)
            public bool UnlockedCure1 => Level >= 2;
            public bool UnlockedAero1 => Level >= 4;
            public bool UnlockedRepose => Level >= 8;
            public bool UnlockedMedica1 => Level >= 10;
            public bool UnlockedEsuna => Level >= 10;
            public bool UnlockedRaise => Level >= 12;
            public bool UnlockedStone2 => Level >= 18;
            public bool UnlockedSwiftcast => Level >= 18;
            public bool UnlockedLucidDreaming => Level >= 24;
            public bool UnlockedCure2 => Level >= 30; // quest-locked
            public bool UnlockedPresenceOfMind => Level >= 30; // quest-locked
            public bool UnlockedFreecure => Level >= 32; // passive, cure1 can apply buff making next cure2 free
            public bool UnlockedRegen => Level >= 35; // quest-locked
            public bool UnlockedCure3 => Level >= 40; // quest-locked
            public bool UnlockedSurecast => Level >= 44;
            public bool UnlockedHoly1 => Level >= 45; // quest-locked
            public bool UnlockedAero2 => Level >= 46;
            public bool UnlockedRescue => Level >= 48;
            public bool UnlockedMedica2 => Level >= 50;
            public bool UnlockedBenediction => Level >= 50; // quest-locked
            public bool UnlockedAfflatusSolace => Level >= 52;
            public bool UnlockedAsylum => Level >= 52; // quest-locked
            public bool UnlockedStone3 => Level >= 54; // quest-locked
            public bool UnlockedAssize => Level >= 56; // quest-locked
            public bool UnlockedThinAir => Level >= 58;
            public bool UnlockedTetragrammaton => Level >= 60; // quest-locked
            public bool UnlockedStone4 => Level >= 64;
            public bool UnlockedDivineBenison => Level >= 66;
            public bool UnlockedPlenaryIndulgence => Level >= 70; // quest-locked
            public bool UnlockedDia => Level >= 72;
            public bool UnlockedGlare1 => Level >= 72;
            public bool UnlockedAfflatusMisery => Level >= 74;
            public bool UnlockedAfflatusRapture => Level >= 76;
            public bool UnlockedTemperance => Level >= 80;
            public bool UnlockedGlare3 => Level >= 82;
            public bool UnlockedHoly3 => Level >= 82;
            public bool UnlockedAquaveil => Level >= 86;
            public bool UnlockedEnhancedDivineBenison => Level >= 88; // passive, grants second charge to DB
            public bool UnlockedLiturgyOfTheBell => Level >= 90;

            // upgrade paths
            public AID BestDia => UnlockedDia ? AID.Dia : UnlockedAero2 ? AID.Aero2 : AID.Aero1;
            public AID BestGlare => UnlockedGlare3 ? AID.Glare3 : UnlockedGlare1 ? AID.Glare1 : UnlockedStone4 ? AID.Stone4 : UnlockedStone3 ? AID.Stone3 : UnlockedStone2 ? AID.Stone2 : AID.Stone1;
            public AID BestHoly => UnlockedHoly3 ? AID.Holy3 : AID.Holy1;

            // num lilies on next GCD
            public int NormalLiliesOnNextGCD => UnlockedAfflatusSolace ? Math.Min(NormalLilies + (NextLilyIn < GCD ? 1 : 0), 3) : 0;

            public override string ToString()
            {
                return $"g={NormalLilies}/{BloodLilies}/{NextLilyIn:f1}, MP={CurMP}, moving={Moving}, RB={RaidBuffsLeft:f1}, Dia={TargetDiaLeft:f1}, Medica={MedicaLeft:f1}, Swiftcast={SwiftcastLeft:f1}, TA={ThinAirLeft:f1}, Freecure={FreecureLeft:f1}, AssizeCD={AssizeCD:f1}, PoMCD={PresenceOfMindCD:f1}, LDCD={LucidDreamingCD:f1}, TACD={ThinAirCD:f1}, PlenCD={PlenaryIndulgenceCD:f1}, AsylumCD={AsylumCD:f1}, BeniCD={DivineBenisonCD:f1}, TetraCD={TetragrammatonCD:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        public class Strategy : CommonRotation.Strategy
        {
            public int NumMedica1Targets; // how many targets would medica1 heal (15y around self)
            public int NumRaptureMedica2Targets; // how many targets would rapture/medica2 heal (20y around self)
            public int NumCure3Targets; // how many targets cure3 would heal (6y around selected/best target)
            // cooldowns
            public bool ExecuteAssize; // TODO: consider auto use
            public bool ExecuteAsylum;
            public bool ExecuteDivineBenison;
            public bool ExecuteTetragrammaton;
            public bool ExecuteBenediction;
            public bool ExecuteLiturgyOfTheBell;
            public bool ExecuteSwiftcast;
            public bool ExecuteTemperance;
            public bool ExecuteAquaveil;
            public bool ExecuteSurecast;

            public override string ToString()
            {
                var sb = new StringBuilder($"M2={NumRaptureMedica2Targets}, C3={NumCure3Targets}, SmartQueue:");
                if (ExecuteBenediction)
                    sb.Append(" Benediction");
                if (ExecuteLiturgyOfTheBell)
                    sb.Append(" Liturgy");
                if (ExecuteSurecast)
                    sb.Append(" Surecast");
                if (ExecuteAssize)
                    sb.Append(" Assize");
                if (ExecuteAsylum)
                    sb.Append(" Asylum");
                if (ExecuteDivineBenison)
                    sb.Append(" DB");
                if (ExecuteTetragrammaton)
                    sb.Append(" Tetra");
                if (ExecuteSwiftcast)
                    sb.Append(" Swiftcast");
                if (ExecuteTemperance)
                    sb.Append(" Temperance");
                if (ExecuteAquaveil)
                    sb.Append(" Aquaveil");
                if (ExecuteSprint)
                    sb.Append(" Sprint");
                return sb.ToString();
            }
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float windowEnd, bool aoe, bool heal)
        {
            // 1. plenary indulgence, if we're gonna cast aoe gcd heal (TODO: reconsider priority)
            if (aoe && heal && state.UnlockedPlenaryIndulgence && state.CanWeave(state.PlenaryIndulgenceCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.PlenaryIndulgence);

            // 2. use cooldowns if requested in rough priority order
            if (strategy.ExecuteBenediction && state.UnlockedBenediction && state.CanWeave(state.BenedictionCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Benediction);
            if (strategy.ExecuteLiturgyOfTheBell && state.UnlockedLiturgyOfTheBell && state.CanWeave(state.LiturgyOfTheBellCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.LiturgyOfTheBell);
            if (strategy.ExecuteSurecast && state.UnlockedSurecast && state.CanWeave(state.SurecastCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Surecast);
            if (strategy.ExecuteAssize && state.UnlockedAssize && state.CanWeave(state.AssizeCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Assize);
            if (strategy.ExecuteAsylum && state.UnlockedAsylum && state.CanWeave(state.AsylumCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Asylum);
            if (strategy.ExecuteDivineBenison && state.UnlockedDivineBenison && state.CanWeave(state.DivineBenisonCD - (state.UnlockedEnhancedDivineBenison ? 30 : 0), 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.DivineBenison);
            if (strategy.ExecuteTetragrammaton && state.UnlockedTetragrammaton && state.CanWeave(state.TetragrammatonCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Tetragrammaton);
            if (strategy.ExecuteSwiftcast && state.UnlockedSwiftcast && state.CanWeave(state.SwiftcastCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Swiftcast);
            if (strategy.ExecuteTemperance && state.UnlockedTemperance && state.CanWeave(state.TemperanceCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Temperance);
            if (strategy.ExecuteAquaveil && state.UnlockedAquaveil && state.CanWeave(state.AquaveilCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Aquaveil);
            if (strategy.ExecuteSprint && state.CanWeave(state.SprintCD, 0.6f, windowEnd))
                return CommonRotation.IDSprint;

            // 3.potion (TODO)

            // 4. pom (TODO: consider delaying until raidbuffs?)
            if (state.UnlockedPresenceOfMind && state.CanWeave(state.PresenceOfMindCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.PresenceOfMind);

            // 5. lucid dreaming, if we won't waste mana (TODO: revise mp limit)
            if (state.CurMP < 7000 && state.UnlockedLucidDreaming && state.CanWeave(state.LucidDreamingCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.LucidDreaming);

            // 6. thin air, if we have some mana deficit and we're either sitting on 2 charges or we're gonna cast expensive spell (TODO: revise mp limit)
            if (state.CurMP < 9000 && state.UnlockedThinAir && state.CanWeave(state.ThinAirCD - 60, 0.6f, windowEnd))
            {
                if (state.ThinAirCD < state.GCD)
                    return ActionID.MakeSpell(AID.ThinAir); // spend second charge to start cooldown ticking, even if we gonna cast glare
                if (heal && state.NormalLiliesOnNextGCD == 0)
                    return ActionID.MakeSpell(AID.ThinAir); // spend last charge if we're gonna cast expensive GCD
            }

            return new();
        }

        public static AID GetNextBestSTDamageGCD(State state, Strategy strategy)
        {
            // 0. just use glare before pull
            if (strategy.Prepull)
                return state.BestGlare;

            // 1. refresh dia/aero, if time is less than 2.5 sec (TODO: verify threshold to make sure we don't miss ticks)
            if (state.UnlockedAero1 && state.TargetDiaLeft < state.GCD + 2.5f)
                return state.BestDia;

            // 2. glare, if not moving or if under swiftcast
            if (!state.Moving || state.SwiftcastLeft > state.GCD)
                return state.BestGlare;

            // 3. afflatus misery
            if (state.BloodLilies >= 3)
                return AID.AfflatusMisery;

            // 4. slidecast glare (TODO: consider early dia refresh if GCD is zero...)
            return state.BestGlare;
        }

        public static AID GetNextBestAOEDamageGCD(State state, Strategy strategy)
        {
            // misery > holy
            return state.BloodLilies >= 3 ? AID.AfflatusMisery : state.BestHoly;
        }

        public static AID GetNextBestSTHealGCD(State state, Strategy strategy)
        {
            // 1. cure 2, if free
            if (state.UnlockedCure2 && (state.FreecureLeft > state.GCD || state.ThinAirLeft > state.GCD))
                return AID.Cure2;

            // 2. afflatus solace, if possible
            if (state.NormalLiliesOnNextGCD > 0)
                return AID.AfflatusSolace;

            // 3. cure 2, if possible
            if (state.UnlockedCure2 && state.CurMP >= 1000)
                return AID.Cure2;

            // 4. fallback to cure1
            return AID.Cure1;
        }

        public static AID GetNextBestAOEHealGCD(State state, Strategy strategy)
        {
            // 1. afflatus rapture, if possible (TODO: consider full overheal for lilies, during downtime or to get last misery)
            bool canCastRapture = state.UnlockedAfflatusRapture && state.NormalLiliesOnNextGCD > 0;
            if (canCastRapture && strategy.NumRaptureMedica2Targets > 0)
                return AID.AfflatusRapture;

            // 2. medica 2, if possible and useful, and buff is not already up; we consider it ok to overwrite last tick
            bool canCastMedica2 = state.UnlockedMedica2 && (state.CurMP >= 1000 || state.ThinAirLeft > state.GCD);
            if (canCastMedica2 && strategy.NumRaptureMedica2Targets > 0 && state.MedicaLeft <= state.GCD + 2.5f)
                return AID.Medica2;

            // 3. cure 3, if possible and useful
            if (strategy.NumCure3Targets > 0 && state.UnlockedCure3 && (state.CurMP >= 1500 || state.ThinAirLeft > state.GCD))
                return AID.Cure3;

            // 4. medica 1, if possible and useful
            if (strategy.NumMedica1Targets > 0 && state.UnlockedMedica1 && (state.CurMP >= 900 || state.ThinAirLeft > state.GCD))
                return AID.Medica1;

            // 5. fallback: overheal medica 2 for hot (e.g. during downtime)
            if (canCastMedica2 && !state.Moving && state.MedicaLeft <= state.GCD + 5)
                return AID.Medica2;

            // 6. fallback: overheal rapture
            if (canCastRapture)
                return AID.AfflatusRapture;

            // 7. fallback to medica 1
            return AID.Medica1;
        }

        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe, bool heal)
        {
            return heal
                ? (aoe ? GetNextBestAOEHealGCD(state, strategy) : GetNextBestSTHealGCD(state, strategy))
                : (aoe ? GetNextBestAOEDamageGCD(state, strategy) : GetNextBestSTDamageGCD(state, strategy));
        }

        public static ActionID GetNextBestAction(State state, Strategy strategy, bool aoe, bool heal)
        {
            ActionID res = new();
            if (state.CanDoubleWeave) // first ogcd slot
                res = GetNextBestOGCD(state, strategy, state.DoubleWeaveWindowEnd, aoe, heal);
            if (!res && state.CanSingleWeave) // second/only ogcd slot
                res = GetNextBestOGCD(state, strategy, state.GCD, aoe, heal);
            if (!res) // gcd
                res = ActionID.MakeSpell(GetNextBestGCD(state, strategy, aoe, heal));
            return res;
        }

        // short string for supported action
        public static string ActionShortString(ActionID action)
        {
            return action == CommonRotation.IDSprint ? "Sprint" : action == IDStatPotion ? "StatPotion" : ((AID)action.ID).ToString();
        }
    }
}
