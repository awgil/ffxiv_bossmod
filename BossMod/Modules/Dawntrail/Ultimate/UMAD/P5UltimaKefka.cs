namespace BossMod.Dawntrail.Ultimate.UMAD;

class P5UltimaRepeater(BossModule module) : Components.RaidwideCastDelay(module, AID.UltimaRepeaterCast, AID.UltimaRepeater, 1.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (++NumCasts >= 4)
                Activation = default;
        }
    }
}

class P5FellForces(BossModule module) : Components.UntelegraphedBait(module)
{
    public void Activate(float delay)
    {
        CurrentBaits.Add(new(Arena.Center, new BitMask(), new AOEShapeCircle(3), WorldState.FutureTime(delay), 1, 2, centerAtTarget: true));
        CurrentBaits.Add(new(Arena.Center, Raid.WithSlot().WhereActor(t => t.Role == Role.Healer).Mask(), new AOEShapeCircle(5), WorldState.FutureTime(delay), 1, 2, centerAtTarget: true));
        CurrentBaits.Add(new(Arena.Center, Raid.WithSlot().WhereActor(t => t.Class.IsDD()).Mask(), new AOEShapeCircle(5), WorldState.FutureTime(delay), 1, 4, centerAtTarget: true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var ix = (AID)spell.Action.ID switch
        {
            AID.FellForcesDPS => 2,
            AID.FellForcesHealer => 1,
            AID.FellForcesTank => 0,
            _ => -1
        };

        if (ix >= 0)
        {
            NumCasts++;
            CurrentBaits.Ref(ix).Activation = WorldState.FutureTime(3.1f);
        }
    }

    public override void Update()
    {
        if (CurrentBaits.Count > 0)
            CurrentBaits.Ref(0).Targets = RaidWithSlotByEnmity(((UMAD)Module).KefkaP5()!).Take(1).Mask();
    }
}

class P5Flood(BossModule module) : Components.GenericAOEs(module, AID.FloodAOE)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(_predicted.Count, 4); i++)
            yield return _predicted[i] with { Color = i < 2 ? ArenaColor.Danger : ArenaColor.AOE };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FloodTelegraph)
            _predicted.Add(new(new AOEShapeRect(40, 5), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 4.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class P5ChaoticFlood(BossModule module) : Components.UntelegraphedBait(module, AID.ChaoticFlood)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FloodTelegraph && CurrentBaits.Count == 0)
            CurrentBaits.Add(new(Arena.Center, Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask(), new AOEShapeCircle(6), Module.CastFinishAt(spell, 4.5f), 1, 8, centerAtTarget: true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Ref(0).Activation = WorldState.FutureTime(1);
        }
    }
}

class P5MaddeningOrchestraFirst(BossModule module) : Components.UniformStackSpread(module, 5, 5, 2, 2)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MaddeningOrchestra)
            AddSpreads(Raid.WithoutSlot(), Module.CastFinishAt(spell, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Holy or AID.Flare)
            NumCasts++;
    }
}

class P5ChaoticFlare : Components.UniformStackSpread
{
    DateTime _activation;

