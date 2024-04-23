namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

// TODO: we could detect aoe positions slightly earlier, when golems spawn
class ConstructiveFigure(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(50, 4));
class NConstructiveFigure(BossModule module) : ConstructiveFigure(module, AID.NAero);
class SConstructiveFigure(BossModule module) : ConstructiveFigure(module, AID.SAero);

class ArcanePoint(BossModule module) : BossComponent(module)
{
    public int NumCasts { get; private set; }
    private readonly ArcanePlot? _plot = module.FindComponent<ArcanePlot>();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts > 0)
            return;
        var spot = CurrentSafeSpot(actor.Position);
        if (spot != null && Raid.WithoutSlot().Exclude(actor).Any(p => CurrentSafeSpot(p.Position) == spot))
            hints.Add("Spread on different squares!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return PlayerPriority.Interesting;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (NumCasts > 0)
            return;
        var spot = CurrentSafeSpot(pc.Position);
        if (spot != null)
            ArcaneArrayPlot.Shape.Draw(Arena, spot.Value, default, ArenaColor.SafeFromAOE);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NPowerfulLight or AID.SPowerfulLight)
        {
            ++NumCasts;
            _plot?.AddAOE(caster.Position, default);
        }
    }

    public WPos? CurrentSafeSpot(WPos pos)
    {
        if (_plot == null)
            return null;
        var index = _plot.SafeZoneCenters.FindIndex(p => ArcaneArrayPlot.Shape.Check(pos, p, default));
        return index >= 0 ? _plot.SafeZoneCenters[index] : null;
    }
}

class ExplosiveTheorem(BossModule module, AID aid) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(aid), 8);
class NExplosiveTheorem(BossModule module) : ExplosiveTheorem(module, AID.NExplosiveTheoremAOE);
class SExplosiveTheorem(BossModule module) : ExplosiveTheorem(module, AID.SExplosiveTheoremAOE);

class TelluricTheorem(BossModule module, AID aid) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(aid), 8);
class NTelluricTheorem(BossModule module) : TelluricTheorem(module, AID.NTelluricTheorem);
class STelluricTheorem(BossModule module) : TelluricTheorem(module, AID.STelluricTheorem);
