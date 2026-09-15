namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class RubyGlow6(BossModule module) : RubyGlowRecolor(module, 9)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (CurRecolorState != RecolorState.BeforeStones && MagicStones.Any())
            yield return new(ShapeQuadrant, QuadrantCenter(AOEQuadrant));
        foreach (var p in ActivePoisonAOEs())
            yield return p;
    }
}
