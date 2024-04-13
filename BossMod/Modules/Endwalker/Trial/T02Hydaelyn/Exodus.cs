namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class Exodus(BossModule module) : BossComponent(module)
{
    private int _numCrystalsDestroyed;
    private DateTime _activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activation != default)
            hints.Add("Raidwide");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), _activation));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.CrystalOfLight)
        {
            ++_numCrystalsDestroyed;
            if (_numCrystalsDestroyed == 6)
                _activation = WorldState.FutureTime(7.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Exodus2)
            _activation = default;
    }
}
