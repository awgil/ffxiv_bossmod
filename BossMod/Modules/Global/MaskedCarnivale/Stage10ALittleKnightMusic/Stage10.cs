namespace BossMod.Global.MaskedCarnivale.Stage10;

public enum OID : uint
{
    Boss = 0x2717, // R1.0/1.5/2.0/2.5 (radius increases with amount of successful King's Will casts)
}

public enum AID : uint
{
    IronJustice1 = 14725, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Cloudcover1 = 14722, // Boss->location, 3.0s cast, range 6 circle
    KingsWill = 14719, // Boss->self, 6.0s cast, single-target, interruptible buff
    IronJustice2 = 14726, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    KingsWill2 = 14720, // Boss->self, 6.0s cast, single-target, interruptible buff
    IronJustice3 = 14727, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    KingsWill3 = 14721, // Boss->self, 6.0s cast, single-target, interruptible buff
    IronJustice4 = 14728, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    Cloudcover2 = 14723, // Boss->player, no cast, range 6 circle
    BlackNebula = 14724, // Boss->self, 6.0s cast, range 50+R circle, interruptible enrage after 3 King's Will casts
}

class IronJustice1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IronJustice1), new AOEShapeCone(9, 60.Degrees()));
class IronJustice2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IronJustice2), new AOEShapeCone(9.5f, 60.Degrees()));
class IronJustice3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IronJustice3), new AOEShapeCone(10, 60.Degrees()));
class IronJustice4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IronJustice4), new AOEShapeCone(10.5f, 60.Degrees()));
class BlackNebula(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.BlackNebula), "Interrupt or wipe!");
class Cloudcover1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Cloudcover1), 6);
class KingsWill1(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.KingsWill), "Interrupt if not going for the achievement");
class KingsWill2(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.KingsWill2), "Interrupt if not going for the achievement");
class KingsWill3(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.KingsWill3), "Interrupt if not going for the achievement");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Crom Dubh will cast King's Will during the fight. Interrupt it with\nFlying Sardine or he will become stronger each time. After 3 casts he\nstarts using the interruptible enrage Black Nebula.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirement for achievement: Let Crom Dubh cast King's Will 3 times.", false);
    }
}

class Stage10States : StateMachineBuilder
{
    public Stage10States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronJustice1>()
            .ActivateOnEnter<IronJustice2>()
            .ActivateOnEnter<IronJustice3>()
            .ActivateOnEnter<IronJustice4>()
            .ActivateOnEnter<Cloudcover1>()
            .ActivateOnEnter<BlackNebula>()
            .ActivateOnEnter<KingsWill1>()
            .ActivateOnEnter<KingsWill2>()
            .ActivateOnEnter<KingsWill3>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 620, NameID = 8100)]
public class Stage10 : BossModule
{
    public Stage10(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
    }
}
