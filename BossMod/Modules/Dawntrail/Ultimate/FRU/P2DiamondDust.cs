namespace BossMod.Dawntrail.Ultimate.FRU;

class P2AxeKick(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AxeKick), new AOEShapeCircle(16));
class P2ScytheKick(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScytheKick), new AOEShapeDonut(4, 20));
class P2IcicleImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IcicleImpact), new AOEShapeCircle(10));
class P2FrigidNeedleCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FrigidNeedleCircle), new AOEShapeCircle(5));
class P2FrigidNeedleCross(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FrigidNeedleCross), new AOEShapeCross(40, 2.5f));

class P2FrigidStone : Components.BaitAwayIcon
{
    public P2FrigidStone(BossModule module) : base(module, new AOEShapeCircle(5), (uint)IconID.FrigidStone, ActionID.MakeSpell(AID.FrigidStone), 8.1f, true)
    {
        EnableHints = false;
        IgnoreOtherBaits = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}

class P2DiamondDustHouseOfLight(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.HouseOfLight))
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null && ForbiddenPlayers.Any())
            foreach (var p in Raid.WithoutSlot().SortedByRange(_source.Position).Take(4))
                CurrentBaits.Add(new(_source, p, _shape, _activation));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ForbiddenPlayers[slot])
        {
            if (ActiveBaitsOn(actor).Any())
                hints.Add("Stay farther away!");
        }
        else
        {
            if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(b).Any()))
                hints.Add("Bait cone away from raid!");
        }

        if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from baited cone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AxeKick or AID.ScytheKick)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.8f);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FrigidStone)
            ForbiddenPlayers.Set(Raid.FindSlot(actor.InstanceID));
    }
}

class P2DiamondDustSafespots(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private bool? _out;
    private bool? _supportsBaitCones;
    private bool? _conesAtCardinals;
    private readonly WDir[] _safeOffs = new WDir[PartyState.MaxPartySize];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_safeOffs[slot] != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.Center + _safeOffs[slot], new WDir(0, 1), Module.Bounds.MapResolution, Module.Bounds.MapResolution, Module.Bounds.MapResolution));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_safeOffs[pcSlot] != default)
            Arena.AddCircle(Module.Center + _safeOffs[pcSlot], 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IcicleImpact:
                if (_conesAtCardinals == null)
                {
                    _conesAtCardinals = IsCardinal(caster.Position - Module.Center);
                    InitIfReady();
                }
                break;
            case AID.AxeKick:
                _out = true;
                InitIfReady();
                break;
            case AID.ScytheKick:
                _out = false;
                InitIfReady();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AxeKick:
                // out done => cone baiters go in, ice baiters stay
                for (int i = 0; i < _safeOffs.Length; ++i)
                    if (_safeOffs[i] != default && Raid[i]?.Class.IsSupport() == _supportsBaitCones)
                        _safeOffs[i] = 4 * _safeOffs[i].Normalized();
                break;
            case AID.ScytheKick:
                // in done => cone baiters stay, ice baiters go out
                for (int i = 0; i < _safeOffs.Length; ++i)
                    if (_safeOffs[i] != default && Raid[i]?.Class.IsSupport() != _supportsBaitCones)
                        _safeOffs[i] = 8 * _safeOffs[i].Normalized();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FrigidStone && _supportsBaitCones == null)
        {
            _supportsBaitCones = actor.Class.IsDD();
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (_out == null || _supportsBaitCones == null || _conesAtCardinals == null)
            return;
        var supportsAtCardinals = _supportsBaitCones == _conesAtCardinals;
        var offsetTH = supportsAtCardinals ? 0.Degrees() : _config.P2DiamondDustSupportsCCW ? 45.Degrees() : -45.Degrees();
        var offsetDD = !supportsAtCardinals ? 0.Degrees() : _config.P2DiamondDustDDCCW ? 45.Degrees() : -45.Degrees();
        foreach (var (slot, group) in _config.P2DiamondDustCardinals.Resolve(Raid))
        {
            var support = group < 4;
            var baitCone = _supportsBaitCones == support;
            var dir = 180.Degrees() - (group & 3) * 90.Degrees();
            dir += support ? offsetTH : offsetDD;
            var radius = (_out.Value ? 16 : 0) + (baitCone ? 1 : 3);
            _safeOffs[slot] = radius * dir.ToDirection();
        }
    }

    private bool IsCardinal(WDir off) => Math.Abs(off.X) < 1 || Math.Abs(off.Z) < 1;
}

class P2HeavenlyStrike(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.HeavenlyStrike))
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly WDir[] _safeDirs = new WDir[PartyState.MaxPartySize];
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Module.Center, 12, _activation);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_safeDirs[slot] != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center + 6 * _safeDirs[slot], 1), _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_safeDirs[pcSlot] != default)
            Arena.AddCircle(Module.Center + 18 * _safeDirs[pcSlot], 1, ArenaColor.Safe);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.IcicleImpact && _activation == default)
        {
            _activation = WorldState.FutureTime(3.9f);
            var safeDir = (caster.Position - Module.Center).Normalized();
            if (safeDir.X > 0.5f || safeDir.Z > 0.8f)
                safeDir = -safeDir; // G1
            foreach (var (slot, group) in _config.P2DiamondDustKnockbacks.Resolve(Raid))
                _safeDirs[slot] = group == 1 ? -safeDir : safeDir;
        }
    }
}

