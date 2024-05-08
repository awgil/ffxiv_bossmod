namespace BossMod.Global.MaskedCarnivale.Stage20.Act2;

public enum OID : uint
{
    Boss = 0x272B, //R=5.1
    Helper = 0x233C, //R=0.5
}

public enum AID : uint
{
    AquaBreath = 14713, // 272B->self, 2.5s cast, range 8+R 90-degree cone
    Megavolt = 14714, // 272B->self, 3.0s cast, range 6+R circle
    ImpSong = 14712, // 272B->self, 6.0s cast, range 50+R circle
    Waterspout = 14718, // 233C->location, 2.5s cast, range 4 circle
    LightningBolt = 14717, // 233C->location, 3.0s cast, range 3 circle
}

class AquaBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AquaBreath), new AOEShapeCone(13.1f, 45.Degrees()));
class Megavolt(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Megavolt), new AOEShapeCircle(11.1f));
class Waterspout(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Waterspout), 4);
class LightningBolt(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LightningBolt), 3);
class ImpSong(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.ImpSong), "Interrupt Ultros!");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Ultros is weak to fire. Interrupt Imp Song.");
    }
}

class Stage20Act2States : StateMachineBuilder
{
    public Stage20Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
        .DeactivateOnEnter<Hints>()
        .ActivateOnEnter<LightningBolt>()
        .ActivateOnEnter<Waterspout>()
        .ActivateOnEnter<Megavolt>()
        .ActivateOnEnter<AquaBreath>()
        .ActivateOnEnter<LightningBolt>()
        .ActivateOnEnter<ImpSong>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 630, NameID = 7111, SortOrder = 2)]
public class Stage20Act2 : BossModule
{
    public Stage20Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }
}
