namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D131DarkElf;

public enum OID : uint
{
    Boss = 0x3FE2, // R=5.0
    HexingStaff = 0x3FE3, // R=1.2
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 872, // 3FE2->player, no cast, single-target
    HexingStaves = 34777, // 3FE2->self, 3.0s cast, single-target
    RuinousHex = 34783, // 3FE3->self, 5.0s cast, single-target
    RuinousHex2 = 35254, // 3FE3->self, 5.0s cast, range 40 width 8 cross
    RuinousHex3 = 34789, // 233C->self, 5.5s cast, range 40 width 8 cross
    RuinousConfluence = 35205, // 3FE2->self, 5.0s cast, single-target
    ShadowySigil = 34779, // 3FE2->self, 6.0s cast, single-target
    ShadowySigil2 = 34780, // 3FE2->self, 6.0s cast, single-target
    Explosion = 34787, // 233C->self, 6.5s cast, range 8 width 8 rect
    SorcerousShroud = 34778, // 3FE2->self, 5.0s cast, single-target
    VoidDarkII = 34781, // 3FE2->self, 2.5s cast, single-target
    VoidDarkII2 = 34788, // 233C->player, 5.0s cast, range 6 circle
    StaffSmite = 35204, // 3FE2->player, 5.0s cast, single-target
    AbyssalOutburst = 34782, // 3FE2->self, 5.0s cast, range 60 circle
}

public enum SID : uint
{
    Doom = 3364, // none->player, extra=0x0
}

class HexingStaves(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _staves = [];
    private static readonly AOEShapeCross cross = new(40, 4);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<Explosion>() is var explosion && explosion != null && !explosion.ActiveAOEs(slot, actor).Any())
            foreach (var c in _staves)
                yield return new(cross, c.Position, c.Rotation, _activation, Risky: _activation.AddSeconds(-5) < WorldState.CurrentTime);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.HexingStaff)
        {
            if (animState2 == 1)
            {
                _staves.Add(actor);
                if (NumCasts == 0)
                    _activation = WorldState.FutureTime(8.1f);
                if (NumCasts == 1)
                    _activation = WorldState.FutureTime(25.9f);
                if (NumCasts > 1)
                    _activation = WorldState.FutureTime(32);
            }
            if (animState2 == 0)
                _staves.Remove(actor);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RuinousConfluence)
            ++NumCasts;
    }
}

class StaffSmite(BossModule module) : Components.SingleTargetCast(module, AID.StaffSmite);
class VoidDarkII(BossModule module) : Components.SpreadFromCastTargets(module, AID.VoidDarkII2, 6);
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeRect(4, 4, 4));
class AbyssalOutburst(BossModule module) : Components.RaidwideCast(module, AID.AbyssalOutburst);

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
        if (_doomed.Contains(actor) && !(actor.Role == Role.Healer || actor.Class == Class.BRD))
            hints.Add("You were doomed! Get cleansed fast.");
        if (_doomed.Contains(actor) && (actor.Role == Role.Healer || actor.Class == Class.BRD))
            hints.Add("Cleanse yourself! (Doom).");
        foreach (var c in _doomed)
            if (!_doomed.Contains(actor) && (actor.Role == Role.Healer || actor.Class == Class.BRD))
                hints.Add($"Cleanse {c.Name} (Doom)");
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

class D131DarkElfStates : StateMachineBuilder
{
    public D131DarkElfStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StaffSmite>()
            .ActivateOnEnter<HexingStaves>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<AbyssalOutburst>()
            .ActivateOnEnter<VoidDarkII>()
            .ActivateOnEnter<Doom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 823, NameID = 12500)]
public class D131DarkElf(WorldState ws, Actor primary) : BossModule(ws, primary, new(-401, -231), new ArenaBoundsSquare(15.5f));
