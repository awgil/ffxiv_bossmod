namespace BossMod.Dawntrail.Trial.T06GuardianArkveld;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x?, Helper type
    Boss = 0x48E2, // R7.800, x?
    BigCrystal = 0x48E4, // R1.200, x?
    SmallCrystal = 0x48E3, // R0.700, x?
}

public enum AID : uint
{
    AutoAttack = 43341, // Boss->player, no cast, single-target

    Roar1 = 43886, // Boss->self, 5.0s cast, range 60 circle
    Roar2 = 45201, // Boss->self, 5.0s cast, range 60 circle

    ChainbladeBlowBossCast1 = 43828, // Boss->self, 5.0s cast, single-target
    ChainbladeBlowBossCast2 = 43829, // Boss->self, 5.0s cast, single-target
    ChainbladeBlowBossCast3 = 45052, // Boss->self, 4.0s cast, single-target
    ChainbladeBlowBossCast4 = 45053, // Boss->self, 4.0s cast, single-target

    ChainbladeBlow1 = 43830, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeBlow2 = 43831, // Helper->self, 6.6s cast, range 40 width 4 rect
    ChainbladeBlow3 = 45050, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeBlow4 = 45051, // Helper->self, 6.6s cast, range 40 width 4 rect
    ChainbladeBlow5 = 45054, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeBlow6 = 45055, // Helper->self, 5.6s cast, range 40 width 4 rect
    ChainbladeBlow7 = 45057, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeBlow8 = 45058, // Helper->self, 5.6s cast, range 40 width 4 rect

    ChainbladeSide1 = 43832, // Helper->self, 7.2s cast, range 80 width 28 rect
    ChainbladeSide2 = 43833, // Helper->self, 7.2s cast, range 80 width 28 rect
    ChainbladeSide3 = 45056, // Helper->self, 6.2s cast, range 80 width 28 rect
    ChainbladeSide4 = 45059, // Helper->self, 6.2s cast, range 80 width 28 rect

    WyvernsRadianceExawaveFirst = 43877, // Helper->self, 2.5s cast, range 8 width 40 rect
    WyvernsRadianceExawaveRest = 43878, // Helper->self, 1.0s cast, range 8 width 40 rect

    WyvernsRadiance1 = 43840, // Helper->self, 10.0s cast, range 40 width 18 rect
    WyvernsRadiance2 = 43839, // Helper->self, 10.0s cast, range 40 width 18 rect
    WyvernsRadiance3 = 45072, // Helper->self, 9.0s cast, range 40 width 18 rect
    WyvernsRadiance4 = 45073, // Helper->self, 9.0s cast, range 40 width 18 rect

    WyvernsRadiancePuddleSmall = 43857, // SmallCrystal->self, 1.4s cast, range 6 circle
    WyvernsRadiancePuddleLarge = 43858, // BigCrystal->self, 1.4s cast, range 12 circle

    WyvernsRadiance13 = 44807, // Helper->self, 2.0s cast, range 6 circle
    WyvernsRadiance12 = 44808, // Helper->self, 2.0s cast, range 12 circle

    WyvernsRadianceQuake1 = 43844, // Helper->self, 7.5s cast, range 8 circle
    WyvernsRadianceQuake2 = 43845, // Helper->self, 9.5s cast, range 20-14 donut
    WyvernsRadianceQuake3 = 43846, // Helper->self, 11.5s cast, range 14-20 donut
    WyvernsRadianceQuake4 = 43847, // Helper->self, 13.5s cast, range 20-26 donut

    WyveCannonMiddle = 43880, // Helper->self, 3.5s cast, range 40 width 8 rect
    WyveCannonRepeat = 43881, // Helper->self, 1.0s cast, range 40 width 4 rect
    WyveCannonEdge = 43882, // Helper->self, 2.0s cast, range 40 width 4 rect

    SiegeflightBoss1 = 43834, // Boss->location, 5.0s cast, range 40 width 4 rect
    SiegeflightBoss2 = 43837, // Boss->location, 5.0s cast, range 40 width 4 rect
    SiegeflightBoss3 = 45067, // Boss->location, 4.0s cast, range 40 width 4 rect
    SiegeflightBoss4 = 45068, // Boss->location, 4.0s cast, range 40 width 4 rect

