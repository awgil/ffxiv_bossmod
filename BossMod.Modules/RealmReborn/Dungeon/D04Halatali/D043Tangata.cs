namespace BossMod.RealmReborn.Dungeon.D04Halatali.D043Tangata;

public enum OID : uint
{
    Boss = 0x4645, // R2.08
    Damantus = 0x4646, // R0.2
    Noxius = 0x4647, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    StraightPunch = 40602, // Boss->player, 5.0s cast, single-target

    BurningWard = 40596, // Boss->self, 5.0s cast, single-target
    ScorchedEarth1 = 40597, // Damantus->self, no cast, range 60 circle
    ScorchedEarth2 = 40598, // Noxius->self, no cast, range 60 circle

    PlainPound = 40599, // Boss->self, 5.0s cast, range 10 circle
    Tremblor = 40600, // Helper->self, 8.0s cast, range 10-20 donut
    Earthquake = 40601, // Helper->self, 11.0s cast, range 20-30 donut

    Firewater = 41123 // Boss->location, 2.5s cast, range 3 circle
}

public enum SID : uint
{
    BurningWard = 4175
}

class StraightPunch(BossModule module) : Components.SingleTargetCast(module, AID.StraightPunch);
class PlainPound(BossModule module) : Components.StandardAOEs(module, AID.PlainPound, 10);
class Tremblor(BossModule module) : Components.StandardAOEs(module, AID.Tremblor, new AOEShapeDonut(10, 20));
class Earthquake(BossModule module) : Components.StandardAOEs(module, AID.Earthquake, new AOEShapeDonut(20, 30));
class Firewater(BossModule module) : Components.StandardAOEs(module, AID.Firewater, 3);
class BurningWardAdds(BossModule module) : Components.AddsMulti(module, [OID.Damantus, OID.Noxius], 1);

class D043TangataStates : StateMachineBuilder
{
    public D043TangataStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StraightPunch>()
            .ActivateOnEnter<PlainPound>()
            .ActivateOnEnter<Tremblor>()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<Firewater>()
            .ActivateOnEnter<BurningWardAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 7, NameID = 1197)]
public class D043Tangata(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(20));
