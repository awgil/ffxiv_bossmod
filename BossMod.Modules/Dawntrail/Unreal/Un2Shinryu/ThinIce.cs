namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class ThinIce(BossModule module) : BossComponent(module)
{
    private DateTime _lands;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DiamondDust)
            _lands = Module.CastFinishAt(spell, 0.6f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus(SID.ThinIce) != null)
            hints.AddSpecialMode(AIHints.SpecialMode.PyreticMove, WorldState.CurrentTime);
        else if (_lands != default && WorldState.CurrentTime < _lands)
            hints.AddSpecialMode(AIHints.SpecialMode.PyreticMove, _lands);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus(SID.ThinIce) != null)
            hints.Add("Thin ice: any step slides you 25y, stand still!");
        else if (_lands != default && WorldState.CurrentTime < _lands && WorldState.CurrentTime > _lands.AddSeconds(-2))
            hints.Add("Stop moving before the ice lands", false);
    }
}
