namespace BossMod.Stormblood.Ultimate.UCOB;

class Hatch : Components.CastCounter
{
    public bool Active = true;
    public int NumNeurolinkSpawns { get; private set; }
    public int NumTargetsAssigned { get; private set; }
    private readonly IReadOnlyList<Actor> _orbs;
    private readonly IReadOnlyList<Actor> _neurolinks;
    private BitMask _targets;

    public bool IsTarget(int slot) => _targets[slot];

    public Hatch(BossModule module) : base(module, AID.Hatch)
    {
        _orbs = module.Enemies(OID.Oviform);
        _neurolinks = module.Enemies(OID.Neurolink);
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
                    break;
                case 2:
                    twintania.DesiredPosition = new(8, 5);
                    twintania.DesiredRotation = 60.Degrees();
                    break;
            }
        }

        if (!Active || _neurolinks.Count == 0)
            return;

        var linkShape = Sdf.Continuous(ShapeDistance.Union([.. _neurolinks.Select(n => ShapeDistance.Circle(n.Position, 2))]));

        if (_targets[slot])
        {
            hints.AddForbiddenZone(linkShape.Inverted(), WorldState.FutureTime(2));

            foreach (var (s, t) in Raid.WithSlot().IncludedInMask(_targets))
                if (s != slot)
                    hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, 8), WorldState.FutureTime(2));
        }
        else
        {
            hints.AddForbiddenZone(linkShape, DateTime.MaxValue);
            foreach (var (_, t) in Raid.WithSlot().IncludedInMask(_targets))
            {
                hints.AddForbiddenZone(ShapeDistance.Capsule(Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(t), Module.PrimaryActor.DistanceToPoint(t.Position), 2));

                if (t.FindStatus(SID.Neurolink) != null)
                    // 2 extra units to account for sudden twister dodge
                    hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, 10));
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Active && _targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            foreach (var o in _orbs.Where(o => !o.IsDead))
                Arena.ZoneCircle(o.Position, 1, ArenaColor.AOE);
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
            foreach (var t in spell.Targets)
                _targets.Clear(Raid.FindSlot(t.ID));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Twintania && id == 0x94)
            ++NumNeurolinkSpawns;
    }
}
