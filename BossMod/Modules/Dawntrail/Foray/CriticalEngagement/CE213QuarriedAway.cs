namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE213QuarriedAway;

public enum OID : uint
{
    AlabasterBlade = 0x4BBE,
    Helper = 0x233C,
    AlabasterGolemVisual = 0x4BBF, // R1.650, x4
    AlabasterGolemCaster = 0x4EBD, // R1.000, x4
    LightAether = 0x4BC0, // R1.600, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50760, // AlabasterBlade->player, no cast, single-target
    EmbrittlingBlade = 47171, // AlabasterBlade->self, 5.0s cast, single-target
    EmbrittlingBladeVisual = 47172, // Helper->self, no cast, ???
    Summon = 47154, // AlabasterBlade->self, 3.0s cast, single-target
    FourfoldAttackOrder = 47155, // AlabasterBlade->self, 10.0s cast, single-target
    AttackOrder = 47156, // AlabasterBlade->self, no cast, single-target

    AcclaimLong = 47157, // 4BBF->self, 12.0s cast, range 40 90.000-degree cone
    AcclaimShort = 47158, // 4BBF->self, 3.0s cast, range 40 90.000-degree cone

    OccultAeroIII = 47170, // Helper->self, 5.0s cast, range 50 width 10 rect
    RightLeftCombination = 47166, // AlabasterBlade->self, 5.0s cast, range 40 180.000-degree cone
    RightLeftCombinationClearout = 47169, // AlabasterBlade->self, no cast, range 40 180.000-degree cone
    LeftRightCombination = 47167, // AlabasterBlade->self, 5.0s cast, range 40 180.000-degree cone
    LeftRightCombinationClearout = 47168, // AlabasterBlade->self, no cast, range 40 180.000-degree cone

    LightPrayer = 47159, // AlabasterBlade->self, 3.0s cast, single-target
    OccultAero = 47163, // Helper->self, 5.0s cast, range 50 width 10 rect
    OccultTornado = 47165, // Helper->location, 5.0s cast, range 5 circle
    OccultStoneII = 47164, // Helper->self, 5.0s cast, range 40 60.000-degree cone

    FalseSpellbladeHoly = 47757, // AlabasterBlade->self, 32.0s cast, single-target
    FalseSpellbladeHolyVisual = 47161, // Helper->self, no cast, ???
}

public enum SID : uint
{
    BlueArrow = 2056, // none->4EBD, extra=0x43B/0x43C/0x43D - 0x43B = 3 turns, 0x43C = 2 turns, 0x43D = 1 turn
}

sealed class EmbrittlingBlade(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmbrittlingBlade);
sealed class OccultTornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OccultTornado, new AOEShapeCircle(5.0f));
sealed class FalseSpellbladeHoly(BossModule module) : Components.RaidwideCast(module, (uint)AID.FalseSpellbladeHoly);

sealed class OccultAeroIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OccultAeroIII, new AOEShapeRect(50.0f, 5.0f));

