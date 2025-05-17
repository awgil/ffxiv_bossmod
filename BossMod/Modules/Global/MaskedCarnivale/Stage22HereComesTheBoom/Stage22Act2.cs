namespace BossMod.Global.MaskedCarnivale.Stage22.Act2;

public enum OID : uint
{
    Boss = 0x26FE, //R=3.75
    ArenaGrenade = 0x26FC, //R=1.2
    ArenaGasBomb = 0x26FD, //R=1.2
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    ScaldingScolding = 14903, // Boss->self, 4.0s cast, range 8+R 120-degree cone
    Sap = 14906, // Boss->location, 3.0s cast, range 8 circle
    Sap2 = 14907, // 233C->location, 7.0s cast, range 8 circle
    BombshellDrop = 14905, // Boss->self, 2.0s cast, single-target
    Ignition = 15040, // Boss->self, 4.0s cast, range 50 circle
    Fulmination = 14901, // ArenaGrenade->self, no cast, range 50+R circle, wipe if failed to kill grenade in one hit or boss finishes casting Ignition when grenade is still alive
    Flashthoom = 14902, // ArenaGasBomb->self, 6.0s cast, range 6+R circle
    Burst = 14904, // Boss->self, 20.0s cast, range 50 circle
}

class Sap(BossModule module) : Components.StandardAOEs(module, AID.Sap, 8);
class Sap2(BossModule module) : Components.StandardAOEs(module, AID.Sap2, 8);
class ScaldingScolding(BossModule module) : Components.StandardAOEs(module, AID.ScaldingScolding, new AOEShapeCone(11.75f, 60.Degrees()));
class Flashthoom(BossModule module) : Components.StandardAOEs(module, AID.Flashthoom, new AOEShapeCircle(7.2f));
class Ignition(BossModule module) : Components.RaidwideCast(module, AID.Ignition, "Wipe if Grenade is not killed yet, otherwise Raidwide");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.Enemies(OID.Boss).FirstOrDefault()!.Name} spawns grenades and gas bombs during the fight. Just as in\nact 1 the grenades must be killed in one hit each or they will wipe you.\nUse Sticky Tongue to pull Gas Bombs to the boss so they interrupt the enrage.\nYou can start the Final Sting combination at about 50% health left.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.Enemies(OID.ArenaGrenade).All(e => e.IsDead))
            hints.Add($"Kill the {Module.Enemies(OID.ArenaGrenade).FirstOrDefault()!.Name} in one hit or it will wipe you. It got 543 HP.");
        if (!Module.Enemies(OID.ArenaGasBomb).All(e => e.IsDead))
            hints.Add($"Use Sticky Tongue to pull the {Module.Enemies(OID.ArenaGasBomb).FirstOrDefault()!.Name} to the bos\nto interrupt the enrage!");
    }
}

class Stage22Act2States : StateMachineBuilder
{
    public Stage22Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sap>()
            .ActivateOnEnter<Sap2>()
            .ActivateOnEnter<Ignition>()
            .ActivateOnEnter<ScaldingScolding>()
            .ActivateOnEnter<Flashthoom>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 632, NameID = 8123, SortOrder = 2)]
public class Stage22Act2 : BossModule
{
    public Stage22Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ArenaGrenade))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.ArenaGasBomb))
            Arena.Actor(s, ArenaColor.Object);
    }
}
