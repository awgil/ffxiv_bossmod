namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D99TheFinalVerse;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x47, Helper type
    Boss = 0x48EA, // R28.500, x1
    EminentGrief = 0x486D, // R1.000, x2
    DevouredEater = 0x48EB, // R15.000, x1, Part type
    VodorigaMinion = 0x48EC, // R1.200, x0 (spawn during fight)
    UnkObj = 0x48ED, // R1.000, x0 (spawn during fight)
    AbyssalBlaze = 0x1EBE70,
}

public enum AID : uint
{
    BallOfFireFast = 44061, // Boss->self, 6.0s cast, single-target
    BallOfFirePuddle = 44062, // Helper->location, 2.1s cast, range 6 circle

    ChainsOfCondemnationFastCast = 44063, // Boss->location, 5.3+0.7s cast, single-target
    ChainsOfCondemnationFast = 44064, // Helper->location, 6.0s cast, range 30 circle

    BladeOfFirstLightInFast = 44065, // DevouredEater->self, 5.2+0.8s cast, single-target
    BladeOfFirstLightOutFast = 44066, // DevouredEater->self, 5.2+0.8s cast, single-target
    BladeOfFirstLightFast = 44067, // Helper->self, 6.0s cast, range 30 width 15 rect

    BallOfFireSlow = 44068, // Boss->self, 9.0s cast, single-target

    ChainsOfCondemnationSlowCast = 44069, // Boss->location, 8.3+0.7s cast, single-target
    ChainsOfCondemnationSlow = 44070, // Helper->location, 9.0s cast, range 30 circle

    BladeOfFirstLightInSlow = 44071, // DevouredEater->self, 8.2+0.8s cast, single-target
    BladeOfFirstLightOutSlow = 44072, // DevouredEater->self, 8.2+0.8s cast, single-target
    BladeOfFirstLightSlow = 44073, // Helper->self, 9.0s cast, range 30 width 15 rect

    AbyssalBlazeHorizontalFirst = 44074, // Boss->self, 3.0s cast, single-target
    AbyssalBlazeVerticalFirst = 44075, // Boss->self, 3.0s cast, single-target
    AbyssalBlazeHorizontalSecond = 44076, // Boss->self, no cast, single-target
    AbyssalBlazeVerticalSecond = 44077, // Boss->self, no cast, single-target

    CrystalAppear = 44078, // Helper->location, no cast, single-target
    AbyssalBlazeFirst = 44079, // Helper->location, 7.0s cast, range 5 circle
    AbyssalBlazeRest = 44080, // Helper->location, no cast, range 5 circle

    BoundsOfSinCast = 44081, // DevouredEater->self, 3.3+0.7s cast, single-target
    BoundsOfSinBind = 44082, // Helper->self, 4.0s cast, range 40 circle
    BoundsOfSinIcicle = 44083, // Helper->self, 3.0s cast, range 3 circle
    BoundsOfSinJail = 44084, // Helper->self, no cast, range 8 circle

    SpinelashStart = 44085, // Boss->self, 2.0s cast, single-target
    SpinelashCast = 44086, // Boss->self, 1.0+0.8s cast, single-target

    DrainLightFast = 44088, // Boss->self, 7.0s cast, range 50 width 50 rect
    DrainLightSlow = 44089, // Boss->self, 12.0s cast, range 50 width 50 rect
    DrainDarkFastVisual = 44090, // DevouredEater->self, 6.0+1.0s cast, single-target
    DrainDarkFast = 44091, // EminentGrief->self, 7.0s cast, range 50 width 50 rect
    DrainDarkSlowVisual = 44092, // DevouredEater->self, 11.0+1.0s cast, single-target
    DrainDarkSlow = 44093, // EminentGrief->self, 12.0s cast, range 50 width 50 rect

    AutoVisual1 = 44094, // Boss->self, no cast, single-target
    AutoVisual2 = 44095, // DevouredEater->self, no cast, single-target
    Auto1 = 44096, // Helper->player, 0.8s cast, single-target
    Auto2 = 44802, // EminentGrief->player, 0.5s cast, single-target
    Auto3 = 44813, // Helper->player, 0.8s cast, single-target
    Auto4 = 44820, // EminentGrief->player, 0.5s cast, single-target

