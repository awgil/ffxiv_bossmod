namespace BossMod.Dawntrail.Raid.RM07NBruteAbombinator;

public enum OID : uint
{
    Boss = 0x4781, // R7.000-19.712, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    BloomingAbomination = 0x4782, // R3.400, x8
    Wall = 0x4785, // R7.000, x5
    BruteAbombinator = 0x481B, // R0.000, x1, Part type
}

public enum AID : uint
{
    AutoAttack = 42263, // Boss->player, no cast, single-target
    AutoAttackWall = 43155, // Wall->player, no cast, single-target
    AutoAttackAdd = 872, // BloomingAbomination->player, no cast, single-target
    Teleport = 42262, // Boss->location, no cast, single-target

    BrutalImpact = 42264, // Boss->self, 5.0s cast, single-target, visual (multi-hit raidwide)
    BrutalImpactAOE = 42265, // Boss->self, no cast, range 60 circle
    StoneringerClub1 = 42266, // Boss->self, 2.0+3.5s cast, single-target, visual (circle next)
    StoneringerSword1 = 42267, // Boss->self, 2.0+3.5s cast, single-target, visual (donut next)
    StoneringerClub2 = 42290, // Boss->self, 2.0+3.5s cast, single-target, visual
    StoneringerSword2 = 42291, // Boss->self, 2.0+3.5s cast, single-target, visual
    Stoneringers1 = 42314, // Boss->self, 2.0+3.5s cast, single-target, visual
    Stoneringers2 = 42315, // Boss->self, 2.0+3.5s cast, single-target, visual
    BrutishSwingOut = 42268, // Boss->self, 4.0+1.0s cast, single-target, visual
    BrutishSwingIn = 42269, // Boss->self, 4.0+1.0s cast, single-target, visual
    BrutishSwingCone = 42292, // Boss->self, 3.0+1.0s cast, single-target, visual
    BrutishSwingJump1 = 42297, // Boss->location, 4.0+3.8s cast, single-target, visual
    BrutishSwingJump2 = 42301, // Boss->location, 4.0+3.8s cast, single-target, visual
    BrutishSwingJump3 = 42316, // Boss->location, 3.0+3.8s cast, single-target, visual
    BrutishSwingJump4 = 42325, // Boss->location, 3.0+3.8s cast, single-target, visual
    BrutishSwingJump5 = 42326, // Boss->location, 3.0+3.8s cast, single-target, visual
    BrutishSwingLand1 = 42305, // Boss->self, no cast, single-target
    BrutishSwingLand2 = 42318, // Boss->self, no cast, single-target
    BrutishSwingLand3 = 42320, // Boss->self, no cast, single-target
    BrutishSwingLand4 = 42554, // Boss->self, no cast, single-target
    BrutishSwingLand5 = 42555, // Boss->self, no cast, single-target
    BrutishSwingCircle = 42270, // Helper->self, 5.0s cast, range 12 circle
    BrutishSwingDonut = 42271, // Helper->self, 5.0s cast, range 9-60 donut
    BrutishSwingConeAOE1 = 42293, // Helper->self, 4.0s cast, range 25 180-degree cone
    BrutishSwingConeAOE2 = 42317, // Helper->self, 6.7s cast, range 25 180-degree cone
    BrutishSwingDonutSegment1 = 42303, // Helper->self, 8.1s cast, range 22-88 180-degree donut segment
    BrutishSwingDonutSegment2 = 42319, // Helper->self, 6.7s cast, range 22-88 180-degree donut segment
    BrutalSmash1 = 42272, // Boss->players, no cast, range 6 circle, shared tankbuster
    BrutalSmash2 = 42273, // Boss->players, no cast, range 6 circle, shared tankbuster

