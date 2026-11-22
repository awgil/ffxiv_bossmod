namespace BossMod.Stormblood.Ultimate.UWU;

class P2Nails(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _nails = [];

    public readonly List<WPos> KilledOrder = [];

    public Angle RelativeNorth { get; private set; }

    public bool Active => _nails.Any(n => n.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_nails, ArenaColor.Enemy);
        foreach (var n in _nails.Where(n => !n.IsDead))
            Arena.AddCircle(n.Position, n.HitboxRadius, ArenaColor.Object);
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

                // standard strat (NA): kill in zigzag pattern starting relative SE
                _nails.SortBy(n =>
                {
                    var dir = n.Position - Arena.Center;
                    var isSouth = dir.Dot(RelativeNorth.ToDirection()) < 0;
                    var isEast = dir.Dot(RelativeNorth.ToDirection().OrthoR()) > 0;
                    return (isSouth ? 0 : 2) + (isEast ? 0 : 1);
                });
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var haveTarget = false;
        foreach (var n in _nails)
        {
            if (n.PendingDead)
                continue;

            if (hints.FindEnemy(n) is { } e)
            {
                e.ForbidDOTs = true;

                if (n.PendingHPRatio > 0.5f)
                    e.Priority = 1;
                else if (n.FindStatus(SID.NailDamageUp)?.Extra >= 2 && !haveTarget)
                {
                    haveTarget = true;
                    e.Priority = 1;
                }
                else
                    e.Priority = AIHints.Enemy.PriorityForbidden;
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.InfernalNail)
            _nails.Remove(actor);
    }

    public override void OnIsDeadChanged(Actor actor)
    {
        if ((OID)actor.OID == OID.InfernalNail && actor.IsDead)
            KilledOrder.Add(actor.Position);
    }
}
