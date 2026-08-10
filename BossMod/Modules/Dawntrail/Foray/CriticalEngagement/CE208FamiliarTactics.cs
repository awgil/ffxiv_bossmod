namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE208FamiliarTactics;

public enum OID : uint
{
    ElmGigas = 0x4BD9,
    Helper = 0x233C,
    ElmGigasPuddle = 0x4BDA, // R4.000, x0 (spawn during fight)
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
    private bool circular = false;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ElmGigasPuddle)
        {
            if (puddles.Count == 0)
            {
                var offset = actor.Position - Arena.Center;
                circular = MathF.Abs(offset.X % 10.0f) > 1.0f || MathF.Abs(offset.Z % 10.0f) > 1.0f;
            }

            puddles.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.InspiritedCrosswinds or (uint)AID.InspiritedCyclone)
        {
            if (puddles.Count > 0)
            {
                puddles.RemoveAt(0);
            }
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
            if (circular)
            {
                var angleDirection = (puddle.Position - Arena.Center).Cross(puddle.Rotation.ToDirection()) > 0.0f;
                var length = 4.0f / (puddle.Position - Arena.Center).Length();
                var lengthDirection = (angleDirection ? -length : length).Radians();
                aoes.Add(new(new AOEShapeArcCapsule(4.2f, lengthDirection, Arena.Center), puddle.Position, puddle.Rotation, color: Colors.Danger));
            }
            else
            {
                aoes.Add(new(new AOEShapeCapsule(4.2f, 4.0f), puddle.Position, puddle.Rotation, color: Colors.Danger));
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class InspiritedImpact : Components.SimpleAOEs
{
    public InspiritedImpact(BossModule module) : base(module, (uint)AID.InspiritedImpact, new AOEShapeCircle(25.0f))
    {
        MaxDangerColor = 3;
        MaxCasts = 3;
    }
}

[SkipLocalsInit]
sealed class CE208FamiliarTacticsStates : StateMachineBuilder
{
    public CE208FamiliarTacticsStates(BossModule module) : base(module)
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

//TODO: Needs extended moving AOE support- once implemented can be moved to Verified after testing
[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CE208FamiliarTacticsStates),
    ConfigType = null, // replace null with typeof(ElmGigasConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
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
public sealed class CE208FamiliarTactics(WorldState ws, Actor primary) : BossModule(ws, primary, new(-390.000f, 700.000f), new ArenaBoundsCircle(30f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
