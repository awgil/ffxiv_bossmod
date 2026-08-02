namespace BossMod.Foray.CriticalEngagement.CE202AcceptNoImitators;

public enum OID : uint
{
    Metamorph = 0x4C77,
    Helper = 0x233C,
    Metamorph1 = 0x4DFD, // R1.000, x1
    Arrow = 0x1EC09B, // R0.500, x0 (spawn during fight), EventObj type
    WindSphere = 0x1EC09C, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 48334, // Metamorph->player, no cast, single-target
    BlackenedRain = 48335, // Metamorph->self, 4.0+1.0s cast, single-target
    BlackenedRainVisual = 48336, // Helper->self, 5.0s cast, ???
    DarkDealing = 48337, // Metamorph->player, 5.0s cast, single-target
    Revert = 48340, // Metamorph->self, no cast, single-target

    ChangeDog = 48338, // Metamorph->self, 4.0s cast, single-target
    AutoAttackDog = 48368, // Metamorph->player, no cast, single-target
    Teleport = 50720, // Metamorph->location, no cast, single-target
    TongueOfFlame = 48341, // Metamorph->self, 4.0s cast, range 15 circle
    HellfireFetchVisual = 48342, // Metamorph->self, no cast, single-target
    HellfireFetch = 48345, // Helper->location, 7.0s cast, range 6 circle
    HellwardBoundStart = 48343, // Metamorph->location, 6.0s cast, width 10 rect charge
    HellwardBoundNext = 48344, // Metamorph->location, no cast, width 10 rect charge
    HellishBreathCast = 48346, // Metamorph->self, 6.0s cast, single-target
    HellishBreathVisual1 = 48347, // Helper->self, 2.0s cast, range 60 ?-degree cone
    HellishBreathVisual2 = 48348, // Helper->self, 4.0s cast, range 60 ?-degree cone
    HellishBreathVisual3 = 48349, // Helper->self, 6.0s cast, range 60 ?-degree cone
    HellishBreath1 = 48350, // Metamorph->self, no cast, single-target
    HellishBreathCast1 = 48662, // Helper->self, 1.1s cast, range 60 ?-degree cone
    HellishBreath2 = 48351, // Metamorph->self, no cast, single-target
    HellishBreathCast2 = 48663, // Helper->self, 1.1s cast, range 60 ?-degree cone
    HellishBreath3 = 48352, // Metamorph->self, no cast, single-target
    HellishBreathCast3 = 50677, // Helper->self, 1.1s cast, range 60 ?-degree cone

    ChangeSnake = 48339, // Metamorph->self, 4.0s cast, single-target
    AutoAttackSnake = 48369, // Metamorph->player, no cast, single-target
    CyclonicRing = 48354, // Metamorph->self, 4.0s cast, range ?-30 donut
    ShapeshiftingSupercellCast = 48355, // Metamorph->self, 5.5+0.5s cast, single-target
    ShapeshiftingSupercell1 = 48356, // Metamorph->self, 5.5+0.5s cast, single-target
    ShapeshiftingSupercell = 48358, // Metamorph->self, no cast, single-target
    ShapeshiftingSupercellInner = 48360, // Helper->self, 6.0s cast, range 8 circle
    ShapeshiftingSupercellInner1 = 50767, // Helper->self, 6.0s cast, range 8 circle
    ShapeshiftingSupercellMiddle = 48361, // Helper->self, 6.0s cast, range 10-16 donut
    ShapeshiftingSupercellOuter = 48362, // Helper->self, 6.0s cast, range 16-30 donut
    ShapeshiftingSupercellCone = 48357, // Helper->self, 6.0s cast, range 60 90.000-degree cone
    ShapeshiftingSupercellCone1 = 48359, // Helper->self, 1.5s cast, range 60 90.000-degree cone
    MadeMagic = 48363, // Metamorph->self, 4.0s cast, single-target
    MadeMagic1 = 48364, // Helper->self, no cast, range 0 circle
    CycloneCrossing = 48365, // Metamorph->self, 10.5+1.0s cast, single-target
    CycloneCrossing1 = 48366, // Helper->self, 11.5s cast, range 60 width 16 cross