    SiegeflightCast1 = 43835, // Helper->self, 6.5s cast, range 40 width 8 rect
    SiegeflightCast2 = 43838, // Helper->self, 6.5s cast, range 40 width 8 rect
    SiegeflightCast3 = 45069, // Helper->self, 5.5s cast, range 40 width 8 rect
    SiegeflightCast4 = 45071, // Helper->self, 5.5s cast, range 40 width 8 rect

    SiegeflightFollowup1 = 43836, // Helper->self, 10.0s cast, range 40 width 16 rect
    SiegeflightFollowup2 = 45070, // Helper->self, 9.0s cast, range 40 width 16 rect

    ChainbladeChargeCast = 43883, // Boss->self, 6.0s cast, single-target
    ChainbladeChargeVisual = 43884, // Boss->player, no cast, single-target
    ChainbladeChargeStack = 44811, // Helper->location, no cast, range 6 circle

    RushCast = 43842, // Boss->location, 6.0s cast, width 12 rect charge
    RushInstant = 43843, // Boss->location, no cast, width 12 rect charge

    RushTelegraph = 43841, // Helper->location, 2.5s cast, width 12 rect charge
    QuakeTelegraph = 45075, // Helper->self, 3.5s cast, range 8 circle

    WyvernsOurobladeCast1 = 43850, // Boss->self, 6.0+1.5s cast, single-target
    WyvernsOurobladeCast2 = 45060, // Boss->self, 5.0+1.5s cast, single-target
    WyvernsOurobladeCast3 = 45062, // Boss->self, 5.0+1.5s cast, single-target
    WyvernsOurobladeCast4 = 43848, // Boss->self, 6.0+1.5s cast, single-target

    WyvernsOuroblade1 = 43851, // Helper->self, 7.0s cast, range 40 180-degree cone
    WyvernsOuroblade2 = 45061, // Helper->self, 6.0s cast, range 40 180-degree cone
    WyvernsOuroblade3 = 45063, // Helper->self, 6.0s cast, range 40 180-degree cone
    WyvernsOuroblade4 = 43849, // Helper->self, 7.0s cast, range 40 180-degree cone

    GuardianResonance1 = 43856, // Helper->location, 4.0s cast, range 6 circle
    GuardianResonance2 = 43853, // Helper->location, 13.0s cast, range 2 circle
    GuardianResonance3 = 43854, // Helper->location, 13.0s cast, range 4 circle

    FlightResonanceVisual1 = 45123, // Helper->self, 7.2s cast, range 40 width 8 rect
    FlightResonanceVisual2 = 45076, // Helper->self, 7.2s cast, range 40 width 8 rect
    FlightResonanceVisual3 = 45124, // Helper->self, 6.2s cast, range 40 width 8 rect
    FlightResonanceVisual4 = 45074, // Helper->self, 6.2s cast, range 40 width 8 rect

    AethericResonance = 43852, // Boss->self, 11.7+1.3s cast, single-target

    WyvernsVengeance = 43860, // Helper->self, 5.0s cast, range 6 circle
    WyvernsVengeance1 = 43861, // Helper->location, no cast, range 6 circle

    WildEnergy = 43866, // Helper->players, 8.0s cast, range 6 circle

    ForgedFuryCast = 43869, // Boss->self, 5.0s cast, single-target
    ForgedFuryHit1 = 43870, // Helper->self, 7.0s cast, range 60 circle
    ForgedFuryHit2 = 44790, // Helper->self, 7.8s cast, range 60 circle
    ForgedFuryHit3 = 44791, // Helper->self, 10.2s cast, range 60 circle

    WyvernsWealCast1 = 45046, // Boss->self, 6.5+1.5s cast, single-target
    WyvernsWealCast2 = 45047, // Boss->self, 6.5+1.5s cast, single-target
    WyvernsWeal1 = 45048, // Helper->self, 8.0s cast, range 60 width 6 rect
    WyvernsWeal2 = 43875, // Helper->self, no cast, range 60 width 6 rect
    WyvernsWeal3 = 45049, // Helper->self, 8.0s cast, range 60 width 6 rect

    WrathfulRattle = 43879, // Boss->self, 1.0+2.5s cast, single-target

    SteeltailThrustCast = 45064, // Boss->self, 3.0s cast, range 60 width 6 rect
    SteeltailThrust = 44804, // Helper->self, 3.6s cast, range 60 width 6 rect

