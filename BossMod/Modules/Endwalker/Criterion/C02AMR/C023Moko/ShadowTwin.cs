namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

// TODO: cast start/end?
sealed class Clearout(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCone _shape = new(27f, 90f.Degrees()); // TODO: verify range, it's definitely bigger than what table suggests... maybe origin is wrong?

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor.OID is (uint)OID.NOniClaw or (uint)OID.SOniClaw)
        {
            AOEs.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(8.3d)));
        }
    }
}

sealed class AccursedEdge : Components.GenericBaitAway
{
    public enum Mechanic { None, Far, Near }

    private Mechanic _curMechanic;
    private readonly Clearout? _clearout;

    private static readonly AOEShapeCircle _shape = new(6f);
    private static readonly WDir[] _safespotDirections = [new(1f, default), new(-1f, default), new(default, 1f), new(default, -1f)];

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
            var players = Raid.WithoutSlot(false, true, true).SortedByRange(Module.PrimaryActor.Position).ToList();
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
            var shouldBait = !ForbiddenPlayers[pcSlot];
            var baitClose = _curMechanic == Mechanic.Near;
            var stayClose = baitClose == shouldBait;
            var baitDistance = stayClose ? 12f : 19f;
            var aoes = CollectionsMarshal.AsSpan(_clearout.AOEs);
            var len = aoes.Length;
            for (var i = 0; i < 4; ++i)
            {
                var dir = _safespotDirections[i];
                var potentialSafespot = Arena.Center + baitDistance * dir;
                var safespotSafe = true;
                for (var j = 0; j < len; ++j)
                {
                    ref var aoe = ref aoes[j];
                    if (aoe.Check(potentialSafespot))
                    {
                        safespotSafe = false;
                        break;
                    }
                }
                if (safespotSafe)
                {
                    Arena.ZoneCircleOutline(potentialSafespot, 1f, Colors.Safe);
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var mechanic = spell.Action.ID switch
        {
            (uint)AID.FarEdge => Mechanic.Far,
            (uint)AID.NearEdge => Mechanic.Near,
            _ => Mechanic.None
        };
        if (mechanic != Mechanic.None)
            _curMechanic = mechanic;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NAccursedEdge or (uint)AID.SAccursedEdge)
        {
            ++NumCasts;
            _curMechanic = Mechanic.None;
            CurrentBaits.Clear();
        }
    }
}
