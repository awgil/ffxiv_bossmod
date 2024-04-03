namespace BossMod.Endwalker.Alliance.A35Eulogia;

class Quintessence : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly Dictionary<AID, DateTime> _formToEffectTiming = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        // Map initial forms to their visual effects and store the timing
        switch ((AID)spell.Action.ID)
        {
            case AID.FirstFormAOE:
            case AID.SecondFormAOE:
            case AID.ThirdFormAOE:
            case AID.FirstFormRight:
            case AID.FirstFormLeft:
            case AID.SecondFormRight:
            case AID.SecondFormLeft:
            case AID.ThirdFormRight:
            case AID.ThirdFormLeft:
                _formToEffectTiming[(AID)spell.Action.ID] = spell.NPCFinishAt;
                break;
        }

        AOEShape? shape = DetermineShape((AID)spell.Action.ID);
        DateTime effectTiming = _formToEffectTiming.GetValueOrDefault((AID)spell.Action.ID, spell.NPCFinishAt); // Use specific timing if available, else fallback to current spell's timing

        if (shape != null)
        {
            _aoes.Add(new AOEInstance(shape, caster.Position, spell.Rotation, effectTiming.AddSeconds(26.25f)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    private AOEShape? DetermineShape(AID actionID)
    {
        return actionID switch
        {
            AID.QuintessenceFirstAOE or AID.QuintessenceSecondAOE or AID.QuintessenceThirdAOE => new AOEShapeDonut(8, 60),
            AID.QuintessenceFirstRight or AID.QuintessenceFirstLeft or
            AID.QuintessenceSecondRight or AID.QuintessenceSecondLeft or
            AID.QuintessenceThirdRight or AID.QuintessenceThirdLeft => new AOEShapeCone(50, 90.Degrees()),
            _ => null
        };
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.QuintessenceFirstRight or AID.QuintessenceFirstLeft or AID.QuintessenceFirstAOE or AID.QuintessenceSecondRight or AID.QuintessenceSecondLeft or AID.QuintessenceSecondAOE or AID.QuintessenceThirdRight or AID.QuintessenceThirdLeft or AID.QuintessenceThirdAOE)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.QuintessenceThirdRight or AID.QuintessenceThirdLeft or AID.QuintessenceThirdAOE)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}