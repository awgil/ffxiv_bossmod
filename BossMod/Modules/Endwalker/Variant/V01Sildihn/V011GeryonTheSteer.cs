namespace BossMod.Endwalker.Variant.V01Sildihn.V011GeryonTheSteer;

public enum OID : uint
{
    Boss = 0x398B, // R9.600, x1
    Helper = 0x233C, // R0.500, x10, Helper type
    PowderKegBlue = 0x39C9, // R1.000, x0 (spawn during fight)
    PowderKegRed = 0x398C, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    ColossalStrike = 29903, // Boss->player, 5.0s cast, single-target
    ExplodingCatapult = 29895, // Boss->self, 5.0s cast, range 60 circle
    ExplosionCircle = 29908, // PowderKegRed/PowderKegBlue->self, 2.5s cast, range 15 circle
    ExplosionDonut = 29909, // PowderKegBlue/PowderKegRed->self, 2.5s cast, range 3-17 donut
    ColossalSlam = 29904, // Boss->self, 6.0s cast, range 60 60-degree cone
    SubterraneanShudder = 29906, // Boss->self, 5.0s cast, range 60 circle
    RollingBoulder = 29914, // Helper->self, no cast, range 10 width 10 rect
    BossJump = 29894, // Boss->location, no cast, single-target
    ColossalLaunch = 29896, // Boss->self, 5.0s cast, range 40 width 40 rect
    FlipToRed = 31260, // PowderKegBlue->self, no cast, single-target
    FlipToBlue = 29907, // PowderKegRed->self, no cast, single-target
    ColossalChargeRight = 29900, // Boss->location, 8.0s cast, width 14 rect charge
    ColossalChargeLeft = 29901, // Boss->location, 8.0s cast, width 14 rect charge
    GigantomillCCWCast = 29897, // Boss->self, 8.0s cast, range 72 width 10 cross
    GigantomillCWCast = 29898, // Boss->self, 8.0s cast, range 72 width 10 cross
    GigantomillRepeat = 29899, // Boss->self, no cast, range 72 width 10 cross
}

class ColossalStrike(BossModule module) : Components.SingleTargetCast(module, AID.ColossalStrike);
class ExplodingCatapult(BossModule module) : Components.RaidwideCast(module, AID.ExplodingCatapult);
class ExplosionCircle(BossModule module) : Components.StandardAOEs(module, AID.ExplosionCircle, 15);
class ExplosionDonut(BossModule module) : Components.StandardAOEs(module, AID.ExplosionDonut, new AOEShapeDonut(3, 17));
class ColossalSlam(BossModule module) : Components.StandardAOEs(module, AID.ColossalSlam, new AOEShapeCone(60, 30.Degrees()));
class SubterraneanShudder(BossModule module) : Components.RaidwideCast(module, AID.SubterraneanShudder);
class ColossalLaunch(BossModule module) : Components.RaidwideCast(module, AID.ColossalLaunch);
class ColossalChargeRight(BossModule module) : Components.ChargeAOEs(module, AID.ColossalChargeRight, 7);
class ColossalChargeLeft(BossModule module) : Components.ChargeAOEs(module, AID.ColossalChargeLeft, 7);

class Gigantomill(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GigantomillCWCast:
                Sequences.Add(new(new AOEShapeCross(72, 5), spell.LocXZ, spell.Rotation, -22.5f.Degrees(), Module.CastFinishAt(spell), 1.7f, 5));
                break;
            case AID.GigantomillCCWCast:
                Sequences.Add(new(new AOEShapeCross(72, 5), spell.LocXZ, spell.Rotation, 22.5f.Degrees(), Module.CastFinishAt(spell), 1.7f, 5));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GigantomillCCWCast or AID.GigantomillCWCast or AID.GigantomillRepeat)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class V011GeryonTheSteerStates : StateMachineBuilder
{
    public V011GeryonTheSteerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ColossalStrike>()
            .ActivateOnEnter<ExplodingCatapult>()
            .ActivateOnEnter<ExplosionCircle>()
            .ActivateOnEnter<ExplosionDonut>()
            .ActivateOnEnter<ColossalSlam>()
            .ActivateOnEnter<SubterraneanShudder>()
            .ActivateOnEnter<ColossalLaunch>()
            .ActivateOnEnter<ColossalChargeRight>()
            .ActivateOnEnter<ColossalChargeLeft>()
            .ActivateOnEnter<Gigantomill>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11442)]
public class V011GeryonTheSteer(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(19.5f));

