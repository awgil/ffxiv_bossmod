namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class ShockDonutBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeDonut(1, 6), (uint)IconID.ShockDonut, ActionID.MakeSpell(AID._Weaponskill_ShockDonut1), centerAtTarget: true);
class ShockCircleBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(4), (uint)IconID.ShockCircle, ActionID.MakeSpell(AID._Weaponskill_ShockCircle1), centerAtTarget: true);

class ShockAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Origin, int Casts, bool Donut)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(c.Donut ? new AOEShapeDonut(1, 6) : new AOEShapeCircle(4), c.Origin));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ShockDonut1:
                Casters.Add((caster.Position, 11, true));
                NumCasts++;
                break;
            case AID._Weaponskill_ShockCircle1:
                if (!Increment(caster))
                    Casters.Add((caster.Position, 11, false));
                NumCasts++;
                break;

            case AID._Weaponskill_ShockCircle2:
            case AID._Weaponskill_ShockCircle3:
            case AID._Weaponskill_ShockCircle4:
            case AID._Weaponskill_ShockCircle5:
            case AID._Weaponskill_ShockCircle6:
                if (!Increment(caster))
                    ReportError($"no circle caster at {caster.Position}");
                NumCasts++;
                break;

            case AID._Weaponskill_ShockDonut2:
                NumCasts++;
                if (!Increment(caster))
                    ReportError($"no donut caster at {caster.Position}");
                break;
        }
    }

    private bool Increment(Actor caster)
    {
        if (Casters.FindIndex(c => c.Origin.AlmostEqual(caster.Position, 0.1f)) is var ix && ix >= 0)
        {
            Casters.Ref(ix).Casts--;
            if (Casters[ix].Casts <= 0)
                Casters.RemoveAt(ix);
            return true;
        }

        return false;
    }
}
