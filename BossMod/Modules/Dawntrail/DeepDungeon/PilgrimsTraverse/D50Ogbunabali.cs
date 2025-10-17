namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D50Ogbunabali;

public enum OID : uint
{
    Boss = 0x4872, // R5.670, x1
    Helper = 0x233C, // R0.500, x8, Helper type
}

public enum AID : uint
{
    AutoAttack = 45130, // Boss->player, no cast, single-target
    Jump = 43539, // Boss->location, no cast, single-target
    Liquefaction = 43531, // Boss->self, 3.0s cast, single-target, activates quicksand
    FallingRock = 43532, // Helper->self, 3.0s cast, range 5 circle
    Sandpit = 43533, // Boss->self, 3.0s cast, single-target
    PitAmbushCast = 43534, // Boss->self, 3.0s cast, range 6 circle
    PitAmbushInstant1 = 43535, // Boss->location, no cast, range 6 circle
    PitAmbushInstant2 = 43536, // Boss->location, no cast, range 6 circle
    Windraiser = 43537, // Boss->self, 3.0s cast, single-target
    BitingWindVoidzone = 45113, // Helper->self, no cast, range 3 circle
    BitingWindKB = 43538, // Helper->self, 8.0s cast, range 20 circle, 20y knockback, negated by quicksand
}

public enum SID : uint
{
    SixFulmsUnder = 567, // none->player, extra=0x40E/0x410/0x412/0x400
    BreakerOfWind = 4625, // none->player, extra=0x0
    Windburn1 = 3069, // none->player, extra=0x0
    Windburn2 = 3070, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tracking = 640, // player->self
    Countdown = 587, // Helper->self, whirlwind knockback indicator
}

