namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

public enum OID : uint
{
    Boss = 0x2E6E,
    Peri = 0x2E6F,
    Huma = 0x2E70,
    Helper = 0x233C,
    Helper2 = 0x2EE8
}

public enum AID : uint
{
    _AutoAttack_ = 21402, // Boss->player, no cast, single-target
    _Ability_Meltdown = 20653, // Boss->self, 3.0s cast, single-target
    _Ability_MeltdownRepeat = 20654, // Helper->self, no cast, range 5 circle
    _Ability_AetherialStep = 21330, // Boss->location, no cast, single-target
    _Ability_TimeEruption = 20641, // Boss->self, 2.0s cast, single-target
    _Ability_TimeEruptionFast = 20642, // Helper->self, 5.0s cast, range 20 width 20 rect
    _Ability_TimeEruptionSlow = 20636, // Helper->self, 8.0s cast, range 20 width 20 rect
    _Ability_ThermalGust = 20655, // Boss->self, 4.0s cast, single-target
    _Ability_ThermalGustAOE = 20656, // Helper->self, 4.0s cast, range 44 width 10 rect
    _Ability_Phenex = 20637, // Boss->self, 2.0s cast, single-target
    _Ability_Flamedive = 20638, // Huma->self, 5.0s cast, range 55 width 6 rect
    _Ability_SearingWind = 20640, // Helper->self, no cast, range 3 circle
    _Ability_Pyrolatry = 20639, // Boss->self, 6.0s cast, single-target
    _Ability_PyrolatryRaidwide = 21401, // Helper->self, no cast, range 100 circle
    _Ability_Unknown = 21400, // Boss->location, no cast, single-target
    _Ability_Flashover = 20643, // Boss->self, 2.0s cast, single-target
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Ability_FlamingRain = 20647, // Boss->self, 2.0s cast, single-target
    _Ability_FlashoverAOE = 20644, // Peri->self, 10.0s cast, range 19 circle
    _Ability_FlamingRainAOE = 20648, // Helper->self, 3.0s cast, range 6 circle
    _Ability_Backdraft = 20634, // Boss->self, 2.0s cast, single-target
    _Ability_BackdraftKB = 20635, // Peri->self, 10.0s cast, range 100 circle
    _Ability_Unknown1 = 21320, // Helper2->player, no cast, single-target
    _Ability_ThermalWave = 20649, // Boss->self, 3.0s cast, single-target
    _Ability_ThermalWave_Unknown1 = 20650, // Boss->self, no cast, single-target
    _Ability_ThermalWave_AOE = 20652, // Helper->self, 4.0s cast, range 60 90-degree cone
    _Ability_ThermalWave_Unknown2 = 20651, // Helper->self, no cast, range 60 ?-degree cone
}

class Meltdown(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID._Ability_Meltdown), centerAtTarget: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_Meltdown)
            CurrentBaits.Add(new Bait(caster, Raid.Player()!, new AOEShapeCircle(5), Module.CastFinishAt(spell, 0.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_MeltdownRepeat)
            CurrentBaits.Clear();
    }
}

class MeltdownAOE(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID._Ability_MeltdownRepeat))
{
    private AOEInstance? aoe;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_MeltdownRepeat)
        {
            aoe ??= new AOEInstance(new AOEShapeCircle(5), caster.Position);
            if (++NumCasts >= 8)
            {
                aoe = null;
                NumCasts = 0;
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(aoe);
}

class TimeEruption(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, DateTime Activation)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select((c, i) => new AOEInstance(new AOEShapeRect(10, 10, 10), c.Caster.Position, c.Caster.Rotation, c.Activation, i < 2 ? ArenaColor.Danger : ArenaColor.AOE, i < 2));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_TimeEruptionFast or AID._Ability_TimeEruptionSlow)
        {
            Casters.Add((caster, Module.CastFinishAt(spell)));
            Casters.SortBy(c => c.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_TimeEruptionFast or AID._Ability_TimeEruptionSlow)
        {
            NumCasts++;
            Casters.RemoveAll(c => c.Caster == caster);
        }
    }
}

class ThermalGust(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_ThermalGustAOE), new AOEShapeRect(44, 5));

class Flamedive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_Flamedive), new AOEShapeRect(55, 3))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Take(6);
}

