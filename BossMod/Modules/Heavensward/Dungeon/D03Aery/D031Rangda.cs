namespace BossMod.Heavensward.Dungeon.D03Aery.D031Rangda;

public enum OID : uint
{
    Boss = 0xEA6, // R4.9
    BlackenedStatue = 0xEA8, // R1.4
    Leyak = 0xEA7, // R3.6
    Helper = 0x1144
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // Leyak->player, no cast, single-target

    ElectricPredation = 3887, // Boss->self, no cast, range 8+R 90-degree cone
    ElectricCachexia = 3889, // Boss->self, 7.0s cast, range 8-60 donut
    IonosphericCharge = 3888, // Boss->self, 3.0s cast, single-target
    Ground = 3892, // Rangda->player/BlackenedStatue, no cast, single-target, failed to give tether to rod
    ElectrocutionVisual = 3890, // Boss->self, 3.0s cast, single-target
    Electrocution = 3891, // Rangda->self, no cast, range 60+R width 5 rect, targets 3 seemingly random players after visual, knockback 15 away from source
    LightningBolt = 3893, // Rangda->location, 3.0s cast, range 3 circle
    Reflux = 3894 // Leyak->player, no cast, single-target
}

public enum TetherID : uint
{
    Lightning = 6 // Boss->player/BlackenedStatue
}

class ElectricPredation(BossModule module) : Components.Cleave(module, (uint)AID.ElectricPredation, new AOEShapeCone(12.9f, 60.Degrees()));

class Electrocution(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(64.9f, 2.5f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrocutionVisual)
        {
            var party = Module.Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            var act = Module.CastFinishAt(spell, 0.9d);
            for (var i = 0; i < len; ++i)
            {
                CurrentBaits.Add(new(caster, party[i], rect, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Electrocution)
        {
            ++NumCasts;
            if (NumCasts == 3 || NumCasts == CurrentBaits.Count) // hits upto 3 random players
            {
                CurrentBaits.Clear();
                NumCasts = 0;
            }
        }
    }
}

class IonosphericCharge(BossModule module) : Components.BaitAwayTethers(module, 0f, (uint)TetherID.Lightning, activationDelay: 10.1f)
{
    private readonly List<Actor> statues = module.Enemies((uint)OID.BlackenedStatue);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBaitTarget(actor))
        {
            hints.Add("Pass the tether to a statue!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!IsBaitTarget(actor))
        {
            return;
        }

        var count = statues.Count;
        if (count == 0)
        {
            return;
        }
        var forbidden = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            forbidden[i] = new SDInvertedCircle(statues[i].Position, 4f);
        }

        hints.AddForbiddenZone(new SDIntersection(forbidden), CurrentBaits.Ref(0).Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!IsBaitTarget(pc))
        {
            return;
        }
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(statues, Colors.Object, true);
        var count = statues.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(statues[i].Position, 4f, Colors.Safe);
        }
    }
}

class ElectricCachexia(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricCachexia, new AOEShapeDonut(8f, 60f));
class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 3f);

class D031RangdaStates : StateMachineBuilder
{
    public D031RangdaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricPredation>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<IonosphericCharge>()
            .ActivateOnEnter<ElectricCachexia>()
            .ActivateOnEnter<Electrocution>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3452)]
public class D031Rangda(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(332.34f, -228.43f), new(337.76f, -228.37f), new(338.36f, -228.27f), new(343.49f, -227.03f), new(344.03f, -226.78f),
    new(348.5f, -224.57f), new(348.95f, -224.28f), new(353.31f, -220.68f), new(356.57f, -216.48f), new(356.89f, -215.99f),
    new(359.19f, -211.05f), new(359.34f, -210.45f), new(360.39f, -205.52f), new(360.44f, -204.98f), new(360.38f, -199.62f),
    new(360.27f, -199.1f), new(359.12f, -194.36f), new(358.97f, -193.81f), new(356.49f, -189.12f), new(356.18f, -188.66f),
    new(353.12f, -184.91f), new(352.73f, -184.54f), new(352.12f, -184.63f), new(351.55f, -184.35f), new(336.81f, -174.73f),
    new(336.3f, -174.57f), new(335.79f, -174.58f), new(335.24f, -174.57f), new(332.67f, -174.66f), new(323.57f, -174.39f),
    new(322.99f, -174.43f), new(319.75f, -179.36f), new(313.09f, -188.8f), new(312.74f, -189.25f), new(310.34f, -194.39f),
    new(310.2f, -194.89f), new(309.86f, -196.48f), new(309.73f, -196.98f), new(309.06f, -200.14f), new(309.13f, -205.8f),
    new(309.23f, -206.4f), new(310.42f, -211.32f), new(310.6f, -211.81f), new(312.87f, -216.37f), new(313.13f, -216.84f),
    new(316.64f, -221.13f), new(317.1f, -221.53f), new(320.95f, -224.51f), new(321.45f, -224.86f), new(326.29f, -227.11f),
    new(326.84f, -227.29f), new(332.07f, -228.41f)];

    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Leyak));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Leyak => 1,
                _ => 0
            };
        }
    }
}
