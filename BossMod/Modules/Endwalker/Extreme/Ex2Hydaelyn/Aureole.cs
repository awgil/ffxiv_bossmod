namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component tracking [lateral] aureole mechanic
class Aureole : BossComponent
{
    public bool Done { get; private set; }
    private AOEShapeCone _aoe = new(40, 75.Degrees());

    public Aureole(BossModule module) : base(module)
    {
        _aoe.DirectionOffset = (AID)(Module.PrimaryActor.CastInfo?.Action.ID ?? 0) is AID.LateralAureole1 or AID.LateralAureole2 ? -90.Degrees() : 0.Degrees();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Check(actor.Position, Module.PrimaryActor) || _aoe.Check(actor.Position, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees()))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _aoe.Draw(Arena, Module.PrimaryActor);
        _aoe.Draw(Arena, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees());
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Aureole1AOE or AID.Aureole2AOE or AID.LateralAureole1AOE or AID.LateralAureole2AOE)
            Done = true;
    }
}
