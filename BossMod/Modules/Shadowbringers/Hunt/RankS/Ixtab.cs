namespace BossMod.Shadowbringers.Hunt.RankS.Ixtab;

public enum OID : uint
{
    Boss = 0x2838, // R=3.24
};

public enum AID : uint
{
    AutoAttack = 17850, // Boss->player, no cast, single-target
    TartareanAbyss = 17848, // Boss->players, 4,0s cast, range 6 circle
    TartareanFlare = 17846, // Boss->location, 4,5s cast, range 18 circle
    TartareanBlizzard = 17845, // Boss->self, 3,0s cast, range 40 45-degree cone
    TartareanFlame = 17999, // Boss->self, 5,0s cast, range 8-40 donut
    TartareanFlame2 = 18074, // Boss->self, no cast, range 8-40 donut
    TartareanThunder = 17843, // Boss->location, 5,0s cast, range 20 circle
    TartareanThunder2 = 18075, // Boss->location, no cast, range 20 circle
    TartareanMeteor = 17844, // Boss->players, 5,0s cast, range 10 circle
    ArchaicDualcast = 18077, // Boss->self, 3,0s cast, single-target, either out/in or in/out with Tartarean Flame and Tartarean Thunder
    Cryptcall = 17847, // Boss->self/players, 3,0s cast, range 35+R 120-degree cone, sets hp to 1, applies heal to full doom with 25s duration
    TartareanQuake = 17849, // Boss->self, 4,0s cast, range 40 circle
    TartareanTwister = 18072, // Boss->self, 5,0s cast, range 55 circle, raidwide + windburn DoT, interruptible
};

public enum SID : uint
{
    Burns = 267, // Boss->player, extra=0x0
    Dualcast = 1798, // Boss->Boss, extra=0x0
    Paralysis = 17, // Boss->player, extra=0x0
    Doom = 1769, // Boss->player, extra=0x0
};

class DualCastTartareanFlameThunder : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = new();
    private static readonly AOEShapeCircle circle = new(20);
    private static readonly AOEShapeDonut donut = new(8, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var dualcast = module.PrimaryActor.FindStatus(SID.Dualcast) != null;
        if ((AID)spell.Action.ID == AID.TartareanThunder)
            if (!dualcast)
                _aoes.Add(new(circle, caster.Position, activation: spell.NPCFinishAt));
            else
            {
                _aoes.Add(new(circle, caster.Position, activation: spell.NPCFinishAt));
                _aoes.Add(new(donut, caster.Position, activation: spell.NPCFinishAt.AddSeconds(5.1f)));
            }
        if ((AID)spell.Action.ID == AID.TartareanFlame)
            if (!dualcast)
                _aoes.Add(new(donut, caster.Position, activation: spell.NPCFinishAt));
            else
            {
                _aoes.Add(new(donut, caster.Position, activation: spell.NPCFinishAt));
                _aoes.Add(new(circle, caster.Position, activation: spell.NPCFinishAt.AddSeconds(5.1f)));
            }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.TartareanThunder or AID.TartareanFlame)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.TartareanThunder2 or AID.TartareanFlame2)
            _aoes.RemoveAt(0);
    }
}

class TartareanTwister : Components.CastInterruptHint
{
    public TartareanTwister() : base(ActionID.MakeSpell(AID.TartareanTwister)) { }
}

class TartareanBlizzard : Components.SelfTargetedAOEs
{
    public TartareanBlizzard() : base(ActionID.MakeSpell(AID.TartareanBlizzard), new AOEShapeCone(40, 22.5f.Degrees())) { }
}

class TartareanQuake : Components.RaidwideCast
{
    public TartareanQuake() : base(ActionID.MakeSpell(AID.TartareanQuake)) { }
}

class TartareanAbyss : Components.BaitAwayCast
{
    public TartareanAbyss() : base(ActionID.MakeSpell(AID.TartareanAbyss), new AOEShapeCircle(6), true) { }
}

class TartareanAbyssHint : Components.SingleTargetCast
{
    public TartareanAbyssHint() : base(ActionID.MakeSpell(AID.TartareanAbyss), "Tankbuster circle") { }
}

class TartareanFlare : Components.LocationTargetedAOEs
{
    public TartareanFlare() : base(ActionID.MakeSpell(AID.TartareanFlare), 18) { }
}

class TartareanMeteor : Components.StackWithCastTargets
{
    public TartareanMeteor() : base(ActionID.MakeSpell(AID.TartareanMeteor), 10) { }
}

class ArchaicDualcast : Components.CastHint
{
    public ArchaicDualcast() : base(ActionID.MakeSpell(AID.ArchaicDualcast), "Preparing In/Out or Out/In AOE") { }
}

class Cryptcall : Components.BaitAwayCast
{
    public Cryptcall() : base(ActionID.MakeSpell(AID.Cryptcall), new AOEShapeCone(38.24f, 60.Degrees())) { }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell) { }
    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell) //bait resolves on cast event instead of cast finish
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}

class CryptcallHint : Components.CastHint
{
    public CryptcallHint() : base(ActionID.MakeSpell(AID.Cryptcall), "Cone reduces health to 1 + applies Doom") { }
}

class Doom : BossComponent
{
    private readonly List<Actor> _doomed = [];
    public bool Doomed { get; private set; }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Add(actor);
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Remove(actor);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_doomed.Contains(actor) && !(actor.Role == Role.Healer))
            hints.Add("You were doomed! Get healed to full fast.");
        if (_doomed.Contains(actor) && (actor.Role == Role.Healer))
            hints.Add("Heal yourself to full! (Doom).");
        if (_doomed.Count > 0 && (actor.Role == Role.Healer) && !_doomed.Contains(actor))
            hints.Add($"One or more players are affected by doom. Heal them to full.");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        foreach (var c in _doomed)
        {
            if (_doomed.Count > 0 && actor.Role == Role.Healer)
                hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Esuna), c, 1, false));
            if (_doomed.Count > 0 && actor.Class == Class.BRD)
                hints.PlannedActions.Add((ActionID.MakeSpell(BRD.AID.WardensPaean), c, 1, false));
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
public class Ixtab : SimpleBossModule
{
    public Ixtab(WorldState ws, Actor primary) : base(ws, primary) { }
}
