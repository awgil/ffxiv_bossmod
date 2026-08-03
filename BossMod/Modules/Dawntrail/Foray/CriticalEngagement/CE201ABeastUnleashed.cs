namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE201ABeastUnleashed;

public enum OID : uint
{
    AtlasCarbuncle = 0x4C4F, // R9.067, x1
    AtlasCarbuncleHelper = 0x233C, // R0.500, x20, Helper type
    AtlasCarbuncle1 = 0x4D88, // R1.000, x1
    TopazStone = 0x4C50, // R1.000, x12
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    Actor1ec031 = 0x1EC031, // R0.500, x1, EventObj type
    Actor1ec045 = 0x1EC045, // R0.500, x1, EventObj type
    Actor1ec046 = 0x1EC046, // R0.500, x2, EventObj type
}

public enum AID : uint
{
    AutoAttack = 50852, // AtlasCarbuncle->player, no cast, single-target
    SonicHowl = 48298, // AtlasCarbuncle->self, 5.0s cast, ???
    SonicHowl1 = 49505, // AtlasCarbuncleHelper->self, no cast, ???
    TailToClaw = 48295, // AtlasCarbuncle->self, 6.0s cast, range 40 180.000-degree cone
    TailToClaw1 = 48297, // AtlasCarbuncle->self, no cast, range 45 ?-degree cone

    SpinebreakingStampedeCast = 48291, // AtlasCarbuncle->location, 8.0s cast, ???
    SpinebreakingStampedeMiddleVisual = 48289, // Helper->self, 2.5s cast, range 40 width 60 rect
    SpinebreakingStampedeMiddle = 49507, // Helper->self, no cast, ???
    SpinebreakingStampedeCircleVisual = 48288, // Helper->self, 2.5s cast, range 60 circle
    SpinebreakingStampedeCircle = 49506, // Helper->self, no cast, ???
    SpinebreakingStampedeTeleport = 48299, // AtlasCarbuncle->location, no cast, single-target
    SpinebreakingStampedeTeleport1 = 48292, // AtlasCarbuncle->location, no cast, ???

    UnknownAbility = 49104, // AtlasCarbuncle1->self, no cast, ???
    ClawToTail = 48296, // AtlasCarbuncle->self, no cast, range 40 ?-degree cone
    TopazStones = 48280, // AtlasCarbuncle->self, 3.0s cast, single-target
    TopazRay1 = 48281, // TopazStone->self, 3.0s cast, range 4 circle
    TopazRay2 = 48282, // TopazStone->self, 3.0s cast, range 4 circle
    UnknownAbility1 = 50461, // AtlasCarbuncle->self, no cast, single-target
    WeaponskillRubyGlow = 48284, // AtlasCarbuncle->self, 3.0s cast, ???
    AbilityRubyGlow = 50637, // AtlasCarbuncleHelper->self, no cast, ???
    ReflectiveCoat = 50418, // AtlasCarbuncle->self, 3.0s cast, single-target
    RubyReflection = 48287, // AtlasCarbuncleHelper->self, no cast, range 40 width 40 rect
    RubyReflection1 = 48286, // AtlasCarbuncleHelper->self, no cast, range 40 width 40 rect
}

public enum SID : uint
{
    DirectionalDisregard = 3808, // none->AtlasCarbuncle, extra=0x0
}

sealed class SonicHowl(BossModule module) : Components.RaidwideCast(module, (uint)AID.SonicHowl);

sealed class TailToClaw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TailToClaw)
        {
            aoes.Add(new(new AOEShapeCone(40.0f, 90.0f.Degrees()), caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCone(40.0f, 90.0f.Degrees()), caster.Position, spell.Rotation + 180.0f.Degrees(), Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TailToClaw or (uint)AID.TailToClaw1)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
        {
            return [];
        }

        var aoe = aoes[0];
        aoe.Color = Colors.Danger;
        aoe.Risky = true;
        aoes[0] = aoe;

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class SpinebreakingStampede(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> knockbacks = [];
    private const float knockbackDistanceMiddle = 15.0f;
    private const float knockbackDistanceCircle = 30.0f;
    private readonly AOEShapeRect rect = new(40.0f, 30.0f);
    private readonly AOEShapeCircle circle = new(60.0f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SpinebreakingStampedeMiddleVisual)
        {
            var position = caster.Position;
            var rotation = spell.Rotation;
            var offset = 90.0f.Degrees();
            knockbacks.Add(new(position, knockbackDistanceMiddle, default, rect, rotation + offset, Kind.DirForward));
            knockbacks.Add(new(position, knockbackDistanceMiddle, default, rect, rotation - offset, Kind.DirForward));
        }

        if (spell.Action.ID == (uint)AID.SpinebreakingStampedeCircleVisual)
        {
            knockbacks.Add(new(caster.Position, knockbackDistanceCircle, shape: circle));
        }
    }

    // TODO if rewritten, take into account that knockback rect is done twice as each knockback is on either side
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SpinebreakingStampedeMiddle)
        {
            if (knockbacks.Count > 0)
            {
                knockbacks.RemoveAll(knockback => knockback.Shape is AOEShapeRect);
            }
        }

        if (spell.Action.ID is (uint)AID.SpinebreakingStampedeCircle)
        {
            if (knockbacks.Count > 0)
            {
                knockbacks.RemoveAll(knockback => knockback.Shape is AOEShapeCircle);
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(knockbacks);
}

[SkipLocalsInit]
sealed class CE201ABeastUnleashedStates : StateMachineBuilder
{
    public CE201ABeastUnleashedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SonicHowl>()
            .ActivateOnEnter<TailToClaw>()
            .ActivateOnEnter<SpinebreakingStampede>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(CE201ABeastUnleashedStates),
    ConfigType = null, // replace null with typeof(ABeastUnleashedConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.AtlasCarbuncle,
    Contributors = "The Combat Reborn Team (LTS) & Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14791u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE201ABeastUnleashed(WorldState ws, Actor primary) : BossModule(ws, primary, new(238f, 352f), new ArenaBoundsSquare(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 20f);
}
