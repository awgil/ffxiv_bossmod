namespace BossMod.Modules.Heavensward.DeepDungeon.PalaceOfTheDead.D150Tisiphone;

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

class BloodRain(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BloodRain), "Heavy Raidwide damage! Also killing any add that is currently up");
class BossAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.FanaticZombie, (uint)OID.FanaticSuccubus]);
class DarkMist(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkMist), new AOEShapeCircle(10));
class Desolation(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Desolation), new AOEShapeRect(57.3f, 3));
class FatalAllure(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.FatalAllure), "Boss is life stealing from the succubus");
class SweetSteel(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SweetSteel), new AOEShapeCone(7, 45.Degrees()));
class TerrorEye(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TerrorEye), 6);
class VoidFireII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VoidFireII), 5);
class VoidFireIV(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VoidFireIV), 10);
class ZombieGrab(BossModule module) : Components.PersistentVoidzone(module, 2, m => m.Enemies(OID.FanaticZombie)); // Future note to Ice(self): Not entirely sure if I'm happy with this per se? It shows to essentially stay away from the zombies but, maybe a better hint when I can think of one

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
            .ActivateOnEnter<VoidFireII>()
            .ActivateOnEnter<VoidFireIV>()
            .ActivateOnEnter<ZombieGrab>()
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