    SporeSac = 42281, // Boss->self, 3.0s cast, single-target, visual (circles)
    SporeSacAOE = 42282, // Helper->location, 4.0s cast, range 8 circle
    Pollen = 42283, // Helper->location, 4.5s cast, range 8 circle
    PulpSmash = 42276, // Boss->self, 3.0+2.0s cast, single-target, visual (stack)
    PulpSmashJump = 42277, // Boss->player, no cast, single-target
    PulpSmashAOE = 42278, // Helper->players, no cast, range 6 circle stack
    TheUnpotted = 42280, // Helper->self, 3.0s cast, range 60 30-degree cone
    ItCameFromTheDirt = 42279, // Helper->location, 3.0s cast, range 6 circle
    CrossingCrosswinds = 43276, // BloomingAbomination->self, 8.5s cast, range 50 width 10 cross
    WindingWildwinds = 43275, // BloomingAbomination->self, 8.5s cast, range 5-60 donut
    QuarrySwamp = 42285, // Boss->self, 4.0s cast, range 60 circle, line of sight
    Explosion = 42286, // Helper->location, 9.0s cast, range 60 circle, proximity
    NeoBombarianSpecial = 42287, // Boss->self, 8.0s cast, range 60 circle, raidwide + knockback 58
    GrapplingIvy = 42288, // Boss->location, no cast, single-target

    GlowerPower = 42310, // Boss->self, 3.7+1.3s cast, single-target, visual (line)
    GlowerPowerAOE = 43339, // Helper->self, 5.0s cast, range 65 width 14 rect
    ElectrogeneticForce = 42311, // Helper->players, 5.0s cast, range 6 circle spread
    AbominableBlink = 42306, // Boss->self, 5.3+1.0s cast, single-target, visual (flare)
    AbominableBlinkAOE = 43154, // Helper->players, no cast, range 60 circle, flare
    RevengeOfTheVines = 42307, // Boss->self, 5.0s cast, range 60 circle, raidwide
    RevengeOfTheVinesInstant = 42552, // Boss->self, no cast, range 60 circle, raidwide
    Sporesplosion = 42308, // Boss->self, 4.0s cast, single-target, visual (circles)
    SporesplosionAOE = 42309, // Helper->location, 5.0s cast, range 8 circle
    Powerslam = 42312, // Boss->location, 5.0s cast, range 60 circle, raidwide
    LashingLariat1 = 42321, // Boss->location, 5.0+0.5s cast, single-target, visual
    LashingLariat2 = 42323, // Boss->location, 5.0+0.5s cast, single-target, visual
    LashingLariatAOE1 = 42322, // Helper->self, 5.5s cast, range 70 width 32 rect
    LashingLariatAOE2 = 42324, // Helper->self, 5.5s cast, range 70 width 32 rect
    Slaminator = 42327, // Boss->location, 4.0+1.0s cast, single-target, visual (tower)
    SlaminatorAOE = 42328, // Helper->self, 5.0s cast, range 8 circle, tower
}

public enum IconID : uint
{
    BrutalSmash = 600, // player
    PulpSmash = 161, // player
    AbominableBlink = 327, // player
    ElectrogeneticForce = 108, // player
}

public enum TetherID : uint
{
    WallFar = 326, // Wall->Boss
    WallNear = 339, // Wall->Boss
}

