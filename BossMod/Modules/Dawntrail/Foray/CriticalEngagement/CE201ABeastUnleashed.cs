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
    ClawToTail = 48294, // AtlasCarbuncle->self, 6.0s cast, range 40 180.000-degree cone
    ClawToTail1 = 48296, // AtlasCarbuncle->self, no cast, range 45 ?-degree cone
    TopazStones = 48280, // AtlasCarbuncle->self, 3.0s cast, single-target
    TopazRay1 = 48281, // TopazStone->self, 3.0s cast, range 4 circle
    TopazRay2 = 48282, // TopazStone->self, 3.0s cast, range 4 circle
    UnknownAbility1 = 50461, // AtlasCarbuncle->self, no cast, single-target
    WeaponskillRubyGlow = 48284, // AtlasCarbuncle->self, 3.0s cast, ???
    AbilityRubyGlow = 50637, // AtlasCarbuncleHelper->self, no cast, ???
    ReflectiveCoat = 50418, // AtlasCarbuncle->self, 3.0s cast, single-target
    RubyReflection = 48287, // AtlasCarbuncleHelper->self, no cast, range 40 width 40 rect
    RubyReflection1 = 48286, // AtlasCarbuncleHelper->self, no cast, range 40 width 40 rect
    RubyReflection2 = 48285, // Helper->self, no cast, range 20 width 20 rect
}

public enum SID : uint
{
    DirectionalDisregard = 3808, // none->AtlasCarbuncle, extra=0x0
}

sealed class SonicHowl(BossModule module) : Components.RaidwideCast(module, (uint)AID.SonicHowl);

