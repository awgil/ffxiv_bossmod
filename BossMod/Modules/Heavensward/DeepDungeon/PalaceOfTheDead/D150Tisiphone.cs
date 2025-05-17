namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D150Tisiphone;

public enum OID : uint
{
    Boss = 0x181C, // R2.000, x1
    FanaticGargoyle = 0x18EB, // R2.300, x0 (spawn during fight)
    FanaticSuccubus = 0x18EE, // R1.000, x0 (spawn during fight)
    FanaticVodoriga = 0x18EC, // R1.200, x0 (spawn during fight)
    FanaticZombie = 0x18ED, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss/FanaticSuccubus->player, no cast, single-target
    BloodRain = 7153, // Boss->location, 5.0s cast, range 100 circle
    BloodSword = 7111, // Boss->FanaticSuccubus, no cast, single-target
    DarkMist = 7108, // Boss->self, 3.0s cast, range 8+R circle
    Desolation = 7112, // FanaticGargoyle->self, 4.0s cast, range 55+R(57.3) width 6 rect
    FatalAllure = 7110, // Boss->FanaticSuccubus, 2.0s cast, single-target, sucks the HP that remained off the FanaticSuccubus and transfers it to boss
    SummonDarkness = 7107, // Boss->self, no cast, single-target
    SweetSteel = 7148, // FanaticSuccubus->self, no cast, range 6+R(7) 90?-degree cone, currently a safe bet on the cone angel, needs to be confirmed
    TerrorEye = 7113, // FanaticVodoriga->location, 4.0s cast, range 6 circle
    VoidAero = 7177, // Boss->self, 3.0s cast, range 40+R(42.3) width 8 rect
    VoidFireII = 7150, // FanaticSuccubus->location, 3.0s cast, range 5 circle
    VoidFireIV = 7109, // Boss->location, 3.5s cast, range 10 circle
}

class BloodRain(BossModule module) : Components.RaidwideCast(module, AID.BloodRain, "Heavy Raidwide damage! Also killing any add that is currently up");
class BossAdds(BossModule module) : Components.AddsMulti(module, [OID.FanaticZombie, OID.FanaticSuccubus])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.Class.GetRole() is Role.Ranged or Role.Healer)
        {
            // ignore all adds, just attack boss
            hints.PrioritizeTargetsByOID(OID.Boss, 5);
            foreach (var zombie in Module.Enemies(OID.FanaticZombie))
            {
                hints.AddForbiddenZone(new AOEShapeCircle(3), zombie.Position);
                hints.AddForbiddenZone(new AOEShapeCircle(8), zombie.Position, activation: WorldState.FutureTime(5));
            }
        }
        else
        {
            // kill zombies first, they have low health
            hints.PrioritizeTargetsByOID(OID.FanaticZombie, 5);
            // attack boss, ignore succubus
            hints.PrioritizeTargetsByOID(OID.Boss, 1);
        }
    }
}
class DarkMist(BossModule module) : Components.StandardAOEs(module, AID.DarkMist, new AOEShapeCircle(10));
class Desolation(BossModule module) : Components.StandardAOEs(module, AID.Desolation, new AOEShapeRect(57.3f, 3));
class FatalAllure(BossModule module) : Components.SingleTargetCast(module, AID.FatalAllure, "Boss is life stealing from the succubus");
class SweetSteel(BossModule module) : Components.StandardAOEs(module, AID.SweetSteel, new AOEShapeCone(7, 45.Degrees()));
class TerrorEye(BossModule module) : Components.StandardAOEs(module, AID.TerrorEye, 6);
class VoidAero(BossModule module) : Components.StandardAOEs(module, AID.VoidAero, new AOEShapeRect(42.3f, 4));
class VoidFireII(BossModule module) : Components.StandardAOEs(module, AID.VoidFireII, 5);
class VoidFireIV(BossModule module) : Components.StandardAOEs(module, AID.VoidFireIV, 10);

class EncounterHints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will spawn 4 zombies, you can either kite them or kill them. The BloodRain raidwide will also kill them if they're still alive. \nThe boss will also life-steal however much HP is left of the Succubus, you're choice if you want to kill it or not.");
    }
}

class D150TisiphoneStates : StateMachineBuilder
{
    public D150TisiphoneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BloodRain>()
            .ActivateOnEnter<BossAdds>()
            .ActivateOnEnter<DarkMist>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<FatalAllure>()
            .ActivateOnEnter<SweetSteel>()
            .ActivateOnEnter<TerrorEye>()
            .ActivateOnEnter<VoidAero>()
            .ActivateOnEnter<VoidFireII>()
            .ActivateOnEnter<VoidFireIV>()
            .DeactivateOnEnter<EncounterHints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 213, NameID = 5424)]
//public class D150Tisiphone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -237.17f), new ArenaBoundsCircle(24));
public class D150Tisiphone : BossModule
{
    public D150Tisiphone(WorldState ws, Actor primary) : base(ws, primary, new(-300, -237.17f), new ArenaBoundsCircle(24))
    {
        ActivateComponent<EncounterHints>();
    }
}
