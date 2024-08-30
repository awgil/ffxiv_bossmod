namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D50Gozu;

public enum OID : uint
{
    Boss = 0x23E9, // R3.450, x1
    Gloom = 0x23EA, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    EyeOfTheFire = 11922, // Boss->self, 3.0s cast, range 40 circle, gaze mechanic
    RustingClaw = 11919, // Boss->self, 3.5s cast, range 8+R ?-degree cone // untelegraph'd, 90 Degrees maybe? Needs to have testing done... which is annoying
    TheSpin = 11921, // Boss->self, 7.5s cast, range 40+R circle // Minimum damage falloff point is 20 yalms
    UnknownSkill = 11923, // Boss->self, no cast, single-target // was classified as a weapon skill, no other name attached
    VoidSpark = 11924, // Gloom->self, 2.0s cast, range 7+R circle, need to make as a persistant void zone while these things are visible
    WordsOfWoe = 11920, // Boss->self, 3.0s cast, range 45+R width 6 rect // untelegraph'd
}

class EyeoftheFire(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFire));
class RustingClaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RustingClaw), new AOEShapeCone(11.45f, 45.Degrees()));
class TheSpin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheSpin), new AOEShapeCircle(20));
class VoidSpark(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.Gloom).Where(z => z.EventState != 7));
class WordsofWoe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WordsOfWoe), new AOEShapeRect(48.45f, 3));

class D50GozuStates : StateMachineBuilder
{
    public D50GozuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EyeoftheFire>()
            .ActivateOnEnter<RustingClaw>()
            .ActivateOnEnter<TheSpin>()
            .ActivateOnEnter<VoidSpark>()
            .ActivateOnEnter<WordsofWoe>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 544, NameID = 7485)]
public class D50Gozu(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(23.7f));
