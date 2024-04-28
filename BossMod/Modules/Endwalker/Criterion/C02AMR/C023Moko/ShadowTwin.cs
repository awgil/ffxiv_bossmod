namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko;

// TODO: cast start/end?
class Clearout(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCone _shape = new(27, 90.Degrees()); // TODO: verify range, it's definitely bigger than what table suggests... maybe origin is wrong?

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && (OID)actor.OID is OID.NOniClaw or OID.SOniClaw)
        {
            AOEs.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(8.3f)));
        }
    }
}

class AccursedEdge : Components.GenericBaitAway
{
    public enum Mechanic { None, Far, Near }

    private Mechanic _curMechanic;
    private readonly Clearout? _clearout;

    private static readonly AOEShapeCircle _shape = new(6);
    private static readonly WDir[] _safespotDirections = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

    public AccursedEdge(BossModule module) : base(module, centerAtTarget: true)
    {
        _clearout = module.FindComponent<Clearout>();
        var baits = module.FindComponent<IaiGiriBait>();
        if (baits != null)
            foreach (var i in baits.Instances)
                ForbiddenPlayers.Set(Raid.FindSlot(i.Target?.InstanceID ?? 0));
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_curMechanic != Mechanic.None)
        {
            var players = Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position).ToList();
            foreach (var p in _curMechanic == Mechanic.Far ? players.TakeLast(2) : players.Take(2))
                CurrentBaits.Add(new(Module.PrimaryActor, p, _shape));
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_curMechanic != Mechanic.None)
            hints.Add($"Untethered bait: {_curMechanic}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // draw safespots (TODO: consider assigning specific side)
        if (_curMechanic != Mechanic.None && _clearout != null)
        {
            bool shouldBait = !ForbiddenPlayers[pcSlot];
            bool baitClose = _curMechanic == Mechanic.Near;
            bool stayClose = baitClose == shouldBait;
            var baitDistance = stayClose ? 12 : 19;
            foreach (var dir in _safespotDirections)
            {
                var potentialSafespot = Module.Center + baitDistance * dir;
                if (!_clearout.AOEs.Any(aoe => aoe.Check(potentialSafespot)))
                    Arena.AddCircle(potentialSafespot, 1, ArenaColor.Safe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var mechanic = (AID)spell.Action.ID switch
        {
            AID.FarEdge => Mechanic.Far,
            AID.NearEdge => Mechanic.Near,
            _ => Mechanic.None
        };
        if (mechanic != Mechanic.None)
            _curMechanic = mechanic;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NAccursedEdge or AID.SAccursedEdge)
        {
            ++NumCasts;
            _curMechanic = Mechanic.None;
            CurrentBaits.Clear();
        }
    }
}
