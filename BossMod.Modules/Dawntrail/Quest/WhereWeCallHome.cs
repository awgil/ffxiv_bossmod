namespace BossMod.Dawntrail.Quest.WhereWeCallHome;

public enum OID : uint
{
    Boss = 0x4B4D, // R8.700, x1
    Helper = 0x233C, // R0.500, x28, Helper type
    Thunderhead = 0x4B4F, // R1.300, x0 (spawn during fight)
    BallLightning = 0x4B4E, // R1.500, x0 (spawn during fight)

    Spriggan = 0x4B52,
}

public enum AID : uint
{
    AutoAttack = 45128, // Boss->4B2C, no cast, single-target
    StormingRoar = 46834, // Boss->self, 5.0s cast, range 40 circle
    StormChase1 = 46825, // Boss->self, 6.0s cast, single-target
    StormChase2 = 46826, // Boss->self, 8.0s cast, single-target
    HyperconductivePlasmaCast = 47528, // Boss->self, 5.0s cast, single-target
    HyperconductivePlasma = 46833, // Helper->players, 6.0s cast, range 6 circle
    ElectrostaticTrouncing = 46829, // Boss->self, 11.0s cast, range 40 135-degree cone
    TrounceFast = 46830, // Helper->self, 5.0s cast, range 20 45-degree cone
    TrounceSlow = 46831, // Helper->self, 11.0s cast, range 20 45-degree cone
    Thundercloud1 = 46814, // Boss->self, 3.0s cast, single-target
    Thundercloud2 = 46815, // Boss->self, no cast, single-target
    Thundercloud3 = 46816, // Boss->self, 23.0s cast, single-target
    LightningBoltCloudInstant = 46817, // 4B4F->self, 1.0s cast, range 8 circle
    LightningBoltCloud = 46818, // 4B4F->self, 4.0s cast, range 8 circle
    LightningBoltSlow = 46827, // Helper->location, 8.5s cast, range 6 circle
    LightningBoltFast = 46828, // Helper->location, 6.5s cast, range 6 circle
    LightningBoltSpread = 46832, // Helper->player/4B2B/4B2D/4B2E/4B2C, 5.0s cast, range 6 circle
    StormingRoarStun = 46821, // Boss->self, 3.0s cast, range 40 circle
    Shock = 46824, // 4B4E->self, no cast, range 8 circle
    EclipticMeteor = 46822, // Boss->self, 57.0s cast, single-target
}

public enum IconID : uint
{
    Stack = 318, // player->self
    Spread = 139, // player/4B2E/4B2B/4B2D/4B2C->self
}

public enum TetherID : uint
{
    AetherSupply = 146, // 4B2E/4B2D->4B2B
    Lightning = 17, // 4B4E->4B2B
}

class StormingRoar(BossModule module) : Components.RaidwideCast(module, AID.StormingRoar);
class LightningBoltFast(BossModule module) : Components.StandardAOEs(module, AID.LightningBoltFast, 6, maxCasts: 11);
class HyperconductivePlasma(BossModule module) : Components.StackWithCastTargets(module, AID.HyperconductivePlasma, 6, minStackSize: 5);
class ElectrostaticTrouncing(BossModule module) : Components.StandardAOEs(module, AID.ElectrostaticTrouncing, new AOEShapeCone(40, 67.5f.Degrees()));
class TrounceCone(BossModule module) : Components.GroupedAOEs(module, [AID.TrounceFast, AID.TrounceSlow], new AOEShapeCone(20, 22.5f.Degrees()), maxCasts: 4);
class LightningBoltCloud(BossModule module) : Components.StandardAOEs(module, AID.LightningBoltCloud, 8);
class LightningBoltSlow(BossModule module) : Components.StandardAOEs(module, AID.LightningBoltSlow, 6, maxCasts: 18);
class LightningBoltSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.LightningBoltSpread, 6);
class BallLightning(BossModule module) : Components.Adds(module, (uint)OID.BallLightning, 1, true);
class EclipticMeteor(BossModule module) : Components.GenericLineOfSightAOE(module, AID.EclipticMeteor, 500, false)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Spriggan)
            Modify(Module.PrimaryActor.Position, [(actor.Position, actor.HitboxRadius)], Module.CastFinishAt(Module.PrimaryActor.CastInfo));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EclipticMeteor)
            Modify(null, []);
    }
}

class GreaterBehemothStates : StateMachineBuilder
{
    public GreaterBehemothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormingRoar>()
            .ActivateOnEnter<LightningBoltFast>()
            .ActivateOnEnter<HyperconductivePlasma>()
            .ActivateOnEnter<ElectrostaticTrouncing>()
            .ActivateOnEnter<TrounceCone>()
            .ActivateOnEnter<LightningBoltCloud>()
            .ActivateOnEnter<LightningBoltSlow>()
            .ActivateOnEnter<LightningBoltSpread>()
            .ActivateOnEnter<BallLightning>()
            .ActivateOnEnter<EclipticMeteor>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70969, NameID = 14360)]
public class GreaterBehemoth(ModuleInit init) : BossModule(init, new(121, -428), new ArenaBoundsCircle(20));

