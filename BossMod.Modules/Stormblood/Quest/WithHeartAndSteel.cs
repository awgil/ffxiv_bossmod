namespace BossMod.Stormblood.Quest.WithHeartAndSteel;

public enum OID : uint
{
    Boss = 0x1AD9, // R2.400
    Helper = 0x18D6, // R0.500
    Bloodfire = 0x1BB7, // R1.000
    Arya = 0x1AE7, // R0.500
    DeepRedXrhunTia = 0x1BB6, // R0.500
}

public enum AID : uint
{
    FlameWreath = 8312, // Boss->player, no cast, range 5 circle, gives Suppuration
    Pirouette = 8313, // Boss->self, no cast, range 6+R circle
    ParticleBeam = 8317, // Boss->location, no cast, range 5 circle
    Soulsteal = 8314, // Boss->player, 9.7s cast, single-target, gives Vulnerability Up and Down for the Count
    VoidRot = 8315, // Boss->self, 3.7s cast, range 60 circle, gives Poison
    ScarletThread = 8311, // Bloodfire->self, 2.7s cast, range 80 width 4 rect
}

class ScarletThread(BossModule module) : Components.StandardAOEs(module, AID.ScarletThread, new AOEShapeRect(80, 2));
class VoidRot(BossModule module) : Components.RaidwideCast(module, AID.VoidRot);
class Soulsteal(BossModule module) : Components.SingleTargetCast(module, AID.Soulsteal);

class LilithStates : StateMachineBuilder
{
    public LilithStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScarletThread>()
            .ActivateOnEnter<VoidRot>()
            .ActivateOnEnter<Soulsteal>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68123, NameID = 6352)]
public class Lilith(ModuleInit init) : BossModule(init, new(-300, -416.5f), new ArenaBoundsCircle(25))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable), ArenaColor.Enemy);
}
