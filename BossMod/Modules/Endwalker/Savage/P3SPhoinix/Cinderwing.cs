namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to cinderwing
class Cinderwing : BossComponent
{
    private AOEShapeCone _aoe = new(60, 90.Degrees());

    public override void Init(BossModule module)
    {
        _aoe.DirectionOffset = (AID)(module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            AID.RightCinderwing => -90.Degrees(),
            AID.LeftCinderwing => 90.Degrees(),
            _ => 0.Degrees()
        };
        if (_aoe.DirectionOffset.Rad == 0)
            module.ReportError(this, $"Failed to initialize cinderwing; unexpected boss cast {module.PrimaryActor.CastInfo?.Action}");
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_aoe.Check(actor.Position, module.PrimaryActor))
            hints.Add("GTFO from wing!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        _aoe.Draw(arena, module.PrimaryActor);
    }
}
