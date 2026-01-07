namespace BossMod.Dawntrail.Raid.RM09VampFatale;

public enum OID : uint
{
    Boss = 0x4ADC, // R4.000, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    VampetteFatale = 0x4C2E, // R1.200, x?
    Coffinmaker = 0x4ADD, // R10.000, x?
    FatalFlail = 0x4ADE, // R3.000, x?
    Neckbiter = 0x4AE5, // R3.000, x?
    Coffinmaker2 = 0x4AE6, // R1.000, x?
}

public enum AID : uint
{
    AutoAttack = 48038, // 4ADC->player, no cast, single-target
    KillerVoice = 45921, // 4ADC->self, 5.0s cast, range 60 circle

    HalfMoonVisual1 = 48823, // 4ADC->self, 4.3+0.7s cast, single-target
    HalfMoonShort1 = 45906, // 233C->self, 5.0s cast, range 60 180-degree cone -- First Cast
    HalfMoonLong1 = 45907, // 233C->self, 8.5s cast, range 60 180-degree cone -- Second Cast

    HalfMoonVisual2 = 48825, // 4ADC->self, 4.3+0.7s cast, single-target
    HalfMoonShort2 = 45910, // 233C->self, 5.0s cast, range 60 180-degree cone
    HalfMoonLong2 = 45911, // 233C->self, 8.5s cast, range 60 180-degree cone

    HalfMoonVisual3 = 48826, // 4ADC->self, 4.3+0.7s cast, single-target
    HalfMoonShort3 = 45912, // 233C->self, 5.0s cast, range 64 180-degree cone
    HalfMoonLong3 = 45913, // 233C->self, 8.5s cast, range 64 180-degree cone

    HalfMoonVisual4 = 48824, // 4ADC->self, 4.3+0.7s cast, single-target
    HalfMoonShort4 = 45908, // 233C->self, 5.0s cast, range 64 180-degree cone
    HalfMoonLong4 = 45909, // 233C->self, 8.5s cast, range 64 180-degree cone

    VampStomp = 45898, // 4ADC->location, 4.1+0.9s cast, single-target
    VampStomp1 = 45899, // 233C->self, 5.0s cast, range 10 circle
    BlastBeat = 45901, // 4C2E->self, 1.5s cast, range 8 circle
    Hardcore = 45914, // 4ADC->self, 3.0+2.0s cast, single-target
    Hardcore1 = 45915, // 233C->player, 5.0s cast, range 6 circle
    SadisticScreech = 45875, // 4ADC->self, 5.0s cast, single-target
    SadisticScreech1 = 45876, // 233C->self, no cast, range 60 circle
    DeadWakeProgression = 46853, // 4ADD->self, 4.5+0.5s cast, single-target
    DeadWake1 = 45877, // 233C->self, 5.0s cast, range 10 width 20 rect

    Coffinfiller1 = 46854, // 4ADD->self, 5.0s cast, single-target
    CoffinfillerLong = 45878, // 233C->self, 5.0s cast, range 32 width 5 rect
    CoffinfillerMed = 45879, // 233C->self, 5.0s cast, range 22 width 5 rect
    CoffinfillerShort = 45880, // 233C->self, 5.0s cast, range 12 width 5 rect

    FlayingFry = 45922, // 4ADC->self, 4.3+0.7s cast, single-target
    FlayingFry1 = 45923, // 233C->player, 5.0s cast, range 5 circle

    PenetratingPitch = 45924, // 4ADC->self, 6.3+0.7s cast, single-target
    PenetratingPitch1 = 45925, // 233C->players, 7.0s cast, range 5 circle

    CrowdKill = 45886, // 4ADC->self, 0.5+4.9s cast, single-target
    CrowdKill1 = 45887, // 233C->self, no cast, range 60 circle

