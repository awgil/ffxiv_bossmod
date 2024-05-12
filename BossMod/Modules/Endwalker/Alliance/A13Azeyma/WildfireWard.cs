namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WildfireWard(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.IlluminatingGlimpse), 15, false, 1, kind: Kind.DirLeft);
class ArenaBounds(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x1C)
        {
            if (state == 0x00020001)
            {
                Arena.Bounds = A13Azeyma.TriangleBounds;
                Arena.Center = A13Azeyma.TriangleCenter;
            }
            if (state == 0x00080004)
            {
                Arena.Bounds = A13Azeyma.NormalBounds;
                Arena.Center = A13Azeyma.NormalCenter;
            }
        }
    }
}
