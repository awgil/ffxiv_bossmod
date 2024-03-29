namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class RubyGlow5 : RubyGlowCommon
{
    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        // TODO: correct explosion time
        foreach (var o in MagicStones)
            yield return new(ShapeQuadrant, QuadrantCenter(module, QuadrantForPosition(module, o.Position)));
        foreach (var p in ActivePoisonAOEs(module))
            yield return p;
    }
}
