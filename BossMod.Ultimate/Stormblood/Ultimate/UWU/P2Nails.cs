namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: kill priorities
class P2Nails(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _nails = [];

    public Angle RelativeNorth { get; private set; }

    public bool Active => _nails.Any(n => n.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_nails, ArenaColor.Enemy);
        foreach (var n in _nails.Where(n => !n.IsDead))
            Arena.AddCircle(n.Position, 1, ArenaColor.Object);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.InfernalNail)
        {
            _nails.Add(actor);

            if (_nails.Count == 4)
            {
                var pos = Module.FindComponent<BossPositioning>()!;
                RelativeNorth = Angle.FromDirection(_nails.Where(n => _nails.InRadiusExcluding(n, 9).Any()).PositionCentroid() - Arena.Center);
                pos.DesiredPosition = Arena.Center + RelativeNorth.ToDirection() * 4;
                pos.DesiredRotation = RelativeNorth;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var n in _nails)
        {
            if (hints.FindEnemy(n) is { } e)
            {
                if (n.FindStatus(1544, DateTime.MaxValue)?.Extra >= 2)
                    e.Priority = 1;
                else if (n.PendingHPRatio < 0.2f)
                    e.Priority = AIHints.Enemy.PriorityForbidden;
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.InfernalNail)
            _nails.Remove(actor);
    }
}
