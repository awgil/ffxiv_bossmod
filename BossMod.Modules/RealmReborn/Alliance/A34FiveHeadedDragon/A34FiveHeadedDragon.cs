namespace BossMod.RealmReborn.Alliance.A34FiveHeadedDragon;

public enum OID : uint
{
    Boss = 0xDFC, // R8.000, x1
    Helper = 0x1B2, // R0.500
    DragonfireFly = 0xDFE, // R0.800
    HeadOfPoison = 0xE16, // R8.000, Part type
    HeadOfFire = 0xE13, // R8.000, Part type
    HeadOfThunder = 0xE15, // R8.000, Part type
    HeadOfIce = 0xE14, // R8.000, Part type
    Prominence = 0xDFD, // R1.000
    PoisonSlime = 0xDFF, // R1.000
    ToxicSlime = 0xE17, // R3.000
    IceFloor = 0x1E972B, // R0.500, has 1E9729 and 1E972A as parts, but just display 2B
}

public enum AID : uint
{
    AutoAttack = 3355, // Boss->players, no cast, range 6+R cone
    WhiteBreath = 3295, // Boss->self, 3.5s cast, range 22+R 120-degree cone
    BreathOfFire = 3285, // Helper->location, 3.0s cast, range 6 circle
    Fireball = 3290, // DragonfireFly->player, no cast, single-target
    BreathOfThunder = 3287, // Helper->players, no cast, range 6 circle (stack)
    Discordance = 3282, // Boss->self, 5.0s cast, range 60 circle
    BreathOfIce = 3286, // Helper->location, 3.0s cast, range 6 circle
    BreathOfLight = 3284, // Helper->location, 3.0s cast, range 6 circle
    BreathOfPoison = 3288, // Helper->location, 3.0s cast, range 6 circle
    Seep = 3291, // PoisonSlime->self, no cast, range 10 circle
    Digest = 724, // ToxicSlime->player, no cast, single-target
    FluidSpread = 725, // ToxicSlime->player, no cast, single-target
    Fermata = 3283, // Boss->location, no cast, range 20 circle
    TheLastSong = 2376, // ToxicSlime->self, no cast, range 60 circle
    Radiance = 3289, // Prominence->self, 10.0s cast, range 60 circle
    AutoAttackSlime = 872, // PoisonSlime/ToxicSlime->player, no cast, single-target
    HeatWave = 3294, // Boss->self, 1.5s cast, range 60 circle
}

public enum SID : uint
{
    Pyretic = 639, // Boss->player
}

public enum IconID : uint
{
    Stack = 318, // player
}

class WhiteBreath(BossModule module) : Components.StandardAOEs(module, AID.WhiteBreath, new AOEShapeCone(30, 60.Degrees()));
class BreathOfFire(BossModule module) : Components.StandardAOEs(module, AID.BreathOfFire, 6);
class BreathOfLight(BossModule module) : Components.StandardAOEs(module, AID.BreathOfLight, 6);
class BreathOfPoison(BossModule module) : Components.StandardAOEs(module, AID.BreathOfPoison, 6);
class IceFloor(BossModule module) : Components.VoidzoneAtCastTarget(module, 12, AID.BreathOfIce, OID.IceFloor, 7.4f);
class Stack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.BreathOfThunder, 6, 6, 3);
class Discordance(BossModule module) : Components.RaidwideCast(module, AID.Discordance, "Kill the heads!");
class Radiance(BossModule module) : Components.RaidwideCast(module, AID.Radiance, "Kill Prominence!");
class HeatWave(BossModule module) : Components.RaidwideCast(module, AID.HeatWave, "Pyretic incoming, stop moving");
class HeatWavePyretic(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}
class Heads(BossModule module) : Components.AddsMulti(module, [OID.HeadOfPoison, OID.HeadOfFire, OID.HeadOfThunder, OID.HeadOfIce], 1);
class Prominence(BossModule module) : Components.Adds(module, (uint)OID.Prominence, 2);
class Slimes(BossModule module) : Components.AddsMulti(module, [OID.PoisonSlime, OID.ToxicSlime], 1);

class A34FiveHeadedDragonStates : StateMachineBuilder
{
    public A34FiveHeadedDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhiteBreath>()
            .ActivateOnEnter<BreathOfFire>()
            .ActivateOnEnter<IceFloor>()
            .ActivateOnEnter<BreathOfLight>()
            .ActivateOnEnter<BreathOfPoison>()
            .ActivateOnEnter<Stack>()
            .ActivateOnEnter<Discordance>()
            .ActivateOnEnter<Radiance>()
            .ActivateOnEnter<HeatWave>()
            .ActivateOnEnter<HeatWavePyretic>()
            .ActivateOnEnter<Heads>()
            .ActivateOnEnter<Prominence>()
            .ActivateOnEnter<Slimes>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3227)]
public class A34FiveHeadedDragon(ModuleInit init) : BossModule(init, new(200, 179), new ArenaBoundsCircle(30));
