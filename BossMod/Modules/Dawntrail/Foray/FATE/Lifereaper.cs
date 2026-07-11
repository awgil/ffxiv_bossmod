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
}

class SoulSweep(BossModule module) : Components.StandardAOEs(module, AID.SoulSweep, new AOEShapeCone(60.0f, 65.0f.Degrees()));
class Menace(BossModule module) : Components.StandardAOEs(module, AID.Menace, new AOEShapeCircle(20.0f));
class MenacingCharge(BossModule module) : Components.ChargeAOEs(module, AID.MenacingChargeCast, 4.0f);
class SweepingCharge(BossModule module) : Components.ChargeAOEs(module, AID.SweepingChargeCast, 4.0f);
class HallOfSorrow(BossModule module) : Components.StandardAOEs(module, AID.HallOfSorrow1, 10.0f, highlightImminent: true);

class ChargeAOE(BossModule module) : Components.GenericAOEs(module, AID.MenaceChargeAOE) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.MenacingChargeCast) {
            aoes.Add(new(new AOEShapeCircle(20.0f), spell.LocXZ, spell.Rotation, Module.WorldState.CurrentTime.AddSeconds(13.2f)));
        }

        if (spell.Action.ID == (uint)AID.SweepingChargeCast) {
            aoes.Add(new(new AOEShapeCone(60.0f, 65.0f.Degrees()), spell.LocXZ, Angle.FromDirection(caster.Position - spell.LocXZ), Module.WorldState.CurrentTime.AddSeconds(13.2f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.MenaceChargeAOE || spell.Action.ID == (uint)AID.SweepingChargeCone) {
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
            .ActivateOnEnter<ChargeAOE>()
            .ActivateOnEnter<HallOfSorrow>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13741)]
public class Lifereaper(WorldState ws, Actor primary) : BossModule(ws, primary, new(416.2f, -10.0f), new ArenaBoundsCircle(40));
