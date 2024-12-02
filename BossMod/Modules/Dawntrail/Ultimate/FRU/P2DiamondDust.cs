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
        IgnoreOtherBaits = true;
    }
}

class P2DiamondDustHouseOfLight(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.HouseOfLight))
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(60, 20.Degrees()); // TODO: verify angle

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
    private readonly WPos[] _safespots = new WPos[PartyState.MaxPartySize];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_safespots[pcSlot] != default)
            Arena.AddCircle(_safespots[pcSlot], 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IcicleImpact:
                _conesAtCardinals ??= IsCardinal(caster.Position - Module.Center);
                InitIfReady();
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

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FrigidStone)
        {
            _supportsBaitCones ??= actor.Class.IsDD();
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
            _safespots[slot] = Module.Center + radius * dir.ToDirection();
        }
    }

    private bool IsCardinal(WDir off) => Math.Abs(off.X) < 1 || Math.Abs(off.Z) < 1;
}

class P2HeavenlyStrike(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.HeavenlyStrike))
{
    private readonly DateTime _activation = module.WorldState.FutureTime(3.9f);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.Center, 12, _activation);
    }
}

class P2SinboundHoly(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
    public int NumCasts;
    private DateTime _nextExplosion;

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
    private readonly Actor? _source = module.Enemies(OID.OraclesReflection).FirstOrDefault();
    public readonly List<AOEInstance> AOEs = [];

    private readonly AOEShapeCone _shapeFront = new(30, 135.Degrees());
    private readonly AOEShapeCone _shapeBack = new(30, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_source, ArenaColor.Object, true);
        if (AOEs.Count > 0)
            Arena.AddCircle(pc.Position, 32, ArenaColor.Safe);
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
