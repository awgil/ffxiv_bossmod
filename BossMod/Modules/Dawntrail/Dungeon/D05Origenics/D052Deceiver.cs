﻿namespace BossMod.Dawntrail.Dungeon.D05Origenics.D052Deceiver;

public enum OID : uint
{
    Boss = 0x4170, // R5.000, x1
    Helper = 0x233C, // R0.500, x10, Helper type
    SentryReal = 0x4171, // R0.900, x0 (spawn during fight)
    SentryFake = 0x4172, // R0.900, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttackBoss = 870, // Boss->player, no cast, single-target
    AutoAttackAdd = 873, // SentryReal->player, no cast, single-target
    Teleport = 36362, // Boss->location, no cast, single-target
    Electrowave = 36371, // Boss->self, 5.0s cast, range 72 circle, raidwide
    BionicThrash = 36369, // Boss->self, 7.0s cast, single-target, visual (two quadrant cleave)
    BionicThrashAOE = 36370, // Helper->self, 8.0s cast, range 30 90-degree cone
    InitializeAndroids = 36363, // Boss->self, 4.0s cast, single-target, visual (sentries)
    SynchroshotFake = 36373, // SentryFake->self, 5.0s cast, range 40 width 4 rect
    SynchroshotReal = 36372, // SentryReal->self, 5.0s cast, range 40 width 4 rect
    InitializeTurrets = 36364, // Boss->self, 4.0s cast, single-target, visual (turrets)
    InitializeTurretsReal = 36365, // Helper->self, 4.7s cast, range 4 width 10 rect (? kill player if he'll be standing inside turret?)
    InitializeTurretsFake = 36426, // Helper->self, 4.7s cast, range 4 width 10 rect (? kill player if he'll be standing inside turret?)
    LaserLashReal = 36366, // Helper->self, 5.0s cast, range 40 width 10 rect
    LaserLashFake = 38807, // Helper->self, 5.0s cast, range 40 width 10 rect
    Surge = 36367, // Boss->location, 8.0s cast, range 40 width 40 rect, knock players left/right 30
    SurgeNPC = 39736, // Helper->self, 8.5s cast, range 40 width 40 rect, knock duty support left/right 15
    Electray = 38320, // Helper->player, 8.0s cast, range 5 circle spread
}

public enum IconID : uint
{
    Electray = 345, // player
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));
class BionicThrash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BionicThrashAOE), new AOEShapeCone(30, 45.Degrees()));
class Synchroshot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SynchroshotReal), new AOEShapeRect(40, 2));
class Sentry(BossModule module) : Components.Adds(module, (uint)OID.SentryReal);
class LaserLash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LaserLashReal), new AOEShapeRect(40, 5));

class Surge(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.Surge))
{
    private readonly List<(WPos origin, WDir dir)> _realTurrets = [];
    private readonly List<(WPos origin, WDir dir)> _fakeTurrets = [];
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
        {
            float distance = 30;
            if (_realTurrets.Any(t => (t.origin.X < Module.Center.X) == (actor.Position.X < Module.Center.X) && Math.Abs(t.origin.Z - actor.Position.Z) <= 5))
                distance = Math.Max(0, 15.5f - MathF.Abs(actor.Position.X - Module.Center.X));
            yield return new(new(Module.Center.X, actor.Position.Z), distance, _activation);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            foreach (var t in _fakeTurrets)
                hints.AddForbiddenZone(ShapeDistance.Rect(t.origin, t.dir, 20, 0, 5), _activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_activation != default)
            foreach (var t in _realTurrets)
                Arena.ZoneRect(t.origin, t.dir, 20, 0, 5, ArenaColor.SafeFromAOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.InitializeTurretsReal:
                _realTurrets.Add((caster.Position, spell.Rotation.ToDirection()));
                break;
            case AID.InitializeTurretsFake:
                _fakeTurrets.Add((caster.Position, spell.Rotation.ToDirection()));
                break;
            case AID.Surge:
                _activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _realTurrets.Clear();
            _fakeTurrets.Clear();
            _activation = default;
        }
    }
}

class Electray(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Electray), 5);

class D052DeceiverStates : StateMachineBuilder
{
    public D052DeceiverStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<BionicThrash>()
            .ActivateOnEnter<Synchroshot>()
            .ActivateOnEnter<Sentry>()
            .ActivateOnEnter<LaserLash>()
            .ActivateOnEnter<Surge>()
            .ActivateOnEnter<Electray>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12693)]
public class D052Deceiver(WorldState ws, Actor primary) : BossModule(ws, primary, new(-172, -142), new ArenaBoundsRect(16, 20));
