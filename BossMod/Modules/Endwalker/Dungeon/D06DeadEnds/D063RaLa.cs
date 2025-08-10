namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D063RaLa;

public enum OID : uint
{
    Boss = 0x34C7,
    Helper = 0x233C,
}

public enum AID : uint
{
    WarmGlow = 25950, // Boss->self, 5.0s cast, range 40 circle
    Pity = 25949, // Boss->player, 5.0s cast, single-target
    LamellarLight = 25939, // Helper->self, 6.0s cast, range 15 circle
    Lifesbreath = 25940, // Boss->self, 4.0s cast, range 50 width 10 rect
    LamellarLightRect = 25951, // Helper->self, 3.0s cast, range 40 width 4 rect
    Benevolence = 25946, // Helper->players, 5.4s cast, range 6 circle
    LovingEmbrace1 = 25943, // Boss->self, 7.0s cast, range 45 180-degree cone
    LovingEmbrace2 = 25944, // Boss->self, 7.0s cast, range 45 180-degree cone
    StillEmbrace = 25948, // Helper->player, 5.4s cast, range 6 circle
}

class WarmGlow(BossModule module) : Components.RaidwideCast(module, AID.WarmGlow);
class Prance(BossModule module) : Components.StandardAOEs(module, AID.LamellarLight, new AOEShapeCircle(15), maxCasts: 3);
class Lifesbreath(BossModule module) : Components.StandardAOEs(module, AID.Lifesbreath, new AOEShapeRect(50, 5));
class LamellarLight(BossModule module) : Components.StandardAOEs(module, AID.LamellarLightRect, new AOEShapeRect(40, 2));
class Benevolence(BossModule module) : Components.StackWithCastTargets(module, AID.Benevolence, 6);
class StillEmbrace(BossModule module) : Components.SpreadFromCastTargets(module, AID.StillEmbrace, 6);
class LovingEmbrace(BossModule module) : Components.GroupedAOEs(module, [AID.LovingEmbrace1, AID.LovingEmbrace2], new AOEShapeCone(45, 90.Degrees()));
class Pity(BossModule module) : Components.SingleTargetCast(module, AID.Pity);

class RaLaStates : StateMachineBuilder
{
    public RaLaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WarmGlow>()
            .ActivateOnEnter<Prance>()
            .ActivateOnEnter<Lifesbreath>()
            .ActivateOnEnter<LamellarLight>()
            .ActivateOnEnter<Benevolence>()
            .ActivateOnEnter<StillEmbrace>()
            .ActivateOnEnter<LovingEmbrace>()
            .ActivateOnEnter<Pity>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10316)]
public class RaLa(WorldState ws, Actor primary) : BossModule(ws, primary, new(-380, -135), new ArenaBoundsCircle(20));
