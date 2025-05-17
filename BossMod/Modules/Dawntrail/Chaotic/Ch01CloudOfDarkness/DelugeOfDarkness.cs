namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

// envcontrols:
// 00 = main bounds telegraph
// - 00200010 - phase 1
// - 00020001 - phase 2
// - 00040004 - remove telegraph (note that actual bounds are controlled by something else!)
class DelugeOfDarkness1(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCustom _shape = new(Ch01CloudOfDarkness.Phase1Bounds.Clipper.Difference(new(CurveApprox.Rect(new(100, 0), new(0, 100))), new(Ch01CloudOfDarkness.Phase1Bounds.Poly)));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DelugeOfDarkness1)
            _aoe = new(_shape, Module.Center, default, Module.CastFinishAt(spell));
    }
}

class DelugeOfDarkness2(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCustom _shape = new(Ch01CloudOfDarkness.Phase2Bounds.Clipper.Difference(new(CurveApprox.Rect(new(100, 0), new(0, 100))), new(Ch01CloudOfDarkness.Phase2Bounds.Poly)));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DelugeOfDarkness2)
            _aoe = new(_shape, Module.Center, default, Module.CastFinishAt(spell));
    }
}

class Phase2AIHints(BossModule module) : Components.GenericInvincible(module)
{
    [Flags]
    enum Position
    {
        None,
        Inside,
        Outside
    }

    private readonly Position[] _playerPositions = new Position[PartyState.MaxAllianceSize];

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var pos = _playerPositions[slot];
        var e = Enumerable.Empty<Actor>();

        if (pos.HasFlag(Position.Inside))
            e = e.Concat(Module.Enemies(OID.Atomos)).Concat(Module.Enemies(OID.StygianShadow));

        if (pos.HasFlag(Position.Outside))
            e = e.Concat([Module.PrimaryActor]);

        return e;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.InnerDarkness:
                SetState(actor, Position.Inside);
                break;
            case SID.OuterDarkness:
                SetState(actor, Position.Outside);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.InnerDarkness:
                ClearState(actor, Position.Inside);
                break;
            case SID.OuterDarkness:
                ClearState(actor, Position.Outside);
                break;
        }
    }

    private void SetState(Actor a, Position flag)
    {
        if (Raid.TryFindSlot(a, out var slot))
            _playerPositions[slot] |= flag;
    }

    private void ClearState(Actor a, Position flag)
    {
        if (Raid.TryFindSlot(a, out var slot))
            _playerPositions[slot] &= ~flag;
    }
}

class Phase2OuterRing(BossModule module) : Components.GenericAOEs(module)
{
    public bool Dangerous;
    private AOEInstance? _aoe;

    private static readonly AOEShapeDonut _shape = new(34, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DarkDominion)
            _aoe = new(_shape, Module.Center, default, Module.CastFinishAt(spell, 1.1f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 2)
            return;
        switch (state)
        {
            case 0x00020001:
                Dangerous = true;
                break;
            case 0x00080004:
                Dangerous = false;
                _aoe = null;
                break;
            default:
                ReportError($"Unexpected envcontrol {state:X8}");
                break;
        }
    }
}

class Phase2InnerCells(BossModule module) : BossComponent(module)
{
    private readonly DateTime[] _breakTime = new DateTime[28];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var cell = CellIndex(actor.Position - Module.Center) - 3;
        var breakTime = cell >= 0 && cell < _breakTime.Length ? _breakTime[cell] : default;
        if (breakTime != default)
        {
            var remaining = Math.Max(0, (breakTime - WorldState.CurrentTime).TotalSeconds);
            hints.Add($"Cell breaks in {remaining:f1}s", remaining < 10);
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        // 03-1E = mid squares
        // - 08000001 - init
        // - 00200010 - become occupied
        // - 02000001 - become free
        // - 00800040 - player is standing for too long (38s), will break soon (in 6s)
        // - 00080004 - break
        // - 00020001 - repair
        // - arrangement:
        //      04             0B
        //   03 05 06 07 0E 0D 0C 0A
        //      08             0F
        //      09             10
        //      17             1E
        //      16             1D
        //   11 13 14 15 1C 1B 1A 18
        //      12             19
        if (index is < 3 or > 30)
            return;
        _breakTime[index - 3] = state switch
        {
            0x00200010 => WorldState.FutureTime(44),
            0x00800040 => WorldState.FutureTime(6),
            _ => default,
        };
    }

    private int CoordinateToCell(float c) => (int)Math.Floor(c / 6);
    private int CellIndex(WDir offset) => CellIndex(CoordinateToCell(offset.X), CoordinateToCell(offset.Z));
    private int CellIndex(int x, int y) => (x, y) switch
    {
        (-4, -3) => 3,
        (-3, -4) => 4,
        (-3, -3) => 5,
        (-2, -3) => 6,
        (-1, -3) => 7,
        (-3, -2) => 8,
        (-3, -1) => 9,
        (+3, -3) => 10,
        (+2, -4) => 11,
        (+2, -3) => 12,
        (+1, -3) => 13,
        (+0, -3) => 14,
        (+2, -2) => 15,
        (+2, -1) => 16,
        (-4, +2) => 17,
        (-3, +3) => 18,
        (-3, +2) => 19,
        (-2, +2) => 20,
        (-1, +2) => 21,
        (-3, +1) => 22,
        (-3, +0) => 23,
        (+3, +2) => 24,
        (+2, +3) => 25,
        (+2, +2) => 26,
        (+1, +2) => 27,
        (+0, +2) => 28,
        (+2, +1) => 29,
        (+2, +0) => 30,
        _ => -1
    };
}
