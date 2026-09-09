namespace BossMod.Dawntrail.Ultimate.UMAD;

class P2WingsOfDestructionLeftRight(BossModule module) : Components.GroupedAOEs(module, [AID.WingsOfDestructionL, AID.WingsOfDestructionR], new AOEShapeRect(80, 20));

// trine triggers 10.7s after EObjAnim 00100020 of either 1EBFB3 or 1EBFB2
// trine is an equilateral triangle with an edge length of 10, so circumcircle radius cR = 10 * √3 / 3
// so casts are at (obj.X + cR, obj.Z), (obj.X - cR / 2, obj.Z + 5), (obj.X - cR / 2, obj.Z - 5) for 0x1EBFB2, flipped horizontally for 0x1EBFB3
class P2Trine(BossModule module) : Components.GenericAOEs(module, AID.Trine)
{
    static readonly float TrineRadius = 10 * MathF.Sqrt(3) / 3;

    readonly List<List<AOEInstance>> _waves = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var group = 0;
        foreach (var waves in _waves.Take(2))
        {
            foreach (var w in waves)
                yield return w with { Risky = group == 0, Color = group == 0 ? ArenaColor.Danger : ArenaColor.AOE };
            group++;
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00100020)
            return;

        // FIXME when they patch this
        var oidToTest = actor.Position.AlmostEqual(new(88.45f, 90), 1) ? 0x1EBFB3 : actor.OID;

        var activation = WorldState.FutureTime(10.7f);

        var flipX = oidToTest switch
        {
            0x1EBFB2 => 1,
            0x1EBFB3 => -1,
            _ => 0
        };

        if (flipX == 0)
            return;

        AddWave(new(new AOEShapeCircle(6), actor.Position + new WDir(TrineRadius * flipX, 0), default, activation));
        AddWave(new(new AOEShapeCircle(6), actor.Position + new WDir(TrineRadius * flipX * -0.5f, 5), default, activation));
        AddWave(new(new AOEShapeCircle(6), actor.Position + new WDir(TrineRadius * flipX * -0.5f, -5), default, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_waves.Count > 0)
            {
                if (_waves[0].Count > 0)
                    _waves[0].RemoveAt(0);
                if (_waves[0].Count == 0)
                    _waves.RemoveAt(0);
            }
        }
    }

    void AddWave(AOEInstance aoe)
    {
        if (_waves.Count is 0)
            _waves.Add([]);
        else if (_waves.Count == 1 && _waves[0].Count == 9)
            _waves.Add([]);
        else if (_waves.Count == 2 && _waves[^1].Count == 3)
            _waves.Add([]);

        _waves[^1].Add(aoe);
    }
}

class P2WingsOfDestructionBuster(BossModule module) : Components.GenericBaitAway(module, AID.WingsOfDestructionBuster, centerAtTarget: true)
{
    DateTime _activation;
    Actor? _source;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WingsOfDestructionBusterCast)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _source = null;
            _activation = default;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (_source == null)
            return;

        var targets = Raid.WithoutSlot().SortedByRange(_source.Position).ToList();
        if (targets.Count > 0)
            CurrentBaits.Add(new(_source, targets[0], new AOEShapeCircle(7), _activation));
        if (targets.Count > 1)
            CurrentBaits.Add(new(_source, targets[^1], new AOEShapeCircle(7), _activation));
    }
}
