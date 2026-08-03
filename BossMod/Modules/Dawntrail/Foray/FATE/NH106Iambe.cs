namespace BossMod.Dawntrail.Foray.FATE.NH106Iambe;

public enum OID : uint
{
    Iambe = 0x4C41,
    Helper = 0x233C,
    Iambe1 = 0x4C42, // R1.000, x0 (spawn during fight)
    WinsomeSeed = 0x4C43, // R0.240-0.528, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50855, // Iambe->player, no cast, single-target
    DirectSeeding = 48029, // Iambe->self, 3.0s cast, single-target
    GardenersHymnCast = 48031, // Iambe->self, 2.5s cast, single-target
    GardenersHymn = 48032, // 4C42->location, 6.0s cast, range 5 circle
    Burst = 48033, // 4C43->self, 2.0s cast, range 15 circle
    OdeOfTheUnderfoot = 48037, // Iambe->self, 5.0s cast, range 10 circle
    IambicMarch = 48035, // Iambe->self, 3.0s cast, range 40 circle
}

public enum SID : uint
{
    ForwardMarch = 5142, // Iambe->player, extra=0x0
    AboutFace = 5143, // Iambe->player, extra=0x0
    ForcedMarch = 1257, // Iambe->player, extra=0x1/0x2

    _Gen_1 = 5106, // 4C42->4C43, extra=0x1
    _Gen_ = 5107, // 4C42->4C43, extra=0x1
}

sealed class GardenersHymn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GardenersHymn, 5f);
sealed class OdeOfTheUnderfoot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OdeOfTheUnderfoot, 10f);
sealed class IambicMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3.0f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, default, default);

sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly List<Actor> seeds = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.WinsomeSeed)
        {
            seeds.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.WinsomeSeed)
        {
            seeds.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GardenersHymn)
        {
            foreach (var seed in seeds)
            {
                if (caster.Position.AlmostEqual(seed.Position, 0.5f))
                {
                    aoes.Add(new(new AOEShapeCircle(15.0f), seed.Position));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 0.5f));
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

[SkipLocalsInit]
sealed class IambeStates : StateMachineBuilder
{
    public IambeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GardenersHymn>()
            .ActivateOnEnter<OdeOfTheUnderfoot>()
            .ActivateOnEnter<IambicMarch>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
    StatesType = typeof(IambeStates),
    ConfigType = null, // replace null with typeof(IambeConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Iambe,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14765u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Iambe(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
