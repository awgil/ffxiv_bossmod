namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

public enum OID : uint
{
    Boss = 0x4A37,
    Helper = 0x233C,
    _Gen_Doomtrain = 0x4A3B, // R1.000, x1, Helper type
    _Gen_LevinSignal = 0x4A38, // R1.000, x0 (spawn during fight)
    _Gen_KinematicTurret = 0x4A39, // R1.200, x0 (spawn during fight)
    _Gen_Aether = 0x4A3A, // R1.500, x0 (spawn during fight)
    _Gen_GhostTrain = 0x4B81, // R2.720, x0 (spawn during fight)
    _Gen_Doomtrain1 = 0x4B7F, // R19.040, x0 (spawn during fight)
    _Gen_ = 0x4A36, // R1.000, x0 (spawn during fight)
}

class DoomtrainStates : StateMachineBuilder
{
    public DoomtrainStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    //private void XXX(uint id, float delay)
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1077, NameID = 14284, DevOnly = true)]
public class Doomtrain(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(10, 14.5f));
