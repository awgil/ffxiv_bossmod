namespace BossMod.Dawntrail.Hunt.RankA.CatsEye;

public enum OID : uint
{
    Boss = 0x4396, // R6.020, x1
}

public enum AID : uint
{
    AutoAttack = 38517, // Boss->player, no cast, single-target
    CatsEyeNormal = 38510, // Boss->location, 7.0s cast, range 40 circle, jump + gaze
    CatsEyeInverted = 38511, // Boss->location, 7.0s cast, range 40 circle, jump + inverted gaze
    KillerCuriosity = 38514, // Boss->self, 4.0s cast, single-target, visual (invert next gazes)
    BloodshotGazeNormal = 38515, // Boss->players, 5.0s cast, range 8 circle, stack + gaze
    BloodshotGazeInverted = 39668, // Boss->players, 5.0s cast, range 8 circle, stack + inverted gaze
    KillerCuriosityEnd = 38516, // Boss->self, no cast, single-target, visual (remove inversion status)
    GravitationalWave = 39887, // Boss->self, 5.0s cast, range 40 circle, raidwide
}

public enum SID : uint
{
    WanderingEyes = 4010, // Boss->Boss, extra=0x0
}

public enum IconID : uint
{
    BloodshotGaze = 548, // player
}

class CatsEyeNormal(BossModule module) : Components.CastGaze(module, AID.CatsEyeNormal, false, 40);
class CatsEyeInverted(BossModule module) : Components.CastGaze(module, AID.CatsEyeInverted, true, 40);
class BloodshotGazeNormal(BossModule module) : Components.CastGaze(module, AID.BloodshotGazeNormal, false, 8);
class BloodshotGazeInverted(BossModule module) : Components.CastGaze(module, AID.BloodshotGazeInverted, true, 8);
class BloodshotStackNormal(BossModule module) : Components.StackWithCastTargets(module, AID.BloodshotGazeNormal, 8, 4);
class BloodshotStackInverted(BossModule module) : Components.StackWithCastTargets(module, AID.BloodshotGazeInverted, 8, 4);
class GravitationalWave(BossModule module) : Components.RaidwideCast(module, AID.GravitationalWave);

class CatsEyeStates : StateMachineBuilder
{
    public CatsEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CatsEyeNormal>()
            .ActivateOnEnter<CatsEyeInverted>()
            .ActivateOnEnter<BloodshotGazeNormal>()
            .ActivateOnEnter<BloodshotGazeInverted>()
            .ActivateOnEnter<BloodshotStackNormal>()
            .ActivateOnEnter<BloodshotStackInverted>()
            .ActivateOnEnter<GravitationalWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13436)]
public class CatsEye(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
