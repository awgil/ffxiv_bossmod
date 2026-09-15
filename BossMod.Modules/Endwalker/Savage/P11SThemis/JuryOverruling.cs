namespace BossMod.Endwalker.Savage.P11SThemis;

class JuryOverrulingProtean(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeRect(50, 4))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.JuryOverrulingProteanLight or AID.JuryOverrulingProteanDark)
            ++NumCasts;
    }
}

class IllusoryGlare(BossModule module) : Components.StandardAOEs(module, AID.IllusoryGlare, new AOEShapeCircle(5));
class IllusoryGloom(BossModule module) : Components.StandardAOEs(module, AID.IllusoryGloom, new AOEShapeDonut(2, 9));
