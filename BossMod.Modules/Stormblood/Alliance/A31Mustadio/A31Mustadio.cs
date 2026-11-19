#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Stormblood.Alliance.A31Mustadio;

public enum OID : uint
{
    Boss = 0x25B7,
    _Gen_IronConstruct = 0x25B9, // R4.550, x8
    _Gen_EarlyTurret = 0x25B8, // R2.660, x4

    // target indicator, gets model state 0 1 0 before Searchlight cast on position 3.1s later
    _Gen_Mustadio = 0x25BC, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 14138, // Boss->player, no cast, single-target
    _Weaponskill_EnergyBurst = 14139, // Boss->self, 4.0s cast, range 90 circle
    _Weaponskill_ArmShot = 14137, // Boss->player, 4.0s cast, single-target
    _Weaponskill_RightHandgonne = 14142, // Boss->self, 4.0s cast, range 30 210-degree cone
    _Weaponskill_LeftHandgonne = 14143, // Boss->self, 4.0s cast, range 30 210-degree cone
    _Weaponskill_Maintenance = 14132, // Boss->self, 8.0s cast, single-target
    _Weaponskill_Compress = 14144, // 25B9->self, 2.0s cast, range 100 width 15 rect
    _Weaponskill_Searchlight = 14141, // Boss->self, no cast, single-target
    _Spell_Searchlight = 14148, // Helper->self, no cast, range 5 circle
    _Weaponskill_Analysis = 14133, // Boss->self, 4.0s cast, single-target
    _Weaponskill_BallisticImpact = 14149, // Boss->self, no cast, range 60 circle
    _Weaponskill_ = 14150, // Boss->self, no cast, single-target
    _Weaponskill_1 = 14188, // Helper->self, no cast, range 60 circle
    _Weaponskill_LastTestament = 14135, // Boss->self, 6.0s cast, range 100 width 60 rect
    _Weaponskill_LegShot = 14136, // Boss->self, 4.0s cast, single-target
    _Spell_LegShot = 14146, // Helper->location, no cast, range 6 circle
    _Spell_SatelliteBeam = 14145, // 25B8->self, 2.0s cast, range 30 width 30 rect
    _Weaponskill_BallisticMissile = 14140, // Boss->self, 3.0s cast, single-target
    _Spell_BallisticImpact = 14147, // Helper->players, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    _Gen_VulnerabilityUp = 202, // Boss/25B9/25B8->player, extra=0x1/0x2
    _Gen_BackUnseen = 1709, // none->player, extra=0x56
    _Gen_LeftUnseen = 1708, // none->player, extra=0x58
    _Gen_RightUnseen = 1707, // none->player, extra=0x57
    _Gen_Stun = 149, // Boss->player, extra=0x0
    _Gen_DownForTheCount = 783, // Boss->player, extra=0xEC7
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon01i = 198, // player->self
    _Gen_Icon_ude_lockon01i = 164, // player->self
    _Gen_Icon_target_ae_s5f = 139, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0361_mainte_0i = 86, // 25B9->Boss, 14.5s before Compress cast event
    _Gen_Tether_chn_m0361_mainte_1i = 88, // 25B8->Boss, 14.5s before Satellite Beam cast event
}

class EnergyBurst(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_EnergyBurst);
class ArmShot(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_ArmShot);
class Handgonne(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_RightHandgonne, AID._Weaponskill_LeftHandgonne], new AOEShapeCone(30, 105.Degrees()));

class MaintenancePredict(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor Caster, DateTime Predicted)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, p) in Casters)
        {
            switch ((OID)c.OID)
            {
                case OID._Gen_IronConstruct:
                    yield return new(new AOEShapeRect(100, 7.5f), c.Position, c.Rotation, p);
                    break;
                case OID._Gen_EarlyTurret:
                    yield return new(new AOEShapeRect(15, 15, 15), c.Position, c.Rotation, p);
                    break;
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID._Gen_Tether_chn_m0361_mainte_0i or TetherID._Gen_Tether_chn_m0361_mainte_1i)
            Casters.Add((source, WorldState.FutureTime(14.5f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_SatelliteBeam or AID._Weaponskill_Compress)
            Casters.RemoveAll(c => c.Caster == caster);
    }
}
class Compress(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Compress, new AOEShapeRect(100, 7.5f));
class SatelliteBeam(BossModule module) : Components.StandardAOEs(module, AID._Spell_SatelliteBeam, new AOEShapeRect(30, 15));
class SearchlightBait(BossModule module) : BossComponent(module)
{
    BitMask Targets;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_ude_lockon01i && Raid.TryFindSlot(actor, out var slot))
            Targets.Set(slot);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Targets[slot])
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius - 1), DateTime.MaxValue);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID._Gen_Mustadio && animState1 == 1)
            Targets.Reset();
    }
}

class Searchlight(BossModule module) : Components.GenericAOEs(module, AID._Spell_Searchlight)
{
    readonly List<Actor> Puddles = [];
    DateTime Activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Puddles.Select(p => new AOEInstance(new AOEShapeCircle(5), p.Position, default, Activation, Risky: Activation != default));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_Mustadio)
            Puddles.Add(actor);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID._Gen_Mustadio && animState1 == 1)
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

class LastTestament(BossModule module) : Components.CastWeakpoint(module, AID._Weaponskill_LastTestament, new AOEShapeRect(100, 30), 0, (uint)SID._Gen_BackUnseen, (uint)SID._Gen_LeftUnseen, (uint)SID._Gen_RightUnseen);

class LegShot(BossModule module) : Components.Voidzone(module, 6, 0x1EAA60);
class BallisticImpact(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_BallisticImpact, 5);

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

//[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7919)]
//public class A31Mustadio(ModuleInit init) : BossModule(init, new(600, 290), new ArenaBoundsRect(29.5f, 29.5f, 45.Degrees(), 29.5f * MathF.Sqrt(2)));