public class Rocks(BossModule module) : BossComponent(module)
{
    public static readonly AOEShapeCustom RockShape = new(new RelSimplifiedComplexPolygon([
        new RelPolygonWithHoles([new WDir(-8.46f, -9.37f), new WDir(-8.24f, -9.31f), new WDir(-8.18f, -9.02f), new WDir(-7.87f, -9.02f), new WDir(-7.69f, -9.07f), new WDir(-7.41f, -8.79f), new WDir(-7.42f, -7.87f), new WDir(-7.00f, -7.54f), new WDir(-7.15f, -7.09f), new WDir(-7.76f, -6.65f), new WDir(-7.96f, -6.64f), new WDir(-8.33f, -6.11f), new WDir(-8.46f, -6.04f), new WDir(-8.87f, -6.53f), new WDir(-9.21f, -6.42f), new WDir(-9.25f, -6.26f), new WDir(-9.54f, -6.19f), new WDir(-9.83f, -6.51f), new WDir(-10.22f, -6.98f), new WDir(-10.49f, -7.59f), new WDir(-10.59f, -7.82f), new WDir(-10.49f, -7.99f), new WDir(-10.38f, -8.09f), new WDir(-10.45f, -8.23f), new WDir(-9.73f, -9.01f), new WDir(-9.47f, -8.99f), new WDir(-9.25f, -9.36f), new WDir(-9.01f, -9.42f), new WDir(-8.58f, -9.42f)]),
        new([new WDir(7.08f, -11.09f), new WDir(7.98f, -10.49f), new WDir(8.10f, -9.94f), new WDir(8.09f, -9.38f), new WDir(7.51f, -8.91f), new WDir(7.58f, -8.84f), new WDir(7.60f, -8.55f), new WDir(7.08f, -7.61f), new WDir(6.78f, -7.55f), new WDir(6.65f, -7.62f), new WDir(6.59f, -7.75f), new WDir(6.05f, -8.18f), new WDir(5.82f, -8.40f), new WDir(5.57f, -8.48f), new WDir(5.45f, -8.29f), new WDir(5.30f, -7.98f), new WDir(4.77f, -8.50f), new WDir(4.65f, -8.41f), new WDir(3.84f, -9.29f), new WDir(4.23f, -9.51f), new WDir(4.09f, -10.44f), new WDir(4.02f, -10.48f), new WDir(4.21f, -10.57f), new WDir(4.70f, -11.06f), new WDir(4.80f, -11.15f), new WDir(5.11f, -11.21f), new WDir(5.73f, -10.92f), new WDir(6.31f, -11.09f), new WDir(6.33f, -11.35f), new WDir(6.59f, -11.36f)]),
        new([new WDir(0.33f, -6.33f), new WDir(1.03f, -6.28f), new WDir(1.14f, -6.19f), new WDir(1.29f, -5.81f), new WDir(1.41f, -5.13f), new WDir(1.65f, -5.00f), new WDir(1.73f, -4.73f), new WDir(1.27f, -3.95f), new WDir(0.76f, -3.22f), new WDir(0.45f, -3.07f), new WDir(-0.47f, -3.30f), new WDir(-1.11f, -4.14f), new WDir(-1.22f, -4.22f), new WDir(-1.46f, -4.44f), new WDir(-1.97f, -5.03f), new WDir(-1.89f, -5.36f), new WDir(-1.95f, -5.43f), new WDir(-1.83f, -5.51f), new WDir(-1.67f, -5.54f), new WDir(-1.37f, -5.70f), new WDir(-1.03f, -5.85f), new WDir(-0.98f, -5.94f), new WDir(-0.71f, -6.42f), new WDir(0.17f, -6.42f)]),
        new([new WDir(-7.84f, -0.77f), new WDir(-7.65f, -0.64f), new WDir(-7.53f, -0.07f), new WDir(-7.40f, 0.04f), new WDir(-7.06f, 0.22f), new WDir(-7.06f, -0.11f), new WDir(-6.60f, 0.24f), new WDir(-6.55f, 0.33f), new WDir(-6.05f, 1.86f), new WDir(-6.23f, 2.21f), new WDir(-6.45f, 2.24f), new WDir(-6.85f, 2.55f), new WDir(-7.57f, 2.41f), new WDir(-7.58f, 2.56f), new WDir(-7.75f, 2.75f), new WDir(-7.72f, 2.86f), new WDir(-8.69f, 3.16f), new WDir(-8.94f, 2.84f), new WDir(-9.15f, 1.68f), new WDir(-9.22f, 1.60f), new WDir(-9.23f, 1.42f), new WDir(-9.71f, 1.51f), new WDir(-9.42f, 0.81f), new WDir(-9.30f, 0.28f), new WDir(-9.45f, -0.04f), new WDir(-9.51f, -0.16f), new WDir(-9.01f, -0.86f), new WDir(-8.15f, -0.74f), new WDir(-8.07f, -0.93f)]),
        new([new WDir(12.28f, -2.68f), new WDir(12.57f, -2.21f), new WDir(12.55f, -2.15f), new WDir(12.70f, -2.02f), new WDir(12.91f, -1.98f), new WDir(13.14f, -2.14f), new WDir(13.38f, -2.02f), new WDir(14.07f, -0.57f), new WDir(13.89f, 0.01f), new WDir(13.60f, 0.37f), new WDir(12.81f, 0.49f), new WDir(12.64f, 0.72f), new WDir(12.70f, 0.79f), new WDir(11.83f, 1.33f), new WDir(11.63f, 1.31f), new WDir(11.12f, 1.07f), new WDir(11.03f, 0.89f), new WDir(11.10f, 0.47f), new WDir(10.94f, 0.05f), new WDir(10.82f, 0.03f), new WDir(10.56f, 0.19f), new WDir(10.38f, 0.13f), new WDir(10.12f, -0.69f), new WDir(9.94f, -1.88f), new WDir(10.32f, -2.01f), new WDir(10.53f, -1.96f), new WDir(10.79f, -2.58f), new WDir(10.99f, -2.75f), new WDir(11.36f, -2.85f), new WDir(11.43f, -3.02f), new WDir(11.57f, -3.03f)]),
        new([new WDir(-2.35f, 8.61f), new WDir(-2.11f, 8.69f), new WDir(-1.89f, 8.70f), new WDir(-1.62f, 8.97f), new WDir(-1.41f, 8.92f), new WDir(-1.24f, 9.87f), new WDir(-0.84f, 9.86f), new WDir(-0.70f, 10.14f), new WDir(-0.78f, 10.52f), new WDir(-1.01f, 10.65f), new WDir(-1.17f, 10.82f), new WDir(-1.76f, 11.21f), new WDir(-1.89f, 11.21f), new WDir(-2.21f, 11.72f), new WDir(-2.56f, 11.98f), new WDir(-2.96f, 11.41f), new WDir(-3.08f, 11.36f), new WDir(-3.63f, 11.59f), new WDir(-3.93f, 11.25f), new WDir(-4.34f, 10.74f), new WDir(-4.69f, 9.96f), new WDir(-4.75f, 9.64f), new WDir(-4.60f, 9.43f), new WDir(-3.90f, 8.68f), new WDir(-3.55f, 8.31f), new WDir(-3.46f, 8.43f), new WDir(-3.21f, 8.35f), new WDir(-2.83f, 8.29f)]),
        new([new WDir(8.14f, 5.46f), new WDir(9.09f, 6.05f), new WDir(9.30f, 6.71f), new WDir(9.25f, 7.22f), new WDir(8.62f, 7.79f), new WDir(8.64f, 8.06f), new WDir(8.17f, 9.10f), new WDir(7.66f, 9.20f), new WDir(7.48f, 9.11f), new WDir(7.36f, 8.86f), new WDir(6.74f, 8.37f), new WDir(6.57f, 8.40f), new WDir(6.41f, 8.73f), new WDir(6.31f, 8.80f), new WDir(5.62f, 8.23f), new WDir(4.79f, 7.33f), new WDir(4.98f, 7.09f), new WDir(5.17f, 7.00f), new WDir(5.02f, 6.13f), new WDir(5.28f, 5.81f), new WDir(5.56f, 5.60f), new WDir(5.54f, 5.49f), new WDir(5.61f, 5.39f), new WDir(6.03f, 5.34f), new WDir(6.21f, 5.31f), new WDir(6.75f, 5.52f), new WDir(6.79f, 5.60f), new WDir(7.00f, 5.61f), new WDir(7.21f, 5.52f), new WDir(7.18f, 5.17f), new WDir(7.60f, 5.16f)])
    ]));