    BloodyClaw = 45114, // VodorigaMinion->player, no cast, single-target
    TerrorEye = 45115, // VodorigaMinion->location, 3.0s cast, range 6 circle
    Spinelash = 45118, // Helper->self, 1.8s cast, range 60 width 4 rect
    VodorigaAuto = 45196, // VodorigaMinion->player, no cast, single-target

    Unk1 = 44087, // Boss->self, no cast, single-target
    Unk2 = 44270, // Helper->Boss, no cast, single-target
    Unk3 = 44314, // Helper->none, no cast, single-target
}

public enum SID : uint
{
    DarkVengeance = 4559, // none->player, extra=0x0
    LightVengeance = 4560, // none->player, extra=0x0
    Bind = 4510, // Helper->player, extra=0x0
    Rehabilitation = 4191, // none->DevouredEater/Boss, extra=0x1/0x2/0x3
    DamageUp = 2550, // none->Boss/DevouredEater, extra=0x1/0x2/0x3
    ChainsOfCondemnation = 4562, // Helper->player, extra=0x0
    BorrowedTime = 4561, // none->Boss, extra=0x0
}

public enum IconID : uint
{
    Spinelash = 234, // player->self
}

[Flags]
public enum Color
{
    None,
    Dark,
    Light
}

public class Buff
{
    public Color Color;
    public DateTime Expire;
}

class LightDark(BossModule module) : BossComponent(module)
{

    private readonly Buff[] _playerStates = Utils.GenArray(4, () => new Buff());

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkVengeance && Raid.TryFindSlot(actor, out var slot))
        {
            _playerStates[slot].Color |= Color.Dark;
            _playerStates[slot].Expire = status.ExpireAt;
        }

        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot2))
        {
            _playerStates[slot2].Color |= Color.Light;
            _playerStates[slot2].Expire = status.ExpireAt;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkVengeance && Raid.TryFindSlot(actor, out var slot))
            _playerStates[slot].Color ^= Color.Dark;

        if ((SID)status.ID == SID.LightVengeance && Raid.TryFindSlot(actor, out var slot2))
            _playerStates[slot2].Color ^= Color.Light;
    }

    public float ColorLeft(int slot, Color color) => _playerStates[slot].Color.HasFlag(color) ? (float)(_playerStates[slot].Expire - WorldState.CurrentTime).TotalSeconds : 0;

    public Color GetColor(int slot) => _playerStates.BoundSafeAt(slot)?.Color ?? Color.None;

    public static RelSimplifiedComplexPolygon ColorShape(Color c) => c == Color.Light ? D99TheFinalVerse.LightShape : D99TheFinalVerse.DarkShape;
    public static RelSimplifiedComplexPolygon OppositeColorShape(Color c) => c == Color.Light ? D99TheFinalVerse.DarkShape : D99TheFinalVerse.LightShape;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_playerStates[slot].Color == Color.None)
        {
            var ctr = Arena.Center;
            hints.GoalZones.Add(p => D99TheFinalVerse.DarkShape.Contains(p - ctr) || D99TheFinalVerse.LightShape.Contains(p - ctr) ? 100 : 0);
        }
    }
}

class BossLightDark(BossModule module) : Components.GenericInvincible(module)
{
    private readonly LightDark _lightDark = module.FindComponent<LightDark>()!;

    private Actor? Grief;
    private Actor? Eater;

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var color = _lightDark.GetColor(slot);
        if (!color.HasFlag(Color.Dark) && Eater != null)
            yield return Eater;
        if (!color.HasFlag(Color.Light) && Grief != null)
            yield return Grief;
    }

    public override void Update()
    {
        Grief ??= Module.PrimaryActor;
        Eater ??= Module.Enemies(OID.DevouredEater).FirstOrDefault();
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CalcHPDifference() is var (boss, pct) && pct < 25)
            hints.Add($"HP: {boss} +{pct:f1}%");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (CalcHPDifference() is var (boss, pct) && pct >= 25)
            hints.Add($"HP: {boss} +{pct:f1}%");
    }

    private (string Boss, float Percent)? CalcHPDifference()
    {
        if (Grief is { } g && Eater is { } e && g.HPMP.CurHP > 1 && e.HPMP.CurHP > 1)
        {
            var ratioDiff = g.HPRatio - e.HPRatio;
            return (ratioDiff > 0 ? "Eminent Grief" : "Devoured Eater", MathF.Abs(ratioDiff) * 100);
        }

        return null;
    }
}

