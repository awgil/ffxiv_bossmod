namespace BossMod.Stormblood.Alliance.A31Mustadio;

public enum OID : uint
{
    Boss = 0x25B7,
    IronConstruct = 0x25B9, // R4.550, x8
    EarlyTurret = 0x25B8, // R2.660, x4
    Searchlight = 0x25BC, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 14138, // Boss->player, no cast, single-target
    EnergyBurst = 14139, // Boss->self, 4.0s cast, range 90 circle
    ArmShot = 14137, // Boss->player, 4.0s cast, single-target
    RightHandgonne = 14142, // Boss->self, 4.0s cast, range 30 210-degree cone
    LeftHandgonne = 14143, // Boss->self, 4.0s cast, range 30 210-degree cone
    Maintenance = 14132, // Boss->self, 8.0s cast, single-target
    Compress = 14144, // 25B9->self, 2.0s cast, range 100 width 15 rect
    SatelliteBeam = 14145, // 25B8->self, 2.0s cast, range 30 width 30 rect
    SearchlightBoss = 14141, // Boss->self, no cast, single-target
    Searchlight = 14148, // Helper->self, no cast, range 5 circle
    Analysis = 14133, // Boss->self, 4.0s cast, single-target
    BallisticImpact = 14149, // Boss->self, no cast, range 60 circle
    Unk1 = 14150, // Boss->self, no cast, single-target
    Unk2 = 14188, // Helper->self, no cast, range 60 circle
    LastTestament = 14135, // Boss->self, 6.0s cast, range 100 width 60 rect
    LegShotCast = 14136, // Boss->self, 4.0s cast, single-target
    LegShot = 14146, // Helper->location, no cast, range 6 circle
    BallisticMissile = 14140, // Boss->self, 3.0s cast, single-target
    BallisticImpactSpread = 14147, // Helper->players, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    RightUnseen = 1707, // none->player, extra=0x57
    LeftUnseen = 1708, // none->player, extra=0x58
    BackUnseen = 1709, // none->player, extra=0x56
    Stun = 149, // Boss->player, extra=0x0
    DownForTheCount = 783, // Boss->player, extra=0xEC7
}

public enum IconID : uint
{
    Tankbuster = 198, // player->self
    Searchlight = 164, // player->self
    Spread = 139, // player->self
}

public enum TetherID : uint
{
    MaintenanceBot = 86, // 25B9->Boss
    MaintenanceTurret = 88, // 25B8->Boss
}

class EnergyBurst(BossModule module) : Components.RaidwideCast(module, AID.EnergyBurst);
class ArmShot(BossModule module) : Components.SingleTargetCast(module, AID.ArmShot);
class Handgonne(BossModule module) : Components.GroupedAOEs(module, [AID.RightHandgonne, AID.LeftHandgonne], new AOEShapeCone(30, 105.Degrees()));

class MaintenancePredict(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor Caster, DateTime Predicted)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, p) in Casters)
        {
            switch ((OID)c.OID)
            {
                case OID.IronConstruct:
                    yield return new(new AOEShapeRect(100, 7.5f), c.Position, c.Rotation, p);
                    break;
                case OID.EarlyTurret:
                    yield return new(new AOEShapeRect(15, 15, 15), c.Position, c.Rotation, p);
                    break;
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.MaintenanceBot or TetherID.MaintenanceTurret)
            Casters.Add((source, WorldState.FutureTime(14.5f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SatelliteBeam or AID.Compress)
            Casters.RemoveAll(c => c.Caster == caster);
    }
}
class Compress(BossModule module) : Components.StandardAOEs(module, AID.Compress, new AOEShapeRect(100, 7.5f));
class SatelliteBeam(BossModule module) : Components.StandardAOEs(module, AID.SatelliteBeam, new AOEShapeRect(30, 15));
class SearchlightBait(BossModule module) : BossComponent(module)
{
    BitMask Targets;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Searchlight && Raid.TryFindSlot(actor, out var slot))
            Targets.Set(slot);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Targets[slot])
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius - 1), DateTime.MaxValue);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.Searchlight && animState1 == 1)
            Targets.Reset();
    }
}

class Searchlight(BossModule module) : Components.GenericAOEs(module, AID.Searchlight)
{
    readonly List<Actor> Puddles = [];
    DateTime Activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Puddles.Select(p => new AOEInstance(new AOEShapeCircle(5), p.Position, default, Activation, Risky: Activation != default));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Searchlight)
            Puddles.Add(actor);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.Searchlight && animState1 == 1)
            Activation = WorldState.FutureTime(3.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            Activation = default;
            Puddles.Clear();
        }
    }
}

class LastTestament(BossModule module) : Components.CastWeakpoint(module, AID.LastTestament, new AOEShapeRect(100, 30), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);

class LegShot(BossModule module) : Components.Voidzone(module, 6, 0x1EAA60);
class BallisticImpact(BossModule module) : Components.SpreadFromCastTargets(module, AID.BallisticImpactSpread, 5);

class A31MustadioStates : StateMachineBuilder
{
    public A31MustadioStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EnergyBurst>()
            .ActivateOnEnter<ArmShot>()
            .ActivateOnEnter<Handgonne>()
            .ActivateOnEnter<MaintenancePredict>()
            .ActivateOnEnter<Compress>()
            .ActivateOnEnter<SatelliteBeam>()
            .ActivateOnEnter<SearchlightBait>()
            .ActivateOnEnter<Searchlight>()
            .ActivateOnEnter<LastTestament>()
            .ActivateOnEnter<LegShot>()
            .ActivateOnEnter<BallisticImpact>()
            .Raw.Update = () => Module.PrimaryActor is { HPMP.CurHP: 1, IsTargetable: false };
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7919)]
public class A31Mustadio(ModuleInit init) : BossModule(init, new(600, 290), new ArenaBoundsRect(29.5f, 29.5f, 45.Degrees(), 29.5f * MathF.Sqrt(2)));

