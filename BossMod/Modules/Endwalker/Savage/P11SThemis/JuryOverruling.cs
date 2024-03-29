namespace BossMod.Endwalker.Savage.P11SThemis;

class JuryOverrulingProtean : Components.BaitAwayEveryone
{
    public JuryOverrulingProtean() : base(new AOEShapeRect(50, 4)) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.JuryOverrulingProteanLight or AID.JuryOverrulingProteanDark)
            ++NumCasts;
    }
}

class IllusoryGlare : Components.SelfTargetedAOEs
{
    public IllusoryGlare() : base(ActionID.MakeSpell(AID.IllusoryGlare), new AOEShapeCircle(5)) { }
}

class IllusoryGloom : Components.SelfTargetedAOEs
{
    public IllusoryGloom() : base(ActionID.MakeSpell(AID.IllusoryGloom), new AOEShapeDonut(2, 9)) { }
}