class DrainAether(BossModule module) : Components.CastCounterMulti(module, [AID.DrainLightFast, AID.DrainLightSlow, AID.DrainDarkFast, AID.DrainDarkSlow])
{
    private readonly LightDark _ld = module.FindComponent<LightDark>()!;
    private readonly List<(Color, DateTime)> _colors = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            // eminent grief
            case AID.DrainLightFast:
            case AID.DrainLightSlow:
                _colors.Add((Color.Light, Module.CastFinishAt(spell)));
                _colors.SortBy(c => c.Item2);
                break;
            case AID.DrainDarkFast:
            case AID.DrainDarkSlow:
                _colors.Add((Color.Dark, Module.CastFinishAt(spell)));
                _colors.SortBy(c => c.Item2);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_colors.Count > 0)
        {
            var nextColor = _colors[0].Item1;
            hints.Add($"Correct color: {nextColor}", !_ld.GetColor(slot).HasFlag(nextColor));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_colors.Count > 0)
        {
            var (nextColor, deadline) = _colors[0];
            if (_ld.GetColor(slot).HasFlag(nextColor))
            {
                // avoid moving into other zone
                var badShape = LightDark.OppositeColorShape(nextColor);
                var ctr = Arena.Center;
                hints.AddForbiddenZone(p => badShape.Contains(p - ctr), deadline.AddSeconds(-1));
            }
            else
            {
                // move to correct zone
                var targetShape = LightDark.ColorShape(nextColor);
                var ctr = Arena.Center;
                hints.AddForbiddenZone(p => !targetShape.Contains(p - ctr), deadline.AddSeconds(-1));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            if (_colors.Count > 0)
                _colors.RemoveAt(0);
        }
    }
}

class BladeOfFirstLight(BossModule module) : Components.GroupedAOEs(module, [AID.BladeOfFirstLightSlow, AID.BladeOfFirstLightFast], new AOEShapeRect(30, 7.5f));

class BallOfFireBait(BossModule module) : BossComponent(module)
{
    private bool _active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BallOfFireFast:
            case AID.BallOfFireSlow:
                _active = true;
                break;
            case AID.BallOfFirePuddle:
                _active = false;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_active)
            Arena.AddCircle(pc.Position, 6, ArenaColor.Danger);
    }
}

class BallOfFirePuddle(BossModule module) : Components.StandardAOEs(module, AID.BallOfFirePuddle, 6);

class BoundsOfSinIcicle(BossModule module) : Components.StandardAOEs(module, AID.BoundsOfSinIcicle, 3, maxCasts: 11);
class BoundsOfSinJail(BossModule module) : Components.GenericAOEs(module, AID.BoundsOfSinJail)
{
    private DateTime _activation;
    private bool _risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeCircle(8), Arena.Center, default, _activation, Risky: _risky);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BoundsOfSinCast)
            _activation = Module.CastFinishAt(spell, 7.1f);

        if ((AID)spell.Action.ID == AID.BoundsOfSinIcicle)
            _risky = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BoundsOfSinJail)
        {
            NumCasts++;
            _risky = false;
            _activation = default;
        }
    }
}

class BoundsOfSinCollision(BossModule module) : BossComponent(module)
{
    private BitMask Icicles;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index < 12)
        {
            if (state == 0x00020001)
                Icicles.Set(index);
            else if (state == 0x00080004)
                Icicles.Clear(index);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bit in Icicles.SetBits())
        {
            var angle = (180 - bit * 30).Degrees();
            var c = ShapeContains.Circle(Arena.Center + angle.ToDirection() * 7, 3);
            hints.TemporaryObstacles.Add(c);
        }
    }
}

class SpinelashBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Spinelash)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeRect(30, 2, 30), WorldState.FutureTime(6.3f), true));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Spinelash)
            CurrentBaits.Clear();
    }
}

