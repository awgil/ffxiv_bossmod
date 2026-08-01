namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE208FamiliarTactics;

// TODO was made with ARR support
//  Status: COMPLETED
//  1. UnbowedSpirit circles don't disappear right away -> check eventcast maybe instead or actor death instead of destroyed?

public enum OID : uint
{
    ElmGigas = 0x4BD9,
    Helper = 0x233C,
    ElmGigasPuddle = 0x4BDA, // R4.000, x0 (spawn during fight)

    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
}

public enum AID : uint
{
    AutoAttack = 50851, // ElmGigas->player, no cast, single-target
    AncientAeroIII = 47544, // ElmGigas->self, 3.5+1.5s cast, single-target
    AncientAeroIIIVisual = 48041, // Helper->self, 5.0s cast, ???
    SpinningSweep = 47541, // ElmGigas->self, 6.0s cast, range 40 120.000-degree cone
    InspiritedCrosswindsCast = 47533, // ElmGigas->self, 6.0+0.8s cast, single-target
    InspiritedCrosswinds = 47535, // 4BDA->self, 6.0s cast, range 60 width 8 cross
    InspiritedImpactCast = 47542, // ElmGigas->self, 3.0s cast, single-target
    InspiritedImpact = 47543, // Helper->self, 9.6s cast, range 25 circle
    InspiritedHurricaneCast = 47536, // ElmGigas->self, 4.3+0.7s cast, single-target
    InspiritedHurricaneCross = 47538, // Helper->self, 5.0s cast, range 60 width 10 cross
    InspiritedHurricaneCircle = 47537, // Helper->self, 5.0s cast, range 12 circle
    AncientAero = 47540, // Helper->self, 3.0s cast, range 70 width 6 rect
    InspiritedCycloneCast = 47532, // ElmGigas->self, 5.0+1.0s cast, single-target
    InspiritedCyclone = 47534, // 4BDA->self, 6.0s cast, range 12 circle
    UnbowedSpiritCast = 47530, // ElmGigas->self, 3.0+1.0s cast, single-target
    UnbowedSpirit = 47531, // Helper->self, no cast, range 4 circle
}

public enum SID : uint
{
    Gen = 2234, // none->4BDA, extra=0xFFAB/0x1E/0xFFE4
}

sealed class AncientAeroIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.AncientAeroIII);
sealed class SpinningSweep(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinningSweep, new AOEShapeCone(40.0f, 60.0f.Degrees()));
sealed class InspiritedCrosswinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InspiritedCrosswinds, new AOEShapeCross(60.0f, 4.0f));
sealed class InspiritedHurricaneCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InspiritedHurricaneCross, new AOEShapeCross(60.0f, 5.0f));
sealed class InspiritedHurricaneCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InspiritedHurricaneCircle, new AOEShapeCircle(12.0f));
sealed class AncientAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientAero, new AOEShapeRect(70.0f, 3.0f));
sealed class InspiritedCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InspiritedCyclone, new AOEShapeCircle(12.0f));

sealed class UnbowedSpirit(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly List<Actor> puddles = [];
    private readonly AOEShapeCircle shape = new(6.0f); // Slightly bigger as they're constantly moving around the map

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ElmGigasPuddle)
        {
            puddles.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.ElmGigasPuddle)
        {
            puddles.Remove(actor);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        aoes.Clear();

        if (puddles.Count == 0)
        {
            return [];
        }

        foreach (var puddle in puddles)
        {
            aoes.Add(new(shape, puddle.Position, puddle.Rotation, color: Colors.Danger));
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class InspiritedImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(25.0f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InspiritedImpact)
        {
            aoes.Add(new(shape, caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.InspiritedImpact)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var show = 0;
        var incomingAOEs = new List<AOEInstance>(aoes);
        incomingAOEs.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        if (incomingAOEs.Count > 3)
        {
            incomingAOEs.RemoveRange(3, incomingAOEs.Count - 3);
        }
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs))
        {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

[SkipLocalsInit]
sealed class FamiliarTacticsStates : StateMachineBuilder
{
    public FamiliarTacticsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientAeroIII>()
            .ActivateOnEnter<SpinningSweep>()
            .ActivateOnEnter<InspiritedCrosswinds>()
            .ActivateOnEnter<InspiritedImpact>()
            .ActivateOnEnter<InspiritedHurricaneCross>()
            .ActivateOnEnter<InspiritedHurricaneCircle>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<InspiritedCyclone>()
            .ActivateOnEnter<UnbowedSpirit>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(FamiliarTacticsStates),
    ConfigType = null, // replace null with typeof(FamiliarTacticsConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.ElmGigas,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14508u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class FamiliarTactics(WorldState ws, Actor primary) : BossModule(ws, primary, new(-390.000f, 700.000f), new ArenaBoundsCircle(30f));
