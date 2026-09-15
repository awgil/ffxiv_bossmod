namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class RubyGlow5(BossModule module) : RubyGlowCommon(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: correct explosion time
        foreach (var o in MagicStones)
            yield return new(ShapeQuadrant, QuadrantCenter(QuadrantForPosition(o.Position)));
        foreach (var p in ActivePoisonAOEs())
            yield return p;
    }
}