class BrutalImpact(BossModule module) : Components.RaidwideMultiHit(module, AID.BrutalImpact, [AID.BrutalImpactAOE], 0.2f, 1.1f, 6)
{
    public void SetHits(int hits)
    {
        if (Remaining <= 0)
            return;
        Remaining = hits;
        LastHit = NextHit.AddSeconds(1.1f * (hits - 1));
    }
}
class BrutalImpactRest(BossModule module) : Components.CastCounter(module, AID.BrutalImpactAOE);
class NeoBombarianSpecial(BossModule module) : Components.RaidwideCast(module, AID.NeoBombarianSpecial);
class RevengeOfTheVines(BossModule module) : Components.RaidwideCast(module, AID.RevengeOfTheVines);
class RevengeOfTheVinesInstant(BossModule module) : Components.RaidwideInstant(module, AID.RevengeOfTheVinesInstant, 4.8f);
class Powerslam(BossModule module) : Components.RaidwideCast(module, AID.Powerslam);
class SporeSac(BossModule module) : Components.StandardAOEs(module, AID.SporeSacAOE, new AOEShapeCircle(8));
class Pollen(BossModule module) : Components.StandardAOEs(module, AID.Pollen, new AOEShapeCircle(8));
class PulpSmash(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PulpSmash, AID.PulpSmashAOE, 6, 5.3f, 4, 8);
class TheUnpotted(BossModule module) : Components.StandardAOEs(module, AID.TheUnpotted, new AOEShapeCone(60, 15.Degrees()));
class ItCameFromTheDirt(BossModule module) : Components.StandardAOEs(module, AID.ItCameFromTheDirt, new AOEShapeCircle(6));
class CrossingCrosswinds(BossModule module) : Components.StandardAOEs(module, AID.CrossingCrosswinds, new AOEShapeCross(50, 5));
class CrossingCrosswindsHint(BossModule module) : Components.CastInterruptHint(module, AID.CrossingCrosswinds, showNameInHint: true);
class WindingWildwinds(BossModule module) : Components.StandardAOEs(module, AID.WindingWildwinds, new AOEShapeDonut(5, 60));
class WindingWildwindsHint(BossModule module) : Components.CastInterruptHint(module, AID.WindingWildwinds, showNameInHint: true);
class GlowerPower(BossModule module) : Components.StandardAOEs(module, AID.GlowerPowerAOE, new AOEShapeRect(65, 7));
class ElectrogeneticForce(BossModule module) : Components.SpreadFromCastTargets(module, AID.ElectrogeneticForce, 6);
class AbominableBlink(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(24), (uint)IconID.AbominableBlink, AID.AbominableBlinkAOE, 6.4f, true);
class Sporesplosion(BossModule module) : Components.StandardAOEs(module, AID.SporesplosionAOE, new AOEShapeCircle(8), 12, highlightImminent: true);
class LashingLariat(BossModule module) : Components.GroupedAOEs(module, [AID.LashingLariatAOE1, AID.LashingLariatAOE2], new AOEShapeRect(70, 16));
class Slaminator(BossModule module) : Components.CastTowers(module, AID.SlaminatorAOE, 8, minSoakers: 8, maxSoakers: 8);

class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCircle(22), 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in Casters)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo));
    }
}

class Stoneringer(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _next;
    private bool _risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_next).Select(a => a with { Origin = Module.PrimaryActor.Position, Risky = _risky });

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StoneringerClub1:
                _next = new(new AOEShapeCircle(12), caster.Position, default, Module.CastFinishAt(spell, 10.8f));
                _risky = false;
                break;
            case AID.StoneringerSword1:
                _next = new(new AOEShapeDonut(9, 60), caster.Position, default, Module.CastFinishAt(spell, 10.8f));
                _risky = false;
                break;
            case AID.BrutishSwingCircle:
            case AID.BrutishSwingDonut:
                _risky = true;
                if (_next != null)
                    _next = _next.Value with { Activation = Module.CastFinishAt(spell) };
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BrutishSwingCircle or AID.BrutishSwingDonut)
        {
            ++NumCasts;
            _next = null;
        }
    }
}

class JumpSwing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _cone = new(25, 90.Degrees());
    private static readonly AOEShapeDonutSector _donut = new(22, 88, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.BrutishSwingConeAOE1 or AID.BrutishSwingConeAOE2 => _cone,
            AID.BrutishSwingDonutSegment1 or AID.BrutishSwingDonutSegment2 => _donut,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BrutishSwingConeAOE1 or AID.BrutishSwingConeAOE2 or AID.BrutishSwingDonutSegment1 or AID.BrutishSwingDonutSegment2)
        {
            ++NumCasts;
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(spell.LocXZ, 1));
        }
    }
}

class BrutalSmash(BossModule module) : Components.GenericSharedTankbuster(module, default, 6)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BrutalSmash)
        {
            Source = Module.PrimaryActor;
            Target = actor;
            Activation = WorldState.FutureTime(6);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BrutalSmash1 or AID.BrutalSmash2)
        {
            ++NumCasts;
            Source = null;
            Target = null;
        }
    }
}

