namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

// note: we could determine magic/poison positions even before they are activated (poison are always at +-4/11, magic are at +-1 from one of the axes), but this is not especially useful
class RubyGlow1 : RubyGlowCommon
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
