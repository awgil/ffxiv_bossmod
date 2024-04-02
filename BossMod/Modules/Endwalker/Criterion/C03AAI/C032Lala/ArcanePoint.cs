namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

// TODO: we could detect aoe positions slightly earlier, when golems spawn
class ConstructiveFigure : Components.SelfTargetedAOEs
{
    public ConstructiveFigure(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(50, 4)) { }
}
class NConstructiveFigure : ConstructiveFigure { public NConstructiveFigure() : base(AID.NAero) { } }
class SConstructiveFigure : ConstructiveFigure { public SConstructiveFigure() : base(AID.SAero) { } }

class ArcanePoint : BossComponent
{
    public int NumCasts { get; private set; }
    private ArcanePlot? _plot;

    public override void Init(BossModule module)
    {
        _plot = module.FindComponent<ArcanePlot>();
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (NumCasts > 0)
            return;
        var spot = CurrentSafeSpot(actor.Position);
        if (spot != null && module.Raid.WithoutSlot().Exclude(actor).Any(p => CurrentSafeSpot(p.Position) == spot))
            hints.Add("Spread on different squares!");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return PlayerPriority.Interesting;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (NumCasts > 0)
            return;
        var spot = CurrentSafeSpot(pc.Position);
        if (spot != null)
            ArcaneArrayPlot.Shape.Draw(arena, spot.Value, default, ArenaColor.SafeFromAOE);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

class ExplosiveTheorem : Components.SpreadFromCastTargets
{
    public ExplosiveTheorem(AID aid) : base(ActionID.MakeSpell(aid), 8) { }
}
class NExplosiveTheorem : ExplosiveTheorem { public NExplosiveTheorem() : base(AID.NExplosiveTheoremAOE) { } }
class SExplosiveTheorem : ExplosiveTheorem { public SExplosiveTheorem() : base(AID.SExplosiveTheoremAOE) { } }

class TelluricTheorem : Components.LocationTargetedAOEs
{
    public TelluricTheorem(AID aid) : base(ActionID.MakeSpell(aid), 8) { }
}
class NTelluricTheorem : TelluricTheorem { public NTelluricTheorem() : base(AID.NTelluricTheorem) { } }
class STelluricTheorem : TelluricTheorem { public STelluricTheorem() : base(AID.STelluricTheorem) { } }
