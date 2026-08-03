namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D131Amhuluk;

public enum OID : uint
{
    Boss = 0x3169, // R7.008, x1
    LightningRod = 0x31B6, // R1.0
    BallOfLevin = 0x31A2, // R1.3
    SuperchargedLevin = 0x31A3, // R2.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 23633, // Boss->location, no cast, single-target

    CriticalRip = 23630, // Boss->player, 5.0s cast, single-target
    LightningBoltVisual = 23627, // Boss->self, 10.0s cast, single-target
    LightningBolt = 23628, // Helper->location, no cast, range 10 circle
    ElectricBurst = 23629, // Boss->self, 4.5s cast, range 50 width 40 rect, raidwide (rect also goes to the back of boss)
    Thundercall = 23632, // Boss->self, 4.0s cast, single-target
    ShockSmall = 23634, // BallOfLevin->self, no cast, range 5 circle
    ShockLarge = 23635, // SuperchargedLevin->self, no cast, range 10 circle
    WideBlaster = 24773, // Boss->self, 4.0s cast, range 26 120-degree cone
    SpikeFlail = 23631 // Boss->self, 1.0s cast, range 25 60-degree cone
}

public enum SID : uint
{
    LightningRod = 2574 // none->player/LightningRod, extra=0x114
}

sealed class ElectricBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElectricBurst);
sealed class CriticalRip(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CriticalRip);

sealed class LightningBolt(BossModule module) : Components.GenericBaitAway(module, (uint)AID.LightningBolt, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private DateTime activation;
    private readonly List<Actor> freeRods = module.Enemies((uint)OID.LightningRod);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBaitTarget(actor))
        {
            hints.Add("Pass the lightning to a rod!");
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.LightningRod)
        {
            if (activation == default)
            {
                activation = WorldState.FutureTime(10.8d);
            }

            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, activation));
            if (actor.OID == (uint)OID.LightningRod)
            {
                freeRods.Remove(actor);
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.LightningRod)
        {
            if (actor.OID == (uint)OID.LightningRod)
            {
                freeRods.Add(actor);
            }

            var count = CurrentBaits.Count;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Target == actor)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LightningBolt)
        {
            CurrentBaits.Clear();
            activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!IsBaitTarget(actor))
        {
            return;
        }
        var count = freeRods.Count;
        var forbidden = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            forbidden[i] = new SDInvertedCircle(freeRods[i].Position, 4f);
        }
        if (count != 0)
        {
            hints.AddForbiddenZone(new SDIntersection(forbidden), activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!IsBaitTarget(pc))
        {
            return;
        }
        base.DrawArenaForeground(pcSlot, pc);
        var count = freeRods.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(freeRods[i].Position, 4f, Colors.Safe);
        }
    }
}

sealed class Shock(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleSmall = new(5f), circleBig = new(10f);
    private readonly List<AOEInstance> _aoes = [with(6)];
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        var shape = actor.OID switch
        {
            (uint)OID.BallOfLevin => circleSmall,
            (uint)OID.SuperchargedLevin => circleBig,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, actor.Position.Quantized(), default, WorldState.FutureTime(3.7d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ShockSmall or (uint)AID.ShockLarge)
        {
            if (++NumCasts == (first ? 72 : 30))
            {
                _aoes.Clear();
                NumCasts = 0;
                first = false;
            }
        }
    }
}

sealed class WideBlasterSpikeFlail(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone coneWide = new(26f, 60f.Degrees()), coneNarrow = new(25f, 30f.Degrees());
    private readonly List<AOEInstance> _aoes = [with(2)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        aoe0.Risky = true;
        if (count > 1)
        {
            aoe0.Color = Colors.Danger;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, Angle offset = default, double delay = default)
        => _aoes.Add(new(shape, caster.Position, spell.Rotation + offset, Module.CastFinishAt(spell, delay), risky: false));
        if (spell.Action.ID == (uint)AID.WideBlaster)
        {
            AddAOE(coneWide);
            AddAOE(coneNarrow, 180f.Degrees(), 2.6d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.WideBlaster or (uint)AID.SpikeFlail)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class D131AmhulukStates : StateMachineBuilder
{
    public D131AmhulukStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricBurst>()
            .ActivateOnEnter<CriticalRip>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<WideBlasterSpikeFlail>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777, NameID = 10075)]
public sealed class D131Amhuluk(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos ArenaCenter = new(-520f, 145f), LightningRod = new(-538.53015f, 137.31409f);

    private static Polygon[] GenerateLightningRods()
    {
        const float radius = 1.5f;
        const int edges = 16;
        var polygons = new Polygon[8];

        polygons[0] = new Polygon(LightningRod, radius, edges);
        for (var i = 1; i < 8; ++i)
        {
            polygons[i] = new Polygon(WPos.RotateAroundOrigin(i * 45f, ArenaCenter, LightningRod), radius, edges);
        }
        return polygons;
    }
    private static readonly ArenaBoundsCustom arena = new([new Polygon(ArenaCenter, 19.5f, 48)], [.. GenerateLightningRods(), new Rectangle(new(-540f, 145.0004f), 8.75f, 1.25f, 89.98f.Degrees()),
    new Rectangle(new(-500f, 145f), 1.25f, 20)]);
}