class Spinelash(BossModule module) : Components.StandardAOEs(module, AID.Spinelash, new AOEShapeRect(60, 2));

class ChainsOfCondemnation(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChainsOfCondemnationFast or AID.ChainsOfCondemnationSlow)
            Array.Fill(PlayerStates, new(Requirement.NoMove, Module.CastFinishAt(spell)));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            SetState(slot, new(Requirement.NoMove, WorldState.CurrentTime));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}

class AbyssalBlaze(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    private WDir _nextDir;

    private readonly List<(WPos Source, WDir Direction)> _orbs = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbyssalBlazeHorizontalFirst or AID.AbyssalBlazeHorizontalSecond)
            _nextDir = new WDir(1, 0);

        if ((AID)spell.Action.ID is AID.AbyssalBlazeVerticalFirst or AID.AbyssalBlazeVerticalSecond)
            _nextDir = new WDir(0, 1);

        if ((AID)spell.Action.ID == AID.CrystalAppear)
            _orbs.Add((spell.TargetXZ, _nextDir));

        if ((AID)spell.Action.ID is AID.AbyssalBlazeFirst)
        {
            var lines = Lines.Where(l => l.Next.InCircle(spell.TargetXZ, 1));
            foreach (var l in lines)
                AdvanceLine(l, spell.TargetXZ);
        }

        if ((AID)spell.Action.ID is AID.AbyssalBlazeRest)
        {
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(spell.TargetXZ, 0.5f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], spell.TargetXZ);
            else
                ReportError($"unrecognized exaflare at {spell.TargetXZ}");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AbyssalBlazeHorizontalFirst or AID.AbyssalBlazeVerticalFirst)
        {
            _orbs.Clear();
            Lines.Clear();
        }

        if ((AID)spell.Action.ID == AID.AbyssalBlazeFirst)
        {
            foreach (var orb in _orbs)
            {
                if (orb.Source.InCircle(spell.LocXZ, 1))
                {
                    AddLine(spell.LocXZ, orb.Direction * 4, Module.CastFinishAt(spell));
                    AddLine(spell.LocXZ, orb.Direction * -4, Module.CastFinishAt(spell));
                }
            }
        }
    }

    private void AddLine(WPos source, WDir advance, DateTime next)
    {
        var numExplosions = 0;
        var tmp = source;
        while (tmp.InRect(Arena.Center, 0.Degrees(), 14.5f, 14.5f, 19.5f))
        {
            tmp += advance;
            numExplosions++;
        }

        Lines.Add(new()
        {
            Next = source,
            Advance = advance,
            NextExplosion = next,
            TimeToMove = 1.2f,
            ExplosionsLeft = numExplosions,
            MaxShownExplosions = Math.Min(5, numExplosions)
        });
    }
}

class D99TheFinalVerseStates : StateMachineBuilder
{
    public D99TheFinalVerseStates(BossModule module) : base(module)
    {
        // no extra checks needed, boss is locked at 1 HP until devoured eater is also dead
        TrivialPhase()
            .ActivateOnEnter<LightDark>()
            .ActivateOnEnter<BossLightDark>()
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<BallOfFireBait>()
            .ActivateOnEnter<BallOfFirePuddle>()
            .ActivateOnEnter<BoundsOfSinIcicle>()
            .ActivateOnEnter<BoundsOfSinJail>()
            .ActivateOnEnter<BoundsOfSinCollision>()
            .ActivateOnEnter<SpinelashBait>()
            .ActivateOnEnter<Spinelash>()
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<AbyssalBlaze>()
            .ActivateOnEnter<DrainAether>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1041, NameID = 14037)]
public class D99TheFinalVerse(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsRect(20, 15))
{
    public static readonly RelSimplifiedComplexPolygon LightShape = Utils.LoadFromAssembly<RelSimplifiedComplexPolygon>("BossMod.Modules.Dawntrail.DeepDungeon.PilgrimsTraverse.D99TheFinalVerse.Light.json");
    public static readonly RelSimplifiedComplexPolygon DarkShape = Utils.LoadFromAssembly<RelSimplifiedComplexPolygon>("BossMod.Modules.Dawntrail.DeepDungeon.PilgrimsTraverse.D99TheFinalVerse.Dark.json");
}
