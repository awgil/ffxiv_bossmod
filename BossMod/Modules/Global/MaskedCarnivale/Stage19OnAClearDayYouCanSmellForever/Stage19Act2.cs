namespace BossMod.Global.MaskedCarnivale.Stage19.Act2;

public enum OID : uint
{
    Boss = 0x2728, //R=5.775
    HotHip = 0x2779, //R=1.50
    voidzone = 0x1EA9F9, //R=0.5
}

public enum AID : uint
{
    Reflect = 15073, // 2728->self, 3.0s cast, single-target, boss starts reflecting all melee attacks
    AutoAttack = 6499, // 2728->player, no cast, single-target
    VineProbe = 15075, // 2728->self, 2.5s cast, range 6+R width 8 rect
    OffalBreath = 15076, // 2728->location, 3.5s cast, range 6 circle
    Schizocarps = 15077, // 2728->self, 5.0s cast, single-target
    ExplosiveDehiscence = 15078, // 2729->self, 6.0s cast, range 50 circle, gaze
    BadBreath = 15074, // 2728->self, 3.5s cast, range 12+R 120-degree cone, interruptible, voidzone
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

class ExplosiveDehiscence(BossModule module) : Components.CastGaze(module, AID.ExplosiveDehiscence)
{
    public bool casting;
    public BitMask _blinded;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_blinded[slot] && casting)
            hints.Add("Cast Ink Jet on boss to get blinded!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Schizocarps)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ExplosiveDehiscence)
            casting = false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Blind)
            _blinded.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Blind)
            _blinded.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        return _blinded[slot] ? [] : base.ActiveEyes(slot, actor);
    }
}

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

class BadBreath(BossModule module) : Components.StandardAOEs(module, AID.BadBreath, new AOEShapeCone(17.775f, 60.Degrees()));
class VineProbe(BossModule module) : Components.StandardAOEs(module, AID.VineProbe, new AOEShapeRect(11.775f, 4));
class OffalBreath(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.OffalBreath, m => m.Enemies(OID.voidzone), 0);

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Same as first act, but this time the boss will cast a gaze from all directions.\nThe easiest counter for this is to blind yourself by casting Ink Jet on the\nboss after it casted Schizocarps.\nThe Final Sting combo window opens at around 75% health.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage19Act2States : StateMachineBuilder
{
    public Stage19Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Reflect>()
            .ActivateOnEnter<BadBreath>()
            .ActivateOnEnter<VineProbe>()
            .ActivateOnEnter<ExplosiveDehiscence>()
            .ActivateOnEnter<OffalBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 629, NameID = 8117, SortOrder = 2)]
public class Stage19Act2 : BossModule
{
    public Stage19Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }
}
