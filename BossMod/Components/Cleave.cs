namespace BossMod.Components;

// generic component for cleaving autoattacks; shows shape outline and warns when anyone other than main target is inside
public class Cleave : CastCounter
{
    public AOEShape Shape { get; init; }
    public bool ActiveForUntargetable { get; init; }
    public bool ActiveWhileCasting { get; init; }
    public bool OriginAtTarget { get; init; }
    public DateTime NextExpected;
    private readonly IReadOnlyList<Actor> _enemies;

    // enemy OID == 0 means 'primary actor'
    public Cleave(BossModule module, ActionID aid, AOEShape shape, uint enemyOID = 0, bool activeForUntargetable = false, bool originAtTarget = false, bool activeWhileCasting = true) : base(module, aid)
    {
        Shape = shape;
        ActiveForUntargetable = activeForUntargetable;
        ActiveWhileCasting = activeWhileCasting;
        OriginAtTarget = originAtTarget;
        _enemies = module.Enemies(enemyOID != 0 ? enemyOID : module.PrimaryActor.OID);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (OriginsAndTargets().Any(e => e.target != actor && Shape.Check(actor.Position, e.origin.Position, e.angle)))
        {
            hints.Add("GTFO from cleave!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (origin, target, angle) in OriginsAndTargets())
        {
            if (actor != target)
            {
                hints.AddForbiddenZone(Shape, origin.Position, angle, NextExpected);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in OriginsAndTargets())
        {
            Shape.Outline(Arena, e.origin.Position, e.angle);
        }
    }

    private IEnumerable<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.IsDead)
                continue;

            if (!ActiveForUntargetable && !enemy.IsTargetable)
                continue;

            if (!ActiveWhileCasting && enemy.CastInfo != null)
                continue;

            var target = WorldState.Actors.Find(enemy.TargetID);
            if (target != null)
            {
                yield return (OriginAtTarget ? target : enemy, target, Angle.FromDirection(target.Position - enemy.Position));
            }
        }
    }
}