class PyrolatryRaidwide(BossModule module) : Components.RaidwideInstant(module, ActionID.MakeSpell(AID._Ability_PyrolatryRaidwide), 0.6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_Pyrolatry)
            Activation = Module.CastFinishAt(spell, Delay);
    }
}

class FlashoverAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_FlashoverAOE), new AOEShapeCircle(19));

class Backdraft(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Ability_BackdraftKB), 16)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        var caster = Casters[0];

        hints.AddForbiddenZone(p =>
        {
            var dir = caster.DirectionTo(p).Normalized();
            var proj = p + dir * 16;
            return proj.AlmostEqual(caster.Position, 18) ? 0 : -1;
        }, Module.CastFinishAt(caster.CastInfo));
    }
}

class FlamingRain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_FlamingRainAOE), new AOEShapeCircle(6));

class ThermalWave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_ThermalWave_AOE), new AOEShapeCone(60, 45.Degrees()));

class WindVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(OID.Peri).Select(p => new AOEInstance(new AOEShapeCircle(3), p.Position));
}

class SartauvoirTheInfernoStates : StateMachineBuilder
{
    public SartauvoirTheInfernoStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Meltdown>()
            .ActivateOnEnter<MeltdownAOE>()
            .ActivateOnEnter<WindVoidzone>()
            .ActivateOnEnter<FlashoverAOE>()
            .ActivateOnEnter<FlamingRain>()
            .ActivateOnEnter<Backdraft>()
            .ActivateOnEnter<ThermalWave>();
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID._Ability_Meltdown, 8.1f, 3, "Meltdown");

        Cast(id + 0x10, AID._Ability_TimeEruption, 7.1f, 2)
            .ActivateOnEnter<TimeEruption>()
            .ActivateOnEnter<ThermalGust>();
        ComponentCondition<TimeEruption>(id + 0x12, 5.6f, t => t.NumCasts >= 2, "Clocks 1");
        ComponentCondition<TimeEruption>(id + 0x14, 3.1f, t => t.NumCasts >= 4, "Clocks 2")
            .DeactivateOnExit<TimeEruption>();

        CastEnd(id + 0x20, 2.6f, "Gust").DeactivateOnExit<ThermalGust>();

        id += 0x10000;

        Cast(id, AID._Ability_Phenex, 7.2f, 2).ActivateOnEnter<Flamedive>();
        ComponentCondition<Flamedive>(id + 0x02, 10.7f, t => t.NumCasts >= 6, "Birds 1");
        CastStart(id + 0x03, AID._Ability_Meltdown, 1.5f);
        ComponentCondition<Flamedive>(id + 0x04, 1.5f, t => t.NumCasts >= 9, "Birds 2")
            .DeactivateOnExit<Flamedive>();
        CastEnd(id + 0x05, 1.4f);

        Cast(id + 0x10, AID._Ability_Pyrolatry, 12.3f, 6, "Transform")
            .ActivateOnEnter<PyrolatryRaidwide>();

        id += 0x10000;

        Cast(id, AID._Ability_Flashover, 5.2f, 2);
        Cast(id + 0x04, AID._Ability_FlamingRain, 5.1f, 2);
        CastStart(id + 0x08, AID._Ability_Backdraft, 3.1f);
        ComponentCondition<FlashoverAOE>(id + 0x10, 0.7f, f => f.NumCasts > 0, "Flashover");
        ComponentCondition<FlamingRain>(id + 0x20, 0.1f, f => f.NumCasts >= 3, "Flaming rain 1");
        ComponentCondition<FlamingRain>(id + 0x30, 2, f => f.NumCasts >= 6, "Flaming rain 2");

        id += 0x10000;

        ComponentCondition<Backdraft>(id, 10.1f, b => b.NumCasts > 0, "Knockback");
        ComponentCondition<ThermalWave>(id + 0x10, 3.2f, t => t.NumCasts > 0, "Cone 1");
        ComponentCondition<ThermalWave>(id + 0x12, 2f, t => t.NumCasts > 1, "Cone 2");

        Timeout(id + 0xFF0000, 10000f, "Enrage");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 735, NameID = 12)]
public class SartauvoirTheInferno(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 145), new ArenaBoundsSquare(18));

