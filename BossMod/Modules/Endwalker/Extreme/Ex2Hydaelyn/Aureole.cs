namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component tracking [lateral] aureole mechanic
class Aureole : BossComponent
{
    public bool Done { get; private set; }
    private AOEShapeCone _aoe = new(40, 75.Degrees());

    public override void Init(BossModule module)
    {
        _aoe.DirectionOffset = (AID)(module.PrimaryActor.CastInfo?.Action.ID ?? 0) is AID.LateralAureole1 or AID.LateralAureole2 ? -90.Degrees() : 0.Degrees();
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_aoe.Check(actor.Position, module.PrimaryActor) || _aoe.Check(actor.Position, module.PrimaryActor.Position, module.PrimaryActor.Rotation + 180.Degrees()))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        _aoe.Draw(arena, module.PrimaryActor);
        _aoe.Draw(arena, module.PrimaryActor.Position, module.PrimaryActor.Rotation + 180.Degrees());
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Aureole1AOE or AID.Aureole2AOE or AID.LateralAureole1AOE or AID.LateralAureole2AOE)
            Done = true;
    }
}
