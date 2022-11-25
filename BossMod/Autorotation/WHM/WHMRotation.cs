using System;

namespace BossMod.WHM
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public int NormalLilies;
            public int BloodLilies;
            public float NextLilyIn; // max 20
            public float SwiftcastLeft; // 0 if buff not up, max 10
            public float ThinAirLeft; // 0 if buff not up, max 12
            public float FreecureLeft; // 0 if buff not up, max 15
            public float MedicaLeft; // 0 if hot not up, max 15
            public float TargetDiaLeft; // 0 if debuff not up, max 30

            // upgrade paths
            public AID BestGlare => Unlocked(AID.Glare3) ? AID.Glare3 : Unlocked(AID.Glare1) ? AID.Glare1 : Unlocked(AID.Stone4) ? AID.Stone4 : Unlocked(AID.Stone3) ? AID.Stone3 : Unlocked(AID.Stone2) ? AID.Stone2 : AID.Stone1;
            public AID BestDia => Unlocked(AID.Dia) ? AID.Dia : Unlocked(AID.Aero2) ? AID.Aero2 : AID.Aero1;
            public AID BestHoly => Unlocked(AID.Holy3) ? AID.Holy3 : AID.Holy1;

            // statuses
            public SID ExpectedDia => Unlocked(AID.Dia) ? SID.Dia : Unlocked(AID.Aero2) ? SID.Aero2 : SID.Aero1;

            // num lilies on next GCD
            public int NormalLiliesOnNextGCD => Unlocked(AID.AfflatusSolace) ? Math.Min(NormalLilies + (NextLilyIn < GCD ? 1 : 0), 3) : 0;

            // can-cast checks
            public bool EnoughMPForGCD(int cost) => CurMP >= cost || ThinAirLeft > GCD;
            public bool CanCastMedica1 => Unlocked(AID.Medica1) && EnoughMPForGCD(900);
            public bool CanCastMedica2 => Unlocked(AID.Medica2) && EnoughMPForGCD(1000);
            public bool CanCastCure2 => Unlocked(AID.Cure2) && (EnoughMPForGCD(1000) || FreecureLeft > GCD);
            public bool CanCastCure3 => Unlocked(AID.Cure3) && EnoughMPForGCD(1500);

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"g={NormalLilies}/{BloodLilies}/{NextLilyIn:f1}, MP={CurMP}, RB={RaidBuffsLeft:f1}, Dia={TargetDiaLeft:f1}, Medica={MedicaLeft:f1}, Swiftcast={SwiftcastLeft:f1}, TA={ThinAirLeft:f1}, Freecure={FreecureLeft:f1}, AssizeCD={CD(CDGroup.Assize):f1}, PoMCD={CD(CDGroup.PresenceOfMind):f1}, LDCD={CD(CDGroup.LucidDreaming):f1}, TACD={CD(CDGroup.ThinAir):f1}, PlenCD={CD(CDGroup.PlenaryIndulgence):f1}, AsylumCD={CD(CDGroup.Asylum):f1}, BeniCD={CD(CDGroup.DivineBenison):f1}, TetraCD={CD(CDGroup.Tetragrammaton):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        public class Strategy : CommonRotation.Strategy
        {
            public bool Heal;
            public bool AOE;
            public int NumAssizeMedica1Targets; // how many targets would assize/medica1 heal (15y around self)
            public int NumRaptureMedica2Targets; // how many targets would rapture/medica2 heal (20y around self)
            public int NumCure3Targets; // how many targets cure3 would heal (10y around selected/best target)
            public int NumHolyTargets; // how many targets holy would hit (8y around self)
            public bool EnableAssize;
            public bool AllowReplacingHealWithMisery; // if true, allow replacing solace/rapture with misery
            public (Actor? Target, float HPRatio) BestSTHeal;

            public override string ToString()
            {
                return $"AOE={NumHolyTargets}, SH={BestSTHeal.Target?.Name.Substring(0, 4)}={BestSTHeal.HPRatio:f2}, AH={NumRaptureMedica2Targets}/{NumCure3Targets}/{NumAssizeMedica1Targets}, no-dots={ForbidDOTs}, movement-in={ForceMovementIn:f3}";
            }
        }

        public static bool CanCast(State state, Strategy strategy, float castTime) => state.SwiftcastLeft > state.GCD || strategy.ForceMovementIn >= state.GCD + castTime;
        public static bool RefreshDOT(State state, float timeLeft) => timeLeft < state.GCD + 3.0f; // TODO: tweak threshold so that we don't overwrite or miss ticks...

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // 1. plenary indulgence, if we're gonna cast aoe gcd heal that will actually heal someone... (TODO: reconsider priority)
            if (strategy.AOE && strategy.Heal && (strategy.NumRaptureMedica2Targets > 0 || strategy.NumCure3Targets > 0) && state.Unlocked(AID.PlenaryIndulgence) && state.CanWeave(CDGroup.PlenaryIndulgence, 0.6f, deadline))
                return ActionID.MakeSpell(AID.PlenaryIndulgence);

            // 3. potion (TODO)

            // 4. assize, if allowed and if we have some mana deficit (TODO: consider delaying until raidbuffs?)
            if (strategy.EnableAssize && state.CurMP <= 9000 && state.Unlocked(AID.Assize) && state.CanWeave(CDGroup.Assize, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Assize);

            // 5. pom (TODO: consider delaying until raidbuffs?)
            if (state.Unlocked(AID.PresenceOfMind) && state.CanWeave(CDGroup.PresenceOfMind, 0.6f, deadline))
                return ActionID.MakeSpell(AID.PresenceOfMind);

            // 6. lucid dreaming, if we won't waste mana (TODO: revise mp limit)
            if (state.CurMP <= 7000 && state.Unlocked(AID.LucidDreaming) && state.CanWeave(CDGroup.LucidDreaming, 0.6f, deadline))
                return ActionID.MakeSpell(AID.LucidDreaming);

            // 7. thin air, if we have some mana deficit and we're either sitting on 2 charges or we're gonna cast expensive spell (TODO: revise mp limit)
            if (state.CurMP <= 9000 && state.Unlocked(AID.ThinAir) && state.CanWeave(state.CD(CDGroup.ThinAir) - 60, 0.6f, deadline))
            {
                if (state.CD(CDGroup.ThinAir) < state.GCD)
                    return ActionID.MakeSpell(AID.ThinAir); // spend second charge to start cooldown ticking, even if we gonna cast glare
                if (strategy.Heal && state.NormalLiliesOnNextGCD == 0)
                    return ActionID.MakeSpell(AID.ThinAir); // spend last charge if we're gonna cast expensive GCD
            }

            return new();
        }

        public static AID GetNextBestSTDamageGCD(State state, Strategy strategy)
        {
            bool allowCasts = CanCast(state, strategy, 1.5f);

            // 0. just use glare before pull
            if (allowCasts && strategy.CombatTimer < 0)
                return state.BestGlare;

            // 1. refresh dia/aero, if needed
            if (!strategy.ForbidDOTs && state.Unlocked(AID.Aero1) && RefreshDOT(state, state.TargetDiaLeft))
                return state.BestDia;

            // 2. glare, if not moving or if under swiftcast
            if (allowCasts)
                return state.BestGlare;

            // 3. afflatus misery
            if (state.BloodLilies >= 3)
                return AID.AfflatusMisery;

            return AID.None;
            // 4. slidecast glare (TODO: consider early dia refresh if GCD is zero...)
            //return strategy.Moving ? state.BestDia : state.BestGlare;
        }

        public static AID GetNextBestAOEDamageGCD(State state, Strategy strategy)
        {
            // misery > holy
            return state.BloodLilies >= 3 ? AID.AfflatusMisery : state.BestHoly;
        }

        public static AID GetNextBestSTHealGCD(State state, Strategy strategy)
        {
            // 1. cure 2, if free
            if (state.Unlocked(AID.Cure2) && (state.FreecureLeft > state.GCD || state.ThinAirLeft > state.GCD))
                return AID.Cure2;

            // 2. afflatus solace, if possible (replace with misery if needed)
            if (state.NormalLiliesOnNextGCD > 0)
                return strategy.AllowReplacingHealWithMisery && state.BloodLilies >= 3 ? AID.AfflatusMisery : AID.AfflatusSolace;

            // 3. cure 2, if possible
            if (state.Unlocked(AID.Cure2) && state.CurMP >= 1000)
                return AID.Cure2;

            // 4. fallback to cure1
            return AID.Cure1;
        }

        public static AID GetNextBestAOEHealGCD(State state, Strategy strategy)
        {
            // 1. afflatus rapture, if possible (replace with misery if needed)
            bool canCastRapture = state.Unlocked(AID.AfflatusRapture) && state.NormalLiliesOnNextGCD > 0;
            if (canCastRapture && strategy.NumRaptureMedica2Targets > 0)
                return strategy.AllowReplacingHealWithMisery && state.BloodLilies >= 3 ? AID.AfflatusMisery : AID.AfflatusRapture;

            // 2. medica 2, if possible and useful, and buff is not already up; we consider it ok to overwrite last tick
            bool canCastMedica2 = state.CanCastMedica2;
            if (canCastMedica2 && strategy.NumRaptureMedica2Targets > 0 && state.MedicaLeft <= state.GCD + 2.5f)
                return AID.Medica2;

            // 3. cure 3, if possible and useful
            if (strategy.NumCure3Targets > 0 && state.CanCastCure3)
                return AID.Cure3;

            // 4. medica 1, if possible and useful
            if (strategy.NumAssizeMedica1Targets > 0 && state.CanCastMedica1)
                return AID.Medica1;

            // 5. fallback: overheal medica 2 for hot (e.g. during downtime)
            if (canCastMedica2 && CanCast(state, strategy, 2) && state.MedicaLeft <= state.GCD + 5)
                return AID.Medica2;

            // 6. fallback: overheal rapture, unless capped on blood lilies
            if (canCastRapture && state.BloodLilies < 3)
                return AID.AfflatusRapture;

            // 7. fallback to medica 1/2
            return canCastMedica2 ? AID.Medica2 : AID.Medica1;
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            return strategy.Heal
                ? (strategy.AOE ? GetNextBestAOEHealGCD(state, strategy) : GetNextBestSTHealGCD(state, strategy))
                : (strategy.AOE ? GetNextBestAOEDamageGCD(state, strategy) : GetNextBestSTDamageGCD(state, strategy));
        }
    }
}
