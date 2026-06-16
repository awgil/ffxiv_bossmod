namespace BossMod.Dawntrail.Ultimate.UMAD;

class P3EarthquakeRaidwide(BossModule module) : Components.RaidwideCast(module, AID.EarthquakeRaidwide);
class P3EarthHints(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _accretions = [];
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Accretion)
            _accretions.Add(actor);
    }

    public override void AddGlobalHints(GlobalHints hints) => hints.Add($"Accretion: {string.Join(", ", _accretions.Select(a => a.Name))}");
}

class P3SlapHappy(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    static readonly float Displacement = MathF.Sqrt(200);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        WDir br;
        DateTime activation;
        switch ((AID)spell.Action.ID)
        {
            case AID.SlapHappyLeftHand:
                br = spell.Rotation.ToDirection().OrthoL() * Displacement;
                activation = Module.CastFinishAt(spell);
                _predicted.Add(new(new AOEShapeCircle(13), Arena.Center + br + br.OrthoL(), default, activation.AddSeconds(0.8f)));
                _predicted.Add(new(new AOEShapeCircle(13), Arena.Center + br, default, activation.AddSeconds(1.3f)));
                _predicted.Add(new(new AOEShapeCircle(13), Arena.Center + br + br.OrthoR(), default, activation.AddSeconds(2.1f)));
                _predicted.Add(new(new AOEShapeCircle(6), Arena.Center, default, activation.AddSeconds(3.3f)));
                break;
            case AID.SlapHappyRightHand:
                br = spell.Rotation.ToDirection().OrthoR() * Displacement;
                activation = Module.CastFinishAt(spell);
                _predicted.Add(new(new AOEShapeCircle(13), Arena.Center + br + br.OrthoR(), default, activation.AddSeconds(0.8f)));
                _predicted.Add(new(new AOEShapeCircle(13), Arena.Center + br, default, activation.AddSeconds(1.3f)));
                _predicted.Add(new(new AOEShapeCircle(13), Arena.Center + br + br.OrthoL(), default, activation.AddSeconds(2.1f)));
                _predicted.Add(new(new AOEShapeCircle(6), Arena.Center, default, activation.AddSeconds(3.3f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_SlapHappy1 or AID._Ability_SlapHappy2)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class P3SlapHappyShockwave(BossModule module) : Components.UntelegraphedBait(module)
{
    int _numExpected;
    public bool Resolved { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        DateTime activation;
        switch ((AID)spell.Action.ID)
        {
            case AID.SlapHappyLeftHand:
                _numExpected = 3;
                activation = Module.CastFinishAt(spell, 3.4f);
                CurrentBaits.Add(new(Arena.Center, Raid.WithSlot().WhereActor(a => a.Class.IsDD()).Mask(), new AOEShapeCone(100, 22.5f.Degrees()), activation, count: 1, stackSize: 4));
                CurrentBaits.Add(new(Arena.Center, Raid.WithSlot().WhereActor(a => a.Class.GetClassCategory() == ClassCategory.Healer).Mask(), new AOEShapeCone(100, 22.5f.Degrees()), activation, count: 1, stackSize: 2));
                CurrentBaits.Add(new(Arena.Center, Raid.WithSlot().WhereActor(a => a.Class.GetClassCategory() == ClassCategory.Tank).Mask(), new AOEShapeCone(100, 22.5f.Degrees()), activation, count: 1, stackSize: 2));
                break;
            case AID.SlapHappyRightHand:
                _numExpected = 1;
                activation = Module.CastFinishAt(spell, 3.4f);
                CurrentBaits.Add(new(Arena.Center, new(0xff), new AOEShapeCone(100, 22.5f.Degrees()), activation, count: 1, stackSize: 8));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SlapHappyShockingImpact or AID.SlapHappyShockwave)
        {
            NumCasts++;
            if (NumCasts >= _numExpected)
            {
                CurrentBaits.Clear();
                Resolved = true;
            }
        }
    }
}

class P3Blackhole(BossModule module) : Components.PersistentVoidzone(module, 2, m => m.Enemies(OID.BlackHoleP3));

class P3Nothingness(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(125, 3), (uint)TetherID.BlackHole, AID.Nothingness)
{
    int _numTethersAppeared;
    readonly List<(Actor Hole, int Set)> _holes = [];

    public override void AddGlobalHints(GlobalHints hints) { } // => hints.Add($"Number of tethers spawned: {_numTethersAppeared}");

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);

        if ((TetherID)tether.ID == TetherID.BlackHole && !_holes.Any(h => h.Hole == source))
        {
            // TODO: prio
            _holes.Add((source, _numTethersAppeared switch
            {
                < 1 => 1,
                < 3 => 2,
                < 6 => 3,
                _ => 4,
            }));
            _numTethersAppeared++;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _holes.RemoveAll(h => h.Hole == caster);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var (h, s) in _holes)
            Arena.Actor(h, ArenaColor.Object, true);
    }
}

class P3DamningEdict(BossModule module) : Components.StandardAOEs(module, AID.DamningEdict, new AOEShapeRect(60, 40));
class P3HotTail(BossModule module) : Components.StandardAOEs(module, AID.LookUponMeAndDespair, new AOEShapeRect(100, 8));