    WyvernsRattle = 43876, // Boss->self, no cast, single-target
    GreaterResonance = 43855, // Helper->location, no cast, range 60 circle

    Unk0 = 45175, // Boss->location, no cast, single-target
    Unk1 = 43827, // Boss->location, no cast, single-target
    Unk2 = 43859, // Boss->self, no cast, single-target
    Unk3 = 43873, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    Share = 100, // player->self
}

class Roar1(BossModule module) : Components.RaidwideCast(module, AID.Roar1);
class Roar2(BossModule module) : Components.RaidwideCast(module, AID.Roar2);

class ChainbladeBlow(BossModule module) : Components.GroupedAOEs(module, [AID.ChainbladeBlow1, AID.ChainbladeBlow2, AID.ChainbladeBlow3, AID.ChainbladeBlow4, AID.ChainbladeBlow5, AID.ChainbladeBlow6, AID.ChainbladeBlow7, AID.ChainbladeBlow8], new AOEShapeRect(40, 2));
class ChainbladeSide(BossModule module) : Components.GroupedAOEs(module, [AID.ChainbladeSide1, AID.ChainbladeSide2, AID.ChainbladeSide3, AID.ChainbladeSide4], new AOEShapeRect(80, 14));

class SiegeflightBoss(BossModule module) : Components.GroupedAOEs(module, [AID.SiegeflightBoss1, AID.SiegeflightBoss2, AID.SiegeflightBoss3, AID.SiegeflightBoss4], new AOEShapeRect(40, 2));
class SiegeflightCast(BossModule module) : Components.GroupedAOEs(module, [AID.SiegeflightCast1, AID.SiegeflightCast2, AID.SiegeflightCast3, AID.SiegeflightCast4], new AOEShapeRect(40, 4));
class SiegeflightFollowup(BossModule module) : Components.GroupedAOEs(module, [AID.SiegeflightFollowup1, AID.SiegeflightFollowup2], new AOEShapeRect(40, 8))
{
    private bool _siegeflightComplete;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => !_siegeflightComplete ? [] : base.ActiveAOEs(slot, actor);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SiegeflightCast1 or AID.SiegeflightCast2 or AID.SiegeflightCast3 or AID.SiegeflightCast4)
            _siegeflightComplete = true;
        base.OnEventCast(caster, spell);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SiegeflightBoss1 or AID.SiegeflightBoss2 or AID.SiegeflightBoss3 or AID.SiegeflightBoss4)
            _siegeflightComplete = false;
        base.OnCastStarted(caster, spell);
    }
}

class WyvernsRadiance(BossModule module) : Components.GroupedAOEs(module, [AID.WyvernsRadiance1, AID.WyvernsRadiance2, AID.WyvernsRadiance3, AID.WyvernsRadiance4], new AOEShapeRect(40, 9))
{
    private bool _siegeflightComplete;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => !_siegeflightComplete ? [] : base.ActiveAOEs(slot, actor);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SiegeflightCast1 or AID.SiegeflightCast2 or AID.SiegeflightCast3 or AID.SiegeflightCast4)
            _siegeflightComplete = true;
        base.OnEventCast(caster, spell);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SiegeflightBoss1 or AID.SiegeflightBoss2 or AID.SiegeflightBoss3 or AID.SiegeflightBoss4)
            _siegeflightComplete = false;
        base.OnCastStarted(caster, spell);
    }
}

