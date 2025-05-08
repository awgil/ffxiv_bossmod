namespace BossMod.Global.MaskedCarnivale.Stage19.Act1;

public enum OID : uint
{
    Boss = 0x2727, //R=5.775
    voidzone = 0x1EA9F9, //R=0.5
}

public enum AID : uint
{
    Reflect = 15073, // 2727->self, 3.0s cast, single-target, boss starts reflecting all melee attacks
    AutoAttack = 6499, // 2727->player, no cast, single-target
    BadBreath = 15074, // 2727->self, 3.5s cast, range 12+R 120-degree cone
    VineProbe = 15075, // 2727->self, 2.5s cast, range 6+R width 8 rect
    OffalBreath = 15076, // 2727->location, 3.5s cast, range 6 circle, interruptible, voidzone
}

public enum SID : uint
{
    Reflect = 518, // Boss->Boss, extra=0x0
    Paralysis = 17, // Boss->player, extra=0x0
    Silence = 7, // Boss->player, extra=0x0
    Blind = 15, // Boss->player, extra=0x0
    Slow = 9, // Boss->player, extra=0x0
    Heavy = 14, // Boss->player, extra=0x32
    Nausea = 2388, // Boss->player, extra=0x0
    Poison = 18, // Boss->player, extra=0x0
    Leaden = 67, // none->player, extra=0x3C
    Pollen = 19, // none->player, extra=0x0
    Stun = 149, // 2729->player, extra=0x0
}

class BadBreath(BossModule module) : Components.StandardAOEs(module, AID.BadBreath, new AOEShapeCone(17.775f, 60.Degrees()));
class VineProbe(BossModule module) : Components.StandardAOEs(module, AID.VineProbe, new AOEShapeRect(11.775f, 4));
class OffalBreath(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.OffalBreath, m => m.Enemies(OID.voidzone), 0);

class Reflect(BossModule module) : BossComponent(module)
{
    private bool reflect;
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Reflect)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Reflect)
        {
            reflect = true;
            casting = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add("Boss will reflect all magic damage!");
        if (reflect)
            hints.Add("Boss reflects all magic damage!");//TODO: could use an AI hint to never use magic abilities after this is casted
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("At the start of the fight Rebekkah will cast Reflect. This will reflect all\nmagic damage back to you. Useful skills: Sharpened Knife,\nFlying Sardine, Ink Jet (Act 2), Exuviation (Act 2), potentially a Final Sting\ncombo. (Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage19Act1States : StateMachineBuilder
{
    public Stage19Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Reflect>()
            .ActivateOnEnter<BadBreath>()
            .ActivateOnEnter<VineProbe>()
            .ActivateOnEnter<OffalBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 629, NameID = 8117, SortOrder = 1)]
public class Stage19Act1 : BossModule
{
    public Stage19Act1(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }
}
