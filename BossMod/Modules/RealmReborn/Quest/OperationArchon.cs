namespace BossMod.RealmReborn.Quest.OperationArchon;

public enum OID : uint
{
    Boss = 0x38F5, // R1.500, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    ImperialPilusPrior = 0x38F7, // R1.500, x0 (spawn during fight)
    ImperialCenturion = 0x38F6, // R1.500, x0 (spawn during fight)
}

public enum SID : uint
{
    DirectionalParry = 680
}

public enum AID : uint
{
    TartareanShockwave = 28871, // 38F5->self, 3.0s cast, range 7 circle
    GalesOfTartarus = 28870, // 38F5->self, 3.0s cast, range 30 width 5 rect
    MagitekMissiles = 28865, // 233C->location, 4.0s cast, range 7 circle
    TartareanTomb = 28869, // 233C->self, 8.0s cast, range 11 circle
    DrillShot = 28874, // Boss->self, 3.0s cast, range 30 width 5 rect
    TartareanShockwave1 = 28877, // Boss->self, 6.0s cast, range 14 circle
    GalesOfTartarus1 = 28876, // Boss->self, 6.0s cast, range 30 width 30 rect
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.ImperialCenturion, OID.ImperialPilusPrior]);

class MagitekMissiles(BossModule module) : Components.StandardAOEs(module, AID.MagitekMissiles, 7);
class DrillShot(BossModule module) : Components.StandardAOEs(module, AID.DrillShot, new AOEShapeRect(30, 2.5f));
class TartareanShockwave(BossModule module) : Components.StandardAOEs(module, AID.TartareanShockwave, new AOEShapeCircle(7));
class BigTartareanShockwave(BossModule module) : Components.StandardAOEs(module, AID.TartareanShockwave1, new AOEShapeCircle(14));
class GalesOfTartarus(BossModule module) : Components.StandardAOEs(module, AID.GalesOfTartarus, new AOEShapeRect(30, 2.5f));
class BigGalesOfTartarus(BossModule module) : Components.StandardAOEs(module, AID.GalesOfTartarus1, new AOEShapeRect(30, 15));
class DirectionalParry(BossModule module) : Components.DirectionalParry(module, (uint)OID.Boss);
class TartareanTomb(BossModule module) : Components.StandardAOEs(module, AID.TartareanTomb, new AOEShapeCircle(11));

class RhitahtynSasArvinaStates : StateMachineBuilder
{
    public RhitahtynSasArvinaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekMissiles>()
            .ActivateOnEnter<TartareanShockwave>()
            .ActivateOnEnter<DirectionalParry>()
            .ActivateOnEnter<TartareanTomb>()
            .ActivateOnEnter<GalesOfTartarus>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<DrillShot>()
            .ActivateOnEnter<BigTartareanShockwave>()
            .ActivateOnEnter<BigGalesOfTartarus>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70057, NameID = 2160)]
public class RhitahtynSasArvina(WorldState ws, Actor primary) : BossModule(ws, primary, new(-689, -815), new ArenaBoundsCircle(14.5f));
