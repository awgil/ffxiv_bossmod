namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class ElectropeEdgeWitchgleam(BossModule module) : Components.GenericAOEs(module, AID.ElectropeEdgeWitchgleamAOE)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCross _shape = new(60, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElectropeEdgeWitchgleam)
            _aoe = new(_shape, caster.Position, 45.Degrees(), Module.CastFinishAt(spell, 1.2f));
    }
}

class ElectropeEdgeSpark1(BossModule module) : Components.StandardAOEs(module, AID.ElectropeEdgeSpark1, new AOEShapeRect(5, 5, 5));
class ElectropeEdgeSpark2(BossModule module) : Components.StandardAOEs(module, AID.ElectropeEdgeSpark2, new AOEShapeRect(15, 15, 15));
class ElectropeEdgeSidewiseSparkR(BossModule module) : Components.StandardAOEs(module, AID.ElectropeEdgeSidewiseSparkR, new AOEShapeCone(60, 90.Degrees()));
class ElectropeEdgeSidewiseSparkL(BossModule module) : Components.StandardAOEs(module, AID.ElectropeEdgeSidewiseSparkL, new AOEShapeCone(60, 90.Degrees()));

class ElectropeEdgeStar(BossModule module) : Components.UniformStackSpread(module, 6, 6, alwaysShowSpreads: true)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Marker)
        {
            switch (status.Extra)
            {
                case 0x2F0:
                    // TODO: can target any role, if not during cage?..
                    var cage = Module.FindComponent<LightningCage>();
                    var targets = Raid.WithSlot(true);
                    targets = cage != null ? targets.WhereSlot(i => cage.Order[i] == 2) : targets.WhereActor(p => p.Class.IsSupport());
                    AddStacks(targets.Actors(), status.ExpireAt.AddSeconds(1));
                    break;
                case 0x2F1:
                    AddSpreads(Raid.WithoutSlot(true), status.ExpireAt.AddSeconds(1));
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FourStar or AID.EightStar)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

class LightningCage(BossModule module) : Components.GenericAOEs(module, AID.LightningCageAOE)
{
    public int NumSparks;
    public int NumGleams;
    public readonly int[] Order = new int[PartyState.MaxPartySize];
    private readonly int[] _gleams = new int[PartyState.MaxPartySize];
    private BitMask _aoePattern;
    private DateTime _activation;

    public bool Active => _aoePattern.Any();

    private static readonly (BitMask pattern, int safe, int two1, int two2, int three1, int three2)[] _patterns =
    [
        (BitMask.Build(0, 1, 3,  4, 10, 18, 24, 26, 28, 33, 34, 35),  2,  8, 12, 32, 36),
        (BitMask.Build(1, 4, 8, 12, 16, 17, 18, 19, 24, 28, 33, 36), 20,  3, 35,  0, 32),
        (BitMask.Build(1, 2, 3,  8, 10, 12, 18, 26, 32, 33, 35, 36), 34, 24, 28,  0,  4),
        (BitMask.Build(0, 3, 8, 12, 17, 18, 19, 20, 24, 28, 32, 35), 16,  1, 33,  4, 36),
    ];

    private static readonly AOEShapeRect _cell = new(4, 4, 4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var i in _aoePattern.SetBits())
            yield return new(_cell, CellCenter(i), default, _activation);
        foreach (var i in SafeCells(slot))
            yield return new(_cell, CellCenter(i), default, _activation, ArenaColor.SafeFromAOE, false);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumGleams >= 8)
            hints.Add($"Order: {Order[slot]}, position: {_gleams[slot] + Order[slot] - 1}", Active && !SafeCells(slot).Any(i => _cell.Check(actor.Position, CellCenter(i), default)));
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ElectricalCondenser && Raid.TryFindSlot(actor.InstanceID, out var slot))
            Order[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 30 ? 1 : 2;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoePattern.Set(PositionToIndex(caster.Position));
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _aoePattern.Reset();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LightningCageWitchgleamAOE:
                ++NumGleams;
                foreach (var t in spell.Targets)
                    if (Raid.TryFindSlot(t.ID, out var slot))
                        ++_gleams[slot];
                break;
            case AID.LightningCageSpark2:
            case AID.LightningCageSpark3:
                ++NumSparks;
                break;
        }
    }

    private IEnumerable<int> SafeCells(int slot)
    {
        var pattern = Array.FindIndex(_patterns, p => p.pattern == _aoePattern);
        if (pattern < 0)
            yield break;
        var nextOrder = NumSparks switch
        {
            < 4 => 1,
            < 8 => 2,
            _ => 3
        };
        if (Order[slot] != nextOrder)
        {
            yield return _patterns[pattern].safe;
        }
        else if (_gleams[slot] + nextOrder == 3)
        {
            yield return _patterns[pattern].two1;
            yield return _patterns[pattern].two2;
        }
        else if (_gleams[slot] + nextOrder == 4)
        {
            yield return _patterns[pattern].three1;
            yield return _patterns[pattern].three2;
        }
    }

    private int CoordinateToIndex(float c) => c switch
    {
        < 88 => 0,
        < 96 => 1,
        < 104 => 2,
        < 112 => 3,
        _ => 4
    };
    private int PositionToIndex(WPos pos) => (CoordinateToIndex(pos.Z) << 3) | CoordinateToIndex(pos.X);
    private float CellCenterCoordinate(int index) => 84 + 8 * index;
    private WPos CellCenter(int index) => new(CellCenterCoordinate(index & 7), CellCenterCoordinate(index >> 3));
}

class LightningCageWitchgleam(BossModule module) : Components.GenericBaitAway(module, AID.LightningCageWitchgleamAOE)
{
    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LightningCageWitchgleam)
            foreach (var p in Raid.WithoutSlot(true))
                CurrentBaits.Add(new(caster, p, _shape, Module.CastFinishAt(spell, 1.2f)));
    }
}
