namespace BossMod.Endwalker.Savage.P11SThemis;

class UpheldOverruling(BossModule module) : Components.UniformStackSpread(module, 6, 13, 7, alwaysShowSpreads: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UpheldOverrulingLight:
            case AID.UpheldRulingLight:
                if (WorldState.Actors.Find(caster.Tether.Target) is var stackTarget && stackTarget != null)
                    AddStack(stackTarget, spell.NPCFinishAt.AddSeconds(0.3f));
                break;
            case AID.UpheldOverrulingDark:
            case AID.UpheldRulingDark:
                if (WorldState.Actors.Find(caster.Tether.Target) is var spreadTarget && spreadTarget != null)
                    AddSpread(spreadTarget, spell.NPCFinishAt.AddSeconds(0.3f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
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

class LightburstBoss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightburstBoss), new AOEShapeCircle(13));
class LightburstClone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightburstClone), new AOEShapeCircle(13));
class DarkPerimeterBoss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkPerimeterBoss), new AOEShapeDonut(8, 50));
class DarkPerimeterClone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkPerimeterClone), new AOEShapeDonut(8, 50));