    MapAreanChange = 48367, // 4DFD->self, no cast, range ?-30 donut - guessed this is the map arena change
    Weaponskill = 48353, // Metamorph->self, no cast, single-target
}

public enum SID : uint
{
    Transfiguration = 2548, // Metamorph->Metamorph, extra=0x173/0x174
    DirectionalDisregard = 3808, // none->Metamorph, extra=0x0
    AreaOfInfluenceUp = 1909, // none->Helper, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
}

public enum IconID : uint
{
    TankBuster = 198, // player->self
    TurnRight = 546, // Metamorph->self
    TurnLeft = 547, // Metamorph->self
}

sealed class BlackenedRain(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlackenedRain);
sealed class TongueOfFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TongueOfFlame, new AOEShapeCircle(15.0f));
sealed class HellfireFetch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HellfireFetch, new AOEShapeCircle(6.0f));
sealed class DarkDealing(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DarkDealing);
sealed class CyclonicRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CyclonicRing, new AOEShapeDonut(10.0f, 30.0f));
sealed class CycloneCrossing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CycloneCrossing1, new AOEShapeCross(60.0f, 8.0f));
sealed class WindSphere(BossModule module) : Components.Voidzone(module, 17.5f, module => module.Enemies((uint)OID.WindSphere).Where(z => z.EventState != 7));

sealed class HellwardBoundCharge : Components.ChargeAOEs
{
    public HellwardBoundCharge(BossModule module) : base(module, (uint)AID.HellwardBoundStart, 5.0f)
    {
        Color = Colors.Danger;
    }
}

sealed class HellwardBound(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> arrows = [];
    private readonly List<AOEInstance> aoes = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Arrow)
        {
            arrows.Add(actor);

            if (arrows.Count == 4)
            {
                CreatePath();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HellwardBoundStart or (uint)AID.HellwardBoundNext)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int show = 0;
        var incomingAOEs = aoes.Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs))
        {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }

    private void CreatePath()
    {
        List<Actor> pathList = [];
        var boss = Module.PrimaryActor;

        // Create the path for the arrows
        while (arrows.Count > 0)
        {
            Actor? nextInLine = null;

            // Case: first one is the closest one
            if (pathList.Count == 0)
            {
                nextInLine = arrows.Closest(boss.Position);
            }
            else
            { // Case: all other arrows take the direction it is looking
                var lastArrow = pathList[^1];
                var forwardDirection = lastArrow.Rotation.ToDirection();
                nextInLine = arrows.MaxBy(a => forwardDirection.Dot((a.Position - lastArrow.Position).Normalized()));
            }

            if (nextInLine != null)
            {
                pathList.Add(nextInLine);
                arrows.Remove(nextInLine);
            }
        }

        if (pathList.Count != 4)
        {
            return;
        }

        // Setup the aoes
        for (int i = 0; i < pathList.Count; i++)
        {
            if (i == 0)
            {
                continue;
            }

            var origin = pathList[i - 1];
            var target = pathList[i];
            var direction = target.Position - origin.Position;
            var shape = new AOEShapeRect(direction.Length(), 5.0f);
            aoes.Add(new(shape, origin.Position, Angle.FromDirection(direction)));
        }

        // final path that doesn't work on arrows - aoe range is just a guess so its never set
        aoes.Add(new(new AOEShapeRect(40.0f, 5.0f), pathList[^1].Position, pathList[^1].Rotation));
    }
}

