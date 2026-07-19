namespace BossMod.Dawntrail.Foray.FATE.Lifereaper;

public enum OID : uint {
    Boss = 0x4772, // R3.500, x1
    Lifereaper = 0x4773, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 42901, // Boss->player, no cast, single-target
    Teleport = 42186, // Boss->location, no cast, single-target
    Teleport2 = 42196, // Boss->location, no cast, single-target

    SoulSweepTarget = 42192, // Boss->player, no cast, single-target
    SoulSweep = 42177, // Boss->self, 6.0s cast, range 60 130-degree cone
    Menace = 42175, // Boss->self, 6.0s cast, range 20 circle

    MenacingChargeCast = 42179, // Boss->location, 8.0s cast, width 8 rect charge
    MenaceChargeAOE = 42180, // Boss->self, 2.0s cast, range 20 circle

    SweepingChargeCast = 42178, // Boss->location, 8.0s cast, width 8 rect charge
    SweepingChargeCone = 42181, // Boss->self, 2.0s cast, range 60 130-degree cone

    HallOfSorrow = 42182, // Boss->self, 3.0s cast, single-target
    HallOfSorrow1 = 42183, // 4773->location, 4.0s cast, range 10 circle

    DismalRoarCast = 42184, // Boss->self, 5.0s cast, single-target
    DismalRoar = 42185, // Lifereaper->self, 5.0s cast, range 60 circle
}

class SoulSweep(BossModule module) : Components.GroupedAOEs(module, [AID.SoulSweep, AID.SweepingChargeCone], new AOEShapeCone(60.0f, 65.0f.Degrees()));
class Menace(BossModule module) : Components.StandardAOEs(module, AID.Menace, new AOEShapeCircle(20.0f));
class HallOfSorrow(BossModule module) : Components.StandardAOEs(module, AID.HallOfSorrow1, 10.0f, highlightImminent: true);
class DismalRoar(BossModule module) : Components.RaidwideCast(module, AID.DismalRoar);

// SweepingChargeCone doesn't seem to be a fixed angle turn, but rather certain points in how the boss will turn
// easier to tell the player to just follow the charge or not
class SweepingCharge(BossModule module) : Components.ChargeAOEs(module, AID.SweepingChargeCast, 4.0f) {
    private bool active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.SweepingChargeCast) {
            active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.SweepingChargeCone) {
            active = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints) {
        base.AddGlobalHints(hints);
        if (active == true) {
            hints.Add("Follow the charge attack!");
        }
    }
}

class MenacingCharge(BossModule module) : Components.ChargeAOEs(module, AID.MenacingChargeCast, 4.0f) {
    private bool active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.MenacingChargeCast) {
            active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.MenaceChargeAOE) {
            active = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints) {
        base.AddGlobalHints(hints);
        if (active == true) {
            hints.Add("Keep away from the charge attack!");
        }
    }
}

class MenacingChargeAOE(BossModule module) : Components.StandardAOEs(module, AID.MenaceChargeAOE, 20.0f) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.MenacingChargeCast) {
            aoes.Add(new(new AOEShapeCircle(20.0f), spell.LocXZ, spell.Rotation, Module.WorldState.CurrentTime.AddSeconds(13.2f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.MenaceChargeAOE) {
            aoes.Clear();
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;
}

class LifereaperStates : StateMachineBuilder {
    public LifereaperStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<SoulSweep>()
            .ActivateOnEnter<Menace>()
            .ActivateOnEnter<MenacingCharge>()
            .ActivateOnEnter<SweepingCharge>()
            .ActivateOnEnter<MenacingChargeAOE>()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<DismalRoar>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13741)]
public class Lifereaper(WorldState ws, Actor primary) : BossModule(ws, primary, new(416.2f, -10.0f), new ArenaBoundsCircle(40));
