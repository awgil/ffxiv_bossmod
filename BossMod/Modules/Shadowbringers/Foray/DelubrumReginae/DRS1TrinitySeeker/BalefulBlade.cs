namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class BalefulBlade : BossComponent
{
    private bool _phantomEdge;

    private static readonly AOEShapeCone _shapeFront = new(DRS1.BarricadeRadius, 22.5f.Degrees());
    private static readonly AOEShapeDonutSector _shapeBehind = new(DRS1.BarricadeRadius, 30, 22.5f.Degrees());

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        hints.Add(_phantomEdge ? "Stay in front of barricade!" : "Hide behind barricade!", !IsSafe(module, actor));
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        AOEShape shape = _phantomEdge ? _shapeFront : _shapeBehind;
        for (int i = 0; i < 4; ++i)
        {
            var center = (45 + i * 90).Degrees();
            shape.Draw(arena, module.Bounds.Center, center, ArenaColor.SafeFromAOE);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BalefulBlade2)
            _phantomEdge = true;
    }

    public bool IsSafe(BossModule module, Actor actor)
    {
        var offset = actor.Position - module.Bounds.Center;
        var angle = Angle.FromDirection(offset).Rad; // 4 barricades to check, at +-45 and +-135
        angle = Math.Abs(angle); // fold around z axis, leaving two barricades to check - at 45 and 135
        angle = Math.Abs(angle - 90.Degrees().Rad); // rotate and fold again, leaving one barricade at 45 +- 22.5
        angle = Math.Abs(angle - 45.Degrees().Rad); // rotate and fold again - now barricade is [0, 22.5]
        if (angle > 22.5f.Degrees().Rad)
            return false; // this is always unsafe, will be knocked into wall
        var behind = offset.LengthSq() > DRS1.BarricadeRadius * DRS1.BarricadeRadius;
        return behind != _phantomEdge;
    }
}
