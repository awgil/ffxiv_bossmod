namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class AeroIII(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    public readonly IReadOnlyList<Actor> Voidzones = module.Enemies(OID.BitingWind);

    private static readonly AOEShapeCircle _shape = new(4);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var v in Voidzones)
            yield return new(v.Position, 25, Shape: _shape);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var v in Voidzones)
            _shape.Outline(Arena, v.Position);
        base.DrawArenaForeground(pcSlot, pc);
    }
}
