namespace BossMod.Dawntrail.Foray.FATE.Stormcaller;

public enum OID : uint
{
    Boss = 0x4BEC,
    Helper = 0x233C,
    Stormcaller = 0x4BED, // R1.000, x0 (spawn during fight)
    BitingWind = 0x4C25, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50854, // Boss->player, no cast, single-target
    Teleport = 45587, // Boss->location, no cast, single-target
    Stormcall = 47580, // Boss->self, 3.0s cast, single-target
    FreefallTeleport = 47598, // Boss->location, 4.0s cast, single-target
    Freefall = 47584, // Boss->location, no cast, range 12 circle
    BitingScratch = 47588, // Boss->self, 5.0s cast, range 40 90-degree cone
    Windage = 47583, // 4C25->self, 2.0s cast, range 7 circle

    FocusedTremor1 = 47587, // 4BED->location, 6.0s cast, range 10 circle
    FocusedTremor2 = 47586, // 4BED->location, 8.0s cast, range 10-20 donut
    FocusedTremor3 = 47585, // 4BED->location, 10.0s cast, range 20-30 donut

    FocusedTremor4 = 47594, // 4BED->location, 9.0s cast, range 10 circle
    FocusedTremor5 = 47593, // 4BED->location, 11.0s cast, range 10-20 donut
    FocusedTremor6 = 47592, // 4BED->location, 13.0s cast, range 20-30 donut

    FocusedTremor7 = 47597, // 4BED->location, 11.5s cast, range 10 circle
    FocusedTremor8 = 47596, // 4BED->location, 13.5s cast, range 10-20 donut
    FocusedTremor9 = 47595, // 4BED->location, 15.5s cast, range 20-30 donut
}

class Windage(BossModule module) : Components.StandardAOEs(module, AID.Windage, 7.0f);
class BitingScratch(BossModule module) : Components.StandardAOEs(module, AID.BitingScratch, new AOEShapeCone(40.0f, 45.0f.Degrees()));

class FocusedTremor(BossModule module) : Components.ConcentricAOEs(module, shapes)
{
    private static readonly AOEShape[] shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FocusedTremor1 or (uint)AID.FocusedTremor4 or (uint)AID.FocusedTremor7)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.FocusedTremor1 or (uint)AID.FocusedTremor4 or (uint)AID.FocusedTremor7 => 0,
                (uint)AID.FocusedTremor2 or (uint)AID.FocusedTremor5 or (uint)AID.FocusedTremor8 => 1,
                (uint)AID.FocusedTremor3 or (uint)AID.FocusedTremor6 or (uint)AID.FocusedTremor9 => 2,
                _ => -1
            };

            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2.0f));
        }
    }
}

class StormcallerStates : StateMachineBuilder
{
    public StormcallerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Windage>()
            .ActivateOnEnter<BitingScratch>()
            .ActivateOnEnter<FocusedTremor>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14776)]
public class Stormcaller(WorldState ws, Actor primary) : BossModule(ws, primary, new(-850.000f, 486.000f), new ArenaBoundsCircle(40));
