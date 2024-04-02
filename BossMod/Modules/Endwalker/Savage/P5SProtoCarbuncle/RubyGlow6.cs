namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class RubyGlow6 : RubyGlowRecolor
{
    public RubyGlow6() : base(9) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (CurRecolorState != RecolorState.BeforeStones && MagicStones.Any())
            yield return new(ShapeQuadrant, QuadrantCenter(module, AOEQuadrant));
        foreach (var p in ActivePoisonAOEs(module))
            yield return p;
    }
}
