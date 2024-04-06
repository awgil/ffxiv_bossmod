namespace BossMod.Global.MaskedCarnivale.Stage03;

public enum OID : uint
{
    Boss = 0x25D4, //R=2.2
    voidzone = 0x1E8FEA,
};

public enum AID : uint
{
    AutoAttack = 6499, // 25D4->player, no cast, single-target
    BoulderClap = 14363, // 25D4->self, 3,0s cast, range 14 120-degree cone
    EarthenHeart = 14364, // 25D4->location, 3,0s cast, range 6 circle
    Obliterate = 14365, // 25D4->self, 6,0s cast, range 60 circle
};

class BoulderClap : Components.SelfTargetedAOEs
{
    public BoulderClap() : base(ActionID.MakeSpell(AID.BoulderClap), new AOEShapeCone(14, 60.Degrees())) { }
}

class Dreadstorm : Components.PersistentVoidzoneAtCastTarget
{
    public Dreadstorm() : base(6, ActionID.MakeSpell(AID.EarthenHeart), m => m.Enemies(OID.voidzone), 0) { }
}

class Obliterate : Components.RaidwideCast
{
    public Obliterate() : base(ActionID.MakeSpell(AID.Obliterate), "Interruptible raidwide") { }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add("Zipacna is weak against water based spells.\nFlying Sardine is recommended to interrupt raidwide.");
    }
}

class Hints2 : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add("Zipacna is weak against water based spells.\nEarth based spells are useless against Zipacna.");
    }
}

class Stage03States : StateMachineBuilder
{
    public Stage03States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BoulderClap>()
            .ActivateOnEnter<Dreadstorm>()
            .ActivateOnEnter<Obliterate>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 613, NameID = 8084)]
public class Stage03 : BossModule
{
    public Stage03(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
    {
        ActivateComponent<Hints>();
    }
}
