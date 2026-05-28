namespace BossMod.Dawntrail.Alliance.A33AlexanderResurrected;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x43-44, Helper type
    Boss = 0x4D5C, // R8.250, x1
    GordiusSystem = 0x4D5D, // R4.005, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50180, // Boss->player, no cast, single-target
    BanishgaIVRaidwide = 50161, // Boss->self, 5.0s cast, range 80 circle
    Jump = 50123, // Boss->location, no cast, single-target

    DivineArrowCWCast = 50124, // Boss->self, 10.0s cast, single-target
    DivineArrowCCWCast = 50125, // Boss->self, 10.0s cast, single-target
    DivineArrowStart1 = 50126, // Boss->self, no cast, single-target
    DivineArrowStart2 = 50127, // Boss->self, no cast, single-target
    DivineArrowStart3 = 50128, // Boss->self, no cast, single-target
    DivineArrowStart4 = 50129, // Boss->self, no cast, single-target
    DivineArrowFirst = 50130, // Helper->self, 1.0s cast, range 45 90-degree cone
    DivineArrowRest = 50478, // Helper->self, no cast, range 45 90-degree cone
    DivineArrowInOut1 = 50131, // Helper->self, 9.5s cast, range 10 circle
    DivineArrowInOut2 = 50132, // Helper->self, 11.5s cast, range 10-23 donut
    DivineArrowInOut3 = 50133, // Helper->self, 13.5s cast, range 23-36 donut
    DivineArrowOutIn3 = 50134, // Helper->self, 13.5s cast, range 10 circle
    DivineArrowOutIn2 = 50135, // Helper->self, 11.5s cast, range 10-23 donut
    DivineArrowOutIn1 = 50136, // Helper->self, 9.5s cast, range 23-36 donut
    DivineArrowRect1 = 50137, // Helper->self, 3.5s cast, range 60 width 10 rect
    DivineArrowRect2 = 50138, // Helper/player->self, 5.5s cast, range 60 width 10 rect

    BanishgaIVSpread = 50163, // Helper->players, 5.0s cast, range 6 circle
    HolyII = 50165, // Helper/player->location, 5.0s cast, range 6 circle

    ImpartialRulingCast1 = 50144, // Boss->self, 6.3+0.7s cast, single-target
    ImpartialRulingCast2 = 50145, // Boss->self, 6.3+0.7s cast, single-target
    ImpartialRuling1 = 50146, // Helper->self, 7.0s cast, range 50 180-degree cone
    ImpartialRuling2 = 50147, // Helper->self, 10.0s cast, range 50 180-degree cone

    CanonizeCoordinates = 50139, // Boss->self, no cast, single-target
    RadiantSacramentCast = 50140, // Boss->self, 4.0s cast, single-target
    RadiantSacramentTile = 50141, // Helper/player->self, no cast, range 10 width 10 rect

    DivineSpearCast = 50142, // Boss->self, 4.0+1.0s cast, single-target
    DivineSpear = 50143, // Helper->self, 8.0s cast, ???

    MegaHolyCast = 50157, // Boss->self, 6.5+0.5s cast, single-target
    MegaHolyFirst = 50158, // Helper->players, 7.0s cast, range 6 circle
    MegaHolyRest = 50159, // Helper->players, no cast, range 6 circle

    SacredAssembly = 50148, // Boss->self, 3.0+1.0s cast, single-target
    Activate = 50219, // Helper->self, 5.0s cast, range 3 circle
    PerfectDefense = 50149, // Boss->self, 6.0s cast, single-target
    KarmicShielding = 50686, // GordiusSystem->self, 4.0s cast, single-target
    Repay = 50166, // Helper->player, no cast, single-target
    HolyFlame = 50150, // Helper->location, 3.0s cast, range 5 circle
    Shock = 50152, // GordiusSystem->self, 4.0s cast, range 7 circle
    CircuitShock = 50169, // GordiusSystem->self, 4.0s cast, range 7-18 donut

    DivineJudgment = 50153, // Boss->self, 10.0s cast, range 50 circle
    Reinforcements = 50154, // Boss->self, 2.0+1.0s cast, single-target
    Electrify = 50156, // GordiusSystem->self, 1.0s cast, range 18 circle

    DivineBoltCast = 50160, // Boss->self/player, 5.0s cast, range 60 width 6 rect
    DivineBolt = 50849, // Helper->self, no cast, range 60 width 6 rect
}

