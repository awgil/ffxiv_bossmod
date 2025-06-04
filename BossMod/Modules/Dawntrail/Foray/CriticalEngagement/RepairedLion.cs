namespace BossMod.Dawntrail.Foray.CriticalEngagement.RepairedLion;

public enum OID : uint
{
    Boss = 0x46CC, // R7.420, x1
    Helper = 0x233C, // R0.500, x24, Helper type
    CrescentLionStatant = 0x45CF, // R3.500, x3 (spawn during fight)
    HolySphere = 0x46CD, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    RadiantWaveCast = 41279, // Boss->self, 5.0s cast, single-target
    RadiantWave = 41345, // Helper->self, no cast, ???
    AetherialExchangeCast = 41280, // Boss->self, 3.0+1.0s cast, single-target
    AetherialExchange1 = 41276, // Helper->46CD, 1.0s cast, single-target
    AetherialExchange2 = 41281, // Helper->46CD, 1.0s cast, single-target
    AncientAeroIIICast = 41286, // Boss->self, 4.0+1.0s cast, single-target
    AncientAeroIIIFast = 41287, // Helper->self, 5.0s cast, range 40 60-degree cone
    AncientStoneIIICast = 41288, // Boss->self, 4.0+1.0s cast, single-target
    AncientStoneIIIFast = 41289, // Helper->self, 5.0s cast, range 40 60-degree cone
    LightSurge = 41294, // 46CD->self, 2.0s cast, range 15 circle
    WindSurge = 41295, // 46CD->self, 2.0s cast, range 15 circle
    SandSurge = 41296, // 46CD->self, 2.0s cast, range 15 circle
    HolyBlaze = 41297, // Boss->self, 4.0s cast, range 60 width 5 rect
    Scratch = 41301, // Boss->player, 5.0s cast, single-target
    Fissure = 41283, // Boss->self, no cast, single-target
    AncientHolyCast = 41282, // Boss->self, no cast, single-target
    AncientHolyAOE1 = 41395, // Helper->location, 11.0s cast, ???
    AncientHolyAOE2 = 41284, // Helper->location, 11.0s cast, ???
    DoubleCast1 = 41290, // Boss->self, 11.5+0.5s cast, single-target
    AncientStoneIIISlow = 41293, // Helper->self, 12.0s cast, range 40 60-degree cone
    AncientAeroIIISlow = 41292, // Helper->self, 12.0s cast, range 40 60-degree cone
    DoubleCast2 = 41291, // Boss->self, 11.5+0.5s cast, single-target
}

public enum SID : uint
{
    HolySphereElement = 2536, // Helper->HolySphere, extra=0x225/0x224
}

class RadiantWave(BossModule module) : Components.RaidwideCast(module, AID.RadiantWaveCast);
class HolyBlaze(BossModule module) : Components.StandardAOEs(module, AID.HolyBlaze, new AOEShapeRect(60, 2.5f));
class Scratch(BossModule module) : Components.SingleTargetCast(module, AID.Scratch);
class AncientHoly(BossModule module) : Components.StandardAOEs(module, AID.AncientHolyAOE2, 26);

class AncientFast(BossModule module) : Components.GroupedAOEs(module, [AID.AncientAeroIIIFast, AID.AncientStoneIIIFast], new AOEShapeCone(40, 30.Degrees()));

public enum Element
{
    None,
    Wind,
    Stone
}

class HolySphere(BossModule module) : BossComponent(module)
{
    public class Sphere(Actor actor)
    {
        public Actor Actor = actor;
        public Element Element;
    }

    private readonly List<Sphere> _spheres = [];

    public IEnumerable<Sphere> Spheres => _spheres;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.HolySphere)
            _spheres.Add(new(actor));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HolySphereElement)
        {
            var ix = _spheres.FindIndex(s => s.Actor == actor);
            if (ix >= 0)
            {
                _spheres[ix].Element = status.Extra switch
                {
                    0x224 => Element.Wind,
                    0x225 => Element.Stone,
                    _ => Element.None
                };
                if (_spheres[ix].Element == Element.None)
                    ReportError($"unknown element for {actor}: 0x{status.Extra:X}");
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WindSurge or AID.SandSurge or AID.LightSurge)
            _spheres.RemoveAll(s => s.Actor == caster);
    }
}

abstract class SurgePredict(BossModule module, AOEShape shape, int maxCasts = int.MaxValue) : Components.GenericAOEs(module)
{
    protected readonly HolySphere _spheres = module.FindComponent<HolySphere>()!;
    protected readonly List<(Actor, DateTime)> _explosions = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _explosions.Take(maxCasts).Select(e => new AOEInstance(new AOEShapeCircle(15), e.Item1.Position, Activation: e.Item2));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WindSurge or AID.SandSurge or AID.LightSurge)
            _explosions.RemoveAll(s => s.Item1 == caster);
    }

    protected void Predict(Element e, ActorCastInfo spell) => _explosions.AddRange(_spheres.Spheres.Where(s => s.Element == e && shape.Check(s.Actor.Position, spell.LocXZ, spell.Rotation)).Select(s => (s.Actor, Module.CastFinishAt(spell, 2.5f))));
}

class SurgePredictBasic(BossModule module) : SurgePredict(module, new AOEShapeCone(40, 30.Degrees()))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AncientAeroIIIFast:
                Predict(Element.Wind, spell);
                break;
            case AID.AncientStoneIIIFast:
                Predict(Element.Stone, spell);
                break;
        }
    }
}

class SurgePredictLight(BossModule module) : SurgePredict(module, new AOEShapeCircle(100))
{
    private DateTime _activation;
    private bool _active;

    public override void Update()
    {
        if (_activation != default && !_active)
        {
            var light = _spheres.Spheres.Where(s => s.Element == Element.None);
            if (light.Count() <= 4)
            {
                _explosions.AddRange(light.Select(s => (s.Actor, _activation.AddSeconds(2.5f))));
                _active = true;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.AncientHolyAOE2)
            _activation = Module.CastFinishAt(spell);

        if ((AID)spell.Action.ID == AID.LightSurge)
        {
            _activation = default;
            _active = false;
        }
    }
}

class AncientSlow(BossModule module) : Components.GroupedAOEs(module, [AID.AncientStoneIIISlow, AID.AncientAeroIIISlow], new AOEShapeCone(40, 30.Degrees()), maxCasts: 4, highlightImminent: true);

class SurgePredictSlow(BossModule module) : SurgePredict(module, new AOEShapeCone(40, 30.Degrees()), maxCasts: 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AncientStoneIIISlow:
                Predict(Element.Stone, spell);
                break;
            case AID.AncientAeroIIISlow:
                Predict(Element.Wind, spell);
                break;
        }
    }
}

class RepairedLionStates : StateMachineBuilder
{
    public RepairedLionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RadiantWave>()
            .ActivateOnEnter<HolyBlaze>()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<AncientHoly>()
            .ActivateOnEnter<AncientFast>()
            .ActivateOnEnter<AncientSlow>()
            .ActivateOnEnter<HolySphere>()
            .ActivateOnEnter<SurgePredictBasic>()
            .ActivateOnEnter<SurgePredictLight>()
            .ActivateOnEnter<SurgePredictSlow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13649)]
public class RepairedLion(WorldState ws, Actor primary) : BossModule(ws, primary, new(870, 180), new ArenaBoundsCircle(25))
{
    public override bool DrawAllPlayers => true;
}

