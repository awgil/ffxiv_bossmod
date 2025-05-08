namespace BossMod.Endwalker.Quest.TheKillingArt;

public enum OID : uint
{
    Boss = 0x3664, // R1.500, x1
    Helper = 0x233C, // R0.500, x10, Helper type
    VoidHecteyes = 0x3666, // R1.200, x0 (spawn during fight)
    VoidPersona = 0x3667, // R1.200, x0 (spawn during fight)
    Voidzone = 0x1E963D
}

public enum AID : uint
{
    MeatySlice = 27590, // Boss->self, 3.4+0.6s cast, single-target
    MeatySlice1 = 27591, // Helper->self, 4.0s cast, range 50 width 12 rect
    Cleaver = 27594, // Boss->self, 3.5+0.5s cast, single-target
    Cleaver1 = 27595, // Helper->self, 4.0s cast, range 40 120-degree cone
    FlankCleaver = 27596, // Boss->self, 3.5+0.5s cast, single-target
    FlankCleaver1 = 27597, // Helper->self, 4.0s cast, range 40 120-degree cone
    Explosion = 27606, // VoidHecteyes->self, 20.0s cast, range 60 circle
    Explosion1 = 27607, // VoidPersona->self, 20.0s cast, range 50 circle
    FocusInferi = 27592, // Boss->self, 2.9+0.6s cast, single-target
    FocusInferi1 = 27593, // Helper->location, 3.5s cast, range 6 circle
    CarnemLevare = 27598, // Boss->self, 4.0s cast, single-target
    CarnemLevare1 = 27599, // Helper->self, 4.0s cast, range 40 width 8 cross
    CarnemLevare2 = 27602, // Helper->self, 3.5s cast, range -17 donut
    CarnemLevare3 = 27600, // Helper->self, 3.5s cast, range -7 donut
    CarnemLevare4 = 27603, // Helper->self, 3.5s cast, range -22 donut
    CarnemLevare5 = 27601, // Helper->self, 3.5s cast, range -12 donut
    VoidMortar = 27604, // Boss->self, 4.0+1.0s cast, single-target
    VoidMortar1 = 27605, // Helper->self, 5.0s cast, range 13 circle
}

class VoidMortar(BossModule module) : Components.StandardAOEs(module, AID.VoidMortar1, new AOEShapeCircle(13));
class FocusInferi(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.FocusInferi1, m => m.Enemies(OID.Voidzone).Where(x => x.EventState != 7), 0);
class CarnemLevareCross(BossModule module) : Components.StandardAOEs(module, AID.CarnemLevare1, new AOEShapeCross(40, 4));
class CarnemLevareDonut(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor, AOEShape)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Take(4).Select(c => new AOEInstance(c.Item2, c.Item1.Position, c.Item1.CastInfo!.Rotation, Module.CastFinishAt(c.Item1.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? sh = (AID)spell.Action.ID switch
        {
            AID.CarnemLevare2 => new AOEShapeDonutSector(12, 17, 90.Degrees()),
            AID.CarnemLevare3 => new AOEShapeDonutSector(2, 7, 90.Degrees()),
            AID.CarnemLevare4 => new AOEShapeDonutSector(17, 22, 90.Degrees()),
            AID.CarnemLevare5 => new AOEShapeDonutSector(7, 12, 90.Degrees()),
            _ => null
        };

        if (sh != null)
            Casters.Add((caster, sh));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CarnemLevare2 or AID.CarnemLevare3 or AID.CarnemLevare4 or AID.CarnemLevare5)
            Casters.RemoveAll(x => x.Item1 == caster);
    }
}
class MeatySlice(BossModule module) : Components.StandardAOEs(module, AID.MeatySlice1, new AOEShapeRect(50, 6));
class Cleaver(BossModule module) : Components.StandardAOEs(module, AID.Cleaver1, new AOEShapeCone(40, 60.Degrees()));
class FlankCleaver(BossModule module) : Components.StandardAOEs(module, AID.FlankCleaver1, new AOEShapeCone(40, 60.Degrees()));
class Adds(BossModule module) : Components.AddsMulti(module, [OID.VoidHecteyes, OID.VoidPersona], 1);

class OrcusStates : StateMachineBuilder
{
    public OrcusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeatySlice>()
            .ActivateOnEnter<Cleaver>()
            .ActivateOnEnter<FlankCleaver>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<FocusInferi>()
            .ActivateOnEnter<CarnemLevareCross>()
            .ActivateOnEnter<CarnemLevareDonut>()
            .ActivateOnEnter<VoidMortar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69614, NameID = 10581)]
public class Orcus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-69.7f, -388.5f), new ArenaBoundsCircle(20));
