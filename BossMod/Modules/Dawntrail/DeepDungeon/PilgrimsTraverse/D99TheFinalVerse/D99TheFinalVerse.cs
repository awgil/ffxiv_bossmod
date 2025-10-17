#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D99TheFinalVerse;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x47, Helper type
    Boss = 0x48EA, // R28.500, x1
    EminentGrief = 0x486D, // R1.000, x2
    DevouredEater = 0x48EB, // R15.000, x1, Part type
    VodorigaMinion = 0x48EC, // R1.200, x0 (spawn during fight)
    _Gen_ = 0x48ED, // R1.000, x0 (spawn during fight)
    AbyssalBlaze = 0x1EBE70,
}

public enum AID : uint
{
    _Spell_Attack = 44094, // Boss->self, no cast, single-target
    _Spell_ = 44820, // EminentGrief->player, 0.5s cast, single-target
    _Spell_Attack1 = 44813, // Helper->player, 0.8s cast, single-target
    _Spell_Attack2 = 44095, // DevouredEater->self, no cast, single-target
    _Spell_1 = 44802, // EminentGrief->player, 0.5s cast, single-target
    _Spell_Attack3 = 44096, // Helper->player, 0.8s cast, single-target
    BallOfFireFast = 44061, // Boss->self, 6.0s cast, single-target
    BallOfFireSlow = 44068, // Boss->self, 9.0s cast, single-target
    BallOfFirePuddle = 44062, // Helper->location, 2.1s cast, range 6 circle
    _Weaponskill_BladeOfFirstLight5 = 44065, // DevouredEater->self, 5.2+0.8s cast, single-target
    _Weaponskill_BladeOfFirstLight3 = 44066, // DevouredEater->self, 5.2+0.8s cast, single-target
    _Weaponskill_BladeOfFirstLight4 = 44067, // Helper->self, 6.0s cast, range 30 width 15 rect
    _Weaponskill_BladeOfFirstLight2 = 44071, // DevouredEater->self, 8.2+0.8s cast, single-target
    _Weaponskill_BladeOfFirstLight = 44072, // DevouredEater->self, 8.2+0.8s cast, single-target
    _Weaponskill_BladeOfFirstLight1 = 44073, // Helper->self, 9.0s cast, range 30 width 15 rect
    _Spell_BoundsOfSin = 44081, // DevouredEater->self, 3.3+0.7s cast, single-target
    _Spell_BoundsOfSin1 = 44082, // Helper->self, 4.0s cast, range 40 circle
    _Weaponskill_BoundsOfSin = 44083, // Helper->self, 3.0s cast, range 3 circle
    _Spell_BoundsOfSin2 = 44084, // Helper->self, no cast, range 8 circle
    _Ability_Spinelash = 44085, // Boss->self, 2.0s cast, single-target
    _Weaponskill_Spinelash = 44086, // Boss->self, 1.0+0.8s cast, single-target
    _Weaponskill_Spinelash1 = 45118, // Helper->self, 1.8s cast, range 60 width 4 rect
    _Ability_ = 44087, // Boss->self, no cast, single-target
    _Spell_AbyssalBlaze = 44074, // Boss->self, 3.0s cast, single-target, horizontal first
    _Spell_AbyssalBlaze4 = 44075, // Boss->self, 3.0s cast, single-target, vertical first
    _Spell_AbyssalBlaze5 = 44076, // Boss->self, no cast, single-target, horizontal second
    _Spell_AbyssalBlaze1 = 44077, // Boss->self, no cast, single-target, vertical second
    _Spell_2 = 44078, // Helper->location, no cast, single-target
    _Spell_3 = 44314, // Helper->none, no cast, single-target
    _Spell_DrainAether3 = 44088, // Boss->self, 7.0s cast, range 50 width 50 rect
    _Spell_DrainAether2 = 44089, // Boss->self, 12.0s cast, range 50 width 50 rect
    _Spell_DrainAether = 44090, // DevouredEater->self, 6.0+1.0s cast, single-target
    _Spell_DrainAether1 = 44091, // EminentGrief->self, 7.0s cast, range 50 width 50 rect
    _Spell_DrainAether4 = 44092, // DevouredEater->self, 11.0+1.0s cast, single-target
    _Spell_DrainAether5 = 44093, // EminentGrief->self, 12.0s cast, range 50 width 50 rect
    _Spell_AbyssalBlaze2 = 44079, // Helper->location, 7.0s cast, range 5 circle
    _Spell_AbyssalBlaze3 = 44080, // Helper->location, no cast, range 5 circle
    _Spell_ChainsOfCondemnation = 44063, // Boss->location, 5.3+0.7s cast, single-target
    _Spell_ChainsOfCondemnation1 = 44064, // Helper->location, 6.0s cast, range 30 circle
    _Spell_TerrorEye = 45115, // VodorigaMinion->location, 3.0s cast, range 6 circle
    _AutoAttack_ = 45196, // VodorigaMinion->player, no cast, single-target
    _Weaponskill_BloodyClaw = 45114, // VodorigaMinion->player, no cast, single-target
    _Spell_4 = 44270, // Helper->Boss, no cast, single-target
    _Spell_ChainsOfCondemnation2 = 44069, // Boss->location, 8.3+0.7s cast, single-target
    _Spell_ChainsOfCondemnation3 = 44070, // Helper->location, 9.0s cast, range 30 circle
}