class QuarrySwamp(BossModule module) : Components.GenericLineOfSightAOE(module, AID.QuarrySwamp, 60, false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(caster.Position, Module.Enemies(OID.BloomingAbomination).Select(b => (b.Position, b.HitboxRadius)), Module.CastFinishAt(spell));
    }

    public override void Update()
    {
        if (Origin != null)
            Modify(Origin, Module.Enemies(OID.BloomingAbomination).Select(b => (b.Position, b.HitboxRadius)), NextExplosion);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(null, []);
    }
}

class NeoBombarianSpecialKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.NeoBombarianSpecial, 58)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var inSquare = Math.Abs(pos.X - 100) < 19.5f && Math.Abs(pos.Z - 100) < 19.5f;
        var inCorridor = Math.Abs(pos.X - 100) < 10 && pos.Z is > -19.5f and < 25;
        return !inSquare && !inCorridor;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(Sdf.Discrete(p => DestinationUnsafe(slot, actor, AwayFromSource(p, src.Origin, src.Distance))), src.Activation);
    }
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    private bool _corridor;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_corridor)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, new WDir(0, 1), 24, 24, 11.5f));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001)
            return;
        if (index == 0x00)
        {
            Arena.Center = new(100, 5);
            Arena.Bounds = new ArenaBoundsRect(12.5f, 25);
            _corridor = true;
        }
        else if (index == 0x01)
        {
            Arena.Bounds = new ArenaBoundsSquare(20);
            _corridor = false;
        }
    }
}