    FinaleFatale = 45888, // 4ADC->self, 5.0s cast, single-target
    FinaleFatale1 = 45890, // 233C->self, no cast, range 60 circle
    PulpingPulse = 45894, // 233C->location, 4.0s cast, range 5 circle
    Aetherletting = 45895, // 4ADC->self, 5.8+1.2s cast, single-target
    Aetherletting1 = 45896, // 233C->players, 7.0s cast, range 6 circle
    AetherlettingCross = 45897, // 233C->self, 7.0s cast, range 40 width 6 cross
    BrutalRain = 45917, // 4ADC->self, 3.8+1.2s cast, single-target
    BrutalRain1 = 45920, // 233C->players, no cast, range 6 circle
    InsatiableThirst = 45892, // 4ADC->self, 2.8+2.2s cast, single-target
    InsatiableThirst1 = 45893, // 233C->self, no cast, range 60 circle
    Gravegrazer = 45881, // 233C->self, no cast, range 10 width 5 rect
    Gravegrazer1 = 45882, // 233C->self, no cast, range 5 width 5 rect
    Plummet = 45883, // 233C->self, 7.0s cast, range 3 circle
    Hardcore2 = 45916, // 233C->player, 5.0s cast, range 15 circle
    FinaleFatale2 = 45889, // 4ADC->self, 5.0s cast, single-target
    FinaleFatale3 = 45891, // 233C->self, no cast, range 60 circle

    BarbedBurst = 45885, // 4ADE->self, 16.0s cast, range 60 circle
}

public enum IconID : uint
{
    TankBuster = 344,
    FlayFrySpread = 376,
    PenPitchStack = 656,
    AetherlettingSpread = 655,
    BrutalRainStack = 305
}
class KillerVoice(BossModule module) : Components.RaidwideCast(module, AID.KillerVoice);
class HalfMoon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoesShort = [];
    private readonly List<AOEInstance> _aoesLong = [];
    private static readonly AOEShapeCone cone = new(60f, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoesShort.Count > 0)
        {
            foreach (var e in _aoesShort)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
        if (_aoesLong.Count > 0 && (_aoesShort.Count == 0))
        {
            foreach (var e in _aoesLong)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HalfMoonShort1 || (AID)spell.Action.ID is AID.HalfMoonShort2 || (AID)spell.Action.ID is AID.HalfMoonShort3 || (AID)spell.Action.ID is AID.HalfMoonShort4)
        {
            _aoesShort.Add(new(cone, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));

        }
        else if ((AID)spell.Action.ID is AID.HalfMoonLong1 || (AID)spell.Action.ID is AID.HalfMoonLong2 || (AID)spell.Action.ID is AID.HalfMoonLong3 || (AID)spell.Action.ID is AID.HalfMoonLong4)
        {
            _aoesLong.Add(new(cone, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoesShort.Count > 0 && ((AID)spell.Action.ID == AID.HalfMoonShort1 || (AID)spell.Action.ID == AID.HalfMoonShort2 || (AID)spell.Action.ID == AID.HalfMoonShort3 || (AID)spell.Action.ID == AID.HalfMoonShort4))
        {
            _aoesShort.Clear();
        }
        else if (_aoesLong.Count > 0 && ((AID)spell.Action.ID == AID.HalfMoonLong1 || (AID)spell.Action.ID == AID.HalfMoonLong2 || (AID)spell.Action.ID == AID.HalfMoonLong3 || (AID)spell.Action.ID == AID.HalfMoonLong4))
        {
            _aoesLong.Clear();
        }
    }
}
class VampStomp(BossModule module) : Components.StandardAOEs(module, AID.VampStomp, 10f);
class BlastBeat(BossModule module) : Components.StandardAOEs(module, AID.BlastBeat, 8f);
class Hardcore(BossModule module) : Components.BaitAwayCast(module, AID.Hardcore1, new AOEShapeCircle(6), true);
class HardcoreBig(BossModule module) : Components.BaitAwayCast(module, AID.Hardcore2, new AOEShapeCircle(15), true);
class SadisticScreech(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 20, -10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SadisticScreech)
        {
            _aoes.Add(new(rect, Module.PrimaryActor.Position, 90.Degrees()));
            _aoes.Add(new(rect, Module.PrimaryActor.Position, -90.Degrees()));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SadisticScreech)
        {
            if ((NumCasts & 1) == 0)
            {
                Arena.Bounds = new ArenaBoundsRect(10, 20);
            }
            else
            {
                Arena.Bounds = new ArenaBoundsRect(20, 20);
            }
            NumCasts++;
            _aoes.Clear();
        }
    }
}
class DeadWake(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(15, 20, -5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DeadWakeProgression)
        {
            _aoes.Add(new(rect, caster.Position));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Coffinmaker)
        {
            _aoes.Clear();
        }
    }
}
class DeadWakeIcons(BossModule module) : Components.IconStackSpread(module, (uint)IconID.PenPitchStack, (uint)IconID.FlayFrySpread, AID.PenetratingPitch1, AID.FlayingFry1, 5, 5, 0, alwaysShowSpreads: true);
class Coffinfiller(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(ulong CasterID, AOEInstance Aoe)> _aoes = [];
    private static readonly AOEShapeRect rectLong = new(32, 2.5f);
    private static readonly AOEShapeRect rectMed = new(22, 2.5f);
    private static readonly AOEShapeRect rectShort = new(12, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes.Take(2))
            {
                yield return e.Aoe with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CoffinfillerLong)
        {
            _aoes.Add((caster.InstanceID, new(rectLong, caster.Position)));
        }
        if ((AID)spell.Action.ID is AID.CoffinfillerMed)
        {
            _aoes.Add((caster.InstanceID, new(rectMed, caster.Position)));
        }
        if ((AID)spell.Action.ID is AID.CoffinfillerShort)
        {
            _aoes.Add((caster.InstanceID, new(rectShort, caster.Position)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CoffinfillerLong || (AID)spell.Action.ID is AID.CoffinfillerMed || (AID)spell.Action.ID is AID.CoffinfillerShort)
        {
            int idx = _aoes.FindIndex(x => x.CasterID == caster.InstanceID);
            if (idx >= 0)
                _aoes.RemoveAt(idx);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Coffinmaker)
        {
            _aoes.Clear();
        }
    }
}
class FinaleFatale(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeDonut donut = new(20, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FinaleFatale || (AID)spell.Action.ID is AID.FinaleFatale2)
        {
            _aoes.Add(new(donut, Module.Center));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FinaleFatale || (AID)spell.Action.ID is AID.FinaleFatale2)
        {
            Arena.Bounds = new ArenaBoundsCircle(20);
            NumCasts++;
            _aoes.Clear();
        }
    }
}
class InsatiableThirst(BossModule module) : Components.RaidwideCast(module, AID.InsatiableThirst)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.InsatiableThirst)
        {
            Arena.Bounds = new ArenaBoundsRect(20, 20);
            NumCasts++;
        }
    }
}
class Aetherletting(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AetherlettingSpread, AID.Aetherletting1, 6, 0, true);
class PulpingPulse(BossModule module) : Components.StandardAOEs(module, AID.PulpingPulse, 5);
class AetherlettingCross(BossModule module) : Components.StandardAOEs(module, AID.AetherlettingCross, new AOEShapeCross(40, 3f));
class BrutalRain(BossModule module) : Components.StackWithIcon(module, (uint)IconID.BrutalRainStack, AID.BrutalRain, 6, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PulpingPulse)
        {
            Stacks.Clear();
            ++NumFinishedStacks;
        }
    }
}
class Plummet(BossModule module) : Components.CastTowers(module, AID.Plummet, 3f, 1);
class HazardDance : Components.PersistentVoidzone
{
    private readonly List<Actor> _hazards = [];

