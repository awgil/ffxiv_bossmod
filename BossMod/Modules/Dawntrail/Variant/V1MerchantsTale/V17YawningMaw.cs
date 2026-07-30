namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V08NYawningMaw;

public enum OID : uint
{
    YawningMaw = 0x4AB2,
    Gems = 0x4ACD,
    Helper = 0x233C
}

public enum AID : uint
{
    Inhale = 45719 // Boss->self, 4.0s cast, range 23 120-degree cone
}

[SkipLocalsInit]
sealed class ForbiddenGoobue(BossModule module) : Components.GenericInvincible(module)
{
    private readonly GemHints _hints = module.FindComponent<GemHints>()!;

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        if (_hints.Gems.Count != 0)
        {
            return CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.YawningMaw));
        }
        return [];
    }
}

[SkipLocalsInit]
sealed class Inhale(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Inhale, new AOEShapeCone(23f, 60f.Degrees()));

[SkipLocalsInit]
sealed class GemHints(BossModule module) : BossComponent(module)
{
    public readonly List<Actor> Gems = module.Enemies((uint)OID.Gems);
    private bool _succ;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Gems.Count != 0 && !_succ)
        {
            var closestGem = Gems.MinBy(actor.DistanceToHitbox);
            if (closestGem != null)
            {
                hints.AddForbiddenZone(new SDInvertedCircle(closestGem.Position, 4f));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Gems, Colors.Object, true);
        {
            var count = Gems.Count;
            for (var i = 0; i < count; ++i)
            {
                Arena.ZoneCircleOutline(Gems[i].Position, 4f, Colors.Safe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Inhale)
        {
            _succ = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Inhale)
        {
            _succ = false;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        base.OnMapEffect(index, state);
        if (state == 0x00020001u)
        {
            WPos removedGem = index switch
            {
                0x36 => new(295f, 692f),
                0x37 => new(287f, 684f),
                0x38 => new(297f, 670f),
                0x39 => new(313f, 679f),
                _ => default
            };

            if (removedGem != default)
            {
                var count = Gems.Count;
                for (var i = 0; i < count; ++i)
                {
                    var rock = Gems[i];
                    if (Gems[i].Position.AlmostEqual(removedGem, 2f))
                    {
                        Gems.Remove(rock);
                        return;
                    }
                }
            }
        }
    }
}

[SkipLocalsInit]
sealed class V08YawningMawStates : StateMachineBuilder
{
    public V08YawningMawStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GemHints>()
            .ActivateOnEnter<ForbiddenGoobue>()
            .ActivateOnEnter<Inhale>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(V08YawningMawStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.YawningMaw,
Contributors = "VeraNala",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1066u,
NameID = 14402u,
SortOrder = 8,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class V08YawningMaw(WorldState ws, Actor primary) : BossModule(ws, primary, new(299.77f, 681.98f), new ArenaBoundsCircle(15f));
