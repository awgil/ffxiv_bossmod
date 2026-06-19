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

class P3KefkaIndicator(BossModule module) : BossComponent(module)
{
    Actor? _bigKefka;
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.P3Max)
            _bigKefka = actor;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_bigKefka is { } k)
            Arena.Actor(k.Position - k.Rotation.ToDirection() * 20, k.Rotation, ArenaColor.Object);
    }
}

class P3SlapHappy(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    const float Displacement = 10;

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
        if ((AID)spell.Action.ID is AID.SlapHappyBig or AID.SlapHappySmall)
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

class P3Nothingness : Components.BaitAwayTethers
{
    enum TargetRole
    {
        Unknown,
        DPS,
        Support,
        Accretion
    }

    record struct TargetOrder(TargetRole Role, int Order);
    readonly TargetOrder[] _order = new TargetOrder[8];

    class Blackhole
    {
        public required Actor Hole;
        public int CastsLeft;
        public TargetOrder DesiredTarget;
    }

    readonly List<Blackhole> _holes = [];

    public P3Nothingness(BossModule module) : base(module, new AOEShapeRect(125, 3), (uint)TetherID.BlackHole, AID.Nothingness)
    {
        DrawTethers = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var hints_ = 0;

        var order = _order[slot];
        if (order != default)
            hints.Add($"Order: {order.Role} {order.Order}", false);

        foreach (var b in CurrentBaits)
        {
            if (_holes.FirstOrDefault(h => h.Hole == b.Source) is { } hole && hole.DesiredTarget != default)
            {
                if (b.Target == actor && hole.DesiredTarget != _order[slot])
                {
                    hints_++;
                    hints.Add("Pass tether!");
                }
                else if (b.Target != actor && hole.DesiredTarget == _order[slot])
                {
                    hints_++;
                    hints.Add("Take tether!");
                }
            }
        }

        if (hints_ == 0)
            base.AddHints(slot, actor, hints);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);

        if ((TetherID)tether.ID == TetherID.BlackHole && !_holes.Any(h => h.Hole == source))
        {
            _holes.Add(new() { Hole = source, CastsLeft = NumCasts is < 3 or >= 21 ? 1 : 3 });
            AssignHoles();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            foreach (var h in _holes)
                if (h.Hole == caster)
                    h.CastsLeft--;
            _holes.RemoveAll(h => h.CastsLeft <= 0);
            AssignHoles();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var bh in _holes)
        {
            if (CurrentBaits.FirstOrNull(b => b.Source == bh.Hole) is { } bait)
            {
                var isMine = bh.DesiredTarget == _order[pcSlot] || bh.DesiredTarget == default && bait.Target == pc;
                var color = isMine ? ArenaColor.Safe : ArenaColor.Danger;
                var width = (bait.Target == pc) == isMine ? 1 : 2;

                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(bait.Source.Position, bait.Target.Position, 0xFF000000, width + 1);
                Arena.AddLine(bait.Source.Position, bait.Target.Position, color, width);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var order = (SID)status.ID switch
        {
            SID.FirstInLine => 1,
            SID.SecondInLine => 2,
            SID.ThirdInLine => 3,
            _ => 0
        };

        if (order > 0 && Raid.TryFindSlot(actor, out var slot))
        {
            _order[slot].Order = order;
            if (_order[slot].Role == TargetRole.Unknown)
                _order[slot].Role = actor.Class.IsDD() ? TargetRole.DPS : TargetRole.Support;
        }

        if ((SID)status.ID == SID.Accretion && Raid.TryFindSlot(actor, out slot))
            _order[slot] = _order[slot] with { Role = TargetRole.Accretion };
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (ActiveBaitsOn(pc).Any(b => _holes.Any(h => h.DesiredTarget == _order[playerSlot])))
            return PlayerPriority.Interesting;

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }

    void AssignHoles()
    {
        foreach (var h in _holes)
            h.DesiredTarget = default;

        TargetOrder[] toApply = (NumCasts, _holes.Count) switch
        {
            // set 1
            (0, 1) => [new(TargetRole.DPS, 1)],
            (1, 2) => [new(TargetRole.DPS, 1), new(TargetRole.Support, 1)],
            // set 2
            (3, 3) => [new(TargetRole.DPS, 1), new(TargetRole.Support, 1), new(TargetRole.Accretion, 1)],
            (6, 3) => [new(TargetRole.DPS, 2), new(TargetRole.Support, 1), new(TargetRole.Accretion, 1)],
            (9, 3) => [new(TargetRole.DPS, 2), new(TargetRole.Support, 2), new(TargetRole.Accretion, 1)],
            // set 3
            (12, 3) => [new(TargetRole.DPS, 2), new(TargetRole.Support, 2), new(TargetRole.Accretion, 2)],
            (15, 3) => [new(TargetRole.DPS, 3), new(TargetRole.Support, 2), new(TargetRole.Accretion, 2)],
            (18, 3) => [new(TargetRole.DPS, 3), new(TargetRole.Support, 3), new(TargetRole.Accretion, 2)],
            // set 4
            (21, 2) => [new(TargetRole.DPS, 3), new(TargetRole.Support, 3)],
            (23, 1) => [new(TargetRole.Support, 3)],
            _ => []
        };

        if (toApply.Length > 0)
        {
            if (((UMAD)Module).KefkaP3() is not { } k3)
            {
                ReportError("unable to assign tethers - can't find kefka???");
                return;
            }
            foreach (var (h, r) in _holes.ClockOrderWith(bh => bh.Hole, k3, k3.Position + k3.Rotation.ToDirection()).Zip(toApply))
                h.DesiredTarget = r;
        }
    }
}

class P3DamningEdict(BossModule module) : Components.StandardAOEs(module, AID.DamningEdict, new AOEShapeRect(60, 40));
class P3HotTail(BossModule module) : Components.StandardAOEs(module, AID.LookUponMeAndDespair, new AOEShapeRect(100, 8));
