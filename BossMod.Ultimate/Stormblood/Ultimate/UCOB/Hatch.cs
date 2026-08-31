namespace BossMod.Stormblood.Ultimate.UCOB;

class Hatch : Components.CastCounter
{
    public bool Active = true;
    public int NumNeurolinkSpawns { get; private set; }
    public int NumTargetsAssigned { get; private set; }
    private readonly List<(Actor orb, DateTime moveStart)> _orbs = [];
    private readonly List<Actor> _neurolinks = [];
    private BitMask _targets;
    private readonly Actor?[] _assignedLinks = new Actor?[PartyState.MaxPartySize];

    public bool Twister;

    public bool IsTarget(int slot) => _targets[slot];

    public Hatch(BossModule module) : base(module, AID.Hatch)
    {
        KeepOnPhaseChange = true;
    }

    public void Reset()
    {
        _targets.Reset();
        NumTargetsAssigned = NumCasts = 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        var inNeurolink = _neurolinks.InRadius(actor.Position, 2).Any();
        if (_targets[slot])
            hints.Add("Go to neurolink!", !inNeurolink);
        else if (inNeurolink)
            hints.Add("GTFO from neurolink!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.IsTargetable)
        {
            var twintania = hints.FindEnemy(Module.PrimaryActor)!;
            twintania.TankDistance = 0.5f;

            switch (_neurolinks.Count)
            {
                case 0:
                    twintania.DesiredPosition = new(0, -8);
                    twintania.DesiredRotation = 180.Degrees();
                    break;
                case 1:
                    twintania.DesiredPosition = new(-8, 5);
                    twintania.DesiredRotation = -60.Degrees();

                    // TODO: find a melee spot that's easy to get twin out of
                    //if (_numHatches == 0)
                    //    twintania.DesiredPosition = new(-7, -8);
                    break;
                case 2:
                    twintania.DesiredPosition = new(8, 5);
                    twintania.DesiredRotation = 60.Degrees();
                    break;
            }
        }

        if (!Active || _neurolinks.Count == 0)
            return;

        var linkShape = ShapeDistance.Union([.. _neurolinks.Select(n => ShapeDistance.Circle(n.Position, 2))]);

        if (_targets[slot])
        {
            // tiebreaker
            var myLink = _assignedLinks[slot];
            if (myLink == null)
                return;

            var leewaySeconds = 10f;

            if (_orbs.Count > 0)
            {
                var waitMove = MathF.Max(0, (float)(_orbs[0].moveStart - WorldState.CurrentTime).TotalSeconds);
                leewaySeconds = waitMove + _orbs.Min(o => actor.DistanceToHitbox(o.orb)) / 5f;
            }

            hints.GoalZones.Add(AIHints.GoalSingleTarget(myLink.Position, 5, 0.5f));

            if (Twister)
                hints.AddForbiddenZone(Sdf.Continuous(ShapeDistance.DonutSector(myLink.Position, 2, 5, Module.PrimaryActor.AngleTo(myLink), 90.Degrees())).Inverted(), WorldState.FutureTime(leewaySeconds));
            else
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(myLink.Position, 2), WorldState.FutureTime(leewaySeconds));
        }
        else
        {
            foreach (var (orb, t) in _orbs)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(orb.Position, 2));
                if (orb.LastFrameMovement == default)
                {
                    foreach (var h in _neurolinks)
                        hints.AddForbiddenZone(ShapeDistance.Cone(orb.Position, 6, orb.AngleTo(h), 60.Degrees()), WorldState.FutureTime(2));
                }
                else
                    hints.AddForbiddenZone(ShapeDistance.Capsule(orb.Position, orb.LastFrameMovement.ToAngle(), 6, 2), WorldState.FutureTime(2));
            }

            // TODO: we need an accurate estimate here because it makes resolving liquid hell + fireball awkward
            // should just track all targets + orbs in Update()
            if (_targets.Any())
                // avoid everything around the neurolink if hatch is active
                hints.AddForbiddenZone(p => linkShape(p) - 8.5f, DateTime.MaxValue);
            else
                // else just avoid it
                hints.AddForbiddenZone(linkShape, DateTime.MaxValue);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Active && _targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            foreach (var (o, _) in _orbs)
                Arena.ZoneCircle(o.Position, 2, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Active)
            foreach (var neurolink in _neurolinks)
                Arena.AddCircle(neurolink.Position, 2, _targets[pcSlot] ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Generate)
        {
            _targets.Set(Raid.FindSlot(actor.InstanceID));
            ++NumTargetsAssigned;
            if (_targets.NumSetBits() == _neurolinks.Count)
                AssignLinks();
        }
    }

    void AssignLinks()
    {
        Array.Fill(_assignedLinks, null);

        // can't use proximity for assignment because positions are different between clients (if player is moving)
        List<Actor> linksAvailable = [.. _neurolinks];
        linksAvailable.SortBy(l => l.InstanceID);

        foreach (var (slot, player) in Raid.WithSlot().IncludedInMask(_targets).OrderBy(p => p.Item2.InstanceID))
        {
            _assignedLinks[slot] = linksAvailable[0];
            linksAvailable.RemoveAt(0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _orbs.RemoveAll(o => o.orb == caster);
            foreach (var t in spell.Targets)
                _targets.Clear(Raid.FindSlot(t.ID));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Twintania && id == 0x94)
            ++NumNeurolinkSpawns;
    }

    public override void OnActorCreated(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.Neurolink:
                _neurolinks.Add(actor);
                break;
            case OID.Oviform:
                _orbs.Add((actor, WorldState.FutureTime(4)));
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.Oviform)
            _orbs.RemoveAll(o => o.orb == actor);
    }
}
