namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class TouchdownSpot(BossModule module) : BossComponent(module)
{
    private static readonly WPos _spot = new(0, -28);
    public bool Done { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Touchdown)
            Done = true;
    }

    private bool Active => !Done && Arena.Center == default;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_spot, 2));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && !actor.Position.InCircle(_spot, 2))
            hints.Add("Go to the north edge, Touchdown throws you onto the platform!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Active)
            Arena.AddCircle(_spot, 2, ArenaColor.Safe, 2);
    }
}