    private readonly DateTime[] _drowning = Utils.MakeArray<DateTime>(4, default);

    private bool _quicksand;
    private DateTime _bitingWind;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0 && state == 0x00020001)
            _quicksand = true;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SixFulmsUnder && Raid.TryFindSlot(actor, out var slot))
            // sigh
            _drowning[slot] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SixFulmsUnder && Raid.TryFindSlot(actor, out var slot))
            _drowning[slot] = default;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BitingWindKB)
            _bitingWind = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BitingWindKB)
            _bitingWind = default;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_quicksand)
            return;

        RockShape.Draw(Arena, Arena.Center, default, _bitingWind > DateTime.MinValue ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_bitingWind != default && RockShape.Check(actor.Position, Arena.Center, default))
            hints.Add("Go to quicksand!");
        else if (_drowning[slot] != default && !RockShape.Check(actor.Position, Arena.Center, default))
            hints.Add("Stay out of quicksand!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var check = RockShape.CheckFn(Arena.Center, default);

        // movement speed in quicksand is 0.86 x base, so if not currently drowning we have 3.44s of leeway
        // if already drowning, movespeed has already been adjusted on the client so we can let pathfinder do its thing
        var drownTime = _drowning[slot] == default ? WorldState.FutureTime(3.44f) : _drowning[slot];

        if (_quicksand)
            hints.AddForbiddenZone(p => !check(p), drownTime);

        if (_bitingWind != default && _bitingWind < WorldState.FutureTime(2))
            hints.AddForbiddenZone(check, _bitingWind);
    }
}

class FallingRock(BossModule module) : Components.StandardAOEs(module, AID.FallingRock, 5);

class PitAmbush(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), AID.PitAmbushCast, AID.PitAmbushInstant1, 5, 2.1f, 4)
{
    private int _target = -1;
    private DateTime _start;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PitAmbushCast or AID.PitAmbushInstant1 or AID.PitAmbushInstant2)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            var advanced = Advance(pos, MoveDistance, WorldState.CurrentTime);
            if (advanced == null)
                ReportError($"unexpected cast from chasing AOE at {pos}");
            if (advanced?.NumRemaining <= 0)
                ExcludedTargets.Clear(Raid.FindSlot(advanced.Target.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == ActionFirst)
            _target = -1;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // bait away from center of arena so we have a safe zone to run to
        if (slot == _target)
            hints.AddForbiddenZone(ShapeContains.Circle(Arena.Center, 10), _start);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Tracking && Raid.TryFindSlot(actor, out var slot))
        {
            _target = slot;
            _start = WorldState.FutureTime(5.1f);
        }
    }
}

class Whirlwind(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            yield return new(new AOEShapeCircle(3), Arena.Center);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 1)
        {
            if (state == 0x00020001)
                _active = true;
            if (state == 0x00100004)
                _active = false;
        }
    }
}

class BitingWind(BossModule module) : Components.Knockback(module, AID.BitingWindKB, ignoreImmunes: true)
{
    private Actor? _caster;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_caster != null && Rocks.RockShape.Check(actor.Position, Arena.Center, default))
            yield return new(Arena.Center, 20, Module.CastFinishAt(_caster.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _caster = null;
    }
}

class D50OgbunabaliStates : StateMachineBuilder
{
    public D50OgbunabaliStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rocks>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<PitAmbush>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<BitingWind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1036, NameID = 14263)]
public class D50Ogbunabali(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(15));
