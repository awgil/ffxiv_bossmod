namespace BossMod.RealmReborn.Dungeon.D04Halatali.D042ThunderclapGuivre;

public enum OID : uint
{
    Boss = 0x4644,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    Electrify = 40595, // Boss->location, 4.0s cast, range 6 circle

    HydroelectricShockVisual = 40593, // Boss->self, 9.0+1.0s cast, single-target visual (water electrifies)
    HydroelectricShock = 41113, // Helper->self, 10.0s cast, custom arena (get to dry ground)

    Levinfang = 40594 // Boss->player, 5.0s cast, single-target
}

class Electrify(BossModule module) : Components.StandardAOEs(module, AID.Electrify, 6);
// TODO: replace hint with map geometry / voidzone for wet ground once dry pads are known from replay
class HydroelectricShock(BossModule module) : Components.CastHint(module, AID.HydroelectricShockVisual, "Get to dry ground!", true);
class Levinfang(BossModule module) : Components.SingleTargetCast(module, AID.Levinfang);

class D042ThunderclapGuivreStates : StateMachineBuilder
{
    public D042ThunderclapGuivreStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrify>()
            .ActivateOnEnter<HydroelectricShock>()
            .ActivateOnEnter<Levinfang>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 7, NameID = 1196)] // TODO: clear after dry-ground geometry
public class D042ThunderclapGuivre(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
