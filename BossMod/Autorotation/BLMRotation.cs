using System.Text;

namespace BossMod
{
    public static class BLMRotation
    {
        public enum AID : uint
        {
            None = 0,

            // single target GCDs
            Blizzard1 = 142,
            Fire1 = 141,
            Thunder1 = 144,
            Scathe = 156,
            Fire3 = 152,
            Blizzard3 = 154,
            Thunder3 = 153,
            Blizzard4 = 3576,
            Fire4 = 3577,
            Foul = 7422,
            Despair = 16505,
            Xenoglossy = 16507,
            Paradox = 25797,

            // aoe GCDs
            Blizzard2 = 25793,
            Fire2 = 147,
            Thunder2 = 7447,
            Freeze = 159,
            Flare = 162,
            Thunder4 = 7420,
            HighFire2 = 25794,
            HighBlizzard2 = 25795,

            // oGCDs
            Transpose = 149,
            Sharpcast = 3574,

            // offsensive CDs
            Swiftcast = 7561,
            Manafont = 158,
            LeyLines = 3573,
            Triplecast = 7421,
            Amplifier = 25796,
            LucidDreaming = 7562,

            // defensive CDs
            Addle = 7560,
            Manaward = 157,
            Surecast = 7559,

            // misc
            AetherialManipulation = 155,
            BetweenTheLines = 7419,
            UmbralSoul = 16506,
            Sleep = 25880,
        }
        public static ActionID IDStatPotion = new(ActionType.Item, 1000000); // hq grade 6 tincture of ???

        public enum SID : uint
        {
            None = 0,
            Thunder1 = 161,
        }

        // full state needed for determining next action
        public class State : CommonRotation.State
        {
            public int ElementalLevel; // -3 (umbral ice 3) to +3 (astral fire 3)
            public float ElementalLeft; // 0 if elemental level is 0, otherwise buff duration, max 15
            public float TargetThunderLeft;
            public float TransposeCD; // 5 max, 0 if ready
            public float AddleCD; // 90 max, 0 if ready
            public float SwiftcastCD; // 60 max, 0 if ready
            public float LucidDreamingCD; // 60 max, 0 if ready

