namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D063LahabreaIgeyorhm;

public enum OID : uint
{
    Boss = 0x3DA4, // R3.5
    Igeyorhm = 0x3DA3, // R3.5
    BurningStar = 0x3DA6, // R1.5
    FrozenStar = 0x3DA5, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 32818, // Igeyorhm/Boss->player, no cast, single-target
    Teleport = 32791, // Igeyorhm->location, no cast, single-target

    AetherialDivideIgeyorhm = 32686, // Igeyorhm->Boss, no cast, single-target
    AetherialDivideLahabrea = 32685, // Boss->Igeyorhm, no cast, single-target

    CircleOfIce = 31878, // Igeyorhm->self, 3.0s cast, single-target
    CircleOfIceAOE = 31879, // FrozenStar->self, 3.0s cast, range 5-15 donut
    CircleOfIcePrime1 = 31881, // FrozenStar->self, no cast, single-target
    CircleOfIcePrime2 = 31882, // FrozenStar->self, no cast, single-target
    CircleOfIcePrimeAOE = 33019, // Helper->self, 3.0s cast, range 5-40 donut

    DarkFireII = 32687, // Boss->self, 6.0s cast, single-target
    DarkFireIIAOE = 32688, // Helper->player, 6.0s cast, range 6 circle

    EndOfDays = 31891, // Boss->self, 5.0s cast, single-target
    EndOfDaysAOE = 33029, // Helper->self, no cast, range 50 width 8 rect
    EndOfDaysTargetSelect = 31892, // Helper->player, no cast, single-target

    EsotericFusion1 = 31880, // Igeyorhm->self, 3.0s cast, single-target
    EsotericFusion2 = 31888, // Boss->self, 3.0s cast, single-target

    FireSphere = 31886, // Boss->self, 3.0s cast, single-target
    FireSphereAOE = 31887, // BurningStar->self, 3.0s cast, range 8 circle
    FireSpherePrime1 = 31889, // BurningStar->self, no cast, single-target
    FireSpherePrime2 = 31890, // BurningStar->self, no cast, single-target
    FireSpherePrimeAOE = 33020, // Helper->self, 2.0s cast, range 16 circle

    GripOfNight = 32790, // Igeyorhm->self, 6.0s cast, range 40 150-degree cone
    ShadowFlare = 31885, // Igeyorhm/Boss->self, 5.0s cast, range 40 circle
}

public enum TetherID : uint
{
    StarTether = 110 // BurningStar/FrozenStar->BurningStar/FrozenStar
}

class ShadowFlare(BossModule module) : Components.RaidwideCast(module, AID.ShadowFlare);
class GripOfNight(BossModule module) : Components.StandardAOEs(module, AID.GripOfNight, new AOEShapeCone(40, 75.Degrees()));
class DarkFireIIAOE(BossModule module) : Components.SpreadFromCastTargets(module, AID.DarkFireIIAOE, 6);
class EndofDays(BossModule module) : Components.SimpleLineStack(module, 4.1f, 50, AID.EndOfDaysTargetSelect, AID.EndOfDaysAOE, 0);

class Stars(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donutSmall = new(5, 15);
    private static readonly AOEShapeDonut donutBig = new(5, 40);
    private static readonly AOEShapeCircle circleSmall = new(8);
    private static readonly AOEShapeCircle circleBig = new(16);

    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Actor> _stars = [];

    private bool _tutorialFire;
    private bool _tutorialIce;
    private DateTime _activation;
    private AOEShape? _shape;
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_shape != null && _active)
        {
            foreach (var star in _stars)
                yield return new(_shape, star.Position, default, _activation);
            foreach (var aoe in _aoes)
                yield return new(aoe.Shape, aoe.Origin, default, aoe.Activation);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.StarTether)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            var targetPos = target.Position;
            var midpoint = new WPos((source.Position.X + targetPos.X) / 2, (source.Position.Z + targetPos.Z) / 2);
            switch (source.OID)
            {
                case (uint)OID.FrozenStar:
                    ActivateAOE(donutSmall, donutBig, midpoint, source, target);
                    break;
                case (uint)OID.BurningStar:
                    ActivateAOE(circleSmall, circleBig, midpoint, source, target);
                    break;
            }
        }
    }

    private void ActivateAOE(AOEShape smallShape, AOEShape bigShape, WPos midpoint, Actor source, Actor target)
    {
        _shape = smallShape;
        _active = true;
        _stars.Remove(source);
        _stars.Remove(target);
        _activation = WorldState.FutureTime(10.6f);
        _aoes.Add(new AOEInstance(bigShape, midpoint, default, _activation));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.FrozenStar or OID.BurningStar)
            _stars.Add(actor);
        if ((OID)actor.OID == OID.FrozenStar && !_tutorialIce)
            Tutorial(donutSmall, ref _tutorialIce);
        else if ((OID)actor.OID == OID.BurningStar && !_tutorialFire)
            Tutorial(circleSmall, ref _tutorialFire);
    }

    private void Tutorial(AOEShape shape, ref bool tutorialFlag)
    {
        _activation = WorldState.FutureTime(7.8f);
        tutorialFlag = true;
        _shape = shape;
        _active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CircleOfIceAOE or AID.CircleOfIcePrimeAOE or AID.FireSphereAOE or AID.FireSpherePrime1)
        {
            _shape = null;
            _stars.Clear();
            _aoes.Clear();
            NumCasts++;
            _active = false;
        }
    }
}

class D063LahabreaIgeyorhmStates : StateMachineBuilder
{
    public D063LahabreaIgeyorhmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShadowFlare>()
            .ActivateOnEnter<GripOfNight>()
            .ActivateOnEnter<Stars>()
            .ActivateOnEnter<EndofDays>()
            .ActivateOnEnter<DarkFireIIAOE>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Igeyorhm).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 2143)]
public class D063LahabreaIgeyorhm(WorldState ws, Actor primary) : BossModule(ws, primary, new(230, -180), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Igeyorhm), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 2,
                OID.Igeyorhm => 1,
                _ => 0
            };
        }
    }
}
