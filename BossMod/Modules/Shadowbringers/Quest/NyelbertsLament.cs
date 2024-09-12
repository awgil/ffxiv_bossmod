﻿namespace BossMod.Shadowbringers.Quest.NyelbertsLament;

// TODO: add AI hint for the "enrage" + paladin safe zone

public enum OID : uint
{
    Boss = 0x2977,
    Helper = 0x233C,
    BovianBull = 0x2976,
    _Gen_LooseBoulder = 0x2978, // R2.400, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Troodon/Bovian/BovianBull->296F/296E, no cast, single-target
    _Weaponskill_DeadlyHold = 16588, // Troodon->296F, no cast, single-target
    _Weaponskill_WildCharge = 16589, // Troodon->player, no cast, single-target
    _Weaponskill_RipperClaw = 16590, // Troodon->self, 3.7s cast, range 9 90-degree cone
    _Weaponskill_2000MinaSwipe = 16591, // Bovian->self, 3.0s cast, range 10 ?-degree cone
    _Weaponskill_DisorientingGroan = 16594, // Bovian->self, 3.0s cast, range 40 circle
    _Ability_ = 16593, // Bovian->self, no cast, single-target
    _Weaponskill_DisorientingGroan1 = 16606, // BovianBull->self, 5.0s cast, range 40 circle
    _Weaponskill_2000MinaSwipe1 = 16605, // BovianBull->self, 3.0s cast, range 9 ?-degree cone
    _Weaponskill_FallingRock = 16595, // Helper->location, 3.0s cast, range 4 circle
    _Ability_1 = 16600, // Bovian->location, no cast, single-target
    _Ability_2 = 16599, // Helper->player, no cast, single-target
    _Weaponskill_ZoomIn = 16596, // Bovian->self, 5.0s cast, single-target
    _Weaponskill_ZoomIn1 = 16597, // Bovian->location, no cast, single-target
    _Weaponskill_ZoomIn2 = 16598, // Helper->self, no cast, range 42 width 8 rect
    _Weaponskill_FallingBoulder = 16607, // _Gen_LooseBoulder->self, no cast, range 4 circle
    _Weaponskill_2000MinaSlashOnPlayer = 16601, // Bovian->self/player, 5.0s cast, range 40 ?-degree cone
    _Weaponskill_2000MinaSlashOnNPC = 16602, // Bovian->self, no cast, range 40 ?-degree cone
    _Weaponskill_Shatter = 16608, // _Gen_LooseBoulder->self, no cast, range 8 circle
}

public enum SID : uint
{
    WingedShield = 1900
}

class TwoThousandMinaSlash : Components.GenericLineOfSightAOE
{
    private readonly List<Actor> _casters = [];

    public TwoThousandMinaSlash(BossModule module) : base(module, ActionID.MakeSpell(AID._Weaponskill_2000MinaSlashOnPlayer), 40, false)
    {
        Refresh();
    }

    public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.RemainingTime);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _casters.Add(caster);
            Refresh();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _casters.Remove(caster);
            Refresh();
        }
    }

    private void Refresh()
    {
        var blockers = Module.Enemies(OID._Gen_LooseBoulder);

        Modify(ActiveCaster?.CastInfo?.LocXZ, blockers.Select(b => (b.Position, b.HitboxRadius)), Module.CastFinishAt(ActiveCaster?.CastInfo));
    }
}

class FallingRock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_FallingRock), 4);
class ZoomIn(BossModule module) : Components.SimpleLineStack(module, 4, 42, ActionID.MakeSpell(AID._Ability_2), ActionID.MakeSpell(AID._Weaponskill_ZoomIn2), 5.1f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source != null)
            hints.AddForbiddenZone(new AOEShapeDonut(3, 100), Arena.Center, default, Activation);
    }
}

class PassageOfArms(BossModule module) : BossComponent(module)
{
    private ActorCastInfo? EnrageCast => Module.PrimaryActor.CastInfo is { Action.ID: 16604 } castInfo ? castInfo : null;
    private Actor? Paladin => WorldState.Actors.FirstOrDefault(x => x.FindStatus(SID.WingedShield) != null);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnrageCast != null && Paladin != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(Paladin.Position, 8, Paladin.Rotation + 180.Degrees(), 60.Degrees()), Module.CastFinishAt(EnrageCast));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (EnrageCast != null && Paladin != null)
            Arena.ZoneCone(Paladin.Position, 0, 8, Paladin.Rotation + 180.Degrees(), 60.Degrees(), ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnrageCast != null && Paladin != null && !actor.Position.InCircleCone(Paladin.Position, 8, Paladin.Rotation + 180.Degrees(), 60.Degrees()))
            hints.Add("Hide behind tank!");
    }
}

class NyelbertAI(BossModule module) : BossComponent(module)
{
    private readonly QuestBattle.Shadowbringers.NyelbertsLament.NyelbertAI _ai = new(module.WorldState);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints, float maxCastTime) => _ai.Execute(actor, hints, maxCastTime);
}

class BovianStates : StateMachineBuilder
{
    public BovianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NyelbertAI>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<ZoomIn>()
            .ActivateOnEnter<TwoThousandMinaSlash>()
            .ActivateOnEnter<PassageOfArms>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69162, NameID = 8363)]
public class Bovian(WorldState ws, Actor primary) : BossModule(ws, primary, new(-440, -691), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => x.IsAlly), ArenaColor.PlayerGeneric);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.BovianBull, 1);
    }
}
