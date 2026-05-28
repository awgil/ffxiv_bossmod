namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class OrbitalOmen(BossModule module) : Components.StandardAOEs(module, AID.OrbitalOmenRect, new AOEShapeRect(60, 5), maxCasts: 4, highlightImminent: true);

class FireAndFury(BossModule module) : Components.GroupedAOEs(module, [AID.FireAndFuryBack, AID.FireAndFuryFront], new AOEShapeCone(60, 45.Degrees()), highlightImminent: true);

class FearsomeFireball1(BossModule module) : Components.GenericWildCharge(module, 3, AID.FearsomeFireballCharge, 60)
{
    private BitMask _meteors;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.FireBreath)
            _meteors.Set(Raid.FindSlot(actor.InstanceID));

        if ((IconID)iconID == IconID.WildCharge)
        {
            Source = actor;
            Activation = WorldState.FutureTime(5.1f);
            foreach (var (i, p) in Raid.WithSlot())
            {
                if (p.InstanceID == targetID)
                    PlayerRoles[i] = PlayerRole.TargetNotFirst;
                else if (_meteors[i])
                    PlayerRoles[i] = PlayerRole.Avoid;
                else
                    PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}

class FearsomeFireball2(BossModule module) : Components.GenericWildCharge(module, 3, AID.FearsomeFireballCharge, 60)
{
    private readonly Comet _cometTracker = module.FindComponent<Comet>()!;
    private BitMask _meteors;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.FireBreath)
            _meteors.Set(Raid.FindSlot(actor.InstanceID));

        if ((IconID)iconID == IconID.WildCharge)
        {
            Source = actor;
            Activation = WorldState.FutureTime(5.1f);
            foreach (var (i, p) in Raid.WithSlot())
                PlayerRoles[i] = _meteors[i] || p.Role == Role.Tank
                    ? PlayerRole.Avoid
                    : p.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (Source == null || PlayerRoles[slot] is PlayerRole.Ignore or PlayerRole.Avoid)
            return;

        foreach (var aoe in EnumerateAOEs())
        {
            var comet = _cometTracker.Comets.Where(c => InAOE(aoe, c)).Closest(Source.Position);
            if (comet == null)
            {
                if (PlayerRoles[slot] == PlayerRole.Target)
                    hints.Add("Hide behind meteor!");
            }
            else
            {
                var dx = (comet.Position - Source.Position).Dot(aoe.dir);
                if ((actor.Position - aoe.origin).LengthSq() < dx * dx)
                    hints.Add("Hide behind meteor!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source == null)
            return;

        List<Func<WPos, bool>> zones = [];
        switch (PlayerRoles[slot])
        {
            case PlayerRole.Target:
            case PlayerRole.TargetNotFirst:
                foreach (var c in _cometTracker.Comets)
                {
                    var hitCometFn = ShapeContains.InvertedCone(Source.Position, 60, Source.AngleTo(c), Angle.Asin(HalfWidth / (Source.Position - c.Position).Length()));
                    var hideFn = ShapeContains.Rect(Source.Position, c.Position, HalfWidth);
                    zones.Add(ShapeContains.Union([hitCometFn, hideFn]));
                }
                if (zones.Count > 0)
                    hints.AddForbiddenZone(ShapeContains.Intersection(zones), Activation);
                break;
            case PlayerRole.Share:
            case PlayerRole.ShareNotFirst:
                foreach (var aoe in EnumerateAOEs())
                {
                    zones.Add(ShapeContains.InvertedRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth));
                    if (_cometTracker.Comets.Where(c => InAOE(aoe, c)).Closest(aoe.origin) is { } blocker)
                        zones.Add(ShapeContains.Rect(aoe.origin, aoe.dir, (blocker.Position - aoe.origin).Dot(aoe.dir), 0, HalfWidth));
                    hints.AddForbiddenZone(ShapeContains.Union(zones), Activation);
                }
                break;
        }

        hints.AddPredictedDamage(Raid.WithSlot().WhereSlot(s => PlayerRoles[s] != PlayerRole.Ignore).Mask(), Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _meteors.Reset();
            NumCasts++;
            Source = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}

class CosmicKiss(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(4), (uint)IconID.FireBreath, activationDelay: 8.1f, centerAtTarget: true)
{
    private readonly Comet _cometTracker = module.FindComponent<Comet>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (ActiveBaitsOn(actor).Any() && _cometTracker.Comets.Any(c => c.Position.InCircle(actor.Position, Comet.TriggerRadius * 2)))
            hints.Add("GTFO from meteor!");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CosmicKiss)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class Comet(BossModule module) : BossComponent(module)
{
    // TODO: figure out what toggles them appearing
    public readonly List<Actor> Comets = [];

    // TODO: this is a random guess
    public const float TriggerRadius = 3;

    public bool DrawTrigger = true;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CosmicKiss)
            Comets.Add(caster);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (animState2 == 1)
            Comets.Remove(actor);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var enemy in Comets)
        {
            Arena.Actor(enemy, ArenaColor.Object, true);
            if (DrawTrigger)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddCircle(enemy.Position, TriggerRadius, 0xFF000000, 2);
                Arena.AddCircle(enemy.Position, TriggerRadius, ArenaColor.Object);
            }
        }
    }
}

