namespace BossMod.Global.MaskedCarnivale.Stage21.Act2;

public enum OID : uint
{
    Boss = 0x2730, //R=3.7
    ArenaImp = 0x2731, //R=0.45
    Voidzone = 0x1E972A,
};

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Blizzard = 14267, // Imp->player, 1,0s cast, single-target
    VoidBlizzard = 15063, // Imp->player, 6,0s cast, single-target
    Icefall = 15064, // Imp->location, 2,5s cast, range 5 circle
    TheRamsVoice = 15079, // Boss->self, 4,0s cast, range 9 circle
    TheDragonsVoice = 15080, // Boss->self, 4,0s cast, range 8-30 donut
    TheRamsKeeper = 15081, // Boss->location, 6,0s cast, range 9 circle

};

class TheRamsKeeper : Components.PersistentVoidzoneAtCastTarget
{
    public TheRamsKeeper() : base(9, ActionID.MakeSpell(AID.TheRamsKeeper), m => m.Enemies(OID.Voidzone), 0) { }
}

class TheRamsKeeperHint : Components.CastHint
{
    public TheRamsKeeperHint() : base(ActionID.MakeSpell(AID.TheRamsKeeper), "Interrupt") { }
}

class TheRamsVoice : Components.SelfTargetedAOEs
{
    public TheRamsVoice() : base(ActionID.MakeSpell(AID.TheRamsVoice), new AOEShapeCircle(9)) { }
}

class TheDragonsVoice : Components.SelfTargetedAOEs
{
    public TheDragonsVoice() : base(ActionID.MakeSpell(AID.TheDragonsVoice), new AOEShapeDonut(8, 30)) { }
}

class Icefall : Components.LocationTargetedAOEs
{
    public Icefall() : base(ActionID.MakeSpell(AID.Icefall), 5) { }
}

class VoidBlizzard : Components.CastHint
{
    public VoidBlizzard() : base(ActionID.MakeSpell(AID.VoidBlizzard), "Interrupt") { }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add("Interrupt The Rams Keeper with Spitting Sardine. You can start the\nFinal Sting combination at about 50% health left.\n(Off-guard->Bristle->Moonflute->Final Sting).\nThe boss will sometimes spawn an Arena Imp during the fight.");
    }
}

class Hints2 : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (!module.Enemies(OID.ArenaImp).All(e => e.IsDead))
            hints.Add("The imps are weak to fire spells and strong against ice.\nInterrupt Void Blizzard with Spitting Sardine.");
    }
}

class Stage21Act2States : StateMachineBuilder
{
    public Stage21Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidBlizzard>()
            .ActivateOnEnter<Icefall>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheRamsKeeper>()
            .ActivateOnEnter<TheRamsKeeperHint>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 631, NameID = 8121, SortOrder = 2)]
public class Stage21Act2 : BossModule
{
    public Stage21Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ArenaImp))
            Arena.Actor(s, ArenaColor.Object);
    }
}
