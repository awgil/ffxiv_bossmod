namespace BossMod.Dawntrail.Raid.M12NLindwurm;

sealed class TheFixer(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheFixer);

sealed class SerpentineScourge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect _rect = new(30f, 10f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Length == 0)
        {
            return _aoe;
        }

        var time = WorldState.CurrentTime;

        ref var aoe = ref _aoe[0];
        aoe.Risky = aoe.Activation.AddSeconds(-4.5d) <= time;
        return _aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var action && action is (uint)AID.BloodshedLeft or (uint)AID.BloodshedRight)
        {
            _aoe = [(new(_rect, new WPos(action == (uint)AID.BloodshedLeft ? 90f : 110f, 85f).Quantized(), Angle.AnglesCardinals[1], WorldState.FutureTime(7.5d)))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SerpentineScourge)
        {
            _aoe = [];
        }
    }

    public override void OnActorUntargetable(Actor actor)
    {
        if (actor == Module.PrimaryActor)
        {
            _aoe = [];
        }
    }
}

sealed class RavenousReach(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RavenousReach, new AOEShapeCone(35f, 60f.Degrees()), riskyWithSecondsLeft: 7d)
{
    public override void OnActorUntargetable(Actor actor)
    {
        if (actor == Module.PrimaryActor)
        {
            Casters.Clear();
        }
    }
}

sealed class Splattershed(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Splattershed1Visual, (uint)AID.Splattershed2Visual]);
sealed class BringDownTheHouse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BringDownTheHouse, new AOEShapeRect(15f, 5f));
sealed class SplitScourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SplitScourge, new AOEShapeRect(30f, 5f));
sealed class VenomousScourge(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VenomousScourge, (uint)AID.VenomousScourge, 5f, 5d);
sealed class GrandEntrance(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GrandEntrance1, (uint)AID.GrandEntrance2], 2f);
sealed class VisceralBurst(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.TankBait, (uint)AID.VisceralBurst, 6f, 5d);
sealed class MindlessFlesh(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MindlessFlesh1, (uint)AID.MindlessFlesh2, (uint)AID.MindlessFlesh3, (uint)AID.MindlessFlesh4, (uint)AID.MindlessFlesh5], new AOEShapeRect(30f, 4f), 2, 2);

sealed class MindlessFleshBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MindlessFleshBig, new AOEShapeRect(30f, 17.5f), riskyWithSecondsLeft: 9d)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(Casters);
        if (aoes[0].Activation.AddSeconds(-9d) < WorldState.CurrentTime)
        {
            return aoes;
        }
        return [];
    }
}

sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(12f);
    private readonly List<AOEInstance> aoes = [];
    public readonly List<WPos> Positions = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020u && actor.OID == (uint)OID.BurstBlob)
        {
            var pos = actor.Position.Quantized();
            aoes.Add(new(circle, pos, activation: WorldState.FutureTime(6.6d)));
            Positions.Add(pos);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            aoes.Clear();
            Positions.Clear();
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(M12NLindwurmStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.Lindwurm,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Raid,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1074u,
NameID = 14378u,
SortOrder = 1,
PlanLevel = 0)]
public sealed class M12NLindwurm(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsRect ArenaBounds = new(20f, 15f);
}
