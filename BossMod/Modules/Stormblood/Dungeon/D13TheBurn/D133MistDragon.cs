namespace BossMod.Stormblood.Dungeon.D13TheBurn.D133MistDragon;
public enum OID : uint
{
    Boss = 0x2431, // R3.000-7.000, x?
    Mist = 0x2433, // R2.160, x?
    DraconicRegard = 0x2432, // R3.000, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    MistDragonHelper = 0x2434, // R7
    Voidzone = 0x1E8713
}

public enum AID : uint
{
    Attack = 870, // 241B/2419/2431->player, no cast, single-target
    RimeWreath = 12619, // 2431->self, 5.0s cast, range 40 circle
    FogPlume = 12612, // 2431->self, 3.0s cast, single-target
    FogPlumeImpact = 12613, // 233C->self, 3.0s cast, range 6 circle
    FogPlumeBurst = 12614, // 233C->self, 1.5s cast, range 40 width 5 cross
    Vaporize = 12608, // 2431->self, 4.0s cast, single-target
    ChillingAspiration = 12621, // 2431->self, no cast, range 40 width 6 rect
    FrostBreath = 12620, // 2431->self, no cast, range 20 ?-degree cone
    ColdFog = 12610, // 233C->self, no cast, range 4 circle
    ColdFogGrowth = 12609, // 2431->self, 24.0s cast, range 4 circle
    DeepFog = 12615, // Boss->self, 4.0s cast, range 40 circle
    Cauterize = 12616, // 2434->self, no cast, range 40 width 16 rect
    Cauterize2 = 12617, // Helper->self, 0.7s cast, range 40 width 16 rect
    Touchdown = 12618, // Boss->self, 6.0s cast, range 40 circle
}
public enum SID : uint
{
    Frostbite = 268,
    DeepFreeze = 1254,
    AreaInfluence = 618,
    Invincibility = 775,
    Transfiguration = 705,
    Concealed = 1621,
}

public enum IconID : uint
{
    ChillingAspiration = 14,
    FrostBreathCleave = 26,
}
public enum TetherID : uint
{
    DragonicRegard = 85,
}
class RimeWreath(BossModule module) : Components.RaidwideCast(module, AID.RimeWreath);
class FogPlumeImpact(BossModule module) : Components.StandardAOEs(module, AID.FogPlumeImpact, new AOEShapeCircle(6));
class FogPlumeBurst(BossModule module) : Components.StandardAOEs(module, AID.FogPlumeBurst, new AOEShapeCross(40, 2.5f));
class ColdFog(BossModule module) : Components.StandardAOEs(module, AID.ColdFog, new AOEShapeCircle(4));
class ColdFogGrowth(BossModule module) : Components.GenericAOEs(module, AID.ColdFogGrowth)
{
    private readonly List<AOEInstance> _aoes = [];
    private float Radius = 4;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            yield return _aoes[0] with { Color = ArenaColor.AOE };
        }
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID != SID.AreaInfluence)
            return;

        if (_aoes.Count == 0)
        {
            _aoes.Add(new(new AOEShapeCircle(Radius), Module.Center));
        }
        else if (_aoes.Count > 0)
        {
            Radius += 1.3f;
            _aoes.RemoveAt(0);
            _aoes.Add(new(new AOEShapeCircle(Radius), Module.Center));
        }
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (_aoes.Count > 0 && (SID)status.ID == SID.AreaInfluence)
        {
            Radius = 4;
            _aoes.RemoveAt(0);
        }
    }
}
class ChillingAspiration(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(40, 3), (uint)IconID.ChillingAspiration, AID.ChillingAspiration);
class ChillingPuddles(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7));
class FrostBreath(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(20, 45.Degrees()), (uint)IconID.FrostBreathCleave, AID.FrostBreath);
class CauterizeDB(BossModule module) : Components.StandardAOEs(module, AID.Cauterize, new AOEShapeRect(40, 8, 5));
class CauterizeDB2(BossModule module) : Components.StandardAOEs(module, AID.Cauterize2, new AOEShapeRect(40, 8, 5));
class CauterizeConceal(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var i in _aoes)
                yield return i with { Color = ArenaColor.AOE };
        }
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Concealed)
        {
            _aoes.Add(new AOEInstance(new AOEShapeRect(40, 8, 5), actor.Position + 2 * actor.Rotation.ToDirection(), actor.Rotation));
        }
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Concealed)
        {
            _aoes.Clear();
        }
    }
}
class TouchDown(BossModule module) : Components.StandardAOEs(module, AID.Touchdown, new AOEShapeCircle(15));
class Adds(BossModule module) : Components.AddsMulti(module, [OID.DraconicRegard, OID.Mist])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Mist => 3,
                OID.DraconicRegard => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
}
class D133MistDragonStates : StateMachineBuilder
{
    public D133MistDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RimeWreath>()
            .ActivateOnEnter<FogPlumeImpact>()
            .ActivateOnEnter<FogPlumeBurst>()
            .ActivateOnEnter<ColdFog>()
            .ActivateOnEnter<ColdFogGrowth>()
            .ActivateOnEnter<ChillingAspiration>()
            .ActivateOnEnter<ChillingPuddles>()
            .ActivateOnEnter<FrostBreath>()
            .ActivateOnEnter<CauterizeDB>()
            .ActivateOnEnter<CauterizeDB2>()
            .ActivateOnEnter<CauterizeConceal>()
            .ActivateOnEnter<TouchDown>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 585, NameID = 7672)]
public class D133MistDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -400f), new ArenaBoundsCircle(20f));