sealed class RightLeftCombination(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone cone = new(40.0f, 90.0f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RightLeftCombination or (uint)AID.LeftRightCombination)
        {
            AddAOE();
            AddAOE(180.0f.Degrees(), 2.2f);
        }

        void AddAOE(Angle offset = default, double delay = default)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var pos = delay != default ? loc - 5f * rot.ToDirection() : loc;
            var rot2 = rot + offset;
            aoes.Add(new(cone, pos, rot2, Module.CastFinishAt(spell, delay), shapeDistance: cone.Distance(pos, rot2)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (aoes.Count is var count && count != 0 && spell.Action.ID is (uint)AID.RightLeftCombination or (uint)AID.RightLeftCombinationClearout or
                (uint)AID.LeftRightCombination or (uint)AID.LeftRightCombinationClearout)
        {
            aoes.RemoveAt(0);
            if (count == 2)
            {
                ref var aoe2 = ref aoes.Ref(0);
                var rot = aoe2.Rotation;
                aoe2.Origin -= 5f * rot.ToDirection();
                aoe2.ShapeDistance = cone.Distance(aoe2.Origin, rot);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class OccultAero(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeRect shape = new(50.0f, 5.0f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OccultAero)
        {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OccultAero)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
        {
            return [];
        }

        var waveTimer = aoes.MinBy(a => a.Activation).Activation.AddSeconds(0.2f);
        int show = 0;
        foreach (ref var aoe in CollectionsMarshal.AsSpan(aoes))
        {
            if (aoe.Activation <= waveTimer)
            {
                aoe.Color = Colors.Danger;
                aoe.Risky = true;
                show++;
            }
        }

        return CollectionsMarshal.AsSpan(aoes)[..show];
    }
}

sealed class OccultStoneII(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(40.0f, 30.0f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OccultStoneII)
        {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OccultStoneII)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
        {
            return [];
        }

        var waveTimer = aoes.MinBy(a => a.Activation).Activation.AddSeconds(0.2f);
        foreach (ref var aoe in CollectionsMarshal.AsSpan(aoes))
        {
            if (aoe.Activation <= waveTimer)
            {
                aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class Acclaim(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(40.0f, 45.0f.Degrees());
    private readonly List<(Actor caster, int turns)> golemCasters = [];
    private int totalGolems = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AcclaimLong)
        {
            if (caster.OID == (uint)OID.AlabasterGolemVisual)
            {
                aoes.Add(new(shape, caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BlueArrow)
        {
            switch (status.Extra)
            {
                case 0x43B:
                    golemCasters.Add((actor, 3));
                    totalGolems++;
                    break;
                case 0x43C:
                    golemCasters.Add((actor, 2));
                    totalGolems++;
                    break;
                case 0x43D:
                    golemCasters.Add((actor, 1));
                    totalGolems++;
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AcclaimLong or (uint)AID.AcclaimShort)
        {
            if (aoes.Count > 0)
            {
                aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
                aoes.RemoveAt(0);
            }

            if (aoes.Count == 0)
            {
                golemCasters.Clear();
                totalGolems = 0;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
        {
            return [];
        }

        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(totalGolems).ToList();
        return CollectionsMarshal.AsSpan(incomingAOEs);
    }

    public override void Update()
    {
        AddFutureAOEs();
    }

    private void AddFutureAOEs()
    {
        if (golemCasters.Count == 0)
        {
            return;
        }

        List<AOEInstance> incomingAOEs = [];
        List<(Actor caster, int turns)> processedGolems = [];
        foreach (var golem in golemCasters)
        {
            foreach (var aoe in CollectionsMarshal.AsSpan(aoes))
            {
                if (aoe.Origin.AlmostEqual(golem.caster.Position, 0.5f))
                {
                    Angle rotation = aoe.Rotation;

                    for (int i = 0; i < golem.turns; i++)
                    {
                        rotation -= 90.0f.Degrees();
                        incomingAOEs.Add(new(shape, aoe.Origin, rotation.Normalized(), aoe.Activation + TimeSpan.FromSeconds(7.3f * (i + 1)), actorID: aoe.ActorID));
                    }

                    for (int i = 0; i < 3 - golem.turns; i++)
                    {
                        incomingAOEs.Add(new(shape, aoe.Origin, rotation.Normalized(), aoe.Activation + TimeSpan.FromSeconds(7.3f * (golem.turns + i + 1)), actorID: aoe.ActorID));
                    }

                    processedGolems.Add(golem);
                }
            }
        }

        foreach (var golem in processedGolems)
        {
            golemCasters.Remove(golem);
        }

        if (incomingAOEs.Count > 0)
        {
            aoes.AddRange(incomingAOEs);
        }
    }
}

[SkipLocalsInit]
sealed class CE213QuarriedAwayStates : StateMachineBuilder
{
    public CE213QuarriedAwayStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmbrittlingBlade>()
            .ActivateOnEnter<Acclaim>()
            .ActivateOnEnter<OccultAeroIII>()
            .ActivateOnEnter<RightLeftCombination>()
            .ActivateOnEnter<OccultAero>()
            .ActivateOnEnter<OccultTornado>()
            .ActivateOnEnter<OccultStoneII>()
            .ActivateOnEnter<FalseSpellbladeHoly>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
    StatesType = typeof(CE213QuarriedAwayStates),
    ConfigType = null, // replace null with typeof(AlabasterBladeConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.AlabasterBlade,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14509u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE213QuarriedAway(WorldState ws, Actor primary) : BossModule(ws, primary, new(-519.000f, -641.000f), new ArenaBoundsCircle(25f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LightAether));
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
