namespace BossMod.Shadowbringers.Raid.E11Fatebreaker;

public enum OID : uint
{
    Boss = 0x3070, // R5.004, Fatebreaker
    Helper = 0x233C, // R0.500
    FatebreakersImage = 0x3071, // R5.004
    BlastingZoneHelper = 0x3076, // R0.500
    DemiGukumatz = 0x3073, // R3.780, snake
    HaloOfFlame = 0x3074, // R1.000
    HaloOfLevin = 0x3075, // R1.000
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    BossMove = 22792, // Boss->location, no cast

    BurntStrikeFire = 22060, // Boss->self, 8.0s cast, range 80 width 10 rect
    Blastburn = 22061, // Helper->self, 10.0s cast, range 80 width 50 rect (knockback from line)
    BurntStrikeLightning = 22062, // Boss->self, 8.0s cast, range 80 width 10 rect
    Burnout = 22063, // Helper->self, 9.7s cast, range 80 width 20 rect
    BurntStrikeLight = 22064, // Boss->self, 8.0s cast, range 80 width 10 rect
    ShiningBlade = 22065, // Helper->location, 3.0s cast, range 6 circle

    BoundOfFaithFire = 19224, // Boss->self, 8.0s cast, single-target
    BoundOfFaithLightning = 19225, // Boss->self, 8.0s cast, single-target
    BoundOfFaithLight = 19227, // Boss->self, 8.0s cast, single-target
    FloatingFetters = 22772, // Boss->player, no cast, single-target
    SolemnChargeFire = 22066, // Boss->player, no cast, single-target
    Sinsmoke = 22067, // Helper->player, no cast, range 6 circle
    SolemnChargeLightning = 22068, // Boss->player, no cast, single-target
    Sinsmite = 22069, // Helper->player, no cast, single-target
    SolemnChargeLight = 22070, // Boss->player, no cast, single-target
    Sinsight = 22071, // Helper->player, no cast, range 6 circle
    MortalBurnMark = 22072, // Helper->self, no cast, range 40 circle

    BurnishedGlory = 22096, // Boss->self, 5.0s cast, range 40 circle
    PowderMark = 22094, // Boss->player, 5.0s cast, single-target
    BurnMark = 22095, // Helper->self, no cast, range 10 circle

    TurnOfTheHeavensFire = 22073, // Boss->self, 8.0s cast, single-target
    TurnOfTheHeavensLightning = 22074, // Boss->self, 8.0s cast, single-target
    BrightfireFlame = 22076, // HaloOfFlame->self, 8.0s cast, range 10 circle
    BrightfireLevin = 22075, // HaloOfLevin->self, 8.0s cast, range 5 circle

    ShiftingSky = 22079, // Boss->self, 8.0s cast, single-target
    ShiftingSkyKB = 22091, // Helper->self, 4.0s cast, range 40+R circle
    AgelessSerpent = 22092, // DemiGukumatz->self, no cast, range 30+R circle (knockback)
    ResoundingCrack = 22093, // DemiGukumatz->self, 5.0s cast, range 40 270-degree cone
    CloneBurntStrikeFire = 22083, // FatebreakersImage->self, 8.0s cast, range 80 width 10 rect
    CloneBlastburn = 22084, // Helper->self, 10.0s cast, range 80 width 50 rect
    CloneBurntStrikeLightning = 22085, // FatebreakersImage->self, 8.0s cast, range 80 width 10 rect
    CloneBurnout = 22086, // Helper->self, 9.7s cast, range 80 width 20 rect

    PrismaticDeception = 22077, // Boss->self, 6.0s cast, single-target
    BlastingZoneVis = 22097, // BlastingZoneHelper->self, 14.0s cast, single-target
    BlastingZone = 22078, // FatebreakersImage->self, no cast, range 50 width 16 rect
}

public enum SID : uint
{
    PowderMark = 2451, // Boss->player
    Invisible = 1621, // none->FatebreakersImage
    BlastingZone = 1678, // none->FatebreakersImage
}

public enum IconID : uint
{
    Sinsmoke = 100, // player stack
    Sinsight = 101, // player spread
}

public enum TetherID : uint
{
    BoundOfFaithFire = 5, // Boss->player
    BoundOfFaithLightning = 6, // Boss->player
    BoundOfFaithLight = 2, // Boss->player
}

class BurnishedGlory(BossModule module) : Components.RaidwideCast(module, AID.BurnishedGlory);
class PowderMark(BossModule module) : Components.SingleTargetCast(module, AID.PowderMark);