sealed class TailToClaw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private static readonly AOEShapeCone cone = new(45f, 90f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ClawToTail:
                aoes.Add(new(cone, caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
                aoes.Add(new(cone, caster.Position, caster.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 3.1d), risky: false));
                break;
            case (uint)AID.TailToClaw:
                aoes.Add(new(cone, caster.Position, caster.Rotation + 180f.Degrees(), Module.CastFinishAt(spell)));
                aoes.Add(new(cone, caster.Position, caster.Rotation, Module.CastFinishAt(spell, 3.1d), risky: false));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.TailToClaw:
                case (uint)AID.TailToClaw1:
                case (uint)AID.ClawToTail:
                case (uint)AID.ClawToTail1:
                    aoes.RemoveAt(0);
                    break;
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

sealed class TopazRay(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Actors = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        List<AOEInstance> aoes = [];
        var count = Actors.Count;
        for (var i = 0; i < count; i++)
        {
            ref var topaz = ref Actors.Ref(i);
            aoes.Add(new(new AOEShapeCircle(4f), topaz.Position));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.TopazStone && id == 0x2489)
        {
            Actors.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TopazRay1 or (uint)AID.TopazRay2)
        {
            Actors.Clear();
        }
    }
}

sealed class RubyReflection(BossModule module) : Components.GenericAOEs(module)
{
    private readonly TopazRay TopazComponent = module.FindComponent<TopazRay>()!;
    private readonly List<AOEInstance> aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.Actor1ec045)
        {
            if (state == 0x00010002)
            {
                var act = WorldState.FutureTime(11.8d);
                var quadCount = Quadrants.Length;
                var topaz = CollectionsMarshal.AsSpan(TopazComponent.Actors);
                var topazCount = topaz.Length;
                for (var i = 0; i < topazCount; i++)
                {
                    for (var j = 0; j < quadCount; j++)
                    {
                        var quad = Quadrants[j];
                        var t = topaz[i];
                        var p = Arena.ClampToBounds(t.Position + (t.Rotation + 180f.Degrees()).ToDirection() * 3f);
                        if (quad.InSquare(t.Position, 10f) && !quad.InSquare(p, 10f))
                        {
                            aoes.Add(new(new AOEShapeRect(10f, 10f, 10f), quad, activation: act));
                        }
                    }
                }
            }
        }
        else if (actor.OID == (uint)OID.Actor1ec046)
        {
            if (state is 0x00100020 or 0x01000200)
            {
                var act = WorldState.FutureTime(14.8d);
                var shapes = state == 0x00100020 ? Reflection2Zero : Reflection1Zero;
                var rubyRot = actor.Rotation;
                var shapeCount = shapes.Length;
                var topaz = CollectionsMarshal.AsSpan(TopazComponent.Actors);
                var topazCount = topaz.Length;
                for (var i = 0; i < topazCount; i++)
                {
                    for (var j = 0; j < shapeCount; j++)
                    {
                        var shape = shapes[j];
                        shape.Polygon = shape.GetCombinedPolygon(Arena.Center).Transform(default, rubyRot.ToDirection());
                        var t = topaz[i];
                        var p = Arena.ClampToBounds(t.Position + (t.Rotation + 180f.Degrees()).ToDirection() * 3f);
                        if (shape.Check(t.Position, Arena.Center, default) && !shape.Check(p, Arena.Center, default))
                        {
                            aoes.Add(new(shape, Arena.Center, activation: act));
                        }
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.RubyReflection:
                case (uint)AID.RubyReflection1:
                case (uint)AID.RubyReflection2:
                    aoes.Clear();
                    break;
            }
        }
    }

    private readonly WPos[] Quadrants = [new(228f, 342f), new(248f, 342f), new(228f, 362f), new(248f, 362f)];
    private readonly AOEShapeCustom[] Reflection1Zero = [
        new([new Square(new(223f, 337f), 5f), new Square(new(233f, 337f), 5f), new Square(new(233f, 347f), 5f), new Square(new(233f, 357f), 5f),]),
        new([new Square(new(223f, 347f), 5f), new Square(new(223f, 357f), 5f), new Square(new(223f, 367f), 5f), new Square(new(233f, 367f), 5f),]),
        new([new Square(new(243f, 337f), 5f), new Square(new(253f, 337f), 5f), new Square(new(253f, 347f), 5f), new Square(new(253f, 357f), 5f),]),
        new([new Square(new(243f, 347f), 5f), new Square(new(243f, 357f), 5f), new Square(new(243f, 367f), 5f), new Square(new(253f, 367f), 5f),]),
    ];
    private readonly AOEShapeCustom[] Reflection2Zero = [
        new([new Square(new(223f, 337f), 5f), new Square(new(223f, 347f), 5f), new Square(new(233f, 347f), 5f), new Square(new(243f, 347f), 5f)]),
        new([new Square(new(233f, 337f), 5f), new Square(new(243f, 337f), 5f), new Square(new(253f, 337f), 5f), new Square(new(253f, 347f), 5f)]),
        new([new Square(new(223f, 357f), 5f), new Square(new(223f, 367f), 5f), new Square(new(233f, 367f), 5f), new Square(new(243f, 367f), 5f)]),
        new([new Square(new(233f, 357f), 5f), new Square(new(243f, 357f), 5f), new Square(new(253f, 357f), 5f), new Square(new(253f, 367f), 5f)]),
    ];
}

sealed class SpinebreakingStampede(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> knockbacks = [with(2)];
    private readonly TopazRay TopazComponent = module.FindComponent<TopazRay>()!;
    private readonly AOEShapeRect rect = new(40f, 30f);
    private bool isAlongZAxis = false;
    private Angle direction = default;
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var kbs = CollectionsMarshal.AsSpan(knockbacks);
        var count = kbs.Length;

        for (var i = 0; i < count; i++)
        {
            ref var kb = ref kbs[i];
            if (kb.Origin.AlmostEqual(Arena.Center, 0.1f))
            {
                var pos = actor.Position;
                if (isAlongZAxis)
                {
                    var p = pos.Z;
                    var a = Arena.Center.Z;
                    var dir = p < a ? -180f.Degrees() : 0f.Degrees();
                    knockbacks[i] = new(Arena.Center, 15f, kb.Activation, kb.Shape, dir, Kind.DirForward);
                }
                else
                {
                    var p = pos.X;
                    var a = Arena.Center.X;
                    var dir = p < a ? -90f.Degrees() : 90f.Degrees();
                    knockbacks[i] = new(Arena.Center, 15f, kb.Activation, kb.Shape, dir, Kind.DirForward);
                }
            }
        }

        return kbs;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SpinebreakingStampedeMiddleVisual)
        {
            var rot = spell.Rotation;
            var offset = 90f.Degrees();
            var rot1 = rot + offset;
            isAlongZAxis = rot1.AlmostEqual(default, Angle.DegToRad) || rot1.AlmostEqual(180f.Degrees(), Angle.DegToRad);
        }
        else if (spell.Action.ID == (uint)AID.SpinebreakingStampedeCircleVisual)
        {
            //5.2d, 8.5d
            direction = (caster.Position - Arena.Center).ToAngle();
            knockbacks.Add(new(Arena.Center, 15f, Module.CastFinishAt(spell, 5.2d), null, default, Kind.None));

            var act = Module.CastFinishAt(spell, 8.5d);
            var pos = caster.Position;
            knockbacks.Add(new(pos, 30f, act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (knockbacks.Count != 0)
        {
            switch (spell.Action.ID)
            {
                // use jump instead of actual kb since helper casts each twice
                case (uint)AID.SpinebreakingStampedeCast:
                case (uint)AID.SpinebreakingStampedeTeleport1:
                    knockbacks.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = knockbacks.Count;
        if (count != 0)
        {
            ref readonly var kb = ref knockbacks.Ref(0);
            var act = kb.Activation;

            if (kb.Origin.AlmostEqual(Arena.Center, 0.1f))
            {
                hints.AddForbiddenZone(rect, Arena.Center, direction + 180f.Degrees(), act);
                hints.AddForbiddenZone(new AOEShapeDonut(3f, 40f), Arena.Center, activation: act);
            }
            else
            {
                var topaz = TopazComponent.ActiveAOEs(slot, actor);
                var topazCount = topaz.Length;
                WPos[] topazPos = new WPos[topazCount];
                for (var i = 0; i < topazCount; i++)
                {
                    topazPos[i] = topaz[i].Origin;
                }
                // smaller AOE size, enough time to run out of AOE if inside
                hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginPlusAOECircles(Arena.Center, kb.Origin, 30f, 18f, topazPos, 4f, count), act);
            }
        }
    }
}

[SkipLocalsInit]
sealed class CE201ABeastUnleashedStates : StateMachineBuilder
{
    public CE201ABeastUnleashedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SonicHowl>()
            .ActivateOnEnter<TopazRay>()
            .ActivateOnEnter<RubyReflection>()
            .ActivateOnEnter<TailToClaw>()
            .ActivateOnEnter<SpinebreakingStampede>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(CE201ABeastUnleashedStates),
    ConfigType = null, // replace null with typeof(ABeastUnleashedConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.AtlasCarbuncle,
    Contributors = "The Combat Reborn Team (LTS)",
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
