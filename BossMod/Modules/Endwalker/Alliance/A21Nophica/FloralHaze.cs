namespace BossMod.Endwalker.Alliance.A21Nophica;

class FloralHaze : Components.StatusDrivenForcedMarch
{
    public FloralHaze(BossModule module) : base(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
    {
        ActivationLimit = 8;
    }
}
