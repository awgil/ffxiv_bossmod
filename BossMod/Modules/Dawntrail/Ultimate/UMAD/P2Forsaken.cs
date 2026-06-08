namespace BossMod.Dawntrail.Ultimate.UMAD;

class P2UltimateEmbrace(BossModule module) : Components.CastSharedTankbuster(module, AID.UltimateEmbrace, 5);

class P2ForsakenRaidwide(BossModule module) : Components.RaidwideCast(module, AID.Forsaken);

// mapeffect: XX.00020001
// index is 1 (N) clockwise through 8 (NW)
// tower triggers 10.2s later
class P2PathOfLight(BossModule module) : Components.GenericTowers(module, AID.ThePathOfLight)
{
    // even towers appear about 0.5s before odd towers trigger, and i DO NOT want to have to write code accounting for 4 towers, so instead we save the even towers until the odd towers despawn
    readonly List<Tower> _stored = [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 1 and <= 8 && state == 0x00020001)
        {
            var angle = (180 - (index - 1) * 45).Degrees();

            var tower = new Tower(Arena.Center + angle.ToDirection() * 8, 4, minSoakers: 2, maxSoakers: 2, default, WorldState.FutureTime(10.2f));
            (Towers.Count >= 2 ? _stored : Towers).Add(tower);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            if (Towers.Count == 0 && _stored.Count > 0)
            {
                Towers.AddRange(_stored);
                _stored.Clear();
            }
        }
    }

    public bool InAnyTower(Actor a) => Towers.Any(t => a.Position.InCircle(t.Position, t.Radius));
}

class P2Shapes : Components.CastCounterMulti
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    public enum Shape
    {
        None,
        Stack,
        Spread,
        Cone
    }

    public readonly Shape[] Shapes = new Shape[8];

    readonly int[] _buddySlot = Utils.MakeArray(8, -1);

    public P2Shapes(BossModule module) : base(module, [AID.Spelldriver, AID.Spellscatter, AID.Spellwave])
    {
        var partnerIds = Utils.GenArray<int[]>(4, () => [-1, -1]);
        foreach (var (slot, grp) in _config.P2ForsakenPairs.Resolve(Raid))
        {
            if (partnerIds[grp][0] == -1)
                partnerIds[grp][0] = slot;
            else
                partnerIds[grp][1] = slot;
        }

        if (partnerIds.All(p => p.All(p1 => p1 >= 0)))
        {
            for (var i = 0; i < partnerIds.Length; i++)
            {
                _buddySlot[partnerIds[i][0]] = partnerIds[i][1];
                _buddySlot[partnerIds[i][1]] = partnerIds[i][0];
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shapes[slot] != default)
            hints.Add($"Current shape: {Shapes[slot]}", false);
        if (_buddySlot[slot] >= 0 && Shapes[_buddySlot[slot]] != default)
            hints.Add($"Buddy shape: {Shapes[_buddySlot[slot]]}", false);
    }

    // delete this nephew
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Service.IsDev)
            return;

        var triangleRadius = 0.577f * 1.5f;
        foreach (var (slot, player) in Raid.WithSlot())
        {
            var shapeOrigin = player.Position - Arena.CameraAzimuth.ToDirection() * 2;
            switch (Shapes[slot])
            {
                case Shape.Cone:
                    var points = (shapeOrigin + Arena.CameraAzimuth.ToDirection() * triangleRadius, shapeOrigin + (Arena.CameraAzimuth - 120.Degrees()).ToDirection() * triangleRadius, shapeOrigin + (Arena.CameraAzimuth + 120.Degrees()).ToDirection() * triangleRadius);
                    Arena.AddTriangle(points.Item1, points.Item2, points.Item3, 0xFF000000, 2);
                    Arena.AddTriangleFilled(points.Item1, points.Item2, points.Item3, ArenaColor.Danger);
                    break;
                case Shape.Spread:
                    Arena.AddCircle(shapeOrigin, 0.75f, 0xFF000000, 2);
                    Arena.AddCircleFilled(shapeOrigin, 0.75f, ArenaColor.Danger);
                    break;
                case Shape.Stack:
                    Arena.AddCircle(shapeOrigin, 0.75f, 0xFF000000, 2);
                    Arena.AddCircle(shapeOrigin, 0.75f, ArenaColor.Danger);
                    break;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var shape = (SID)status.ID switch
        {
            SID.ForsakenStack => Shape.Stack,
            SID.ForsakenSpread => Shape.Spread,
            SID.ForsakenCone => Shape.Cone,
            _ => default
        };

        if (shape != default && Raid.TryFindSlot(actor, out var slot))
            Shapes[slot] = shape;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.ForsakenStack or SID.ForsakenSpread or SID.ForsakenCone && Raid.TryFindSlot(actor, out var slot))
            Shapes[slot] = Shape.None;
    }

    public void Reset()
    {
        NumCasts = 0;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _buddySlot[pcSlot] == playerSlot ? PlayerPriority.Critical : PlayerPriority.Irrelevant;
}

