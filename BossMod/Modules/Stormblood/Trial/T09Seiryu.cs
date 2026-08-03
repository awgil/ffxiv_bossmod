namespace BossMod.Stormblood.Trial.WreathOfSnakes.T09Seiryu;

public enum OID : uint
{
    Boss = 0x25F4, // actual boss target
    AoNoShiki = 0x233C, // R0.500, x?, Helper type
    AkaNoShiki = 0x2786, // R2.600, x?
    AoNoShiki1 = 0x2787, // R3.000, x?
    IwaNoShiki = 0x2788, // R4.000, x?
    BlueOrochi = 0x2672, // R1.000, x?
    TenNoShiki = 0x25F8, // R2.700, x?
    NumaNoShiki = 0x25F6, // R2.400, x?
    DoroNoShiki = 0x25F7, // R1.440, x?
    BlueOrochi1 = 0x25F5, // R1.000, x?
    BlueOrochi2 = 0x2658, // R1.000, x?
    BlueOrochi3 = 0x2659, // R1.000, x?
}



public enum AID : uint
{

}

class T09SeiryuStates : StateMachineBuilder
{
    public T09SeiryuStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Contributors = "skmagiik", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 637, NameID = 7922)]
public class T09Seiryu(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(38));
