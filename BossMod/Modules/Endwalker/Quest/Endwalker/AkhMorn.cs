namespace BossMod.Endwalker.Quest.Endwalker;

class AkhMorn(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private DateTime _activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activation != default)
            hints.Add($"Tankbuster x{NumExpectedCasts()}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.PredictedDamage.Add((new(1), _activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, Raid.Player()!, new AOEShapeCircle(4)));
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
            ++NumCasts;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornVisual)
        {
            ++NumCasts;
            if (NumCasts == NumExpectedCasts())
            {
                CurrentBaits.Clear();
                NumCasts = 0;
                _activation = default;
            }
        }
    }

    private int NumExpectedCasts() => Module.PrimaryActor.IsDead ? 8 : 6;
}
