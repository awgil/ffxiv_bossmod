namespace BossMod.Dawntrail.Variant.V01MerchantsTale.V07StonePuppet;

public enum OID : uint
{
    StonePuppet = 0x4ABC, // R24.000, x?
    Rock = 0x4ACD,
    Helper = 0x233C
}

public enum AID : uint
{
    MagneticRock = 45721 // 5f circ
}

public enum IconID : uint
{
    MagneticMarker = 315 // player->self
}

[SkipLocalsInit]
sealed class ForbiddenGolem(BossModule module) : Components.GenericInvincible(module)
{
    private bool _rocksCharged;

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        if (!_rocksCharged)
        {
            return CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.StonePuppet));
        }
        return [];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u && index == 0x29)
        {
            _rocksCharged = true;
        }
    }
}

[SkipLocalsInit]
sealed class MagneticRock(BossModule module) : Components.BaitAwayIcon(module, 5f, (uint)IconID.MagneticMarker, (uint)AID.MagneticRock, centerAtTarget: true);

[SkipLocalsInit]
sealed class RockHints(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> rocks = module.Enemies((uint)OID.Rock);
    private Actor? MagneticTarget;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MagneticMarker)
        {
            MagneticTarget = actor;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = rocks.Count;
        if (count != 0)
        {
            if (actor == MagneticTarget)
            {
                var closestRock = rocks.MinBy(actor.DistanceToHitbox);
                if (closestRock != null)
                {
                    hints.AddForbiddenZone(new SDInvertedCircle(closestRock.Position, 4f));
                }
            }
            switch (count)
            {
                case 1:
                    hints.AddForbiddenZone(new SDRect(Arena.Center, -90f.Degrees(), 40f, 20f, 40f));
                    break;
                case 2:
                    hints.AddForbiddenZone(new SDRect(Arena.Center, 90f.Degrees(), 40f, 0f, 40f));
                    break;
                case 3:
                    hints.AddForbiddenZone(new SDRect(Arena.Center, -50f.Degrees(), 40f, 40f, 12f));
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(rocks, Colors.Object, true);

        if (pc == MagneticTarget)
        {
            var count = rocks.Count;
            for (var i = 0; i < count; ++i)
            {
                Arena.ZoneCircleOutline(rocks[i].Position, 4f, Colors.Safe);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MagneticRock)
        {
            MagneticTarget = null;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        base.OnMapEffect(index, state);
        if (state == 0x00020001u)
        {
            WPos removedRock = index switch
            {
                0x26 => new(129f, -638f),
                0x27 => new(150f, -632f),
                0x28 => new(179f, -649f),
                _ => default
            };

            if (removedRock != default)
            {
                var count = rocks.Count;
                for (var i = 0; i < count; ++i)
                {
                    var rock = rocks[i];
                    if (rocks[i].Position.AlmostEqual(removedRock, 2f))
                    {
                        rocks.Remove(rock);
                        return;
                    }
                }
            }
        }
    }
}

[SkipLocalsInit]
sealed class V07StonePuppetStates : StateMachineBuilder
{
    public V07StonePuppetStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagneticRock>()
            .ActivateOnEnter<RockHints>()
            .ActivateOnEnter<ForbiddenGolem>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(V07StonePuppetStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.StonePuppet,
Contributors = "VeraNala",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1066u,
NameID = 14353u,
SortOrder = 7,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class V07StonePuppet : BossModule
{
    public V07StonePuppet(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private V07StonePuppet(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new PolygonCustom([new(171.5f, -656.5f), new(181f, -650.64f), new(181f, -644.64f),
        new(152.34f, -619.77f), new(126.16f, -615.1f), new(120.31f, -649.2f), new(129.59f, -639.89f), new(134.96f, -625.99f)])]);
        return (arena.Center, arena);
    }
}
