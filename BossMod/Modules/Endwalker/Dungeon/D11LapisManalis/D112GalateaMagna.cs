namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D112GalateaMagna;

public enum OID : uint
{
    Boss = 0x3971, //R=5.0
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 32625, // Boss->location, no cast, single-target, boss teleports to spot marked by icons 1,2,3,4 or to mid
    WaningCycle0 = 32623, // Boss->self, no cast, single-target (between in->out)
    WaningCycle1 = 32622, // Boss->self, 4.0s cast, range 10-40 donut
    WaningCycle2 = 32624, // Helper->self, 6.0s cast, range 10 circle
    WaxingCycle0 = 31378, // Boss->self, no cast, single-target (between out->in)
    WaxingCycle1 = 31377, // Boss->self, 4.0s cast, range 10 circle
    WaxingCycle2 = 31379, // Helper->self, 6.7s cast, range 10-40 donut
    SoulScythe = 31386, // Boss->location, 6.0s cast, range 18 circle
    SoulNebula = 31390, // Boss->self, 5.0s cast, range 40 circle, raidwide
    ScarecrowChase = 31387, // Boss->self, 8.0s cast, single-target
    ScarecrowChase2 = 31389, // Boss->self, no cast, single-target
    ScarecrowChase3 = 32703, // Helper->self, 1.8s cast, range 40 width 10 cross
    Tenebrism = 31382, // Boss->self, 4.0s cast, range 40 circle, small raidwide, spawns 4 towers, applies glass-eyed on tower resolve
    Burst = 31383, // Helper->self, no cast, range 5 circle, tower success
    BigBurst = 31384, // Helper->self, no cast, range 60 circle, tower fail
    StonyGaze = 31385, // Helper->self, no cast, gaze
}

public enum IconID : uint
{
    Icon1 = 336, // 3D06
    Icon2 = 337, // 3D06
    Icon3 = 338, // 3D06
    Icon4 = 339, // 3D06
    PlayerGaze = 73, // player
}

public enum SID : uint
{
    SustainedDamage = 2935, // Helper->player, extra=0x0
    ScarecrowChase = 2056, // none->Boss, extra=0x22B
    Doom = 3364, // Helper->player, extra=0x0
    GlassyEyed = 3511, // Boss->player, extra=0x0, takes possession of the player after status ends and does a petrifying attack in all direction
}

class ScarecrowChase(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor actor, uint icon)> _casters = [];
    private List<Actor> _casterssorted = [];
    private static readonly AOEShapeCross cross = new(40, 5);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var activation = 3 * (_casters.Count - _casterssorted.Count);
        if (_casterssorted.Count == 1)
            yield return new(cross, _casterssorted[0].Position, 45.Degrees(), _activation.AddSeconds(activation), ArenaColor.Danger);
        if (_casterssorted.Count > 1)
        {
            yield return new(cross, _casterssorted[0].Position, 45.Degrees(), _activation.AddSeconds(activation), ArenaColor.Danger);
            yield return new(cross, _casterssorted[1].Position, 45.Degrees(), _activation.AddSeconds(3 + activation));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var icon = (IconID)iconID;
        if (icon is >= IconID.Icon1 and <= IconID.Icon4)
        {
            _casters.Add((actor, iconID));
            if (_activation == default)
                _activation = WorldState.FutureTime(9.9f);
        }
        _casterssorted = [.. _casters.OrderBy(x => x.icon).Select(x => x.actor)];
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_casters.Count > 0 && _casterssorted.Count > 0 && (AID)spell.Action.ID == AID.ScarecrowChase3)
        {
            _casterssorted.RemoveAt(0);
            if (_casterssorted.Count == 0)
            {
                _casters.Clear();
                _activation = default;
            }
        }
    }
}

class OutInAOE(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 40)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WaxingCycle1)
            AddSequence(Module.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.WaxingCycle1 => 0,
                AID.WaxingCycle2 => 1,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2.7f));
        }
    }
}

class InOutAOE(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(10, 40), new AOEShapeCircle(10)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WaningCycle1)
            AddSequence(Module.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.WaningCycle1 => 0,
                AID.WaningCycle2 => 1,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
        }
    }
}

class GlassyEyed(BossModule module) : Components.GenericGaze(module)
{
    private DateTime _activation;
    private readonly List<Actor> _affected = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        foreach (var a in _affected)
            if (_affected.Count > 0 && WorldState.CurrentTime > _activation.AddSeconds(-10))
                yield return new(a.Position, _activation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GlassyEyed)
        {
            _activation = status.ExpireAt;
            _affected.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GlassyEyed)
            _affected.Remove(actor);
    }
}

public class TenebrismTowers(BossModule module) : Components.GenericTowers(module)
{
    private WPos position;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00010008)
        {
            switch (index)
            {
                case 0x07:
                    position = new(350, -404);
                    break;
                case 0x08:
                    position = new(360, -394);
                    break;
                case 0x09:
                    position = new(350, -384);
                    break;
                case 0x0A:
                    position = new(340, -394);
                    break;
            }
            Towers.Add(new(position, 5, 1, 1));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Burst or AID.BigBurst)
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
    }
}

class Doom(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _doomed = [];

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

class SoulScythe(BossModule module) : Components.StandardAOEs(module, AID.SoulScythe, 18);

class D112GalateaMagnaStates : StateMachineBuilder
{
    public D112GalateaMagnaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<TenebrismTowers>()
            .ActivateOnEnter<InOutAOE>()
            .ActivateOnEnter<OutInAOE>()
            .ActivateOnEnter<GlassyEyed>()
            .ActivateOnEnter<SoulScythe>()
            .ActivateOnEnter<ScarecrowChase>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 10308)]
public class D112GalateaMagna(WorldState ws, Actor primary) : BossModule(ws, primary, new(350, -394), new ArenaBoundsCircle(19.5f));
