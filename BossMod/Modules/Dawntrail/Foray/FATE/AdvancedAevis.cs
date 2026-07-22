namespace BossMod.Modules.Dawntrail.Foray.FATE.AdvancedAevis;

public enum OID : uint {
    Boss = 0x4737,
    Helper = 0x233C,
    AdvancedAevis = 0x4738, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 42005, // Boss->player, no cast, single-target
    Teleport = 41995, // Boss->location, no cast, single-target

    ZombieScalesCast = 41998, // Boss->self, 8.0s cast, single-target
    ZombieScalesCast1 = 41999, // Boss->self, 8.0s cast, single-target
    ZombieScalesCast2 = 41996, // Boss->self, 8.0s cast, single-target
    ZombieScalesCast3 = 41997, // Boss->self, 8.0s cast, single-target
    ZombieScales = 42000, // 4738->location, 8.0s cast, range 40 ?-degree cone
    ZombieScales1 = 42001, // 4738->location, 8.0s cast, range 40 ?-degree cone

    AeroIICast = 42017, // Boss->self, 3.0s cast, single-target
    AeroII = 42018, // 4738->location, 3.0s cast, range 4 circle

    TripleFlight = 42012, // Boss->self, 4.0s cast, range 10-20 donut
    CyclonicRing = 42013, // Boss->self, no cast, range ?-20 donut
    Cyclone = 42009, // 4738->location, 4.0s cast, range 10 circle
    Cyclone1 = 42011, // AdvancedAevis->location, no cast, range 10 circle
    Cyclone2 = 42010, // Boss->self, no cast, single-target
    FlashFoehn = 42014, // Boss->self, no cast, range 80 width 10 rect

    ZombieBreath = 42004, // Boss->self, 5.0s cast, range 40 180-degree cone

    BreathWingCast = 42008, // Boss->self, 4.0s cast, single-target
    BreathWingCast1 = 42006, // Boss->self, 5.0s cast, single-target
    BreathWing = 42007, // AdvancedAevis->location, 5.0s cast, range 30 circle

    QuarryLakeBoss = 42002, // Boss->self, 5.0s cast, single-target
    QuarryLake = 42003, // AdvancedAevis->location, 5.0s cast, range 40 circle
}

class ZombieScales(BossModule module) : Components.GroupedAOEs(module, [AID.ZombieScales, AID.ZombieScales1], new AOEShapeCone(40.0f, 22.5f.Degrees()), 4, true);
class AeroII(BossModule module) : Components.StandardAOEs(module, AID.AeroII, 4f);
class ZombieBreath(BossModule module) : Components.StandardAOEs(module, AID.ZombieBreath, new AOEShapeCone(40f, 90.Degrees()));
class BreathWing(BossModule module) : Components.RaidwideCast(module, AID.BreathWing);
class QuarryLake(BossModule module) : Components.CastGaze(module, AID.QuarryLake);

class TripleFlight(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TripleFlight) {
            aoes.Add(new(new AOEShapeDonut(10.0f, 20.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCircle(10.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 2.1f), Risky: false));
            aoes.Add(new(new AOEShapeRect(40.0f, 5.0f, 40.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 4.2f), Risky: false));
        }

        if (spell.Action.ID == (uint)AID.Cyclone) {
            aoes.Add(new(new AOEShapeCircle(10.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), Risky: false));
            aoes.Add(new(new AOEShapeDonut(10.0f, 20.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 2.1f)));
            aoes.Add(new(new AOEShapeRect(40.0f, 5.0f, 40.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 4.2f), Risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TripleFlight || spell.Action.ID == (uint)AID.Cyclone ||
            spell.Action.ID == (uint)AID.CyclonicRing || spell.Action.ID == (uint)AID.Cyclone1 ||
            spell.Action.ID == (uint)AID.FlashFoehn) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        foreach (var aoe in aoes.Take(2)) {
            yield return aoe with { Color = show == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = show == 0 };
            show++;
        }
    }
}

class AdvancedAevisStates : StateMachineBuilder {
    public AdvancedAevisStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<ZombieScales>()
            .ActivateOnEnter<AeroII>()
            .ActivateOnEnter<TripleFlight>()
            .ActivateOnEnter<ZombieBreath>()
            .ActivateOnEnter<BreathWing>()
            .ActivateOnEnter<QuarryLake>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13704)]
public class AdvancedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-48.0f, -320.0f), new ArenaBoundsCircle(40));
