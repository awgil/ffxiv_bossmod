namespace BossMod.Global.MaskedCarnivale.Stage25.Act3;

public enum OID : uint
{
    Boss = 0x2680, //R=1.2
    Maelstrom = 0x2681, //R=0.6
    Helper = 0x233C,
    LavaVoidzone = 0x1EA8BB,
};

public enum AID : uint
{
    RepellingSpray = 14768, // 2680->self, 2,0s cast, single-target, boss reflect magic attacks
    IceSpikes = 14762, // 2680->self, 2,0s cast, single-target, boss reflects physical attacks
    ApocalypticBolt = 14766, // 2680->self, 3,0s cast, range 50+R width 8 rect
    TheRamsVoice = 14763, // 2680->self, 3,5s cast, range 8 circle
    TheDragonsVoice = 14764, // 2680->self, 3,5s cast, range 6-30 donut
    ApocalypticRoar = 14767, // 2680->self, 5,0s cast, range 35+R 120-degree cone
    Charybdis = 14772, // 2680->self, 3,0s cast, single-target
    Charybdis2 = 14773, // 233C->self, 4,0s cast, range 8 circle
    Maelstrom = 14780, // 2681->self, 1,0s cast, range 8 circle
    Web = 14770, // 2680->player, 3,0s cast, single-target
    Meteor = 14771, // 2680->location, 7,0s cast, range 15 circle
    Plaincracker = 14765, // 2680->self, 3,5s cast, range 6+R circle
    TremblingEarth = 14774, // 233C->self, 3,5s cast, range 10-20 donut
    TremblingEarth2 = 14775, // 233C->self, 3,5s cast, range 20-30 donut
};

public enum SID : uint
{
    RepellingSpray = 556, // Boss->Boss, extra=0x64
    IceSpikes = 1307, // Boss->Boss, extra=0x64
    Doom = 910, // Boss->player, extra=0x0
};

class Charybdis : Components.SelfTargetedAOEs
{
    public Charybdis() : base(ActionID.MakeSpell(AID.Charybdis2), new AOEShapeCircle(8)) { }
}

class Web : BossComponent
{
    private bool casting;
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Web)
            casting = true;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Web)
            casting = false;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (casting)
            hints.Add("Bait the Meteor to the edge of the arena!\nUse Loom to escape or Diamondback to survive.");
    }
}

class Plaincracker : Components.SelfTargetedAOEs
{
    public Plaincracker() : base(ActionID.MakeSpell(AID.Plaincracker), new AOEShapeCircle(7.2f)) { }
}

class TremblingEarth : Components.SelfTargetedAOEs
{
    public TremblingEarth() : base(ActionID.MakeSpell(AID.TremblingEarth), new AOEShapeDonut(10, 20)) { }
}

class TremblingEarth2 : Components.SelfTargetedAOEs
{
    public TremblingEarth2() : base(ActionID.MakeSpell(AID.TremblingEarth2), new AOEShapeDonut(20, 30)) { }
}

class ApocalypticBolt : Components.SelfTargetedAOEs
{
    public ApocalypticBolt() : base(ActionID.MakeSpell(AID.ApocalypticBolt), new AOEShapeRect(51.2f, 4)) { }
}

class ApocalypticRoar : Components.SelfTargetedAOEs
{
    public ApocalypticRoar() : base(ActionID.MakeSpell(AID.ApocalypticRoar), new AOEShapeCone(36.2f, 60.Degrees())) { }
}

class TheRamsVoice : Components.SelfTargetedAOEs
{
    public TheRamsVoice() : base(ActionID.MakeSpell(AID.TheRamsVoice), new AOEShapeCircle(8)) { }
}

class TheDragonsVoice : Components.SelfTargetedAOEs
{
    public TheDragonsVoice() : base(ActionID.MakeSpell(AID.TheDragonsVoice), new AOEShapeDonut(6, 30)) { }
}

class Maelstrom : Components.PersistentVoidzone
{
    public Maelstrom() : base(8, m => m.Enemies(OID.Maelstrom)) { }
}

class Meteor : Components.LocationTargetedAOEs
{
    public Meteor() : base(ActionID.MakeSpell(AID.Meteor), 15) { }
}

class MeteorVoidzone : Components.PersistentVoidzoneAtCastTarget
{
    public MeteorVoidzone() : base(10, ActionID.MakeSpell(AID.Meteor), m => m.Enemies(OID.LavaVoidzone).Where(z => z.EventState != 7), 0) { }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"In this act {module.PrimaryActor.Name} will switch between magic and physical reflects.\nSpend attention to that so you don't accidently kill yourself.\nAs soon as he starts casting Web go to the edge to bait Meteor, then use Loom\nto escape. You can start the Final Sting combination at about 50% health left.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Hints2 : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        var magicreflect = module.Enemies(OID.Boss).Where(x => x.FindStatus(SID.RepellingSpray) != null).FirstOrDefault();
        var physicalreflect = module.Enemies(OID.Boss).Where(x => x.FindStatus(SID.IceSpikes) != null).FirstOrDefault();
        if (magicreflect != null)
            hints.Add($"{module.PrimaryActor.Name} will reflect all magic damage!");
        if (physicalreflect != null)
            hints.Add($"{module.PrimaryActor.Name} will reflect all physical damage!");
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var doomed = actor.FindStatus(SID.Doom);
        if (doomed != null)
            hints.Add("You were doomed! Cleanse it with Exuviation or finish the act fast.");
    }
}

class Stage25Act3States : StateMachineBuilder
{
    public Stage25Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ApocalypticBolt>()
            .ActivateOnEnter<ApocalypticRoar>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<Plaincracker>()
            .ActivateOnEnter<TremblingEarth>()
            .ActivateOnEnter<TremblingEarth2>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<MeteorVoidzone>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<Web>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 635, NameID = 8129, SortOrder = 3)]
public class Stage25Act3 : BossModule
{
    public Stage25Act3(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }
}
