namespace BossMod.Modules.Dawntrail.Foray.FATE.Execrator;

public enum OID : uint {
    Boss = 0x47DF,
    Helper = 0x233C,
    BallOfFire = 0x483D, // R1.000-1.860, x0 (spawn during fight)
    VodorigaServant = 0x47E0, // R1.200, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 43273, // Boss->player, no cast, single-target
    AutoAttackAdd = 43371, // 47E0->player, no cast, single-target
    Teleport = 43165, // Boss->location, no cast, single-target
    Teleport2 = 45286, // Boss->location, no cast, single-target

    GreatBallOfFire = 43271, // Boss->self, 4.0s cast, single-target
    Max = 43270, // Boss->self, 5.0s cast, range 100 circle
    ArmOfPurgatory = 43269, // 483D->location, 8.0s cast, range 8 circle

    DarkMist = 43048, // Boss->self, 5.0s cast, range 30 circle
    VoidFireIII = 43049, // Boss->self, 4.0s cast, range 10 circle
    Mini = 43046, // Boss->self, 4.0s cast, range 30 ?-degree cone

    BloodyClaw = 43051, // VodorigaServant->player, 4.0s cast, single-target
}

class ArmOfPurgatory(BossModule module) : Components.StandardAOEs(module, AID.ArmOfPurgatory, 20f);
class VoidFireIII(BossModule module) : Components.StandardAOEs(module, AID.VoidFireIII, 10f);
class DarkMist(BossModule module) : Components.StandardAOEs(module, AID.DarkMist, 22.0f);
class Mini(BossModule module) : Components.StandardAOEs(module, AID.Mini, new AOEShapeCone(30f, 60.0f.Degrees()));

class ExecratorStates : StateMachineBuilder {
    public ExecratorStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<ArmOfPurgatory>()
            .ActivateOnEnter<VoidFireIII>()
            .ActivateOnEnter<DarkMist>()
            .ActivateOnEnter<Mini>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13855)]
public class Execrator(WorldState ws, Actor primary) : BossModule(ws, primary, new(79.0f, 278.0f), new ArenaBoundsCircle(40)) {
    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.VodorigaServant), ArenaColor.Enemy);
    }
}