    public HazardDance(BossModule module) : base(module, 2.5f, _ => [], 15)
    {
        Sources = _ => _hazards;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Coffinmaker2 or OID.Neckbiter && !actor.Position.AlmostEqual(Module.Center, 5))
            _hazards.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Coffinmaker2 or OID.Neckbiter)
            _hazards.Remove(actor);
    }
}
class RM09VampFataleStates : StateMachineBuilder
{
    public RM09VampFataleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KillerVoice>()
            .ActivateOnEnter<HalfMoon>()
            .ActivateOnEnter<Hardcore>()
            .ActivateOnEnter<HardcoreBig>()
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<BlastBeat>()
            .ActivateOnEnter<SadisticScreech>()
            .ActivateOnEnter<DeadWake>()
            .ActivateOnEnter<DeadWakeIcons>()
            .ActivateOnEnter<Coffinfiller>()
            .ActivateOnEnter<FinaleFatale>()
            .ActivateOnEnter<PulpingPulse>()
            .ActivateOnEnter<BrutalRain>()
            .ActivateOnEnter<InsatiableThirst>()
            .ActivateOnEnter<Aetherletting>()
            .ActivateOnEnter<AetherlettingCross>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<HazardDance>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1068, NameID = 14300)]
public class RM09VampFatale(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FatalFlail), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Coffinmaker), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 3,
                OID.Coffinmaker => 2,
                OID.FatalFlail => 1,
                _ => 0
            };
        }
    }
}
