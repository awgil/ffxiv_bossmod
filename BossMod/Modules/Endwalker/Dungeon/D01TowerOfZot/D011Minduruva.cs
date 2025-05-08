namespace BossMod.Endwalker.Dungeon.D01TowerOfZot.D011Minduruva;

public enum OID : uint
{
    Boss = 0x33EE, // R=2.04
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ManusyaBio = 25248, // Boss->player, 4.0s cast, single-target
    Teleport = 25241, // Boss->location, no cast, single-target
    ManusyaBlizzardIII1 = 25234, // Boss->self, 4.0s cast, single-target
    ManusyaBlizzardIII2 = 25238, // Helper->self, 4.0s cast, range 40+R 20-degree cone
    ManusyaFireIII1 = 25233, // Boss->self, 4.0s cast, single-target
    ManusyaFireIII2 = 25237, // Helper->self, 4.0s cast, range 5-40 donut
    ManusyaThunderIII1 = 25235, // Boss->self, 4.0s cast, single-target
    ManusyaThunderIII2 = 25239, // Helper->self, 4.0s cast, range 3 circle
    ManusyaBioIII1 = 25236, // Boss->self, 4.0s cast, single-target
    ManusyaBioIII2 = 25240, // Helper->self, 4.0s cast, range 40+R 180-degree cone
    TransmuteFireIII = 25242, // Boss->self, 2.7s cast, single-target
    Unknown = 25243, // Helper->Boss, 3.6s cast, single-target
    ManusyaFire2 = 25699, // Boss->player, 2.0s cast, single-target
    Dhrupad = 25244, // Boss->self, 4.0s cast, single-target, after this each of the non-tank players get hit once by a single-target spell (ManusyaBlizzard, ManusyaFire1, ManusyaThunder)
    ManusyaFire1 = 25245, // Boss->player, no cast, single-target
    ManusyaBlizzard = 25246, // Boss->player, no cast, single-target
    ManusyaThunder = 25247, // Boss->player, no cast, single-target
    TransmuteBlizzardIII = 25371, // Boss->self, 2.7s cast, single-target
    TransmuteThunderIII = 25372, // Boss->self, 2.7s cast, single-target
}

public enum SID : uint
{
    Poison = 18, // Boss->player, extra=0x0
    ThunderAlchemy = 2753, // Boss->Boss, extra=0x0
    Burns = 2082, // Boss->player, extra=0x0
    Frostbite = 2083, // Boss->player, extra=0x0
    Electrocution = 2086, // Boss->player, extra=0x0
    IceAlchemy = 2752, // Boss->Boss, extra=0x0
    ToxicAlchemy = 2754, // Boss->Boss, extra=0x0
    FireAlchemy = 2751, // Boss->Boss, extra=0x0
}

class ManusyaBio(BossModule module) : Components.SingleTargetCast(module, AID.ManusyaBio, "Tankbuster + cleansable poison");

class Poison(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _poisoned = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Poison)
            _poisoned.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Poison)
            _poisoned.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_poisoned.Contains(actor) && !(actor.Role == Role.Healer || actor.Class == Class.BRD)) //theoretically only the tank can ge poisoned, this is just in here incase of bad tanks
            hints.Add("You were poisoned! Get cleansed fast.");
        if (_poisoned.Contains(actor) && (actor.Role == Role.Healer || actor.Class == Class.BRD))
            hints.Add("Cleanse yourself! (Poison).");
        foreach (var c in _poisoned)
            if (!_poisoned.Contains(actor) && (actor.Role == Role.Healer || actor.Class == Class.BRD))
                hints.Add($"Cleanse {c.Name} (Poison)");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in _poisoned)
        {
            if (_poisoned.Count > 0 && actor.Role == Role.Healer)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Esuna), c, ActionQueue.Priority.High);
            if (_poisoned.Count > 0 && actor.Class == Class.BRD)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.WardensPaean), c, ActionQueue.Priority.High);
        }
    }
}

class Dhrupad(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Dhrupad)
            active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ManusyaFire1 or AID.ManusyaBlizzard or AID.ManusyaThunder)
        {
            ++NumCasts;
            if (NumCasts == 3)
            {
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (active)
            hints.Add("3 single target hits + DoTs");
    }
}

class ManusyaThunderIII(BossModule module) : Components.StandardAOEs(module, AID.ManusyaThunderIII2, new AOEShapeCircle(3));
class ManusyaBioIII(BossModule module) : Components.StandardAOEs(module, AID.ManusyaBioIII2, new AOEShapeCone(40.5f, 90.Degrees()));
class ManusyaBlizzardIII(BossModule module) : Components.StandardAOEs(module, AID.ManusyaBlizzardIII2, new AOEShapeCone(40.5f, 10.Degrees()));
class ManusyaFireIII(BossModule module) : Components.StandardAOEs(module, AID.ManusyaFireIII2, new AOEShapeDonut(5, 60));

class D011MinduruvaStates : StateMachineBuilder
{
    public D011MinduruvaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Dhrupad>()
            .ActivateOnEnter<ManusyaBio>()
            .ActivateOnEnter<Poison>()
            .ActivateOnEnter<ManusyaThunderIII>()
            .ActivateOnEnter<ManusyaFireIII>()
            .ActivateOnEnter<ManusyaBioIII>()
            .ActivateOnEnter<ManusyaBlizzardIII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783, NameID = 10256)]
public class D011Minduruva(WorldState ws, Actor primary) : BossModule(ws, primary, new(68, -124), new ArenaBoundsCircle(19.5f));
