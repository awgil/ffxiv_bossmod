namespace BossMod.Stormblood.Trial.T02Lakshmi;

class DivineDenial(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DivineDenial));
class Stotram1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Stotram1));
class Stotram2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Stotram2));
class ThePallOfLightStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ThePallOfLightStack), 7, 8);
class ThePullOfLightTB1(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ThePullOfLightTB1));
class ThePullOfLightTB2(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ThePullOfLightTB2));
class BlissfulArrow1(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlissfulArrow1));
class BlissfulArrow2(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlissfulArrow2));
class BlissfulSpear1(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlissfulSpear1));
class BlissfulSpear2(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlissfulSpear2));
class BlissfulSpear3(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlissfulSpear3));
class BlissfulSpear4(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlissfulSpear4));

class BlissfulSpear : Components.BaitAwayIcon
{
    public BlissfulSpear(BossModule module) : base(module, new AOEShapeCircle(7), (uint)IconID.Spread1, ActionID.MakeSpell(AID.BlissfulSpear1), 3) { CenterAtTarget = true; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}

class BlissfulArrow : Components.BaitAwayIcon
{
    public BlissfulArrow(BossModule module) : base(module, new AOEShapeCircle(7), (uint)IconID.Spread2, ActionID.MakeSpell(AID.BlissfulArrow2), 3) { CenterAtTarget = true; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 263, NameID = 6385)]
public class T02Lakshmi(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(0, 0), 20));