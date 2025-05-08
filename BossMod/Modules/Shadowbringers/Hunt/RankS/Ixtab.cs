namespace BossMod.Shadowbringers.Hunt.RankS.Ixtab;

public enum OID : uint
{
    Boss = 0x2838, // R=3.24
}

public enum AID : uint
{
    AutoAttack = 17850, // Boss->player, no cast, single-target
    TartareanAbyss = 17848, // Boss->players, 4.0s cast, range 6 circle
    TartareanFlare = 17846, // Boss->location, 4.5s cast, range 18 circle
    TartareanBlizzard = 17845, // Boss->self, 3.0s cast, range 40 45-degree cone
    TartareanFlame = 17999, // Boss->self, 5.0s cast, range 8-40 donut
    TartareanFlame2 = 18074, // Boss->self, no cast, range 8-40 donut
    TartareanThunder = 17843, // Boss->location, 5.0s cast, range 20 circle
    TartareanThunder2 = 18075, // Boss->location, no cast, range 20 circle
    TartareanMeteor = 17844, // Boss->players, 5.0s cast, range 10 circle
    ArchaicDualcast = 18077, // Boss->self, 3.0s cast, single-target, either out/in or in/out with Tartarean Flame and Tartarean Thunder
    Cryptcall = 17847, // Boss->self/players, 3.0s cast, range 35+R 120-degree cone, sets hp to 1, applies heal to full doom with 25s duration
    TartareanQuake = 17849, // Boss->self, 4.0s cast, range 40 circle
    TartareanTwister = 18072, // Boss->self, 5.0s cast, range 55 circle, raidwide + windburn DoT, interruptible
}

public enum SID : uint
{
    Burns = 267, // Boss->player, extra=0x0
    Dualcast = 1798, // Boss->Boss, extra=0x0
    Paralysis = 17, // Boss->player, extra=0x0
    Doom = 1769, // Boss->player, extra=0x0
}

class DualCastTartareanFlameThunder(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(20);
    private static readonly AOEShapeDonut donut = new(8, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var dualcast = Module.PrimaryActor.FindStatus(SID.Dualcast) != null;
        if ((AID)spell.Action.ID == AID.TartareanThunder)
            if (!dualcast)
                _aoes.Add(new(circle, caster.Position, default, Module.CastFinishAt(spell)));
            else
            {
                _aoes.Add(new(circle, caster.Position, default, Module.CastFinishAt(spell)));
                _aoes.Add(new(donut, caster.Position, default, Module.CastFinishAt(spell, 5.1f)));
            }
        if ((AID)spell.Action.ID == AID.TartareanFlame)
            if (!dualcast)
                _aoes.Add(new(donut, caster.Position, default, Module.CastFinishAt(spell)));
            else
            {
                _aoes.Add(new(donut, caster.Position, default, Module.CastFinishAt(spell)));
                _aoes.Add(new(circle, caster.Position, default, Module.CastFinishAt(spell, 5.1f)));
            }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.TartareanThunder or AID.TartareanFlame)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.TartareanThunder2 or AID.TartareanFlame2)
            _aoes.RemoveAt(0);
    }
}

class TartareanTwister(BossModule module) : Components.CastInterruptHint(module, AID.TartareanTwister);
class TartareanBlizzard(BossModule module) : Components.StandardAOEs(module, AID.TartareanBlizzard, new AOEShapeCone(40, 22.5f.Degrees()));
class TartareanQuake(BossModule module) : Components.RaidwideCast(module, AID.TartareanQuake);
class TartareanAbyss(BossModule module) : Components.BaitAwayCast(module, AID.TartareanAbyss, new AOEShapeCircle(6), true);
class TartareanAbyssHint(BossModule module) : Components.SingleTargetCast(module, AID.TartareanAbyss, "Tankbuster circle");
class TartareanFlare(BossModule module) : Components.StandardAOEs(module, AID.TartareanFlare, 18);
class TartareanMeteor(BossModule module) : Components.StackWithCastTargets(module, AID.TartareanMeteor, 10);
class ArchaicDualcast(BossModule module) : Components.CastHint(module, AID.ArchaicDualcast, "Preparing In/Out or Out/In AOE");

class Cryptcall(BossModule module) : Components.BaitAwayCast(module, AID.Cryptcall, new AOEShapeCone(38.24f, 60.Degrees()))
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }
    public override void OnEventCast(Actor caster, ActorCastEvent spell) //bait resolves on cast event instead of cast finish
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}

class CryptcallHint(BossModule module) : Components.CastHint(module, AID.Cryptcall, "Cone reduces health to 1 + applies Doom");

class Doom(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _doomed = [];
    public bool Doomed { get; private set; }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_doomed.Contains(actor) && !(actor.Role == Role.Healer))
            hints.Add("You were doomed! Get healed to full fast.");
        if (_doomed.Contains(actor) && (actor.Role == Role.Healer))
            hints.Add("Heal yourself to full! (Doom).");
        if (_doomed.Count > 0 && (actor.Role == Role.Healer) && !_doomed.Contains(actor))
            hints.Add($"One or more players are affected by doom. Heal them to full.");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in _doomed)
        {
            if (_doomed.Count > 0 && actor.Role == Role.Healer)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Esuna), c, ActionQueue.Priority.High);
            if (_doomed.Count > 0 && actor.Class == Class.BRD)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.WardensPaean), c, ActionQueue.Priority.High);
        }
    }
}

class IxtabStates : StateMachineBuilder
{
    public IxtabStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DualCastTartareanFlameThunder>()
            .ActivateOnEnter<TartareanTwister>()
            .ActivateOnEnter<TartareanBlizzard>()
            .ActivateOnEnter<TartareanQuake>()
            .ActivateOnEnter<TartareanAbyss>()
            .ActivateOnEnter<TartareanAbyssHint>()
            .ActivateOnEnter<TartareanFlare>()
            .ActivateOnEnter<TartareanMeteor>()
            .ActivateOnEnter<ArchaicDualcast>()
            .ActivateOnEnter<Cryptcall>()
            .ActivateOnEnter<CryptcallHint>()
            .ActivateOnEnter<Doom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8890)]
public class Ixtab(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