// TODO: add preview for when the towers are hit by an aoe
class WyvernsRadiancePuddle(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiancePuddleSmall, 6);
class WyvernsRadianceBigPuddle(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiancePuddleLarge, 12);
class WyvernsRadianceExawave(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8, 20))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsRadianceExawaveFirst)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 8,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.5f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WyvernsRadianceExawaveFirst or AID.WyvernsRadianceExawaveRest && Lines.Count > 0)
        {
            AdvanceLine(Lines[0], caster.Position);
            Lines.RemoveAll(l => l.ExplosionsLeft <= 0);
        }
    }
}
class WyvernsRadianceQuake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(8), new AOEShapeDonut(8, 14), new AOEShapeDonut(14, 20), new AOEShapeDonut(20, 26)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsRadianceQuake1)
            AddSequence(caster.Position, Module.CastFinishAt(spell), default);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.WyvernsRadianceQuake1 => 0,
            AID.WyvernsRadianceQuake2 => 1,
            AID.WyvernsRadianceQuake3 => 2,
            AID.WyvernsRadianceQuake4 => 3,
            _ => -1
        };
        if (order >= 0)
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class WyveCannonMiddle(BossModule module) : Components.Exaflare(module, new AOEShapeRect(40, 2, 40))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyveCannonMiddle)
        {
            var rnd = caster.Rotation.ToDirection();
            Lines.Add(new()
            {
                Next = caster.Position + rnd.OrthoR() * 2,
                Advance = rnd.OrthoR() * 4,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.6f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
            Lines.Add(new()
            {
                Next = caster.Position + rnd.OrthoL() * 2,
                Advance = rnd.OrthoL() * 4,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.6f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WyveCannonMiddle)
        {
            NumCasts++;
            foreach (var l in Lines)
                if (l.Next.AlmostEqual(caster.Position, 4) && l.Rotation.AlmostEqual(caster.Rotation, 0.1f))
                    AdvanceLine(l, caster.Position + l.Advance / 2);
        }

        if ((AID)spell.Action.ID == AID.WyveCannonRepeat)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 0.1f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}
class WyveCannonEdge(BossModule module) : Components.Exaflare(module, new AOEShapeRect(40, 2, 40))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyveCannonEdge)
        {
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = (Arena.Center - caster.Position).Normalized() * 4,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.6f,
                ExplosionsLeft = 10,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WyveCannonEdge or AID.WyveCannonRepeat)
        {
            var imminentLines = Lines.Select(l => l.ExplosionsLeft).DefaultIfEmpty(-1).Max();
            if (imminentLines == -1)
                return;

            var ix = Lines.FindIndex(l => l.ExplosionsLeft == imminentLines && l.Next.AlmostEqual(caster.Position, 0.5f));
            if (ix >= 0)
            {
                NumCasts++;
                AdvanceLine(Lines[ix], caster.Position);
            }
        }
    }
}

class Rush(BossModule module) : Components.GenericAOEs(module, AID.RushCast)
{
    private readonly List<(WPos From, WPos To, DateTime Activation)> _charges = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _charges)
            yield return new(new AOEShapeRect((c.To - c.From).Length(), 6), c.From, (c.To - c.From).ToAngle(), c.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RushTelegraph)
            _charges.Add((caster.Position, spell.LocXZ, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RushCast or AID.RushInstant)
        {
            NumCasts++;
            _charges.RemoveAll(c => c.To.AlmostEqual(spell.TargetXZ, 1));
        }
    }
}

class ChainbladeCharge(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Share, AID.ChainbladeChargeStack, 6, 8.3f, minStackSize: 5)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            Stacks.Clear(); // clear all stacks when resolved
            ++NumFinishedStacks;
        }
    }
};

class WyvernsOuroblade(BossModule module) : Components.GroupedAOEs(module, [AID.WyvernsOuroblade1, AID.WyvernsOuroblade2, AID.WyvernsOuroblade3, AID.WyvernsOuroblade4], new AOEShapeCone(40, 90.Degrees()));

class GuardianResonancePuddle(BossModule module) : Components.StandardAOEs(module, AID.GuardianResonance1, 6);
class GuardianResonanceTowerSmall(BossModule module) : Components.CastTowers(module, AID.GuardianResonance2, 2);
class GuardianResonanceTowerLarge(BossModule module) : Components.CastTowers(module, AID.GuardianResonance3, 4);

class WyvernsVengeance(BossModule module) : Components.GenericAOEs(module, AID.WyvernsVengeance1)
{
    private readonly List<(WPos pos, DateTime activation)> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in _aoes)
            yield return new(new AOEShapeCircle(6), aoe.pos, default, aoe.activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsVengeance1)
        {
            _aoes.Add((spell.TargetXZ, WorldState.CurrentTime.AddSeconds(0.5f)));
            NumCasts++;
        }
    }

    public override void Update() => _aoes.RemoveAll(aoe => WorldState.CurrentTime > aoe.activation.AddSeconds(1));
}

class ForgedFury(BossModule module) : Components.CastHint(module, null, "Raidwide")
{
    private readonly List<DateTime> _activations = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ForgedFuryHit1 or AID.ForgedFuryHit2 or AID.ForgedFuryHit3)
        {
            _activations.Add(Module.CastFinishAt(spell));
            _activations.Sort();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activations.Count > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activations.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _activations[0]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ForgedFuryHit1 or AID.ForgedFuryHit2 or AID.ForgedFuryHit3)
        {
            NumCasts++;
            if (_activations.Count > 0)
                _activations.RemoveAt(0);
        }
    }
}