    public P5ChaoticFlare(BossModule module) : base(module, 5, 0, 2, 2)
    {
        _activation = WorldState.FutureTime(3.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ChaoticFlare)
            _activation = default;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => IsStackTarget(player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void Update()
    {
        Stacks.Clear();

        if (_activation == default)
            return;

        foreach (var tank in RaidByEnmity(((UMAD)Module).KefkaP5()!).Take(1))
            AddStack(tank, _activation, Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask());
    }
}

class P5MaddeningOrchestraSecond(BossModule module) : Components.GenericBaitAway(module, AID.Holy, centerAtTarget: true)
{
    DateTime _activation;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Holy)
        {
            if (NumCasts++ < 3)
                ForbiddenPlayers.Set(Raid.FindSlot(spell.MainTargetID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MaddeningOrchestra)
            _activation = Module.CastFinishAt(spell, 4.1f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (ForbiddenPlayers[slot])
        {
            var (_, baiter) = Raid.WithSlot().ExcludedFromMask(ForbiddenPlayers).Farthest(Arena.Center);
            // dont dash in you idiot
            if (baiter != null)
                hints.AddForbiddenZone(ShapeContains.Circle(Arena.Center, (Arena.Center - baiter.Position).Length() + 1), _activation);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (NumCasts < 3)
            return;

        var boss = ((UMAD)Module).KefkaP5()!;

        CurrentBaits.AddRange(Raid.WithoutSlot().SortedByRange(boss.Position).Take(3).Select(a => new Bait(boss, a, new AOEShapeCircle(5), _activation)));
    }
}

class P5SurpriseBait : Components.GenericBaitAway
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    public P5SurpriseBait(BossModule module) : base(module, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
    {
        foreach (var player in Raid.WithoutSlot())
        {
            if (player.FindStatus(SID.SurpriseFlare) is { } fl)
                CurrentBaits.Add(new(player, player, new AOEShapeCircle(25), fl.ExpireAt));
            else if (player.FindStatus(SID.SurpriseHoly) is { } h && _config.P5MaddeningSpreadAll)
                CurrentBaits.Add(new(player, player, new AOEShapeCircle(6), h.ExpireAt));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ChaoticHoly or AID.FlareDiffusion)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class P5ChaoticHoly : Components.UniformStackSpread
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    public int NumCasts { get; private set; }

    public P5ChaoticHoly(BossModule module) : base(module, 6, 0, 7)
    {
        if (_config.P5MaddeningSpreadAll)
            return;

        foreach (var player in Raid.WithoutSlot())
            if (player.FindStatus(SID.SurpriseHoly) is { } h)
                AddStack(player, h.ExpireAt, Raid.WithSlot().WhereActor(a => a.FindStatus(SID.SurpriseFlare) != null).Mask());
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ChaoticHoly)
        {
            NumCasts++;
            Stacks.Clear();
        }
    }
}

class P5ForsakenHoles(BossModule module) : BossComponent(module)
{
    BitMask _holes;

    // map effects 0x14 - 0x1C
    static readonly WPos[] HolePositions = [
        new(100, 100),
        new(113.5f, 100),
        new(100, 86.5f),
        new(86.5f, 100),
        new(100, 113.5f),
        new(113.5f, 113.5f),
        new(113.5f, 86.5f),
        new(86.5f, 86.5f),
        new(86.5f, 113.5f),
    ];

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00200010)
        {
            var ix = index - 0x14;
            if (ix <= 8)
            {
                _holes.Set(ix);
                Rebuild();
            }
        }
    }

    void Rebuild()
    {
        var bounds = new RelSimplifiedComplexPolygon(CurveApprox.Circle(20, 1 / 90f));

        foreach (var bit in _holes.SetBits())
        {
            var off = HolePositions[bit] - new WPos(100, 100);
            bounds = Arena.Bounds.Clipper.Difference(new(bounds), new(CurveApprox.Circle(8, 1 / 90f).Select(d => d + off)));
        }

        // trim off tiny arena bits nobody is standing on because they're annoying
        var w = BitMask.Build(3, 7, 8);
        if ((_holes & w) == w)
            bounds = Arena.Bounds.Clipper.Difference(new(bounds), new(CurveApprox.Rect(new WDir(-20, 0), new WDir(5, 0), new WDir(0, 20))));

        var n = BitMask.Build(2, 6, 7);
        if ((_holes & n) == n)
            bounds = Arena.Bounds.Clipper.Difference(new(bounds), new(CurveApprox.Rect(new WDir(0, -20), new WDir(20, 0), new WDir(0, 5))));

        var e = BitMask.Build(1, 5, 6);
        if ((_holes & e) == e)
            bounds = Arena.Bounds.Clipper.Difference(new(bounds), new(CurveApprox.Rect(new WDir(20, 0), new WDir(5, 0), new WDir(0, 20))));

        var s = BitMask.Build(5, 4, 8);
        if ((_holes & s) == s)
            bounds = Arena.Bounds.Clipper.Difference(new(bounds), new(CurveApprox.Rect(new WDir(0, 20), new WDir(20, 0), new WDir(0, 5))));

        Arena.Bounds = new ArenaBoundsCustom(20, bounds);
    }
}

class P5ForsakenRaidwideCast(BossModule module) : Components.RaidwideCast(module, AID.ForsakenP5Cast);
class P5ForsakenRaidwideInstant(BossModule module) : Components.RaidwideInstant(module, AID.ForsakenP5Instant, 3.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.ForsakenBonds)
            Activation = WorldState.FutureTime(Delay);
    }
}
class P5ForsakenGround(BossModule module) : Components.StandardAOEs(module, AID.ForsakenGround, 8);
class P5ForsakenPuddle(BossModule module) : Components.StandardAOEs(module, AID.ForsakenPuddle, 8);
class P5ForsakenBonds(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Share6y, AID.ForsakenBonds, 6, 5.1f);
