namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D091LindblumZaghnal;

public enum OID : uint
{
    Boss = 0x4641, // R9.0
    RawElectrope = 0x4642, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 40622, // Boss->location, no cast, single-target

    ElectricalOverload = 40635, // Boss->self, 5.0s cast, range 40 circle

    Gore1 = 40630, // Boss->self, 3.0s cast, single-target
    Gore2 = 41266, // Boss->self, 3.0s cast, single-target
    CaberToss = 40624, // Boss->self, 19.0s cast, single-target
    LineVoltageWide1 = 41121, // Helper->self, 3.3s cast, range 50 width 10 rect
    LineVoltageWide2 = 40627, // Helper->self, 3.5s cast, range 50 width 10 rect
    LineVoltageNarrow1 = 41122, // Helper->self, 3.0s cast, range 50 width 5 rect
    LineVoltageNarrow2 = 40625, // Helper->self, 4.0s cast, range 50 width 5 rect
    CellShock = 40626, // Helper->self, 2.0s cast, range 26 circle

    LightningStormVisual = 40636, // Boss->self, 4.5s cast, single-target
    LightningStorm = 40637, // Helper->player, 5.0s cast, range 5 circle, spread

    SparkingFissureVisual = 40632, // Boss->self, 13.0s cast, single-target
    SparkingFissure = 41258, // Helper->self, 13.7s cast, range 40 circle
    SparkingFissureFirst = 41267, // Helper->self, 5.2s cast, range 40 circle
    SparkingFissureRepeat = 40631, // Helper->self, no cast, range 40 circle

    LightningBolt = 40638, // Helper->location, 5.0s cast, range 6 circle
    Electrify = 40634, // RawElectrope->self, 16.0s cast, range 40 circle
}

sealed class LineVoltage(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rectNarrow = new(50f, 2.5f), rectWide = new(50f, 5f);
    public readonly List<AOEInstance> AOEs = [with(18)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        ref var aoe0 = ref aoes[0];
        ref var aoeL = ref aoes[^1];
        var deadline = aoe0.Activation.AddSeconds(1.5d);
        var isNotLastSet = aoeL.Activation > deadline;
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (isNotLastSet)
                {
                    aoe.Color = color;
                }
                aoe.Risky = true;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LineVoltageNarrow1:
            case (uint)AID.LineVoltageNarrow2:
                AddAOE(rectNarrow);
                break;
            case (uint)AID.LineVoltageWide1:
            case (uint)AID.LineVoltageWide2:
                AddAOE(rectWide);
                break;
        }

        void AddAOE(AOEShapeRect shape) => AOEs.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), risky: false, actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LineVoltageNarrow1:
            case (uint)AID.LineVoltageNarrow2:
            case (uint)AID.LineVoltageWide1:
            case (uint)AID.LineVoltageWide2:
                var count = AOEs.Count;
                var id = caster.InstanceID;
                for (var i = 0; i < count; ++i)
                {
                    if (AOEs[i].ActorID == id)
                    {
                        AOEs.RemoveAt(i);
                        return;
                    }
                }
                break;
        }
    }
}

sealed class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 6f);
sealed class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.LightningStorm, 5f);
sealed class ElectricalOverloadSparkingFissure(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ElectricalOverload, (uint)AID.SparkingFissureFirst, (uint)AID.SparkingFissure]);

sealed class CellShock(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(26f);
    private AOEInstance[] _aoe = [];
    private readonly LineVoltage _aoes = module.FindComponent<LineVoltage>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.AOEs.Count == 0 ? _aoe : [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (state is 0x00020001u or 0x00200010u)
        {
            if (state == 0x00200010u)
                index = index switch
                {
                    0x0D => 0x10,
                    0x0E => 0x0F,
                    0x0F => 0x0E,
                    0x10 => 0x0D,
                    _ => index
                };

            WPos position = index switch
            {
                0x0D => new(81.132f, 268.868f),
                0x0E => new(81.132f, 285.132f),
                0x0F => new(64.868f, 268.868f),
                0x10 => new(64.868f, 285.132f),
                _ => default
            };
            _aoe = [new(circle, position.Quantized(), default, WorldState.FutureTime(8d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CellShock)
        {
            _aoe = [];
        }
    }
}

sealed class D091LindblumZaghnalStates : StateMachineBuilder
{
    public D091LindblumZaghnalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LineVoltage>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<LightningStorm>()
            .ActivateOnEnter<ElectricalOverloadSparkingFissure>()
            .ActivateOnEnter<CellShock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008u, NameID = 13623u, SortOrder = 3)]
public sealed class D091LindblumZaghnal : BossModule
{
    public D091LindblumZaghnal(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D091LindblumZaghnal(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(73f, 277f), 19.5f, 64)], [new Rectangle(new(72f, 297f), 20f, 1.1f),
            new Rectangle(new(72f, 257f), 20f, 1.05f)]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.RawElectrope));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.RawElectrope => 1,
                _ => 0
            };
        }
    }
}
