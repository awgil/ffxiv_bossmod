namespace BossMod.Stormblood.Quest.ThePowerToProtect;

public enum OID : uint
{
    Boss = 0x1BCB, // R5.400, x1
    CorpseBrigadeKnuckledancer = 0x1C0C, // R0.500, x2 (spawn during fight)
    CorpseBrigadeBowdancer = 0x1C0D, // R0.500, x2 (spawn during fight)
    HeweraldIronaxe = 0x1C01, // R0.500, x1
    CorpseBrigadeFiredancer = 0x1C00, // R0.500, x0 (spawn during fight)
    CorpseBrigadeBowdancer1 = 0x1BFF, // R0.500, x0 (spawn during fight)
    CorpseBrigadeKnuckledancer1 = 0x1BFE, // R0.500, x0 (spawn during fight)
    CorpseBrigadeBarber = 0x1BFD, // R0.500, x0 (spawn during fight)
    SalvagedSlasher = 0x1C1F, // R1.050, x0 (spawn during fight)
    CorpseBrigadeVanguard = 0x1C02, // R2.000, x0 (spawn during fight)
    FireII = 0x1EA4C6,
}

public enum AID : uint
{
    IronTempest = 1003, // HeweraldIronaxe->self, 3.5s cast, range 5+R circle
    FireII = 2175, // CorpseBrigadeFiredancer->location, 2.5s cast, range 5 circle
    Overpower = 720, // HeweraldIronaxe->self, 2.5s cast, range 6+R 90-degree cone
    Rive = 1135, // HeweraldIronaxe->self, 2.5s cast, range 30+R width 2 rect
    DiffractiveLaser = 8348, // Boss->location, 4.0s cast, range 5 circle
}

public enum SID : uint
{
    ExtremeCaution = 1269, // Boss->player, extra=0x0

}

class ExtremeCaution(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ExtremeCaution)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ExtremeCaution)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}
class IronTempest(BossModule module) : Components.StandardAOEs(module, AID.IronTempest, new AOEShapeCircle(5.5f));
class FireII(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.FireII, m => m.Enemies(OID.FireII).Where(x => x.EventState != 7), 0);
class Overpower(BossModule module) : Components.StandardAOEs(module, AID.Overpower, new AOEShapeCone(6.5f, 45.Degrees()));
class Rive(BossModule module) : Components.StandardAOEs(module, AID.Rive, new AOEShapeRect(30.5f, 1));
class DiffractiveLaser(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaser, 5);

class IoStates : StateMachineBuilder
{
    public IoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronTempest>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<ExtremeCaution>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67966, NameID = 5667)]
public class Io(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, B)
{
    public static readonly WPos ArenaCenter = new(76.28f, -659.47f);
    public static readonly WPos[] Corners = [new(101.93f, -666.63f), new(94.49f, -639.63f), new(50.64f, -652.38f), new(57.58f, -679.32f)];

    public static readonly ArenaBoundsCustom B = new(25, new(Corners.Select(c => c - ArenaCenter)));

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
