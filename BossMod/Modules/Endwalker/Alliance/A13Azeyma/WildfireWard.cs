namespace BossMod.Endwalker.Alliance.A13Azeyma;

class Voidzone(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x1C)
        {
            if (state == 0x00020001)
                Arena.Bounds = new ArenaBoundsPolygon(Helpers.CalculateEquilateralTriangleVertices(new(-750, -756.25f), 12));
            if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsCircle(new(-750, -750), 30);
        }
    }
}

class WildfireWard(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.IlluminatingGlimpse), 15, false, 1, kind: Kind.DirLeft);
