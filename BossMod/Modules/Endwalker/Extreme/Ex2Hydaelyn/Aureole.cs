namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class LateralAureole1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LateralAureole1AOE), new AOEShapeCone(40, 75.Degrees()));
class LateralAureole2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LateralAureole2AOE), new AOEShapeCone(40, 75.Degrees()));
class Aureole1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Aureole1AOE), new AOEShapeCone(40, 75.Degrees()));
class Aureole2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Aureole2AOE), new AOEShapeCone(40, 75.Degrees()));

// component tracking [lateral] aureole mechanic, only exists for the timeline anymore
class Aureole(BossModule module) : BossComponent(module)
{
    public bool Done { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Aureole1AOE or AID.Aureole2AOE or AID.LateralAureole1AOE or AID.LateralAureole2AOE)
            Done = true;
    }
}