public enum SID : uint
{
    _Gen_DarkVengeance = 4559, // none->player, extra=0x0
    _Gen_LightVengeance = 4560, // none->player, extra=0x0
    _Gen_Bind = 4510, // Helper->player, extra=0x0
    _Gen_Rehabilitation = 4191, // none->DevouredEater/Boss, extra=0x1/0x2/0x3
    _Gen_DamageUp = 2550, // none->Boss/DevouredEater, extra=0x1/0x2/0x3
    _Gen_ = 3913, // Boss->Boss, extra=0x3C6
    _Gen_ChainsOfCondemnation = 4562, // Helper->player, extra=0x0
    _Gen_BorrowedTime = 4561, // none->Boss, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_lockon6_t0t = 234, // player->self
}

class LightDebuff(BossModule module) : Components.GenericInvincible(module)
{
    [Flags]
    enum Shade
    {
        None,
        Dark,
        Light
    }

    private Actor? Grief;
    private Actor? Eater;

    private readonly Shade[] _playerStates = Utils.MakeArray(4, Shade.None);

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var st = _playerStates.BoundSafeAt(slot);
        if (!st.HasFlag(Shade.Dark) && Eater != null)
            yield return Eater;
        if (!st.HasFlag(Shade.Light) && Grief != null)
            yield return Grief;
    }

    public override void Update()
    {
        Grief ??= Module.PrimaryActor;
        Eater ??= Module.Enemies(OID.DevouredEater).FirstOrDefault();
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_DarkVengeance && Raid.TryFindSlot(actor, out var slot))
            _playerStates[slot] |= Shade.Dark;

        if ((SID)status.ID == SID._Gen_LightVengeance && Raid.TryFindSlot(actor, out var slot2))
            _playerStates[slot2] |= Shade.Light;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_DarkVengeance && Raid.TryFindSlot(actor, out var slot))
            _playerStates[slot] ^= Shade.Dark;

        if ((SID)status.ID == SID._Gen_LightVengeance && Raid.TryFindSlot(actor, out var slot2))
            _playerStates[slot2] ^= Shade.Light;
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

class BladeOfFirstLight(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_BladeOfFirstLight1, AID._Weaponskill_BladeOfFirstLight4], new AOEShapeRect(30, 7.5f));

class BallOfFireBait(BossModule module) : BossComponent(module)
{
    private bool _active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BallOfFireFast:
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

class BoundsOfSinIcicle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BoundsOfSin, 3, maxCasts: 11);
class BoundsOfSinJail(BossModule module) : Components.GenericAOEs(module, AID._Spell_BoundsOfSin2)
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
        // 33.62 -> 40.71
        if ((AID)spell.Action.ID == AID._Spell_BoundsOfSin)
            _activation = Module.CastFinishAt(spell, 7.1f);

        if ((AID)spell.Action.ID == AID._Weaponskill_BoundsOfSin)
            _risky = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_BoundsOfSin2)
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
        if ((IconID)iconID == IconID._Gen_Icon_lockon6_t0t)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeRect(30, 2, 30), WorldState.FutureTime(6.3f), true));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Spinelash1)
            CurrentBaits.Clear();
    }
}

class Spinelash(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Spinelash1, new AOEShapeRect(60, 2));

class ChainsOfCondemnation(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_ChainsOfCondemnation1 or AID._Spell_ChainsOfCondemnation3)
            Array.Fill(PlayerStates, new(Requirement.Stay, Module.CastFinishAt(spell)));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            SetState(slot, new(Requirement.Stay, default));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_ChainsOfCondemnation && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}

class AbyssalBlaze(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    private WDir _nextDir;

    private readonly List<(WPos Source, WDir Direction)> _orbs = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_AbyssalBlaze or AID._Spell_AbyssalBlaze5)
            _nextDir = new WDir(1, 0);

        if ((AID)spell.Action.ID is AID._Spell_AbyssalBlaze4 or AID._Spell_AbyssalBlaze1)
            _nextDir = new WDir(0, 1);

        if ((AID)spell.Action.ID == AID._Spell_2)
            _orbs.Add((spell.TargetXZ, _nextDir));

        if ((AID)spell.Action.ID is AID._Spell_AbyssalBlaze2)
        {
            var lines = Lines.Where(l => l.Next.InCircle(spell.TargetXZ, 1));
            foreach (var l in lines)
                AdvanceLine(l, spell.TargetXZ);
        }

        if ((AID)spell.Action.ID is AID._Spell_AbyssalBlaze3)
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
        if ((AID)spell.Action.ID is AID._Spell_AbyssalBlaze or AID._Spell_AbyssalBlaze4)
        {
            _orbs.Clear();
            Lines.Clear();
        }

        if ((AID)spell.Action.ID == AID._Spell_AbyssalBlaze2)
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
            .ActivateOnEnter<LightDebuff>()
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<BallOfFireBait>()
            .ActivateOnEnter<BallOfFirePuddle>()
            .ActivateOnEnter<BoundsOfSinIcicle>()
            .ActivateOnEnter<BoundsOfSinJail>()
            .ActivateOnEnter<BoundsOfSinCollision>()
            .ActivateOnEnter<SpinelashBait>()
            .ActivateOnEnter<Spinelash>()
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<AbyssalBlaze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1041, NameID = 14037, DevOnly = true)]
public class D99TheFinalVerse(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsRect(20, 15))
{
    //private readonly RelSimplifiedComplexPolygon _white = Utils.LoadFromAssembly<RelSimplifiedComplexPolygon>("BossMod.Modules.Dawntrail.DeepDungeon.PilgrimsTraverse.D99TheFinalVerse.Light.json");
    //private readonly RelSimplifiedComplexPolygon _dark = Utils.LoadFromAssembly<RelSimplifiedComplexPolygon>("BossMod.Modules.Dawntrail.DeepDungeon.PilgrimsTraverse.D99TheFinalVerse.Dark.json");
}
