namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P1JagdDolls(BossModule module) : BossComponent(module)
{
    public int NumExhausts;
    private readonly List<Actor> _dolls = module.Enemies((uint)OID.JagdDoll);
    private readonly HashSet<ulong> _exhaustsDone = [];

    private readonly Actor?[] _dollsByAssignment = new Actor?[8];
    private bool _haveDollAssignments;

    private readonly GroupAssignmentFourUnique _assignment = Service.Config.Get<TEAConfig>().P1DollAssignments;
    private readonly bool _forbidDolls = Service.Config.Get<TEAConfig>().P1DollPullSafety;

    private const float _exhaustRadius = 8.8f;

    private IEnumerable<Actor> ActiveDolls => _dolls.Where(d => d.IsTargetable && !d.IsDead);
    public bool Active => ActiveDolls.Any();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumExhausts < 2 && ActiveDolls.InRadius(actor.Position, _exhaustRadius).Count() > 1)
        {
            hints.Add("GTFO from exhaust intersection");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in _dolls)
        {
            if (hints.FindEnemy(t) is AIHints.Enemy enemy)
            {
                enemy.ForbidDOTs = true;
                if (enemy.Actor.PendingHPRatio < 0.25f)
                {
                    enemy.Priority = AIHints.Enemy.PriorityForbidden;
                    continue;
                }

                if (_haveDollAssignments)
                {
                    if (_dollsByAssignment[slot] == t)
                        enemy.Priority = 1;
                    else if (_forbidDolls)
                        enemy.Priority = AIHints.Enemy.PriorityForbidden;
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var doll in ActiveDolls)
        {
            var isMine = _haveDollAssignments && _dollsByAssignment[pcSlot] == doll;

            Arena.Actor(doll, isMine ? Colors.Enemy : Colors.Object);

            if (NumExhausts < 2)
                Arena.ZoneCircleOutline(doll.Position, _exhaustRadius);

            var tether = WorldState.Actors.Find(doll.Tether.Target);
            if (tether != null)
                Arena.AddLine(doll.Position, tether.Position);
            else if (isMine)
                Arena.ZoneCircleOutline(doll.Position, 1.5f, Colors.Safe);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Exhaust && NumExhausts < 2)
        {
            if (!_exhaustsDone.Contains(caster.InstanceID))
            {
                NumExhausts = 1;
                _exhaustsDone.Add(caster.InstanceID);
            }
            else
            {
                NumExhausts = 2;
            }
        }
    }

    public override void OnActorTargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.JagdDoll)
        {
            _dolls.Add(actor);
            if (_dolls.Count == 4)
                AssignDolls();
        }
    }

    private void AssignDolls()
    {
        if (!_assignment.Validate())
            return;

        var pairs = _assignment.Resolve(Raid).ToList();
        if (pairs.Count == 0)
            return;

        var rages = Module.Enemies((uint)OID.LiquidRage).ToList();
        var ragesPos = rages.Select(r => r.Position).Aggregate((a, b) => new WPos(a.X + b.X, a.Z + b.Z));
        var average = new WPos(ragesPos.X / 3f, ragesPos.Z / 3f);
        var south = rages.MinBy(r => r.DistanceToPoint(average));
        var relSouth = (south!.Position - Arena.Center).ToAngle();

        _haveDollAssignments = true;

        var dolls = new Actor[4];
        foreach (var doll in _dolls)
        {
            var dollRel = (doll.Position - Arena.Center).ToAngle();
            var dollOrder = (dollRel - relSouth).Normalized().Deg switch
            {
                > 0 and < 90 => 2,
                > 90 and < 180 => 1,
                < 0 and > -90 => 3,
                _ => 0
            };
            dolls[dollOrder] = doll;
        }

        foreach (var (slot, ass) in pairs)
        {
            if (ass is >= 0 and < 4)
                _dollsByAssignment[slot] = dolls[ass];
        }
    }
}