public enum SID : uint
{
    PerfectDefenseBoss = 5376, // Boss->Boss, extra=0x0
    PerfectDefensePillar = 5377, // none->GordiusSystem, extra=0x0
    Unk = 2766, // none->GordiusSystem, extra=0xF
}

public enum IconID : uint
{
    DivineArrowNCW = 691, // Boss->self
    DivineArrowSCW = 692, // Boss->self
    DivineArrowECW = 693, // Boss->self
    DivineArrowWCW = 694, // Boss->self
    DivineArrowNCCW = 695, // Boss->self
    DivineArrowSCCW = 696, // Boss->self
    DivineArrowECCW = 697, // Boss->self
    DivineArrowWCCW = 698, // Boss->self
    RulingLeftRight = 699, // Boss->self
    RulingRightLeft = 700, // Boss->self
    Spread = 215, // player->self
    StackMulti = 590, // player->self
    TankLaser = 471, // player->self
}

public enum TetherID : uint
{
    Energize = 414, // GordiusSystem->Boss
    Electrify = 428, // GordiusSystem->Boss
}

class BanishgaRaidwide(BossModule module) : Components.RaidwideCast(module, AID.BanishgaIVRaidwide);

class DivineArrow(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Angle starting;
        Angle advance;

        switch ((IconID)iconID)
        {
            case IconID.DivineArrowNCW:
                starting = 180.Degrees();
                advance = -20.Degrees();
                break;
            case IconID.DivineArrowSCW:
                starting = default;
                advance = -20.Degrees();
                break;
            case IconID.DivineArrowECW:
                starting = 90.Degrees();
                advance = -20.Degrees();
                break;
            case IconID.DivineArrowWCW:
                starting = -90.Degrees();
                advance = -20.Degrees();
                break;
            case IconID.DivineArrowNCCW:
                starting = 180.Degrees();
                advance = 20.Degrees();
                break;
            case IconID.DivineArrowSCCW:
                starting = default;
                advance = 20.Degrees();
                break;
            case IconID.DivineArrowECCW:
                starting = 90.Degrees();
                advance = 20.Degrees();
                break;
            case IconID.DivineArrowWCCW:
                starting = -90.Degrees();
                advance = 20.Degrees();
                break;
            default:
                return;
        }

        Sequences.Add(new(new AOEShapeCone(45, 45.Degrees()), actor.Position, starting, advance, WorldState.FutureTime(13.3f), 0.67f, 18, MaxShownAOEs: 9));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DivineArrowFirst or AID.DivineArrowRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class DivineArrowInOut(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 23), new AOEShapeDonut(23, 36)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DivineArrowInOut1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.DivineArrowInOut1 => 0,
            AID.DivineArrowInOut2 => 1,
            AID.DivineArrowInOut3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}
class DivineArrowOutIn(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonut(23, 36), new AOEShapeDonut(10, 23), new AOEShapeCircle(10)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DivineArrowOutIn1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.DivineArrowOutIn1 => 0,
            AID.DivineArrowOutIn2 => 1,
            AID.DivineArrowOutIn3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}
class DivineArrowRect(BossModule module) : Components.GroupedAOEs(module, [AID.DivineArrowRect1, AID.DivineArrowRect2], new AOEShapeRect(60, 5), maxCasts: 5);

class HolyII(BossModule module) : Components.StandardAOEs(module, AID.HolyII, 6);
class BanishgaSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.BanishgaIVSpread, 6);

