namespace BossMod.Global.MaskedCarnivale.Stage25.Act1;

public enum OID : uint
{
    Boss = 0x2678, //R=1.2
    Helper = 0x233C,
};

public enum AID : uint
{
    IceSpikes = 14762, // 2678->self, 2,0s cast, single-target, boss reflects all physical damage
    ApocalypticBolt = 14766, // 2678->self, 3,0s cast, range 50+R width 8 rect
    TheRamsVoice = 14763, // 2678->self, 3,5s cast, range 8 circle
    TheDragonsVoice = 14764, // 2678->self, 3,5s cast, range 6-30 donut
    Plaincracker = 14765, // 2678->self, 3,5s cast, range 6+R circle
    TremblingEarth = 14774, // 233C->self, 3,5s cast, range 10-20 donut
    TremblingEarth2 = 14775, // 233C->self, 3,5s cast, range 20-30 donut
    ApocalypticRoar = 14767, // 2678->self, 5,0s cast, range 35+R 120-degree cone
};

public enum SID : uint
{
    IceSpikes = 1307, // Boss->Boss, extra=0x64
    Doom = 910, // Boss->player, extra=0x0
};

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

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"{module.PrimaryActor.Name} will reflect all physical damage in act 1, all magic damage in act 2\nand switch between both in act 3. Loom, Exuviation and Diamondback\nare recommended. In act 3 can start the Final Sting combination\nat about 50% health left. (Off-guard->Bristle->Moonflute->Final Sting)");
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        hints.Add("Requirements for achievement: Take no damage, use all 6 magic elements,\nuse all 3 melee types and finish faster than ideal time", false);
    }
}

class Hints2 : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        var physicalreflect = module.Enemies(OID.Boss).Where(x => x.FindStatus(SID.IceSpikes) != null).FirstOrDefault();
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

class Stage25Act1States : StateMachineBuilder
{
    public Stage25Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ApocalypticBolt>()
            .ActivateOnEnter<ApocalypticRoar>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<Plaincracker>()
            .ActivateOnEnter<TremblingEarth>()
            .ActivateOnEnter<TremblingEarth2>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 635, NameID = 8129, SortOrder = 1)]
public class Stage25Act1 : BossModule
{
    public Stage25Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
    {
        ActivateComponent<Hints>();
    }
}
