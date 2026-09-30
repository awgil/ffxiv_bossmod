namespace BossMod.RealmReborn.Dungeon.D01Sastasha.D012CaptainMadison;

public enum OID : uint
{
    Boss = 0x19D, // R0.65, Captain Madison first combat (NameID 1382)
    BossSecond = 0x1A0, // R0.65, Captain Madison after Waverider Gate
    ShallowtailReaver = 0x3EE, // R0.50, NameID 342 (also used as trash elsewhere)
    ScurvyDog = 0x19E, // R1.32, NameID 1205 (second Madison)
    ScurvyDogSmall = 0x19F, // R0.66, NameID 1205
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    FastBlade = 9, // BossSecond->player, no cast, single-target
    Sandslinger = 1000, // BossSecond->player, no cast, single-target (Accuracy Down)
}

class MadisonAdds(BossModule module) : Components.AddsMulti(module, [OID.ShallowtailReaver, OID.ScurvyDog, OID.ScurvyDogSmall], 1);

class D012CaptainMadisonStates : StateMachineBuilder
{
    public D012CaptainMadisonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MadisonAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1382)]
public class D012CaptainMadison(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1382,
    PrimaryActorOID = (uint)OID.BossSecond, StatesType = typeof(D012CaptainMadisonStates), SortOrder = 2)]
public class D012CaptainMadisonSecond(ModuleInit init) : D012CaptainMadison(init);
