namespace BossMod.Endwalker.Quest.Endwalker;

class AetherialRay(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private DateTime _activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activation != default)
            hints.Add("Tankbuster 5x");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddPredictedDamage(new(1), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialRay)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialRay)
            ++NumCasts;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialRayVisual)
        {
            ++NumCasts;
            if (NumCasts == 5)
            {
                CurrentBaits.Clear();
                NumCasts = 0;
                _activation = default;
            }
        }
    }
}
