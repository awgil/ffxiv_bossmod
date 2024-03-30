namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class Exodus : BossComponent
{
    private int _numCrystalsDestroyed;
    private DateTime _activation;

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_activation != default)
            hints.Add("Raidwide");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), _activation));
    }

    public override void OnActorDestroyed(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.CrystalOfLight)
        {
            ++_numCrystalsDestroyed;
            if (_numCrystalsDestroyed == 6)
                _activation = module.WorldState.CurrentTime.AddSeconds(7.2f);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Exodus2)
            _activation = default;
    }
}
