namespace BossMod;

// custom action queue
// the idea is to allow multiple providers (manual input, autorotation, boss modules, etc.) to gather prioritized list of actions
// the framework then selects the highest priority action
// if it is still on cooldown, the framework then looks for next-best action that can fit without delaying previously selected action
// this process repeats until no more actions can be found
public sealed class ActionQueue
{
    public record struct Entry(ActionID Action, Actor? Target, Vector3 TargetPos, Angle? FacingAngle, float Priority);

    // reference priority guidelines
    // values divisible by 1000 are reserved for standard cooldown planner priorities; to disambiguate, small value < 1 could be added to the actions whose window ends earlier
    // code should avoid adding several actions with identical priority for consistence; consider using values like 1100 or 1230 (not divisible by 1000, but divisible by 10) to allow user to fine-tune custom priorities
    // note that actions with priority < 0 will never be executed; they can still be added to the queue if it's convenient for the implementation
    public static class Priority
    {
        public const float Minimal = 0; // minimal priority for action to be still considered for execution; do not use directly
        // priorities > Minimal and < VeryLow should be used for ??? (don't know good usecases)
        public const float VeryLow = 1000; // only use this action if there is nothing else to press
        // priorities > VeryLow and < Low should be used for actions that can be safely delayed without affecting dps (eg. ability with charges when there is no risk of overcapping or losing raidbuffs any time soon)
        public const float Low = 2000; // only use this action if it won't delay dps action (eg. delay if there are any ogcds that need to be used)
        // priorities > Low and < Medium should be used for normal ogcds that are part of the rotation
        public const float Medium = 3000; // use this action in first possible ogcd slot, unless there's some hugely important rotational ogcd; you should always have at least 1 slot per gcd to execute Medium actions
        // priorities > Medium and < High should be used for ogcds that can't be delayed (eg. GNB continuation); code should be careful not to queue more than one such action per gcd window
        public const float High = 4000; // use this action asap, unless it would delay gcd (that is - in first possible ogcd slot); careless use could severely affect dps
        // priorities > High and < VeryHigh should be used for gcds, or any other actions that need to delay gcd
        public const float VeryHigh = 5000; // drop everything and use this action asap, delaying gcd if needed; almost guaranteed to severely affect dps
        // priorities > VeryHigh should not be used by general code

        public const float ManualOGCD = 4001; // manually pressed ogcd should be higher priority than any non-gcd, but lower than any gcd
        public const float ManualGCD = 4999; // manually pressed gcd should be higher priority than any gcd; it's still lower priority than VeryHigh, since presumably that action is planned to delay gcd
        public const float ManualEmergency = 9000; // this action should be used asap, because user is spamming it
    }
}
