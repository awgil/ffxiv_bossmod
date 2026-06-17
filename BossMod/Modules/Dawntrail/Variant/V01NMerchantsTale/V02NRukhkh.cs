namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V02NRukhkh;

public enum OID : uint
{
    Boss = 0x4AD5, // R7.500, x?
    SandPearl = 0x4AD6, // R1.500-4.500, x?
    GrowBall = 0x4ADA,
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{

    SphereOfSand = 45748, // 4AD5->self, 3.0s cast, range 40 circle
    Sandplume = 45751, // 4AD5->self, 5.0s cast, single-target
    SandplumeShort = 46836, // 233C->self, 2.5s cast, range 20 45-degree cone
    SandplumeLong = 45752, // 233C->self, 5.3s cast, range 20 45-degree cone

    Grow = 45959, // 233C->4AD6, no cast, single-target

    SandBurst = 46835, // 4AD5->self, 3.0s cast, single-target
    SphereShatterSmall = 45749, // 4AD6->self, 1.0s cast, range 5 circle
    SphereShatterBig = 45750, // 4AD6->self, 1.0s cast, range 17 circle
    BanishingMist = 45753, // 4AD5->self, 4.0s cast, single-target

    BanishingMistBoss = 45754, // 4AD5->self, 3.0s cast, single-target
    SonicHowl = 45756, // 4AD5->self, no cast, single-target
    SonicHowlCast = 45761, // 233C->self, 3.0s cast, range 24 circle
    StreamingSands = 45764, // 4AD5->self, 5.0s cast, range 40 circle

    Beaksbane = 45765, // 233C->location, 6.0s cast, range 11 circle
    BitingScratch = 45763, // 4AD5->self, 5.0s cast, range 40 90-degree cone

    DryTyphoonCast = 46956, // 4AD5->self, 5.0s cast, single-target
    DryTyphoon1 = 46957, // 233C->self, 5.0s cast, range 20 30-degree cone
    DryTyphoon2 = 46958, // 233C->self, 4.5s cast, range 20 30-degree cone

    WindborneSeeds = 45760, // 4AD5->self, 3.0s cast, single-target
    Seedsprout = 47234, // 4AD5->self, 7.0s cast, single-target
    SyrupSpout = 45762, // 4ADA->self, 1.0s cast, range 9 circle

    BigBurst = 45758, // 4AD9->self, 4.0s cast, range 30 circle
    FallingRock = 45759, // Helper->self, no cast, range 9 width 6 rect

}
class SphereOfSand(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _pearls = [];
    private readonly List<Actor> _grownPearls = [];
    private static readonly AOEShapeCircle smallCirc = new(5f);
    private static readonly AOEShapeCircle bigCirc = new(17f);
    private bool _active;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
        {
            foreach (var pearl in _pearls)
            {
                var shape = _grownPearls.Contains(pearl) ? bigCirc : smallCirc;
                yield return new AOEInstance(shape, pearl.Position) with { Color = ArenaColor.AOE };
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SandPearl && !_pearls.Contains(actor))
            _pearls.Add(actor);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SphereOfSand)
        {
            _active = true;
            _grownPearls.Clear();
        }

        if ((AID)spell.Action.ID is AID.SphereShatterSmall or AID.SphereShatterBig)
        {
            _active = false;
            _grownPearls.Clear();
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SandplumeLong or AID.SandplumeShort)
        {
            var rot = caster.Rotation;

            if ((rot + 180f.Degrees()).Abs() < 1f.Degrees())
                rot = 180f.Degrees();

            foreach (var pearl in _pearls)
            {
                if (InCone(pearl.Position, Arena.Center, rot, 20f, 22.5f.Degrees()) && !_grownPearls.Contains(pearl))
                    _grownPearls.Add(pearl);
            }
        }
    }
    private static bool InCone(WPos point, WPos origin, Angle rotation, float length, Angle halfAngle)
    {
        var offset = point - origin;
        if (offset.LengthSq() > length * length)
            return false;

        var dir = offset.ToAngle();
        return (dir - rotation).Abs() <= halfAngle;
    }
}
class Sandplume(BossModule module) : Components.GroupedAOEs(module, [AID.SandplumeShort, AID.SandplumeLong], new AOEShapeCone(20f, 22.5f.Degrees()));
class SonicHowl(BossModule module) : Components.StandardAOEs(module, AID.SonicHowlCast, 24f)
{
    public bool _aiHint;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aiHint)
        {
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(Module.Arena.Center, 5f));
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID is AID.BanishingMistBoss)
        {
            _aiHint = true;
        }
        if ((AID)spell.Action.ID is AID.SonicHowlCast)
        {
            _aiHint = false;
        }
    }
}
class WindborneSeeds(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _seeds = [];
    private static readonly AOEShapeCircle circ = new(9f);
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
        {
            foreach (var s in _seeds)
            {
                yield return new AOEInstance(circ, s.Position) with { Color = ArenaColor.AOE };
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.GrowBall && !_seeds.Contains(actor))
            _seeds.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.GrowBall)
            _seeds.Remove(actor);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Seedsprout)
        {
            _active = true;
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Seedsprout)
        {
            _active = false;
        }
    }
}

class BitingScratch(BossModule module) : Components.StandardAOEs(module, AID.BitingScratch, new AOEShapeCone(40f, 45.Degrees()));
class StreamingSands(BossModule module) : Components.RaidwideCast(module, AID.StreamingSands);
class DryTyphoon(BossModule module) : Components.GroupedAOEs(module, [AID.DryTyphoon1, AID.DryTyphoon2], new AOEShapeCone(20f, 15.Degrees()), 8, true);
class Beaksbane(BossModule module) : Components.StandardAOEs(module, AID.Beaksbane, 11f, 3);
class BigBurst(BossModule module) : Components.StandardAOEs(module, AID.BigBurst, 10f);
class FallingRock(BossModule module) : Components.GenericAOEs(module, AID.FallingRock)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(18f, 3f, 18f);
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
    public override void OnMapEffect(byte index, uint state)
    {
        base.OnMapEffect(index, state);
        if (state is 0x00020001)
        {

            var offset = index switch
            {
                0 => -15,
                1 => -9,
                2 => -3,
                3 => 3,
                4 => 9,
                5 => 15,
                _ => default
            };
            if (offset != default)
                _aoes.Add(new(rect, new WPos(Module.Center.X, (Module.Center.Z + offset)), -90.Degrees(), WorldState.FutureTime(5.1f)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FallingRock)
        {
            _aoes.Clear();
        }
    }
}

class V02NRukhkhStates : StateMachineBuilder
{
    public V02NRukhkhStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sandplume>()
            .ActivateOnEnter<SphereOfSand>()
            .ActivateOnEnter<SonicHowl>()
            .ActivateOnEnter<WindborneSeeds>()
            .ActivateOnEnter<DryTyphoon>()
            .ActivateOnEnter<StreamingSands>()
            .ActivateOnEnter<BitingScratch>()
            .ActivateOnEnter<Beaksbane>()
            .ActivateOnEnter<BigBurst>()
            .ActivateOnEnter<FallingRock>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1066, NameID = 14377)]
public class V02NRukhkh(WorldState ws, Actor primary) : BossModule(ws, primary, new(primary.Position.X, primary.Position.Z + 10), new ArenaBoundsCircle(18f));