sealed class HellishBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(60.0f, 30.0f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HellishBreathVisual1 or (uint)AID.HellishBreathVisual2 or (uint)AID.HellishBreathVisual3)
        {
            aoes.Add(new(shape, caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HellishBreathCast1 or (uint)AID.HellishBreathCast2 or (uint)AID.HellishBreathCast3)
        {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int show = 0;
        var incomingAOEs = aoes.OrderBy(aoe => aoe.Activation).Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs))
        {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class ShapeshiftingSupercellRings(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ShapeshiftingSupercellInner or (uint)AID.ShapeshiftingSupercellInner1)
        {
            aoes.Add(new(new AOEShapeCircle(8.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }

        if (spell.Action.ID == (uint)AID.ShapeshiftingSupercellMiddle)
        {
            aoes.Add(new(new AOEShapeDonut(8.0f, 16.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }

        if (spell.Action.ID == (uint)AID.ShapeshiftingSupercellOuter)
        {
            aoes.Add(new(new AOEShapeDonut(16.0f, 30.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ShapeshiftingSupercellInner or (uint)AID.ShapeshiftingSupercellInner1 or (uint)AID.ShapeshiftingSupercellMiddle or
            (uint)AID.ShapeshiftingSupercellOuter)
        {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int show = 0;
        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs))
        {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class ShapeshiftingSupercell(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(60.0f, 45.0f.Degrees());
    private int direction = 0; // -1 = right, 1 = left
    private bool futureAOEsAdded = false;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.TurnRight)
        {
            direction = -1;
        }

        if (iconID == (uint)IconID.TurnLeft)
        {
            direction = 1;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShapeshiftingSupercellCone)
        {
            aoes.Add(new(shape, caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ShapeshiftingSupercellCone or (uint)AID.ShapeshiftingSupercellCone1)
        {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }

            if (aoes.Count == 0)
            {
                direction = 0;
                futureAOEsAdded = false;
            }
        }
    }

    public override void Update()
    {
        if (direction == 0)
        {
            return;
        }

        AddFutureAOEs();
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(3).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs))
        {
            aoe.Color = Colors.Danger;
            aoe.Risky = true;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }

    private void AddFutureAOEs()
    {
        if (futureAOEsAdded)
        {
            return;
        }

        List<AOEInstance> futureAOEs = [];
        foreach (var aoe in aoes)
        {
            futureAOEs.Add(new(shape, aoe.Origin, aoe.Rotation + 30.0f.Degrees() * direction, aoe.Activation.AddSeconds(1.5f)));
            futureAOEs.Add(new(shape, aoe.Origin, aoe.Rotation + 60.0f.Degrees() * direction, aoe.Activation.AddSeconds(3.0f)));
            futureAOEs.Add(new(shape, aoe.Origin, aoe.Rotation + 90.0f.Degrees() * direction, aoe.Activation.AddSeconds(4.5f)));
            futureAOEs.Add(new(shape, aoe.Origin, aoe.Rotation + 120.0f.Degrees() * direction, aoe.Activation.AddSeconds(6.0f)));
            futureAOEs.Add(new(shape, aoe.Origin, aoe.Rotation + 150.0f.Degrees() * direction, aoe.Activation.AddSeconds(7.5f)));
        }

        aoes.AddRange(futureAOEs);
        futureAOEsAdded = true;
    }
}

[SkipLocalsInit]
sealed class CE202AcceptNoImitatorsStates : StateMachineBuilder
{
    public CE202AcceptNoImitatorsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlackenedRain>()
            .ActivateOnEnter<TongueOfFlame>()
            .ActivateOnEnter<HellfireFetch>()
            .ActivateOnEnter<HellwardBoundCharge>()
            .ActivateOnEnter<HellwardBound>()
            .ActivateOnEnter<HellishBreath>()
            .ActivateOnEnter<DarkDealing>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<ShapeshiftingSupercellRings>()
            .ActivateOnEnter<ShapeshiftingSupercell>()
            .ActivateOnEnter<CycloneCrossing>()
            .ActivateOnEnter<WindSphere>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(CE202AcceptNoImitatorsStates),
    ConfigType = null, // replace null with typeof(MetamorphConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Metamorph,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14801u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE202AcceptNoImitators(WorldState ws, Actor primary) : BossModule(ws, primary, new(500.000f, -310.000f), new ArenaBoundsCircle(25f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
