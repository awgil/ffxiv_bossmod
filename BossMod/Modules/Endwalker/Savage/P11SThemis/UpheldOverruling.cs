namespace BossMod.Endwalker.Savage.P11SThemis;

class UpheldOverruling : Components.UniformStackSpread
{
    public UpheldOverruling() : base(6, 13, 7, alwaysShowSpreads: true) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UpheldOverrulingLight:
            case AID.UpheldRulingLight:
                if (module.WorldState.Actors.Find(caster.Tether.Target) is var stackTarget && stackTarget != null)
                    AddStack(stackTarget, spell.NPCFinishAt.AddSeconds(0.3f));
                break;
            case AID.UpheldOverrulingDark:
            case AID.UpheldRulingDark:
                if (module.WorldState.Actors.Find(caster.Tether.Target) is var spreadTarget && spreadTarget != null)
                    AddSpread(spreadTarget, spell.NPCFinishAt.AddSeconds(0.3f));
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UpheldOverrulingAOELight:
            case AID.UpheldRulingAOELight:
                Stacks.Clear();
                break;
            case AID.UpheldOverrulingAOEDark:
            case AID.UpheldRulingAOEDark:
                Spreads.Clear();
                break;
        }
    }
}

class LightburstBoss : Components.SelfTargetedAOEs
{
    public LightburstBoss() : base(ActionID.MakeSpell(AID.LightburstBoss), new AOEShapeCircle(13)) { }
}

class LightburstClone : Components.SelfTargetedAOEs
{
    public LightburstClone() : base(ActionID.MakeSpell(AID.LightburstClone), new AOEShapeCircle(13)) { }
}

class DarkPerimeterBoss : Components.SelfTargetedAOEs
{
    public DarkPerimeterBoss() : base(ActionID.MakeSpell(AID.DarkPerimeterBoss), new AOEShapeDonut(8, 50)) { }
}

class DarkPerimeterClone : Components.SelfTargetedAOEs
{
    public DarkPerimeterClone() : base(ActionID.MakeSpell(AID.DarkPerimeterClone), new AOEShapeDonut(8, 50)) { }
}