class WyvernsWeal(BossModule module) : Components.GenericAOEs(module, null)
{
    private readonly List<(Angle rotation, DateTime activation)> _rotations = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsWeal1) // first: store initial rotation
        {
            _rotations.Clear();
            _rotations.Add((spell.Rotation, Module.CastFinishAt(spell)));
        }
        else if ((AID)spell.Action.ID is AID.WyvernsWeal2 or AID.WyvernsWeal3) // subsequent: add rotation at cast finish
            _rotations.Add((spell.Rotation, Module.CastFinishAt(spell, 0.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsWeal1)
        {
            if (_rotations.Count > 0)
            {
                var now = WorldState.CurrentTime;
                _rotations[^1] = (_rotations[^1].rotation, now);
                _rotations.Add((_rotations[^1].rotation, now.AddSeconds(0.7f)));
            }
        }
        else if ((AID)spell.Action.ID == AID.WyvernsWeal2)
            _rotations.Add((caster.Rotation, WorldState.CurrentTime.AddSeconds(0.7f)));

        if ((AID)spell.Action.ID is AID.WyvernsWeal1 or AID.WyvernsWeal2 or AID.WyvernsWeal3)
            NumCasts++;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var now = WorldState.CurrentTime;

        if (_rotations.Count == 0)
            yield break;

        // Only show non expired rotations (w/ 1s grace period)
        var activeRotations = _rotations.Where(r => r.activation.AddSeconds(1) > now).ToList();

        if (activeRotations.Count == 0)
            yield break;

        var current = activeRotations.Last();
        yield return new AOEInstance(new AOEShapeRect(60, 3), Module.PrimaryActor.Position, current.rotation, current.activation);

        if (activeRotations.Count >= 2)
        {
            var prev = activeRotations[^2];
            var delta = current.rotation - prev.rotation;
            var nextTime = current.activation.AddSeconds(0.7f);
            if (nextTime > now && nextTime < now.AddSeconds(5))
            {
                var predictedDelta = Math.Clamp(delta.Deg, -11.25f, 11.25f).Degrees();
                var nextRotation = current.rotation + predictedDelta;
                yield return new AOEInstance(new AOEShapeRect(60, 3), Module.PrimaryActor.Position, nextRotation, nextTime);
            }
        }
    }

    public override void Update() => _rotations.RemoveAll(r => r.activation.AddSeconds(2) < WorldState.CurrentTime);
}

class SteeltailThrust(BossModule module) : Components.StandardAOEs(module, AID.SteeltailThrust, new AOEShapeRect(60, 3));

class WildEnergy(BossModule module) : Components.SpreadFromCastTargets(module, AID.WildEnergy, 6);

class T06GuardianArkveldStates : StateMachineBuilder
{
    public T06GuardianArkveldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Roar1>()
            .ActivateOnEnter<Roar2>()
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<ChainbladeSide>()
            .ActivateOnEnter<SiegeflightBoss>()
            .ActivateOnEnter<SiegeflightCast>()
            .ActivateOnEnter<SiegeflightFollowup>()
            .ActivateOnEnter<WyvernsRadiance>()
            .ActivateOnEnter<WyvernsRadiancePuddle>()
            .ActivateOnEnter<WyvernsRadianceBigPuddle>()
            .ActivateOnEnter<WyvernsRadianceExawave>()
            .ActivateOnEnter<WyvernsRadianceQuake>()
            .ActivateOnEnter<WyveCannonMiddle>()
            .ActivateOnEnter<WyveCannonEdge>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<ChainbladeCharge>()
            .ActivateOnEnter<WyvernsOuroblade>()
            .ActivateOnEnter<GuardianResonancePuddle>()
            .ActivateOnEnter<GuardianResonanceTowerSmall>()
            .ActivateOnEnter<GuardianResonanceTowerLarge>()
            .ActivateOnEnter<WyvernsVengeance>()
            .ActivateOnEnter<ForgedFury>()
            .ActivateOnEnter<WyvernsWeal>()
            .ActivateOnEnter<SteeltailThrust>()
            .ActivateOnEnter<WildEnergy>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1043, NameID = 14237)]
public class T06GuardianArkveld(ModuleArgs init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
