namespace BossMod.Stormblood.Ultimate.UCOB;

class Hatch : Components.CastCounter
{
    public bool Active = true;
    public int NumNeurolinkSpawns { get; private set; }
    public int NumTargetsAssigned { get; private set; }
    private readonly List<Actor> _orbs = [];
    private readonly List<Actor> _neurolinks = [];
    private BitMask _targets;

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
            var myOrder = _targets.SetBits().OrderBy(b => Raid[b]?.Class ?? Class.None).Index().First(b => b.Item == slot).Index;
            var myLink = _neurolinks.OrderBy(n => n.InstanceID).Skip(myOrder).First();

            hints.GoalZones.Add(AIHints.GoalSingleTarget(myLink.Position, 5, 0.5f));

            if (Twister)
            {
                // plant near links but not inside
                hints.AddForbiddenZone(Sdf.Continuous(ShapeDistance.Union([.. _neurolinks.Select(n => {
                    var safeDir = Module.PrimaryActor.AngleTo(n);
                    return ShapeDistance.DonutSector(n.Position, 2, 5, safeDir, 90.Degrees());
                })])).Inverted(), WorldState.FutureTime(8));
            }
            else
            {
                hints.AddForbiddenZone(p => -linkShape(p), actor.FindStatus(SID.Neurolink, DateTime.MaxValue) == null ? WorldState.FutureTime(8) : default);
            }
        }
        else
        {
            foreach (var orb in _orbs)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(orb.Position, 2));
                if (orb.LastFrameMovement == default)
                {
                    foreach (var (_, target) in Raid.WithSlot().IncludedInMask(_targets))
                        hints.AddForbiddenZone(ShapeDistance.Capsule(orb.Position, orb.AngleTo(target), 6, 2), WorldState.FutureTime(2));
                }
                else
                    hints.AddForbiddenZone(ShapeDistance.Capsule(orb.Position, orb.LastFrameMovement.ToAngle(), 6, 2), WorldState.FutureTime(2));
            }

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
            foreach (var o in _orbs)
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
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _orbs.Remove(caster);
            foreach (var t in spell.Targets)
                _targets.Clear(Raid.FindSlot(t.ID));
        }

        //if ((AID)spell.Action.ID == AID.Hatch)
        //    _numHatches++;
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
                _orbs.Add(actor);
                break;
        }
    }
}