class ImpartialRuling(BossModule module) : Components.GroupedAOEs(module, [AID.ImpartialRuling1, AID.ImpartialRuling2], new AOEShapeCone(50, 90.Degrees()), highlightImminent: true);

// 0x14 -> 0x2C
// 8.1s delay from appearance to activation
class RadiantSacrament(BossModule module) : Components.GenericAOEs(module, AID.RadiantSacramentTile)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime imm = default;

        foreach (var p in _predicted.Take(17))
        {
            if (imm == default)
                imm = p.Activation;

            yield return p with { Color = p.Activation < imm.AddSeconds(0.2f) ? ArenaColor.Danger : ArenaColor.AOE };
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x14 and <= 0x2C && state == 0x00020001)
        {
            var ix = index - 0x14;
            var row = ix % 5;
            var col = ix / 5;
            var wd = new WDir(10 * col, 10 * row) + (Arena.Center - new WDir(20, 20));
            _predicted.Add(new(new AOEShapeRect(5, 5, 5), wd, default, WorldState.FutureTime(8.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

// divine spear:
// 31: N
// 32: W
// 33: E
// 34: S
// 35/36: N
// 37/38: S
// 00080004: turn -90
// 00100004: turn 90
// 00200004: turn -135
// 00400004: turn 135
// 36.00080004: N -> NE
// 36.00100004: N -> NW
// 36.00200004: N -> E
// 36.00400004: N -> W
class DivineSpear(BossModule module) : Components.GenericAOEs(module, AID.DivineSpear)
{
    readonly List<AOEInstance> _predicted = [];
    static readonly AOEShapeTriCone Triangle = new(25, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnMapEffect(byte index, uint state)
    {
        Angle fromBoss;

        switch (index)
        {
            case 0x31:
            case 0x35:
            case 0x36:
            case 0x39:
            case 0x3A:
                fromBoss = 180.Degrees();
                break;
            case 0x32:
                fromBoss = -90.Degrees();
                break;
            case 0x33:
                fromBoss = 90.Degrees();
                break;
            case 0x34:
            case 0x37:
            case 0x38:
            case 0x3B:
            case 0x3C:
                fromBoss = default;
                break;
            default:
                return;
        }

        switch (state)
        {
            case 0x00080004:
                _predicted.Add(new(Triangle, Arena.Center + fromBoss.ToDirection() * 25, fromBoss - 135.Degrees(), WorldState.FutureTime(8)));
                break;
            case 0x00100004:
                _predicted.Add(new(Triangle, Arena.Center + fromBoss.ToDirection() * 25, fromBoss + 135.Degrees(), WorldState.FutureTime(8)));
                break;
            case 0x00200004:
                _predicted.Add(new(Triangle, Arena.Center, fromBoss - 45.Degrees(), WorldState.FutureTime(8)));
                break;
            case 0x00400004:
                _predicted.Add(new(Triangle, Arena.Center, fromBoss + 45.Degrees(), WorldState.FutureTime(8)));
                break;
        }
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

class MegaHoly(BossModule module) : Components.StackWithCastTargets(module, AID.MegaHolyFirst, 6)
{
    public int NumCasts { get; private set; }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MegaHolyFirst or AID.MegaHolyRest)
        {
            if (++NumCasts >= 3)
            {
                Stacks.Clear();
                NumFinishedStacks++;
                NumCasts = 0;
            }
        }
    }
}

class Activate(BossModule module) : Components.StandardAOEs(module, AID.Activate, 3);
class GordiusSystem(BossModule module) : Components.Adds(module, (uint)OID.GordiusSystem, 1, true);
class PerfectDefense(BossModule module) : Components.InvincibleStatus(module, (uint)SID.PerfectDefensePillar, priority: AIHints.Enemy.PriorityForbidden);
class HolyFlame(BossModule module) : Components.StandardAOEs(module, AID.HolyFlame, 5);
class Shock(BossModule module) : Components.StandardAOEs(module, AID.Shock, 7);
class CircuitShock(BossModule module) : Components.StandardAOEs(module, AID.CircuitShock, new AOEShapeDonut(7, 18));

class DivineJudgment(BossModule module) : Components.RaidwideCast(module, AID.DivineJudgment);

class Electrify(BossModule module) : Components.GenericAOEs(module, AID.Electrify)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(2);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Electrify)
            _predicted.Add(new(new AOEShapeCircle(18), source.Position, default, WorldState.FutureTime(9)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class DivineBolt(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 3), (uint)IconID.TankLaser, AID.DivineBolt, 5.1f);

class A33AlexanderResurrectedStates : StateMachineBuilder
{
    public A33AlexanderResurrectedStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase).ActivateOnEnter<HolyII>();
    }

    private void SinglePhase(uint id)
    {
        Banishga(id, 8.2f);
        DivineArrow(id + 0x1000, 12, 5.3f);
        ImpartialRuling(id + 0x2000, 7.5f);
        RadiantSacrament(id + 0x3000, 10.7f);
        DivineSpear(id + 0x4000, 5.4f);

        Activate(id + 0x10000, 8.9f);

        DivineArrow(id + 0x20000, 16.7f, 3.4f);
        Electrify(id + 0x21000, 4.6f);
        DivineBolt(id + 0x22000, 5);

        Timeout(id + 0xFF0000, 10000, "Repeat mechanics until death")
            .ActivateOnEnter<BanishgaRaidwide>()
            .ActivateOnEnter<DivineArrow>()
            .ActivateOnEnter<DivineArrowInOut>()
            .ActivateOnEnter<DivineArrowOutIn>()
            .ActivateOnEnter<DivineArrowRect>()
            .ActivateOnEnter<BanishgaSpread>()
            .ActivateOnEnter<ImpartialRuling>()
            .ActivateOnEnter<RadiantSacrament>()
            .ActivateOnEnter<DivineSpear>()
            .ActivateOnEnter<MegaHoly>()
            .ActivateOnEnter<Activate>()
            .ActivateOnEnter<Electrify>();
    }

    void Banishga(uint id, float delay)
    {
        Cast(id, AID.BanishgaIVRaidwide, delay, 5, "Raidwide")
            .ActivateOnEnter<BanishgaRaidwide>()
            .DeactivateOnExit<BanishgaRaidwide>();
    }

    void DivineArrow(uint id, float delay, float spreadDelay)
    {
        CastStartMulti(id, [AID.DivineArrowCWCast, AID.DivineArrowCCWCast], delay)
            .ActivateOnEnter<DivineArrow>()
            .ActivateOnEnter<DivineArrowInOut>()
            .ActivateOnEnter<DivineArrowOutIn>()
            .ActivateOnEnter<DivineArrowRect>()
            .ActivateOnEnter<BanishgaSpread>();

        ComponentCondition<DivineArrow>(id + 0x10, 13.2f, d => d.NumCasts > 0, "Spinning AOE start");
        ComponentCondition<DivineArrow>(id + 0x20, 11.4f, d => d.NumCasts >= 18, "Spinning AOE finish");

        ComponentCondition<BanishgaSpread>(id + 0x100, spreadDelay, b => b.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<DivineArrow>()
            .DeactivateOnExit<DivineArrowInOut>()
            .DeactivateOnExit<DivineArrowOutIn>()
            .DeactivateOnExit<DivineArrowRect>()
            .DeactivateOnExit<BanishgaSpread>();
    }

    void ImpartialRuling(uint id, float delay)
    {
        CastStartMulti(id, [AID.ImpartialRulingCast2, AID.ImpartialRulingCast1], delay)
            .ActivateOnEnter<ImpartialRuling>();

        ComponentCondition<ImpartialRuling>(id + 0x10, 7, r => r.NumCasts > 0, "Left/right 1");
        ComponentCondition<ImpartialRuling>(id + 0x20, 3, r => r.NumCasts > 1, "Left/right 2")
            .DeactivateOnExit<ImpartialRuling>();
    }

    void RadiantSacrament(uint id, float delay)
    {
        CastStart(id, AID.RadiantSacramentCast, delay)
            .ActivateOnEnter<RadiantSacrament>();

        ComponentCondition<RadiantSacrament>(id + 0x10, 14.1f, r => r.NumCasts > 0, "Tiles start");
        ComponentCondition<RadiantSacrament>(id + 0x20, 2.7f, r => r.NumCasts >= 25, "Tiles finish")
            .DeactivateOnExit<RadiantSacrament>();
    }

    void DivineSpear(uint id, float delay)
    {
        CastStart(id, AID.DivineSpearCast, delay)
            .ActivateOnEnter<DivineSpear>()
            .ActivateOnEnter<MegaHoly>();

        ComponentCondition<DivineSpear>(id + 0x10, 14.1f, s => s.NumCasts > 0, "Triangles")
            .DeactivateOnExit<DivineSpear>();

        ComponentCondition<MegaHoly>(id + 0x100, 5, h => h.NumCasts > 0, "Stack 1");
        ComponentCondition<MegaHoly>(id + 0x110, 3.2f, h => h.NumFinishedStacks > 0, "Stack 3")
            .DeactivateOnExit<MegaHoly>();
    }

    void Activate(uint id, float delay)
    {
        Cast(id, AID.SacredAssembly, delay, 3)
            .ActivateOnEnter<Activate>()
            .ActivateOnEnter<GordiusSystem>()
            .ActivateOnEnter<PerfectDefense>()
            .ActivateOnEnter<HolyFlame>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<CircuitShock>();

        ComponentCondition<Activate>(id + 0x10, 2.1f, a => a.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Activate>();

        Cast(id + 0x100, AID.PerfectDefense, 1.1f, 6);

        ComponentCondition<GordiusSystem>(id + 0x200, 0.7f, g => g.ActiveActors.Any(), "Boss disappears + adds appear");

        Cast(id + 0x1000, AID.DivineJudgment, 40, 10, "Raidwide (adds enrage)")
            .ActivateOnEnter<DivineJudgment>()
            .DeactivateOnExit<DivineJudgment>()
            .DeactivateOnExit<GordiusSystem>()
            .DeactivateOnExit<PerfectDefense>()
            .DeactivateOnExit<HolyFlame>()
            .DeactivateOnExit<Shock>()
            .DeactivateOnExit<CircuitShock>();
    }

    void Electrify(uint id, float delay)
    {
        Cast(id, AID.SacredAssembly, delay, 3)
            .ActivateOnEnter<Activate>()
            .ActivateOnEnter<Electrify>();

        ComponentCondition<Electrify>(id + 0x10, 16.4f, e => e.NumCasts >= 2, "AOEs 1");
        ComponentCondition<Electrify>(id + 0x20, 3.2f, e => e.NumCasts >= 4, "AOEs 2")
            .DeactivateOnExit<Electrify>()
            .DeactivateOnExit<Activate>();
    }

    void DivineBolt(uint id, float delay)
    {
        CastStart(id, AID.DivineBoltCast, delay)
            .ActivateOnEnter<DivineBolt>()
            .ActivateOnEnter<BanishgaSpread>();

        ComponentCondition<BanishgaSpread>(id + 0x10, 4.9f, s => s.NumFinishedSpreads > 0, "Spreads");
        ComponentCondition<DivineBolt>(id + 0x20, 0.8f, b => b.NumCasts > 0, "Line tankbuster")
            .DeactivateOnExit<DivineBolt>()
            .DeactivateOnExit<BanishgaSpread>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117, NameID = 14529)]
public class A33AlexanderResurrected(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 360), new ArenaBoundsSquare(25));