class BurntStrike(BossModule module) : Components.GroupedAOEs(module, [AID.BurntStrikeFire, AID.BurntStrikeLightning, AID.BurntStrikeLight, AID.CloneBurntStrikeFire, AID.CloneBurntStrikeLightning], new AOEShapeRect(80, 5));

class Burnout(BossModule module) : Components.GroupedAOEs(module, [AID.Burnout, AID.CloneBurnout], new AOEShapeRect(80, 10));
class ShiningBlade(BossModule module) : Components.StandardAOEs(module, AID.ShiningBlade, 6);

class Blastburn(BossModule module) : Components.Knockback(module)
{
    public const float Distance = 12;
    private readonly List<(Actor Caster, WPos Origin, Angle Rotation, DateTime Activation)> _sources = [];

    private static WPos NearestOnLine(WPos p, WPos origin, Angle rotation)
    {
        var dir = rotation.ToDirection();
        return origin + (p - origin).Dot(dir) * dir;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var s in _sources)
            yield return new(NearestOnLine(actor.Position, s.Origin, s.Rotation), Distance, s.Activation, Kind: Kind.AwayFromOrigin);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Blastburn or AID.CloneBlastburn)
            _sources.Add((caster, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Blastburn or AID.CloneBlastburn)
            _sources.RemoveAll(s => s.Caster == caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        static WPos Predict(WPos p, WPos lineOrigin, Angle lineRotation) => AwayFromSource(p, NearestOnLine(p, lineOrigin, lineRotation), Distance);
        foreach (var s in _sources)
        {
            if (IsImmune(slot, s.Activation))
                continue;
            var origin = s.Origin;
            var rot = s.Rotation;
            hints.AddForbiddenZone(Sdf.Discrete(p => !Module.InBounds(Predict(p, origin, rot))), s.Activation);
        }
    }
}

class Sinsmoke(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Sinsmoke, AID.Sinsmoke, 6, 8);
class Sinsight(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Sinsight, AID.Sinsight, 6, 8);

class BurnMark(BossModule module) : Components.UniformStackSpread(module, 0, 10)
{
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.PowderMark)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BurnMark)
            Spreads.Clear();
    }
}

class BrightfireFlame(BossModule module) : Components.StandardAOEs(module, AID.BrightfireFlame, 10);
class BrightfireLevin(BossModule module) : Components.StandardAOEs(module, AID.BrightfireLevin, 5);

class ShiftingSkyKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ShiftingSkyKB, 15);
class ResoundingCrack(BossModule module) : Components.StandardAOEs(module, AID.ResoundingCrack, new AOEShapeCone(40, 135.Degrees()));

class PrismaticDeception(BossModule module) : Components.CastHint(module, AID.PrismaticDeception, "Walk near the edge to find the clones.");

class BlastingZone(BossModule module) : Components.GenericAOEs(module, AID.BlastingZone)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(50, 8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.BlastingZoneVis || _aoes.Count > 0)
            return;

        var act = Module.CastFinishAt(spell);
        foreach (var image in Module.Enemies(OID.FatebreakersImage))
        {
            if (image.FindStatus(SID.BlastingZone) != null || image.FindStatus(SID.Invisible) != null)
            {
                // images face outward, lines fire across the arena toward center
                var dir = Angle.FromDirection(Module.Center - image.Position);
                _aoes.Add(new(_shape, image.Position, dir, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}

class E11FatebreakerStates : StateMachineBuilder
{
    public E11FatebreakerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurnishedGlory>()
            .ActivateOnEnter<PowderMark>()
            .ActivateOnEnter<BurnMark>()
            .ActivateOnEnter<BurntStrike>()
            .ActivateOnEnter<Burnout>()
            .ActivateOnEnter<Blastburn>()
            .ActivateOnEnter<ShiningBlade>()
            .ActivateOnEnter<Sinsmoke>()
            .ActivateOnEnter<Sinsight>()
            .ActivateOnEnter<BrightfireFlame>()
            .ActivateOnEnter<BrightfireLevin>()
            .ActivateOnEnter<ShiftingSkyKnockback>()
            .ActivateOnEnter<ResoundingCrack>()
            .ActivateOnEnter<PrismaticDeception>()
            .ActivateOnEnter<BlastingZone>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 751, NameID = 9707)]
public class E11Fatebreaker(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FatebreakersImage), ArenaColor.Object);
        Arena.Actors(Enemies(OID.DemiGukumatz), ArenaColor.Enemy);
    }
}
