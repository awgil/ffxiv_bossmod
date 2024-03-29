namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class SpinyShield : BossComponent
{
    private IReadOnlyList<Actor> _shield = ActorEnumeration.EmptyList;
    public Actor? ActiveShield => _shield.FirstOrDefault(a => a.EventState != 7);

    private static readonly float _radius = 6; // TODO: verify

    public override void Init(BossModule module)
    {
        _shield = module.Enemies(OID.SpinyShield);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var shield = ActiveShield;
        if (shield != null && !actor.Position.InCircle(shield.Position, _radius))
            hints.Add("Go inside shield");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shield = ActiveShield;
        if (shield != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(shield.Position, _radius));
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var shield = ActiveShield;
        if (shield != null)
            arena.ZoneCircle(shield.Position, _radius, ArenaColor.SafeFromAOE);
    }
}