class RM07NBruteAbombinatorStates : StateMachineBuilder
{
    public RM07NBruteAbombinatorStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<RevengeOfTheVinesInstant>()
            .ActivateOnEnter<Stoneringer>()
            .ActivateOnEnter<JumpSwing>()
            .ActivateOnEnter<BrutalSmash>()
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<CrossingCrosswinds>()
            .ActivateOnEnter<CrossingCrosswindsHint>()
            .ActivateOnEnter<WindingWildwinds>()
            .ActivateOnEnter<WindingWildwindsHint>();
    }

    private void SinglePhase(uint id)
    {
        BrutalImpact(id, 5.2f, 3);
        StoneringerSwing(id + 0x10000, 4.3f, 1);
        SporeSacPulpSmash(id + 0x20000, 5.2f);
        QuarrySwamp(id + 0x30000, 20.3f);
        ComponentCondition<Explosion>(id + 0x40000, 12, c => c.NumCasts >= 1, "Proximity")
            .ActivateOnEnter<Explosion>();
        StoneringerSwing(id + 0x50000, 3.1f, 2);
        Cast(id + 0x60000, AID.NeoBombarianSpecial, 7.6f, 8, "Raidwide + knockback")
            .DeactivateOnExit<Explosion>()
            .ActivateOnEnter<NeoBombarianSpecial>()
            .ActivateOnEnter<NeoBombarianSpecialKnockback>()
            .DeactivateOnExit<NeoBombarianSpecial>()
            .DeactivateOnExit<NeoBombarianSpecialKnockback>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart);

        Targetable(id + 0x100000, true, 15.4f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        CastMulti(id + 0x110000, [AID.StoneringerClub2, AID.StoneringerSword2], 5.1f, 2);
        Swing(id + 0x120000, [AID.BrutishSwingCone], 5.9f, 3, 1, 1);
        CastMulti(id + 0x130000, [AID.StoneringerClub2, AID.StoneringerSword2], 4.1f, 2);
        Swing(id + 0x140000, [AID.BrutishSwingJump1, AID.BrutishSwingJump2], 5.8f, 4, 2, 4.1f);
        GlowerPower(id + 0x150000, 0.9f);
        AbominableBlink(id + 0x160000, 1.7f);
        RevengeOfTheVines(id + 0x170000, 3.2f);
        CastMulti(id + 0x180000, [AID.StoneringerClub2, AID.StoneringerSword2], 5.1f, 2);
        Cast(id + 0x190000, AID.Sporesplosion, 6.9f, 4)
            .ActivateOnEnter<Sporesplosion>();
        Swing(id + 0x1A0000, [AID.BrutishSwingJump1, AID.BrutishSwingJump2], 4.6f, 4, 3, 4.1f);
        GlowerPower(id + 0x1C0000, 0.8f);
        Cast(id + 0x1D0000, AID.Powerslam, 7.9f, 5, "Raidwide")
            .DeactivateOnExit<Sporesplosion>()
            .ActivateOnEnter<Powerslam>()
            .DeactivateOnExit<Powerslam>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart);

        Targetable(id + 0x200000, true, 15.5f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        CastMulti(id + 0x210000, [AID.Stoneringers1, AID.Stoneringers2], 4.1f, 2);
        Cast(id + 0x220000, AID.SporeSac, 5.9f, 3);
        Swing(id + 0x230000, [AID.BrutishSwingJump3, AID.BrutishSwingJump4, AID.BrutishSwingJump5], 4.7f, 3, 4, 3.7f, true);
        LashingLariat(id + 0x240000, 3.1f);
        Swing(id + 0x250000, [AID.BrutishSwingJump3, AID.BrutishSwingJump4, AID.BrutishSwingJump5], 1.7f, 3, 5, 3.7f);
        Slaminator(id + 0x260000, 1.1f);
        QuarrySwamp(id + 0x270000, 6.2f);
        BrutalImpact(id + 0x280000, 5.1f, 6);
        StoneringerSwing(id + 0x290000, 6.1f, 3);
        PulpSmash(id + 0x2A0000, 2.1f);
        CastMulti(id + 0x2B0000, [AID.Stoneringers1, AID.Stoneringers2], 4.3f, 2);
        Swing(id + 0x2C0000, [AID.BrutishSwingJump3, AID.BrutishSwingJump4, AID.BrutishSwingJump5], 8.7f, 3, 6, 3.7f);
        GlowerPower(id + 0x2D0000, 1.1f);
        LashingLariat(id + 0x2E0000, 0.8f);
        Swing(id + 0x2F0000, [AID.BrutishSwingJump3, AID.BrutishSwingJump4, AID.BrutishSwingJump5], 1.6f, 3, 7, 3.7f, true);
        Slaminator(id + 0x300000, 3.3f);

        SimpleState(id + 0x310000, 120, "Until enrage")
            .ActivateOnEnter<BrutalImpact>()
            .ActivateOnEnter<RevengeOfTheVines>()
            .ActivateOnEnter<PulpSmash>()
            .ActivateOnEnter<TheUnpotted>()
            .ActivateOnEnter<ItCameFromTheDirt>()
            .ActivateOnEnter<QuarrySwamp>()
            .ActivateOnEnter<GlowerPower>()
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<AbominableBlink>()
            .ActivateOnEnter<Sporesplosion>()
            .ActivateOnEnter<LashingLariat>()
            .ActivateOnEnter<Slaminator>();
    }

    private void BrutalImpact(uint id, float delay, int hits)
    {
        Cast(id, AID.BrutalImpact, delay, 5)
            .ActivateOnEnter<BrutalImpact>()
            .ActivateOnEnter<BrutalImpactRest>()
            .ExecOnEnter<BrutalImpact>(c => c.SetHits(hits));
        ComponentCondition<BrutalImpactRest>(id + 0x10, 0.2f, c => c.NumCasts > 0, "Raidwide first hit")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpactRest>(id + 0x20, 1.1f * (hits - 1), c => c.NumCasts >= hits, "Raidwide last hit")
            .DeactivateOnExit<BrutalImpact>()
            .DeactivateOnExit<BrutalImpactRest>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StoneringerSwing(uint id, float delay, int count)
    {
        CastMulti(id, [AID.StoneringerClub1, AID.StoneringerSword1], delay, 2);
        CastMulti(id + 0x10, [AID.BrutishSwingOut, AID.BrutishSwingIn], 5.8f, 4);
        ComponentCondition<Stoneringer>(id + 0x20, 1, c => c.NumCasts >= count, "In/out");
        ComponentCondition<BrutalSmash>(id + 0x30, 0.9f, c => c.NumCasts >= count, "Shared tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SporeSacPulpSmash(uint id, float delay)
    {
        Cast(id, AID.SporeSac, delay, 3);
        ComponentCondition<SporeSac>(id + 0x10, 5.1f, c => c.NumCasts > 0, "Circles");
        PulpSmash(id + 0x100, 4);
    }

    private void PulpSmash(uint id, float delay)
    {
        CastStart(id, AID.PulpSmash, delay)
            .ActivateOnEnter<PulpSmash>()
            .ActivateOnEnter<TheUnpotted>()
            .ActivateOnEnter<ItCameFromTheDirt>();
        CastEnd(id + 1, 3);
        ComponentCondition<PulpSmash>(id + 0x10, 2.2f, c => c.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<PulpSmash>();
        ComponentCondition<TheUnpotted>(id + 0x20, 3, c => c.NumCasts > 0, "Cones")
            .DeactivateOnExit<TheUnpotted>()
            .DeactivateOnExit<ItCameFromTheDirt>();
    }

    private void QuarrySwamp(uint id, float delay)
    {
        Cast(id, AID.QuarrySwamp, delay, 4, "Line of sight")
            .ActivateOnEnter<QuarrySwamp>()
            .DeactivateOnExit<QuarrySwamp>();
    }

    private void Swing(uint id, AID[] aids, float delay, float castTime, int count, float resolve, bool vines = false)
    {
        CastMulti(id, aids, delay, castTime)
            .ExecOnEnter<RevengeOfTheVinesInstant>(c => c.Activation = Module.WorldState.FutureTime(castTime + c.Delay), vines);
        ComponentCondition<JumpSwing>(id + 0x10, resolve, c => c.NumCasts >= count, "Swing");
    }

    private void GlowerPower(uint id, float delay)
    {
        Cast(id, AID.GlowerPower, delay, 3.7f)
            .ActivateOnEnter<GlowerPower>()
            .ActivateOnEnter<ElectrogeneticForce>();
        ComponentCondition<GlowerPower>(id + 0x10, 1.3f, c => c.NumCasts > 0, "Line + spreads")
            .DeactivateOnExit<GlowerPower>()
            .DeactivateOnExit<ElectrogeneticForce>();
    }

    private void AbominableBlink(uint id, float delay)
    {
        CastStart(id, AID.AbominableBlink, delay)
            .ActivateOnEnter<AbominableBlink>();
        CastEnd(id + 1, 5.3f);
        ComponentCondition<AbominableBlink>(id + 0x10, 1, c => c.NumCasts > 0, "Flare")
            .DeactivateOnExit<AbominableBlink>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RevengeOfTheVines(uint id, float delay)
    {
        Cast(id, AID.RevengeOfTheVines, delay, 5, "Raidwide")
            .ActivateOnEnter<RevengeOfTheVines>()
            .DeactivateOnExit<RevengeOfTheVines>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void LashingLariat(uint id, float delay)
    {
        CastMulti(id, [AID.LashingLariat1, AID.LashingLariat2], delay, 5)
            .ActivateOnEnter<LashingLariat>();
        ComponentCondition<LashingLariat>(id + 0x10, 0.5f, c => c.NumCasts > 0, "Lariat")
            .DeactivateOnExit<LashingLariat>();
    }

    private void Slaminator(uint id, float delay)
    {
        CastStart(id, AID.Slaminator, delay)
            .ActivateOnEnter<Slaminator>();
        CastEnd(id + 1, 4);
        ComponentCondition<Slaminator>(id + 0x10, 1, c => c.NumCasts > 0, "Tower")
            .DeactivateOnExit<Slaminator>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1023, NameID = 13756)]
public class RM07NBruteAbombinator(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.BloomingAbomination), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            if ((OID)e.Actor.OID == OID.BloomingAbomination)
                e.Priority = 1;
    }
}
