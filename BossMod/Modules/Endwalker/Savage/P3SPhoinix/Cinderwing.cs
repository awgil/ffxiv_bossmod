namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to cinderwing
class Cinderwing : BossComponent
{
    private readonly AOEShapeCone _aoe = new(60, 90.Degrees());

    public Cinderwing(BossModule module) : base(module)
    {
        _aoe.DirectionOffset = (AID)(Module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            AID.RightCinderwing => -90.Degrees(),
            AID.LeftCinderwing => 90.Degrees(),
            _ => 0.Degrees()
        };
        if (_aoe.DirectionOffset.Rad == 0)
            ReportError($"Failed to initialize cinderwing; unexpected boss cast {Module.PrimaryActor.CastInfo?.Action}");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Check(actor.Position, Module.PrimaryActor))
            hints.Add("GTFO from wing!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _aoe.Draw(Arena, Module.PrimaryActor);
    }
}
