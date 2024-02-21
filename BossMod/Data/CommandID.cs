namespace BossMod
{
    public enum CommandID : uint
    {
        None = 0,
        StatusOff = 104,
        UseLostAction = 2950
    }

    public static class CIDExtensions {
        public static bool CausesAnimationLock(this CommandID id) => id == CommandID.UseLostAction;
    }
}
