namespace BossMod.Endwalker.Quest.Endwalker;

class AetherialRay : Components.GenericBaitAway
{
    private DateTime _activation;

    public AetherialRay() : base(centerAtTarget: true) { }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_activation != default)
            hints.Add("Tankbuster 5x");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.PredictedDamage.Add((new(1), _activation));
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialRay)
            _activation = spell.NPCFinishAt;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialRay)
            ++NumCasts;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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