class P2SinboundHoly(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
    public int NumCasts;
    private DateTime _nextExplosion;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // resolves automatically

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SinboundHoly)
        {
            AddStacks(Raid.WithoutSlot().Where(p => p.Role == Role.Healer), Module.CastFinishAt(spell, 0.9f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SinboundHolyAOE && WorldState.CurrentTime > _nextExplosion)
        {
            ++NumCasts;
            _nextExplosion = WorldState.FutureTime(0.5f);
        }
    }
}

class P2SinboundHolyVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.SinboundHolyVoidzone).Where(z => z.EventState != 7));

class P2SinboundHolyAIBait(BossModule module) : BossComponent(module)
{
    private WDir _finalDir; // if oracle jumps directly to one of the safespots, both groups run opposite in one (arbitrary, CW) direction, and the one that ends up behind boss slides across - in that case this is kept zeroed

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // stay on border
        hints.AddForbiddenZone(ShapeDistance.Circle(Module.Center, 18));

        // and move towards safety (CW is arbitrary)
        var preferredDir = _finalDir != default ? _finalDir : (actor.Position - Module.Center).Normalized().OrthoR();
        hints.AddForbiddenZone(ShapeDistance.HalfPlane(Module.Center - 2 * preferredDir, preferredDir));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // note: we assume that when this component is activated, only last pair of icicles remain (orthogonal to original safe spots), and oracle is already at position
        if ((AID)spell.Action.ID == AID.IcicleImpact && Module.Enemies(OID.OraclesReflection).FirstOrDefault() is var oracle && oracle != null && _finalDir == default)
        {
            var destDir = (caster.Position - Module.Center).Normalized();
            var idealDir = (Module.Center - oracle.Position).Normalized();
            var dot = destDir.Dot(idealDir);
            if (dot < 0)
            {
                destDir = -destDir;
                dot = -dot;
            }
            if (dot > 0.5f)
                _finalDir = destDir;
            // else: single direction mode
        }
    }
}

class P2ShiningArmor(BossModule module) : Components.GenericGaze(module, ActionID.MakeSpell(AID.ShiningArmor))
{
    private Actor? _source;
    private DateTime _activation;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_source.Position, _activation);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BossP2 && id == 0x1E43)
        {
            _source = actor;
            _activation = WorldState.FutureTime(7.2f);
        }
    }
}

class P2TwinStillnessSilence(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    public bool EnableAIHints;
    private readonly Actor? _source = module.Enemies(OID.OraclesReflection).FirstOrDefault();
    private BitMask _thinIce;

    private readonly AOEShapeCone _shapeFront = new(30, 135.Degrees());
    private readonly AOEShapeCone _shapeBack = new(30, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!EnableAIHints || _source == null)
            return;

        if (!_thinIce[slot])
        {
            // preposition
            // this is a bit hacky - we need to stay either far away from boss, or close (and slide over at the beginning of the ice)
            hints.AddForbiddenZone(ShapeDistance.Rect(_source.Position, _source.Rotation, 25, 5, 20));
            return;
        }

        // at this point, we have thin ice, so we can either stay or move fixed distance
        hints.AddForbiddenZone(ShapeDistance.Donut(actor.Position, 1, 31));
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(actor.Position, 33));

        if (AOEs.Count == 0)
        {
            // if we're behind boss, slide over
            hints.AddForbiddenZone(ShapeDistance.Rect(_source.Position, _source.Rotation, 10, 20, 20));
        }
        else
        {
            // otherwise just dodge next aoe
            ref var nextAOE = ref AOEs.Ref(0);
            hints.AddForbiddenZone(nextAOE.Shape.Distance(nextAOE.Origin, nextAOE.Rotation), nextAOE.Activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_source, ArenaColor.Object, true);
        if (AOEs.Count > 0)
        {
            Arena.AddCircle(pc.Position, 32, ArenaColor.Vulnerable);
            Arena.AddLine(pc.Position, pc.Position - 32 * WorldState.Client.CameraAzimuth.ToDirection(), ArenaColor.Vulnerable);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ThinIce)
            _thinIce.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (shape1, off1, shape2, off2) = (AID)spell.Action.ID switch
        {
            AID.TwinStillnessFirst => (_shapeFront, 0.Degrees(), _shapeBack, 180.Degrees()),
            AID.TwinSilenceFirst => (_shapeBack, 0.Degrees(), _shapeFront, 180.Degrees()),
            _ => (null, default, null, default)
        };
        if (shape1 != null && shape2 != null)
        {
            AOEs.Add(new(shape1, caster.Position, spell.Rotation + off1, Module.CastFinishAt(spell)));
            AOEs.Add(new(shape2, caster.Position, spell.Rotation + off2, Module.CastFinishAt(spell, 2.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TwinStillnessFirst or AID.TwinStillnessSecond or AID.TwinSilenceFirst or AID.TwinSilenceSecond)
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}
