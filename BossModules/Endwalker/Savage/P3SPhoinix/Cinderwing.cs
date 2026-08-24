namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to cinderwing
class Cinderwing : BossComponent
{
    private readonly AOEShapeCone _aoe;

    public Cinderwing(BossModule module) : base(module)
    {
        var directionOffset = (AID)(Module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            AID.RightCinderwing => -90.Degrees(),
            AID.LeftCinderwing => 90.Degrees(),
            _ => default
        };
        if (directionOffset == default)
            ReportError($"Failed to initialize cinderwing; unexpected boss cast {Module.PrimaryActor.CastInfo?.Action}");

        _aoe = new(60, 90.Degrees(), directionOffset);
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
