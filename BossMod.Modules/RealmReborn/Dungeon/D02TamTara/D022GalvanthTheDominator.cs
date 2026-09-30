namespace BossMod.RealmReborn.Dungeon.D02TamTara.D022GalvanthTheDominator;

public enum OID : uint
{
    Boss = 0x4C, // R1.95,
    InconspicuousImp = 0x7D, // R0.45
    SkeletonSoldier = 0x7E, // R0.75
    DeepcroftMiteling = 0x7F // R1.8
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast

    Water = 971, // Boss->player, 1.0s cast, single target
    DrainTouch = 988, // Boss->player, no cast, single target
    MindBlast = 987, // Boss->self, 5.0s cast, range 8+R (9.95) circle
    HellSlash = 341 // SkeletonSoldier->player, no cast, single target
}

class MindBlast(BossModule module) : Components.StandardAOEs(module, AID.MindBlast, 9.95f);
class GalvanthAdds(BossModule module) : Components.AddsMulti(module, [OID.InconspicuousImp, OID.SkeletonSoldier, OID.DeepcroftMiteling], 1);

class D022GalvanthTheDominatorStates : StateMachineBuilder
{
    public D022GalvanthTheDominatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MindBlast>()
            .ActivateOnEnter<GalvanthAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 2, NameID = 73)]
public class D022GalvanthTheDominator(ModuleInit init) : BossModule(init, new(-52.765f, -12.789f), new ArenaBoundsCircle(18));