            // per-level ability unlocks (TODO: consider abilities unlocked by quests - they could be unavailable despite level being high enough)
            public bool UnlockedFire1 => Level >= 2;
            public bool UnlockedTranspose => Level >= 4;
            public bool UnlockedThunder1 => Level >= 6;
            public bool UnlockedAddle => Level >= 8;
            public bool UnlockedSleep => Level >= 10;
            public bool UnlockedBlizzard2 => Level >= 12;
            public bool UnlockedLucidDreaming => Level >= 14;
            public bool UnlockedScathe => Level >= 15; // quest-locked
            public bool UnlockedFire2 => Level >= 18;
            public bool UnlockedSwiftcast => Level >= 18;
            public bool UnlockedThunder2 => Level >= 26;
            public bool UnlockedManaward => Level >= 30; // quest-locked
            public bool UnlockedManafont => Level >= 30; // quest-locked
            public bool UnlockedFire3 => Level >= 35;
            public bool UnlockedBlizzard3 => Level >= 35; // quest-locked
            public bool UnlockedFreeze => Level >= 40; // quest-locked
            public bool UnlockedSurecast => Level >= 44;
            public bool UnlockedThunder3 => Level >= 45; // quest-locked
            public bool UnlockedAetherialManipulation => Level >= 50;
            public bool UnlockedFlare => Level >= 50; // quest-locked
            public bool UnlockedLeyLines => Level >= 52; // quest-locked
            public bool UnlockedSharpcast => Level >= 54; // quest-locked
            public bool UnlockedBlizzard4 => Level >= 58; // quest-locked
            public bool UnlockedFire4 => Level >= 60; // quest-locked
            // TODO: L62+

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, Elem={ElementalLevel}/{ElementalLeft:f1}, Thunder={TargetThunderLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            // cooldowns
            public bool ExecuteSwiftcast;
            public bool ExecuteSurecast;
            public bool ExecuteAddle;

            public override string ToString()
            {
                var sb = new StringBuilder("SmartQueue:");
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

        public static uint AdjustedFireCost(State state, uint baseCost)
        {
            return state.ElementalLevel switch
            {
                > 0 => baseCost * 2,
                < 0 => 0,
                _ => baseCost
            };
        }

        public static ActionID GetNextBestAction(State state, Strategy strategy, bool aoe, bool moving)
        {
            // TODO: consider ogcd weaving for blm...
            if (strategy.ExecuteSurecast && state.UnlockedSurecast)
                return ActionID.MakeSpell(AID.Surecast);
            if (strategy.ExecuteAddle && state.UnlockedAddle)
                return ActionID.MakeSpell(AID.Addle);
            if (strategy.ExecuteSwiftcast && state.UnlockedSwiftcast)
                return ActionID.MakeSpell(AID.Swiftcast);
            if (strategy.ExecuteSprint)
                return CommonRotation.IDSprint;

            if (moving)
            {
                // TODO: not sure about this...
                if (!state.CanSingleWeave && state.UnlockedScathe && state.CurMP >= 800)
                    return ActionID.MakeSpell(AID.Scathe);

                if (state.UnlockedTranspose && state.TransposeCD <= state.AnimationLock && (state.ElementalLevel < 0 && state.CurMP >= 9600 || state.ElementalLevel > 0 && state.CurMP < 2800))
                    return ActionID.MakeSpell(AID.Transpose);

                return new();
            }

            if (aoe && state.UnlockedBlizzard2)
            {
                // TODO: this works until ~L40
                if (state.UnlockedThunder2 && state.TargetThunderLeft <= state.GCD)
                    return ActionID.MakeSpell(AID.Thunder2);

                return state.UnlockedFire2 ? TransposeRotation(state, AID.Blizzard2, 800, AID.Fire2, 1500) : ActionID.MakeSpell(AID.Blizzard2);
            }
            else
            {
                // TODO: this works until ~L35
                // 1. thunder (TODO: tweak threshold so that we don't overwrite or miss ticks...)
                if (state.UnlockedThunder1 && state.TargetThunderLeft <= state.GCD)
                    return ActionID.MakeSpell(AID.Thunder1);

                return state.UnlockedFire1 ? TransposeRotation(state, AID.Blizzard1, 400, AID.Fire1, 800) : ActionID.MakeSpell(AID.Blizzard1);
            }
        }

        // short string for supported action
        public static string ActionShortString(ActionID action)
        {
            return action == CommonRotation.IDSprint ? "Sprint" : action == IDStatPotion ? "StatPotion" : ((AID)action.ID).ToString();
        }

        private static ActionID TransposeRotation(State state, AID iceSpell, int iceCost, AID fireSpell, int fireCost)
        {
            if (state.ElementalLevel < 0)
            {
                // continue blizzard1 spam until full mana, then swap to fire
                // TODO: take mana ticks into account, if it happens before GCD
                if (state.CurMP < 10000 - iceCost)
                    return ActionID.MakeSpell(iceSpell);
                else
                    return ActionID.MakeSpell(state.UnlockedTranspose ? AID.Transpose : fireSpell);
            }
            else if (state.ElementalLevel > 0)
            {
                // continue fire1 spam until oom, then swap to ice
                if (state.CurMP >= fireCost * 2 + iceCost * 3 / 4)
                    return ActionID.MakeSpell(fireSpell);
                else
                    return ActionID.MakeSpell(state.UnlockedTranspose ? AID.Transpose : iceSpell);
            }
            else
            {
                // dropped buff => fire if have some mana (TODO: better limit), blizzard otherwise
                return ActionID.MakeSpell(state.CurMP < fireCost * 3 + iceCost * 3 / 4 ? iceSpell : fireSpell);
            }
        }
    }
}
