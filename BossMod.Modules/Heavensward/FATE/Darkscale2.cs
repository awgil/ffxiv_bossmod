namespace BossMod.Heavensward.FATE.Darkscale2;

public enum OID : uint
{
    Thuban = 0x12F2, // R3.740, x1
    Rastaban = 0x12F3, // R3.850, x1
    Etamin = 0x12F4, // R3.200, x1
    Helper = 0x233C,

    Vedrfolnir = 0x12F5,
}

public enum AID : uint
{
    AutoAttack = 870, // Thuban/Rastaban/Etamin->Vedrfolnir, no cast, single-target
    OriginalSin = 4702, // Thuban->Vedrfolnir, no cast, single-target
    TripleHelix = 4553, // Rastaban->self, 3.0s cast, range 6+R circle
    RipperClaw = 5137, // Etamin->self, 3.0s cast, range 5+R ?-degree cone
    ParadiseLost = 4522, // Thuban->self, 3.0s cast, range 6+R 90-degree cone
    Headwind = 4554, // Rastaban->self, 3.0s cast, range 6+R 90-degree cone
    Fireball = 5138, // Etamin->location, 4.0s cast, range 6 circle
}

class TripleHelix(BossModule module) : Components.StandardAOEs(module, AID.TripleHelix, 9.85f);
class RipperClaw(BossModule module) : Components.StandardAOEs(module, AID.RipperClaw, new AOEShapeCone(8.2f, 60.Degrees())); // guessing angle
class ParadiseLost(BossModule module) : Components.StandardAOEs(module, AID.ParadiseLost, new AOEShapeCone(9.74f, 45.Degrees()));
class Headwind(BossModule module) : Components.StandardAOEs(module, AID.Headwind, new AOEShapeCone(9.85f, 45.Degrees()));
class Fireball(BossModule module) : Components.StandardAOEs(module, AID.Fireball, 6);

class Darkscale2States : StateMachineBuilder
{
    public Darkscale2States(BossModule module) : base(module)
    {
        var th = (Darkscale2)module;

        TrivialPhase()
            .ActivateOnEnter<TripleHelix>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<ParadiseLost>()
            .ActivateOnEnter<Headwind>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<Darkscale1.DragonBabysitter>()
            .Raw.Update = () => th.PrimaryActor.IsDeadOrDestroyed && th.Boss2 is { IsDeadOrDestroyed: true } && th.Boss3 is { IsDeadOrDestroyed: true };
    }
}

[ModuleInfo(PrimaryActorOID = (uint)OID.Etamin, GroupType = BossModuleInfo.GroupType.Fate, GroupID = 869, NameID = 3982)]
public class Darkscale2(ModuleInit init) : BossModule(init, new(-202.19f, -504.73f), new ArenaBoundsCircle(25))
{
    public Actor? Boss2;
    public Actor? Boss3;

    protected override void UpdateModule()
    {
        Boss2 ??= Enemies(OID.Rastaban).FirstOrDefault();
        Boss3 ??= Enemies(OID.Thuban).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(Boss2, ArenaColor.Enemy);
        Arena.Actor(Boss3, ArenaColor.Enemy);

        Arena.Actors(Enemies(OID.Vedrfolnir), ArenaColor.Object);
    }
}

