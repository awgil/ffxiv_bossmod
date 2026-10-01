namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D072InfernoDrake;

public enum OID : uint
{
    Boss = 0x3DE, // Inferno Drake

    TemplestBiast = 0x3DF // Templest Biast
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    BurningCyclone = 984, // Boss->self, 0.5s, range 9.6 ?-degree cone cleave (120-degree)

    AutoAttackTrash = 871, // Trash->player, no cast
    Levinshower = 985, // Trash->self, 0.5s cast, range 8.2 ?-degree cone cleave
    Levinfang = 519 // Trash->player, no cast, single target
}

class BurningCyclone(BossModule module) : Components.StandardAOEs(module, AID.BurningCyclone, new AOEShapeCone(9.6f, 60.Degrees())); // TODO: verify angle
class Levinshower(BossModule module) : Components.StandardAOEs(module, AID.Levinshower, new AOEShapeCone(8.2f, 45.Degrees())); // TODO: verify angle
class TemplestBiast(BossModule module) : Components.Adds(module, (uint)OID.TemplestBiast, 1);

class D072InfernoDrakeStates : StateMachineBuilder
{
    public D072InfernoDrakeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningCyclone>()
            .ActivateOnEnter<Levinshower>()
            .ActivateOnEnter<TemplestBiast>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 8, NameID = 1284)]
public class D072InfernoDrake(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
