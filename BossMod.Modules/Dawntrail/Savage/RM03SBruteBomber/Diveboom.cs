namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class OctoboomDiveProximity(BossModule module) : Components.StandardAOEs(module, AID.OctoboomDiveProximityAOE, new AOEShapeCircle(20)); // TODO: verify falloff
class OctoboomDiveKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.OctoboomDiveKnockbackAOE, 25);
class QuadroboomDiveProximity(BossModule module) : Components.StandardAOEs(module, AID.QuadroboomDiveProximityAOE, new AOEShapeCircle(20)); // TODO: verify falloff
class QuadroboomDiveKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.QuadroboomDiveKnockbackAOE, 25);

class Diveboom(BossModule module) : Components.UniformStackSpread(module, 5, 5, alwaysShowSpreads: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OctoboomDiveProximityAOE:
            case AID.OctoboomDiveKnockbackAOE:
                AddSpreads(Raid.WithoutSlot(true), Module.CastFinishAt(spell));
                break;
            case AID.QuadroboomDiveProximityAOE:
            case AID.QuadroboomDiveKnockbackAOE:
                // TODO: can target any role
                AddStacks(Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DiveboomSpread or AID.DiveboomPair)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