class P2StackSpread(BossModule module) : Components.UniformStackSpread(module, 5, 5, 3, alwaysShowSpreads: true)
{
    readonly P2Shapes _shapes = module.FindComponent<P2Shapes>()!;
    readonly P2PathOfLight _towers = module.FindComponent<P2PathOfLight>()!;

    bool _castPending;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ThePathOfLight:
                _castPending = true;
                break;
            case AID.Spellscatter:
            case AID.Spellwave:
            case AID.Spelldriver:
                _castPending = false;
                break;
        }
    }

    public override void Update()
    {
        if (_castPending)
            return;

        Stacks.Clear();
        Spreads.Clear();

        foreach (var (i, player) in Raid.WithSlot().WhereActor(_towers.InAnyTower))
        {
            if (_shapes.Shapes[i] == P2Shapes.Shape.Stack)
                AddStack(player, _towers.Towers[0].Activation);
            else if (_shapes.Shapes[i] == P2Shapes.Shape.Spread)
                AddSpread(player, _towers.Towers[0].Activation);
        }
    }
}

class P2Spellwave(BossModule module) : Components.GenericBaitAway(module, AID.Spellwave)
{
    readonly P2Shapes _shapes = module.FindComponent<P2Shapes>()!;
    readonly P2PathOfLight _towers = module.FindComponent<P2PathOfLight>()!;

    bool _castPending;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ThePathOfLight:
                _castPending = true;
                break;
            case AID.Spellscatter:
            case AID.Spellwave:
            case AID.Spelldriver:
                _castPending = false;
                break;
        }
    }

    public override void Update()
    {
        if (_castPending)
            return;

        CurrentBaits.Clear();

        foreach (var (i, player) in Raid.WithSlot().WhereSlot(s => _shapes.Shapes[s] == P2Shapes.Shape.Cone).WhereActor(_towers.InAnyTower))
        {
            var closest = Raid.WithoutSlot().Exclude(player).Closest(player.Position);
            if (closest != null)
                CurrentBaits.Add(new(player, closest, new AOEShapeCone(40, 45.Degrees()), _towers.Towers[0].Activation));
        }
    }
}

// boss/clone jumps on nearest player
class P2PastsEndFuturesEnd(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    WPos _source;
    DateTime _activation;
    int _numJumps;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PastsEndCast or AID.FuturesEndCast)
        {
            _source = spell.LocXZ;
            _activation = Module.CastFinishAt(spell, 0.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FuturesEndBossAOE or AID.PastsEndBossAOE or AID.FuturesEndCloneAOE or AID.PastsEndCloneAOE)
        {
            _numJumps++;
            if (_numJumps == 4)
            {
                _source = default;
                _numJumps = 0;
            }
        }
    }

    public override void Update()
    {
        Spreads.Clear();

        if (_source == default)
            return;

        AddSpreads(Raid.WithoutSlot().SortedByRange(_source).Take(4), _activation);
    }
}

class P2AllThingsEnding(BossModule module) : Components.GroupedAOEs(module, [AID.AllThingsEnding1, AID.AllThingsEnding2], new AOEShapeCone(100, 90.Degrees()));

// cast starts 6s after boss castevent
// based on replay analysis, it seems like these are each individually baited on a random player
class P2AllThingsEndingBait(BossModule module) : BossComponent(module)
{
    enum Bait
    {
        None,
        Front,
        Behind
    }

    Bait _next;
#pragma warning disable IDE0052 // Remove unread private members
    DateTime _activation;
#pragma warning restore IDE0052 // Remove unread private members

    readonly List<Actor> _sources = [];

    public bool Draw;
    public bool Casting { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FuturesEndCast:
                _next = Bait.Front;
                _activation = Module.CastFinishAt(spell, 6);
                break;
            case AID.PastsEndCast:
                _next = Bait.Behind;
                _activation = Module.CastFinishAt(spell, 6);
                break;
            case AID.AllThingsEnding1:
            case AID.AllThingsEnding2:
                _sources.Remove(caster);
                if (_sources.Count == 0)
                {
                    Casting = true;
                    _next = default;
                    _activation = default;
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FuturesEndBossAOE or AID.PastsEndBossAOE or AID.FuturesEndCloneAOE or AID.PastsEndCloneAOE) // or FuturesEnd1/FuturesEnd2
            _sources.Add(caster);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Draw)
        {
            foreach (var src in _sources)
            {
                var dir = src.AngleTo(pc);
                if (_next == Bait.Behind)
                    dir += 180.Degrees();
                Arena.AddCone(src.Position, 30, dir, 90.Degrees(), ArenaColor.Danger);
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_next != default)
            hints.Add($"Next bait: {_next}");
    }
}

class P2LightOfJudgment(BossModule module) : Components.RaidwideCast(module, AID.LightOfJudgmentP2);
