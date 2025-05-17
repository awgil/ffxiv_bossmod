namespace BossMod.Endwalker.Criterion.C01ASS.C010Dryad;

public enum OID : uint
{
    NBoss = 0x3AD1, // R3.000, x1
    NOdqan = 0x3AD2, // R1.050, x2
    SBoss = 0x3ADA, // R3.000, x1
    SOdqan = 0x3ADB, // R1.050, x2
}

public enum AID : uint
{
    AutoAttack = 31320, // NBoss/NOdqan/SBoss/SOdqan->player, no cast, single-target
    NArborealStorm = 31063, // NBoss->self, 5.0s cast, range 12 circle
    NAcornBomb = 31064, // NBoss->location, 3.0s cast, range 6 circle
    NGelidGale = 31065, // NOdqan->location, 3.0s cast, range 6 circle
    NUproot = 31066, // NOdqan->self, 3.0s cast, range 6 circle
    SArborealStorm = 31087, // SBoss->self, 5.0s cast, range 12 circle
    SAcornBomb = 31088, // SBoss->location, 3.0s cast, range 6 circle
    SGelidGale = 31089, // SOdqan->location, 3.0s cast, range 6 circle
    SUproot = 31090, // SOdqan->self, 3.0s cast, range 6 circle
}

class ArborealStorm(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCircle(12));
class NArborealStorm(BossModule module) : ArborealStorm(module, AID.NArborealStorm);
class SArborealStorm(BossModule module) : ArborealStorm(module, AID.SArborealStorm);

class AcornBomb(BossModule module, AID aid) : Components.StandardAOEs(module, aid, 6);
class NAcornBomb(BossModule module) : AcornBomb(module, AID.NAcornBomb);
class SAcornBomb(BossModule module) : AcornBomb(module, AID.SAcornBomb);

class GelidGale(BossModule module, AID aid) : Components.StandardAOEs(module, aid, 6);
class NGelidGale(BossModule module) : GelidGale(module, AID.NGelidGale);
class SGelidGale(BossModule module) : GelidGale(module, AID.SGelidGale);

class Uproot(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCircle(6));
class NUproot(BossModule module) : Uproot(module, AID.NUproot);
class SUproot(BossModule module) : Uproot(module, AID.SUproot);

class C010DryadStates : StateMachineBuilder
{
    public C010DryadStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NArborealStorm>(!savage)
            .ActivateOnEnter<NAcornBomb>(!savage)
            .ActivateOnEnter<NGelidGale>(!savage)
            .ActivateOnEnter<NUproot>(!savage)
            .ActivateOnEnter<SArborealStorm>(savage)
            .ActivateOnEnter<SAcornBomb>(savage)
            .ActivateOnEnter<SGelidGale>(savage)
            .ActivateOnEnter<SUproot>(savage)
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(savage ? OID.SOdqan : OID.NOdqan).All(a => a.IsDead);
    }
}
class C010NDryadStates(BossModule module) : C010DryadStates(module, false);
class C010SDryadStates(BossModule module) : C010DryadStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11513, SortOrder = 4)]
public class C010NDryad(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.NOdqan), ArenaColor.Enemy);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11513, SortOrder = 4)]
public class C010SDryad(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SOdqan), ArenaColor.Enemy);
    }
}