class CometExplosion(BossModule module) : Components.StandardAOEs(module, AID.ExplosionComet, 8);

class ForegoneFatality(BossModule module) : Components.CastCounter(module, AID.ForegoneFatality)
{
    private readonly List<(Actor Source, Actor Target)> _tethered = [];
    private readonly RM11STheTyrantConfig _config = Service.Config.Get<RM11STheTyrantConfig>();
    private readonly PartyRolesConfig _roles = Service.Config.Get<PartyRolesConfig>();
    private readonly Comet _cometTracker = module.FindComponent<Comet>()!;

    // empty except for tanks
    private readonly Actor?[] _assignedTether = new Actor?[8];
    private int _numActiveTethers;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.ForegoneFatality && WorldState.Actors.Find(tether.Target) is { } target)
        {
            _tethered.Add((source, target));
            if (_numActiveTethers < 2)
            {
                _numActiveTethers++;
                if (_numActiveTethers == 2)
                    AssignTethers();
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.ForegoneFatality && WorldState.Actors.Find(tether.Target) is { } target)
            _tethered.RemoveAll(t => t.Source == source && t.Target == target);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (t, p) in _tethered)
            Arena.AddLine(t.Position, p.Position, _assignedTether[pcSlot] == t ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _tethered.Clear();
            Array.Fill(_assignedTether, null);
            _numActiveTethers = 0;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_assignedTether[slot] is { } src)
        {
            if (_tethered.Any(t => t.Source == src && t.Target != actor))
                hints.Add("Take tether!");
        }
        else if (_assignedTether.Any(t => t != null) && _tethered.Any(t => t.Target == actor))
            hints.Add("Pass tether!");
    }

    void AssignTethers()
    {
        if (_config.ForegoneFatalityAssignments == RM11STheTyrantConfig.HintForegoneFatality.None)
            return;

        if (_tethered.Count != 2)
        {
            ReportError($"Expected two tethers, but there are {_tethered.Count}, aborting");
            return;
        }

        var t0 = _tethered[0];
        var t1 = _tethered[1];

        var comet0 = (OID)t0.Target.OID == OID.Comet ? t0.Target : _cometTracker.Comets.Closest(t0.Target.Position);
        if (comet0 == null)
        {
            ReportError($"Can't assign tethers, no targetable comets are present [0]");
            return;
        }

        var comet1 = (OID)t1.Target.OID == OID.Comet ? t1.Target : _cometTracker.Comets.Exclude(comet0).Closest(t1.Target.Position);
        if (comet1 == null)
        {
            ReportError($"Can't assign tethers, no targetable comets are present [1]");
            return;
        }

        List<(Actor Source, Actor Comet)> cometsOrdered = [(t0.Source, comet0), (t1.Source, comet1)];
        cometsOrdered.SortBy(c => c.Comet.Position.Z);

        foreach (var (i, player) in Raid.WithSlot())
        {
            var assignment = _roles[Raid.Members[i].ContentId];
            if (assignment == PartyRolesConfig.Assignment.MT)
                _assignedTether[i] = cometsOrdered[0].Source;
            else if (assignment == PartyRolesConfig.Assignment.OT)
                _assignedTether[i] = cometsOrdered[1].Source;
        }
    }
}

class ShockwaveCounter(BossModule module) : Components.CastCounter(module, AID.Shockwave);

class TripleTyrannhilation(BossModule module) : Components.GenericLineOfSightAOE(module, AID.Shockwave, 60, false)
{
    private readonly Comet _cometTracker = module.FindComponent<Comet>()!;
    private readonly List<Actor> _comets = [];
    private DateTime _next;

    public const float CometRadius = 2f; // guessing

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TripleTyrannhilationCast)
        {
            _comets.AddRange(_cometTracker.Comets.SortedByRange(caster.Position));
            Modify(caster.Position, _comets.Select(c => (c.Position, CometRadius)), Module.CastFinishAt(spell, 1.1f));
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (animState2 == 1)
        {
            _comets.Remove(actor);
            if (NumCasts < 3)
                Modify(Module.PrimaryActor.Position, _comets.Select(c => (c.Position, CometRadius)), _next);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _next = WorldState.FutureTime(1.6f);
            if (NumCasts >= 3)
                Modify(null, []);
        }
    }
}
