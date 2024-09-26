namespace BossMod.Heavensward.DeepDungeon.PalaceoftheDead.D100NybethObdilord;

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

class Abyss(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Abyss), new AOEShapeCircle(6), true);
class Catapult(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Catapult), 6);
class CorseAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.BicephalicCorse, (uint)OID.GiantCorse, (uint)OID.IronCorse]);
class Doom(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Doom), new AOEShapeCone(47.4f, 60.Degrees()));
class Shackle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shackle), new AOEShapeRect(52.4f, 4, 0));
class SummonDarkness(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SummonDarkness), "Summoning the corses, use Resolution if you want them permanently dead");

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
            .ActivateOnEnter<SummonDarkness>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 208, NameID = 5356)]
public class D100NybethObdilord(WorldState ws, Actor primary) : BossModule(ws, primary, new(300, 300), new ArenaBoundsCircle(24));
