namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D100NybethObdilord;

public enum OID : uint
{
    Boss = 0x1808, // R2.400, x1
    BicephalicCorse = 0x180A, // R1.900, x0 (spawn during fight)
    GiantCorse = 0x1809, // R1.900, x0 (spawn during fight)
    IronCorse = 0x180B, // R1.900, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss/BicephalicCorse/GiantCorse/IronCorse->player, no cast, single-target

    Abyss = 6872, // Boss->player, 2.0s cast, range 6 circle // kinda like a tankbuster? It's a circle on the player
    ButterflyFloat = 6879, // IronCorse->player, 3.0s cast, single-target
    Catapult = 6878, // BicephalicCorse->location, 3.0s cast, range 6 circle
    Doom = 6875, // Boss->self, 5.0s cast, range 45+R 120-degree cone, feels like this is wrong,
    GlassPunch = 6877, // GiantCorse/BicephalicCorse->self, no cast, range 6+R ?-degree cone
    Shackle = 6874, // Boss->self, 3.0s cast, range 50+R width 8 rect
    SummonDarkness = 6876, // Boss->self, 3.0s cast, ???, Summons Corse's,
    WordOfPain = 6873, // Boss->self, no cast, range 40+R circle
}

class Abyss(BossModule module) : Components.BaitAwayCast(module, AID.Abyss, new AOEShapeCircle(6), true);
class Catapult(BossModule module) : Components.StandardAOEs(module, AID.Catapult, 6);
class CorseAdds(BossModule module) : Components.AddsMulti(module, [OID.BicephalicCorse, OID.GiantCorse, OID.IronCorse]);
class Doom(BossModule module) : Components.StandardAOEs(module, AID.Doom, new AOEShapeCone(47.4f, 60.Degrees()));
class Shackle(BossModule module) : Components.StandardAOEs(module, AID.Shackle, new AOEShapeRect(52.4f, 4));
class SummonDarkness(BossModule module) : Components.CastHint(module, AID.SummonDarkness, "Summoning the corse, incoming Adds! \nRemember to use a resolution to make them permanently disappear");

class EncounterHints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"There is 3 sets of adds that spawn at HP %'s -> (90%, 65%, 40%) \nA resolution can make the adds permanently disappear once they are at 0% HP/the corpse are just laying on the floor.\nResolution is also does high damage to the adds + 0.3% to the Boss\nSolo tip: Either pop a resolution on all add packs, or pop lust -> resolution on 2nd ad pack. Make sure to keep regen up!");
    }
}

class D100NybethObdilordStates : StateMachineBuilder
{
    public D100NybethObdilordStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Abyss>()
            .ActivateOnEnter<Catapult>()
            .ActivateOnEnter<CorseAdds>()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<Shackle>()
            .ActivateOnEnter<SummonDarkness>()
            .DeactivateOnEnter<EncounterHints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 208, NameID = 5356)]
public class D100NybethObdilord : BossModule
{
    public D100NybethObdilord(WorldState ws, Actor primary) : base(ws, primary, new(300, 300), new ArenaBoundsCircle(24))
    {
        ActivateComponent<EncounterHints>();
    }
}
