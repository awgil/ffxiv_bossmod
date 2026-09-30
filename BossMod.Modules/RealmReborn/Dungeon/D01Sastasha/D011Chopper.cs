namespace BossMod.RealmReborn.Dungeon.D01Sastasha.D011Chopper;

public enum OID : uint
{
    Boss = 0x19B, // R3.15, Chopper (NameID 1204)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ChargedWhisker = 351, // Boss->self, 3.0s cast, range 3+R circle (paralysis)
}

class ChargedWhisker(BossModule module) : Components.StandardAOEs(module, AID.ChargedWhisker, 6.15f);

class D011ChopperStates : StateMachineBuilder
{
    public D011ChopperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChargedWhisker>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1204)]
public class D011Chopper(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
