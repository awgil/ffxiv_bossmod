namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class SpinyShield(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _shield = module.Enemies(OID.SpinyShield);
    public Actor? ActiveShield => _shield.FirstOrDefault(a => a.EventState != 7);

    private const float _radius = 6; // TODO: verify

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var shield = ActiveShield;
        if (shield != null && !actor.Position.InCircle(shield.Position, _radius))
            hints.Add("Go inside shield");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shield = ActiveShield;
        if (shield != null)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(shield.Position, _radius));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var shield = ActiveShield;
        if (shield != null)
            Arena.ZoneCircle(shield.Position, _radius, ArenaColor.SafeFromAOE);
    }
}
